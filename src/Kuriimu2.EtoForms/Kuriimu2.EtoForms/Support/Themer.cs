using Eto.Drawing;
using Eto.Forms;
using Kuriimu2.EtoForms.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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

        #region Theme Constants

        private const string lightThemeLocation_ = "Kuriimu2.EtoForms.Resources.Themes.Light.json";
        private const string darkThemeLocation_ = "Kuriimu2.EtoForms.Resources.Themes.Dark.json";

        #endregion

        public readonly Dictionary<string, Theme> themes = new Dictionary<string, Theme>();

        private string _currentThemeKey;
        private bool _firstLoad = true;

        public void LoadThemes()
        {
            if (_firstLoad)
            {
                _currentThemeKey = Settings.Default.Theme;

                LoadJson();

                if (!themes.ContainsKey(_currentThemeKey))
                    _currentThemeKey = "Light";

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
            var files = new List<string>
            {
                LoadEmbeddedFile(lightThemeLocation_),
                LoadEmbeddedFile(darkThemeLocation_)
            };
            foreach (var dir in themeDirs)
            {
                files.Add(File.ReadAllText(dir));
            }
            foreach (var file in files)
            {
                Theme theme;
                try
                {
                    theme = JsonConvert.DeserializeObject<Theme>(file);
                }
                catch (JsonReaderException)
                {
                    continue;
                }
                catch (JsonSerializationException)
                {
                    continue;
                }
                
                if(theme != null)   
                {
                    themes.TryAdd(theme.Name, theme);
                }
            }
        }
        private string LoadEmbeddedFile(string path)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            {
                using(var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
public class Theme
{
    public string Name { get; set; }
    public Color MainColor { get; set; }//Main background color
    public Color AltColor { get; set; }//text and foreground color
    public Color LoggerBackColor { get; set; }//Background of logger text areas
    public Color LoggerTextColor { get; set; }//Text of logger text areas
    public Color LogFatalColor { get; set; }//fatal logger errors color
    public Color LogInfoColor { get; set; }//Info logger text color
    public Color LogErrorColor { get; set; }//Error logger text color
    public Color LogWarningColor { get; set; }//warning logger text color
    public Color LogDefaultColor { get; set; }//defualt logger text color
    public Color HexByteBack1Color { get; set; } //every second byte in hex viewer
    public Color HexSidebarBackColor { get; set; }//side bar color in hex viewer
    public Color ControlColor { get; set; }
    public Color MenuBarBackColor { get; set; }//Back colour of top menu bar
    public Color UnselectedTabBackColor { get; set; }//Background of unselected tab
    public Color WindowBackColor { get; set; } //Back of the main window, NOT the main panel
    public Color ArchiveChangedColor { get; set; }//Archive viewer text color when a file is modified
    public Color ProgressColor { get; set; } //Colour of the moving bar in a progress bar
    public Color ProgressBorderColor { get; set; } //border color of progress bar
    public Color ProgressControlColor { get; set; }//Background color of the progress bar
    public Color ButtonBackColor { get; set; } //Background colour of a button
    public Color ButtonDisabledTextColor { get; set; } //Text colour of a greyedout/disabledbutton
    public Color GridViewHeaderGradientColor { get; set; } //Graident END color of gridview header
    public Color GridViewHeaderBorderColor { get; set; } //Border of grid view header
    public Color ImageViewBackColor { get; set; } //Background of image viewer
    public Color InactiveTreeGridSelectionColor { get; set; } //Background of image viewer


}