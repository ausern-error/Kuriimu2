﻿using Eto.Drawing;
using Eto.Forms;
using Kuriimu2.EtoForms.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kuriimu2.EtoForms.Support
{
    public sealed class Themer
    {
        #region Localization Keys

        private const string ThemeRestartTextKey_ = "ThemeRestartText";
        private const string ThemeRestartCaptionKey_ = "ThemeRestartCaption";
        private const string ThemeUnsupportedPlatformTextKey_ = "ThemeUnsupportedPlatformText";
        private const string ThemeUnsupportedPlatformCaptionKey_ = "ThemeUnsupportedPlatformCaption";

        #endregion

        #region Singleton

        private static readonly Lazy<Themer> Lazy = new Lazy<Themer>(() => new Themer());
        public static Themer Instance => Lazy.Value;

        #endregion

        public readonly Dictionary<string, Theme> themes = new Dictionary<string, Theme>();

        private string _currentThemeKey;
        private bool _firstLoad = true;

        public void LoadThemes()
        {
            if (_firstLoad)
            {
                _currentThemeKey = Settings.Default.Theme;
                #region Themes

                #region Light theme

                themes.Add("light", new Theme(name:"light",
                mainColor: KnownColors.ThemeLight, altColor: KnownColors.Black, loggerBackColor: KnownColors.Black,
                loggerTextColor: KnownColors.NeonGreen, logFatalColor: KnownColors.DarkRed, logInfoColor: KnownColors.NeonGreen,
                logErrorColor: KnownColors.Red, logWarningColor: KnownColors.Orange, logDefaultColor: KnownColors.Wheat, hexByteBack1Color: Color.FromArgb(0xf0, 0xfd, 0xff),
                hexSidebarBackColor: Color.FromArgb(0xcd, 0xf7, 0xfd), controlColor: Color.FromArgb(0xf0, 0xfd, 0xff), menuBarBackColor: Color.FromArgb(245, 245, 245),
                unselectedTabBackColor: Color.FromArgb(238, 238, 238), windowBackColor: Color.FromArgb(240, 240, 240), archiveChangedColor: KnownColors.Orange,
                progressColor: KnownColors.LimeGreen, progressBorderColor: KnownColors.ControlDark, progressControlColor: KnownColors.Control, buttonBackColor: Color.FromArgb(221, 221, 221),
                buttonDisabledTextColor: KnownColors.Black, gridViewHeaderGradientColor: Color.FromArgb(243, 243, 243), gridViewHeaderBorderColor: Color.FromArgb(213, 213, 213),
                imageViewBackColor: KnownColors.DarkGreen,inactiveTreeGridSelectionColor:Color.FromArgb(240, 240, 240)));

                #endregion

                #endregion
                LoadJson();

                if (!themes.ContainsKey(_currentThemeKey))
                    _currentThemeKey = "light";

                _firstLoad = false;
            }
            else
            {
                #region Styling

                Eto.Style.Add<Label>(null, text =>
                {
                    text.TextColor = GetTheme().AltColor;
                });
                Eto.Style.Add<Dialog>(null, dialog =>
                {
                    dialog.BackgroundColor = GetTheme().MainColor;
                });
                Eto.Style.Add<CheckBox>(null, checkbox =>
                {
                    checkbox.BackgroundColor = GetTheme().MainColor;
                    checkbox.TextColor = GetTheme().AltColor;
                });
                Eto.Style.Add<GroupBox>(null, groupBox =>
                {
                    groupBox.BackgroundColor = GetTheme().MainColor;
                    groupBox.TextColor = GetTheme().AltColor;
                });

                #endregion
            }
        }
        public void ChangeTheme(string theme)
        {
            if (Application.Instance.Platform.IsWpf)
            {
                Settings.Default.Theme = theme;
                Settings.Default.Save();

                MessageBox.Show(Application.Instance.Localize(this, ThemeRestartTextKey_), Application.Instance.Localize(this, ThemeRestartCaptionKey_));
            }
            else
            {
                MessageBox.Show(Application.Instance.Localize(this, ThemeUnsupportedPlatformTextKey_), Application.Instance.Localize(this, ThemeUnsupportedPlatformCaptionKey_));
            }
        }
        public Theme GetTheme()
        {
            return themes[_currentThemeKey];
        }
        private void LoadJson()
        {
            var themeDirs = Directory.GetFiles(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Themes");
            foreach (var dir in themeDirs)
            {
                var theme = JsonConvert.DeserializeObject<Theme>(File.ReadAllText(dir));
                if(!themes.TryAdd(theme.Name, theme)){
                    themes.Add(theme.Name + theme.GetHashCode(), theme);
                }
            }
        }
    }
}
public class Theme
{
    public string Name { get; }
    public Color MainColor { get; }//Main background color
    public Color AltColor { get; }//text and foreground color
    public Color LoggerBackColor { get; }//Background of logger text areas
    public Color LoggerTextColor { get; }//Text of logger text areas
    public Color LogFatalColor { get; }//fatal logger errors color
    public Color LogInfoColor { get; }//Info logger text color
    public Color LogErrorColor { get; }//Error logger text color
    public Color LogWarningColor { get; }//warning logger text color
    public Color LogDefaultColor { get; }//defualt logger text color
    public Color HexByteBack1Color { get; } //every second byte in hex viewer
    public Color HexSidebarBackColor { get; }//side bar color in hex viewer
    public Color ControlColor { get; }
    public Color MenuBarBackColor { get; }//Back colour of top menu bar
    public Color UnselectedTabBackColor { get; }//Background of unselected tab
    public Color WindowBackColor { get; } //Back of the main window, NOT the main panel
    public Color ArchiveChangedColor { get; }//Archive viewer text color when a file is modified
    public Color ProgressColor { get; } //Colour of the moving bar in a progress bar
    public Color ProgressBorderColor { get; } //border color of progress bar
    public Color ProgressControlColor { get; }//Background color of the progress bar
    public Color ButtonBackColor { get; } //Background colour of a button
    public Color ButtonDisabledTextColor { get; } //Text colour of a greyedout/disabledbutton
    public Color GridViewHeaderGradientColor { get; } //Graident END color of gridview header
    public Color GridViewHeaderBorderColor { get; } //Border of grid view header
    public Color ImageViewBackColor { get; } //Background of image viewer
    public Color InactiveTreeGridSelectionColor { get; } //Background of image viewer
    public Theme(string name,Color mainColor, Color altColor, Color loggerBackColor, Color loggerTextColor,
        Color logFatalColor, Color logInfoColor, Color logErrorColor, Color logWarningColor, Color logDefaultColor,
        Color hexByteBack1Color, Color hexSidebarBackColor, Color controlColor, Color menuBarBackColor,
        Color unselectedTabBackColor, Color windowBackColor, Color archiveChangedColor, Color progressColor, Color progressBorderColor,
        Color progressControlColor, Color buttonDisabledTextColor, Color buttonBackColor, Color gridViewHeaderGradientColor, Color gridViewHeaderBorderColor,
        Color imageViewBackColor,Color inactiveTreeGridSelectionColor)
    {
        Name = name;
        MainColor = mainColor;
        AltColor = altColor;
        LoggerBackColor = loggerBackColor;
        LoggerTextColor = loggerTextColor;
        LogFatalColor = logFatalColor;
        LogInfoColor = logInfoColor;
        LogErrorColor = logErrorColor;
        LogWarningColor = logWarningColor;
        LogDefaultColor = logDefaultColor;
        HexByteBack1Color = hexByteBack1Color;
        HexSidebarBackColor = hexSidebarBackColor;
        ControlColor = controlColor;
        MenuBarBackColor = menuBarBackColor;
        UnselectedTabBackColor = unselectedTabBackColor;
        WindowBackColor = windowBackColor;
        ArchiveChangedColor = archiveChangedColor;
        ProgressColor = progressColor;
        ProgressBorderColor = progressBorderColor;
        ProgressControlColor = progressControlColor;
        ButtonBackColor = buttonBackColor;
        ButtonDisabledTextColor = buttonDisabledTextColor;
        GridViewHeaderGradientColor = gridViewHeaderGradientColor;
        GridViewHeaderBorderColor = gridViewHeaderBorderColor;
        ImageViewBackColor = imageViewBackColor;
        InactiveTreeGridSelectionColor = inactiveTreeGridSelectionColor;
    }
}