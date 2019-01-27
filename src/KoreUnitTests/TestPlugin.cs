﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract;
using Kontract.Attributes;
using Kontract.Interfaces.Common;
using System.IO;
using Kontract.Interfaces.FileSystem;
using Kontract.Interfaces.Archive;

namespace KoreUnitTests
{
    [Export(typeof(IIdentifyFiles))]
    [Export(typeof(ILoadFiles))]
    [Export(typeof(ISaveFiles))]
    [Export(typeof(ITest))]
    [PluginExtensionInfo("*.test")]
    [PluginInfo("TestTextId", "TestPlugin", "Test", "onepiecefreak", "github.com", "A test plugin for UnitTests")]
    public class TestPlugin : IIdentifyFiles, ILoadFiles, ISaveFiles, ITest
    {
        public List<string> Communication { get; set; }

        public bool LeaveOpen { get; set; }

        public void Dispose()
        {
            ;
        }

        public bool Identify(StreamInfo file)
        {
            using (var br = new BinaryReader(file.FileData, Encoding.ASCII, LeaveOpen))
                return br.ReadUInt32() == 0x16161616;
        }

        public void Load(StreamInfo filename)
        {
            Communication = new List<string>() { "string1", "string2" };
        }

        public void Save(StreamInfo initialFile, int versionIndex = 0)
        {
            using (var bw = new BinaryWriter(initialFile.FileData, Encoding.ASCII, LeaveOpen))
                bw.Write(0x16161616);
            Communication.Add("string3");
        }
    }

    [Export(typeof(IArchiveAdapter))]
    [Export(typeof(ILoadFiles))]
    [Export(typeof(IIdentifyFiles))]
    [PluginExtensionInfo("*.testarch")]
    [PluginInfo("TestArchiveId", "TestArchivePlugin", "Archive", "onepiecefreak", "github.com", "A test archive plugin for UnitTests")]
    public class TestArchive : IArchiveAdapter, ILoadFiles, IIdentifyFiles
    {
        public List<ArchiveFileInfo> Files { get; private set; }

        public bool FileHasExtendedProperties => throw new NotImplementedException();

        public bool LeaveOpen { get; set; }

        public void Dispose()
        {
            ;
        }

        public bool Identify(StreamInfo file)
        {
            using (var br = new BinaryReader(file.FileData))
                return br.ReadUInt32() == 0x16161617;
        }

        public void Load(StreamInfo file)
        {
            using (var br = new BinaryReader(file.FileData, Encoding.ASCII, true))
            {
                br.BaseStream.Position = 4;

                var fileCount = br.ReadInt16();
                Files = new List<ArchiveFileInfo>();
                for (int i = 0; i < fileCount; i++)
                {
                    var length = br.ReadInt32();
                    Files.Add(new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = Encoding.ASCII.GetString(br.ReadBytes(0x20)).TrimEnd('\0'),
                        FileData = new MemoryStream(br.ReadBytes(length))
                    });
                }
            }
        }
    }
}
