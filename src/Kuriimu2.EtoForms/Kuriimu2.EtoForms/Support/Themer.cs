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
        private const string baseThemeKey_ = "Kuriimu2.EtoForms.Resources.Themes.Light.json";

        private const string detailsKey_ = "Details";
        private const string mainColorKey_ = "MainColor";
        private const string altColorKey_ = "AltColor";

        #endregion

        public readonly Dictionary<string, Dictionary<string, Dictionary<string, Color>>> themes = new Dictionary<string, Dictionary<string, Dictionary<string, Color>>>();

        private string _currentThemeKey;

        public void LoadThemes()
        {
            if (themes.Count == 0)
            {
                _currentThemeKey = Settings.Default.Theme;

                LoadJson();

                if (!themes.ContainsKey(_currentThemeKey))
                    _currentThemeKey = baseThemeKey_;

            }
            #region Styling

            Eto.Style.Add<Label>(null, text =>
            {
                text.TextColor = GetTheme(detailsKey_,altColorKey_);
            });
            Eto.Style.Add<Dialog>(null, dialog =>
            {
                dialog.BackgroundColor = GetTheme(detailsKey_, mainColorKey_);
            });
            Eto.Style.Add<CheckBox>(null, checkbox =>
            {
                checkbox.BackgroundColor = GetTheme(detailsKey_, mainColorKey_);
                checkbox.TextColor = GetTheme(detailsKey_, altColorKey_);
            });
            Eto.Style.Add<GroupBox>(null, groupBox =>
            {
                groupBox.BackgroundColor = GetTheme(detailsKey_, mainColorKey_);
                groupBox.TextColor = GetTheme(detailsKey_, altColorKey_);
            });

            #endregion
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

        public Color GetTheme(string sectionKey, string colorKey)
        {
            if (themes[_currentThemeKey].TryGetValue(sectionKey, out var section))
            {
                if (section.TryGetValue(colorKey, out var color))
                {
                    return color;
                }
                else
                {
                    return themes[baseThemeKey_][sectionKey][colorKey];
                }
            }
            else
            {
                return themes[baseThemeKey_][sectionKey][colorKey];
            }

        }
        private void LoadJson()
        {
            var themeDirs = Directory.GetFiles(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Themes");

            var lightTheme = CreateTheme(LoadEmbeddedFile(lightThemeLocation_));
            themes.Add(lightThemeLocation_, lightTheme);
            var darkTheme = CreateTheme(LoadEmbeddedFile(darkThemeLocation_));
            themes.Add(darkThemeLocation_, darkTheme);

            foreach (var dir in themeDirs)
            {
                var theme = CreateTheme(File.ReadAllText(dir));
                if (theme != null)
                {
                    themes.Add(dir, theme);
                }
            }
        }
        private string LoadEmbeddedFile(string path)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        private Dictionary<string, Dictionary<string, Color>> CreateTheme(string json)
        {
            Dictionary<string, Dictionary<string, Color>> theme;
            try
            {
                theme = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Color>>>(json);
            }
            catch (JsonReaderException)
            {
                return null;
            }
            catch (JsonSerializationException)
            {
                return null;
            }
            return theme;
        }
    }
}