﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Komponent.IO.Streams;
using Kontract.Kanvas;
using Kontract.Models.Image;
using Kryptography.Hash.Crc;
using plugin_level5.Compression;

namespace plugin_level5.Mobile.Images
{
    class Imga
    {
        private static readonly int HeaderSize = Tools.MeasureType(typeof(ImgaHeader));

        private ImgaHeader _header;

        private Level5CompressionMethod _tileTableCompression;
        private Level5CompressionMethod _imageDataCompression;

        public ImageInfo Load(Stream input)
        {
            using var br = new BinaryReaderX(input);

            // Header
            _header = br.ReadType<ImgaHeader>();

            // Get tile table
            var tileTableComp = new SubStream(input, _header.tableDataOffset, _header.tileTableSize);
            var tileTable = new MemoryStream();
            _tileTableCompression = Level5Compressor.PeekCompressionMethod(tileTableComp);
            Level5Compressor.Decompress(tileTableComp, tileTable);

            // Get image data
            var imageDataComp = new SubStream(input, _header.tableDataOffset + _header.tileTableSizePadded,
                _header.imgDataSize);
            var imageData = new MemoryStream();
            _imageDataCompression = Level5Compressor.PeekCompressionMethod(imageDataComp);
            Level5Compressor.Decompress(imageDataComp, imageData);

            // Combine tiles to full image data
            tileTable.Position = imageData.Position = 0;
            var combinedImageStream = CombineTiles(tileTable, imageData, _header.bitDepth);

            // Split image data and mip map data
            var images = new byte[_header.imageCount][];
            var (width, height) = ((_header.width + 7) & ~7, (_header.height + 7) & ~7);
            for (var i = 0; i < _header.imageCount; i++)
            {
                images[i] = new byte[width * height * _header.bitDepth / 8];
                combinedImageStream.Read(images[i], 0, images[i].Length);

                (width, height) = (width >> 1, height >> 1);
            }

            var imageInfo = new ImageInfo(images.FirstOrDefault(), images.Skip(1).ToArray(), _header.imageFormat, new Size(_header.width, _header.height));
            imageInfo.RemapPixels.With(context => new ImgaSwizzle(context));

            return imageInfo;
        }

        public void Save(Stream output, IKanvasImage image)
        {
            using var bw = new BinaryWriterX(output);

            // Header
            _header.width = (short)image.ImageSize.Width;
            _header.height = (short)image.ImageSize.Height;
            _header.imageFormat = (byte)image.ImageFormat;
            _header.bitDepth = (byte)image.BitDepth;

            // Write image data to stream
            var combinedImageStream = new MemoryStream();
            combinedImageStream.Write(image.ImageInfo.ImageData);
            for (var i = 0; i < image.ImageInfo.MipMapCount; i++)
                combinedImageStream.Write(image.ImageInfo.MipMapData[i]);

            // Create reduced tiles and indices
            var (imageData, tileTable) = SplitTiles(combinedImageStream, image.BitDepth);

            // Write tile table
            output.Position = HeaderSize;
            Level5Compressor.Compress(tileTable, output, _tileTableCompression);

            _header.tileTableSize = (int)output.Length - HeaderSize;
            _header.tileTableSizePadded = (_header.tileTableSize + 3) & ~3;

            // Write image tiles
            output.Position = HeaderSize + _header.tileTableSizePadded;
            Level5Compressor.Compress(imageData, output, _imageDataCompression);
            bw.WriteAlignment(4);

            _header.imgDataSize = (int)(output.Length - (HeaderSize + _header.tileTableSizePadded));

            // Write header
            bw.BaseStream.Position = 0;
            bw.WriteType(_header);
        }

        private Stream CombineTiles(Stream tileTableStream, Stream imageDataStream, int bitDepth)
        {
            using var tileTable = new BinaryReaderX(tileTableStream);

            var tileByteDepth = 64 * bitDepth / 8;
            var tile = new byte[tileByteDepth];

            var result = new MemoryStream();
            while (tileTableStream.Position < tileTableStream.Length)
            {
                var entry = tileTable.ReadInt16();

                if (entry >= 0)
                {
                    imageDataStream.Position = entry * tileByteDepth;
                    imageDataStream.Read(tile, 0, tile.Length);
                }
                else
                    Array.Clear(tile, 0, tile.Length);

                result.Write(tile, 0, tile.Length);
            }

            result.Position = 0;
            return result;
        }

        private (Stream imageData, Stream tileData) SplitTiles(Stream image, int bitDepth)
        {
            var tileByteDepth = 64 * bitDepth / 8;

            var tileTable = new MemoryStream();
            var imageData = new MemoryStream();
            using var tileBw = new BinaryWriterX(tileTable, true);
            using var imageBw = new BinaryWriterX(imageData, true);

            var crc32 = Crc32.Default;
            var tileDictionary = new Dictionary<uint, short>();

            // Add placeholder tile for all 0's
            var zeroTileHash = crc32.ComputeValue(new byte[tileByteDepth]);
            tileDictionary[zeroTileHash] = -1;

            var imageOffset = 0;
            var tileIndex = 0;
            while (imageOffset < image.Length)
            {
                var tile = new SubStream(image, imageOffset, tileByteDepth);
                var tileHash = crc32.ComputeValue(tile);
                if (!tileDictionary.ContainsKey(tileHash))
                {
                    tile.Position = 0;
                    tile.CopyTo(imageBw.BaseStream);

                    tileDictionary[tileHash] = (short)tileIndex++;
                }

                tileBw.Write(tileDictionary[tileHash]);

                imageOffset += tileByteDepth;
            }

            imageData.Position = tileTable.Position = 0;
            return (imageData, tileTable);
        }
    }
}
