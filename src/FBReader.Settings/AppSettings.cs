/*
 * Author: CactusSoft (http://cactussoft.biz/), 2013
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using FBReader.PhoneServices;
using Microsoft.Phone.Controls;

namespace FBReader.Settings
{
    public class AppSettings
    {
        private const bool DEFAULT_LOCK_SCREEN = false;
        private const SupportedPageOrientation DEFAULT_ORIENTATION = SupportedPageOrientation.PortraitOrLandscape;
        private const bool DEFAULT_HIDE_MENU = false;
        private const SupportedMargins DEFAULT_MARGIN = SupportedMargins.Medium;
        private const ColorSchemes DEFAULT_COLOR_SCHEME = ColorSchemes.Day;
        private const string DEFAULT_TRANSLATE_LANGUAGE = "en";
        private const bool DEFAULT_USE_CSS_FONTSIZE = false;
        private const bool DEFAULT_HYPHENATION = true;
        private const FlippingMode DEFAULT_FLIPPING_MODE = FlippingMode.TouchOrSlide;
        private const FlippingStyle DEFAULT_FLIPPING_STYLE = FlippingStyle.Overlap;
        private readonly string DEFAULT_LANGUAGE;

        private readonly SettingsStorage _settingsStorage = new SettingsStorage();
        private readonly FontSettings _fontSettings;

        private static readonly Dictionary<SupportedMargins, Thickness> Margins = new Dictionary<SupportedMargins, Thickness>
                                                                                       {
                                                                                           { SupportedMargins.None, new Thickness(2) },
                                                                                           { SupportedMargins.Small, new Thickness(8) },
                                                                                           { SupportedMargins.Medium, new Thickness(12) },
                                                                                           { SupportedMargins.Big, new Thickness(18) }
                                                                                       }; 
        private readonly List<CultureInfo> _uiLanguages = new List<string>{ "ru", "en" }
            .Select(l => new CultureInfo(l)).ToList();

        private readonly List<string> _defaultTranslateLanguages = new List<string> { "ru", "en" };

        private readonly static Lazy<AppSettings> LazyInstance = new Lazy<AppSettings>(() => new AppSettings());
        public static AppSettings Default { get { return LazyInstance.Value; } }

        private AppSettings()
        {
            _fontSettings = new FontSettings(_settingsStorage);

            var defaultLang =
                UILanguages.SingleOrDefault(
                    l => l.TwoLetterISOLanguageName == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

            DEFAULT_LANGUAGE = defaultLang == null ? "en" : defaultLang.TwoLetterISOLanguageName;

        }

        public bool LockScreen
        {
            get { return _settingsStorage.GetValueWithDefault("LockScreen", DEFAULT_LOCK_SCREEN); }
            set { _settingsStorage.SetValue("LockScreen", value); }
        }

        public SupportedPageOrientation Orientation
        {
            get { return _settingsStorage.GetValueWithDefault("Orientation", DEFAULT_ORIENTATION); }
            set { _settingsStorage.SetValue("Orientation", value); }
        }

        public FontSettings FontSettings
        {
            get { return _fontSettings; }
        }

        public bool HideMenu
        {
            get { return _settingsStorage.GetValueWithDefault("HideMenu", DEFAULT_HIDE_MENU); }
            set { _settingsStorage.SetValue("HideMenu", value); }
        }

        public SupportedMargins MarginKey
        {
            get { return _settingsStorage.GetValueWithDefault("MarginKey", DEFAULT_MARGIN); }
            set { _settingsStorage.SetValue("MarginKey", value); }
        }

        public Thickness Margin
        {
            get
            {
                return Margins[MarginKey];
            }
        }

        public ColorSchemes ColorSchemeKey
        {
            get { return _settingsStorage.GetValueWithDefault("ColorSchemeKey", DEFAULT_COLOR_SCHEME); }
            set { _settingsStorage.SetValue("ColorSchemeKey", value); }
        }

        public Scheme ColorScheme
        {
            get
            {
                return BookThemes.Default[ColorSchemeKey];
            }
        }

        public List<Scheme> Schemes
        {
            get
            {
                return BookThemes.Default.ToList();
            }
        }

        public List<CultureInfo> UILanguages
        {
            get
            {
                return _uiLanguages;
            }
        }

        public CultureInfo CurrentUILanguage
        {
            get { return new CultureInfo(_settingsStorage.GetValueWithDefault("CurrentUILanguage", DEFAULT_LANGUAGE)); }
            set { _settingsStorage.SetValue("CurrentUILanguage", value.TwoLetterISOLanguageName); }
        }

        public bool AreTranslateLanguagesSet
        {
            get
            {
                var langNamesList = _settingsStorage.GetValueWithDefault("TranslateLanguages", _defaultTranslateLanguages);

                return !langNamesList.SequenceEqual(_defaultTranslateLanguages);
            }
        }

        public List<CultureInfo> TranslateLanguages
        {
            get
            {
                var langNamesList = _settingsStorage.GetValueWithDefault("TranslateLanguages", _defaultTranslateLanguages);

                return langNamesList.Select(l => new CultureInfo(l)).ToList();
            }
            set
            {
                _settingsStorage.SetValue("TranslateLanguages", value.Select(ci => ci.TwoLetterISOLanguageName).ToList());
            }
        }

        public CultureInfo CurrentTranslateLanguage
        {
            get { return new CultureInfo(_settingsStorage.GetValueWithDefault("CurrentTranslateLanguage", DEFAULT_TRANSLATE_LANGUAGE)); }
            set { _settingsStorage.SetValue("CurrentTranslateLanguage", value.TwoLetterISOLanguageName); }
        }

        public bool UseCSSFontSize
        {
            get { return _settingsStorage.GetValueWithDefault("UseCSSFontSize", DEFAULT_USE_CSS_FONTSIZE); }
            set { _settingsStorage.SetValue("UseCSSFontSize", value); }
        }

        public bool Hyphenation
        {
            get { return _settingsStorage.GetValueWithDefault("Hyphenation", DEFAULT_HYPHENATION); }
            set { _settingsStorage.SetValue("Hyphenation", value); }
        } 

        public FlippingMode FlippingMode
        {
            get { return _settingsStorage.GetValueWithDefault("FlippingMode", DEFAULT_FLIPPING_MODE); }
            set { _settingsStorage.SetValue("FlippingMode", value); }
        }

        public FlippingStyle FlippingStyle
        {
            get { return _settingsStorage.GetValueWithDefault("FlippingStyle", DEFAULT_FLIPPING_STYLE); }
            set { _settingsStorage.SetValue("FlippingStyle", value); }
        }

        public bool IsFirstTimeRunning
        {
            get { return _settingsStorage.GetValueWithDefault("IsFirstTimeRunning", true); }
            set { _settingsStorage.SetValue("IsFirstTimeRunning", value); }
        }

        public bool DontShowUploadToSkyDriveMessage
        {
            get { return _settingsStorage.GetValueWithDefault("DontShowUploadToSkyDriveMessage", false); }
            set { _settingsStorage.SetValue("DontShowUploadToSkyDriveMessage", value); }
        }

        public int Brightness
        {
            get { return _settingsStorage.GetValueWithDefault("Brightness", 100); }
            set { _settingsStorage.SetValue("Brightness", value); }
        }
    }
}
