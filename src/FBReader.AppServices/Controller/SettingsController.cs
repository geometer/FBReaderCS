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
using FBReader.Localization;
using FBReader.Settings;
using Microsoft.Phone.Controls;

namespace FBReader.AppServices.Controller
{
    public class SettingsController
    {

        public string GetLockScreenValue()
        {
            return (AppSettings.Default.LockScreen ? UISettings.SettingsPage_On : UISettings.SettingsPage_Off).ToLowerInvariant();
        }

        public string GetOrientationValue()
        {
            switch (AppSettings.Default.Orientation)
            {
                case SupportedPageOrientation.PortraitOrLandscape:
                    return UISettings.SettingsPage_Orientation_LandscapeOrPortrait.ToLowerInvariant();
                case SupportedPageOrientation.Portrait:
                    return UISettings.SettingsPage_Orientation_Portrait.ToLowerInvariant();
                case SupportedPageOrientation.Landscape:
                    return UISettings.SettingsPage_Orientation_Landscape.ToLowerInvariant();
            }
            throw new NotSupportedException();
        }

        public string GetFontValue()
        {
            return string.Format(UISettings.SettingsPage_Formatting_Font_Format,
                                 AppSettings.Default.FontSettings.FontFamily, AppSettings.Default.FontSettings.FontSize,
                                 AppSettings.Default.FontSettings.FontInterval);
        }

        public string GetMenuValue()
        {
            return AppSettings.Default.HideMenu
                       ? UISettings.SettingsPage_Formatting_Hide
                       : UISettings.SettingsPage_Formatting_Show;
        }

        public string GetMarginValue()
        {
            switch(AppSettings.Default.MarginKey)
            {
                case SupportedMargins.None:
                    return UISettings.SettingsPage_Margin_None.ToLowerInvariant();
                case SupportedMargins.Small:
                    return UISettings.SettingsPage_Margin_Small.ToLowerInvariant();
                case SupportedMargins.Medium:
                    return UISettings.SettingsPage_Margin_Middle.ToLowerInvariant();
                case SupportedMargins.Big:
                    return UISettings.SettingsPage_Margin_Big.ToLowerInvariant();
            }
            throw new NotSupportedException();
        }

        public string GetColorSchemeValue()
        {
            return GetColorSchemeName(AppSettings.Default.ColorSchemeKey).ToLowerInvariant();
        }

        public string GetColorSchemeName(ColorSchemes colorScheme)
        {
            switch (colorScheme)
            {
                case ColorSchemes.Day:
                    return UISettings.SettingsPage_Themes_Day;
                case ColorSchemes.Night:
                    return UISettings.SettingsPage_Themes_Night;
                case ColorSchemes.GrayOne:
                    return UISettings.SettingsPage_Themes_GrayOne;
                case ColorSchemes.GrayTwo:
                    return UISettings.SettingsPage_Themes_GrayTwo;
                case ColorSchemes.Sepia:
                    return UISettings.SettingsPage_Themes_Sepia;
                case ColorSchemes.Coffee:
                    return UISettings.SettingsPage_Themes_Coffee;
                case ColorSchemes.Sky:
                    return UISettings.SettingsPage_Themes_Sky;
                case ColorSchemes.Asphalt:
                    return UISettings.SettingsPage_Themes_Asphalt;

            }
            throw new NotSupportedException();
        }

        public string GetUILanguageValue()
        {
            return AppSettings.Default.CurrentUILanguage.NativeName.ToLowerInvariant();
        }

        public string GetTranslateLanguageValue()
        {
            return AppSettings.Default.CurrentTranslateLanguage.NativeName.ToLowerInvariant();
        }

        public string GetCSSValue()
        {
            return AppSettings.Default.UseCSSFontSize
                       ? UISettings.SettingPage_Formatting_CSS_FontSize_On
                       : UISettings.SettingPage_Formatting_CSS_FontSize_Off;
        }

        public string GetHyphenationValue()
        {
            return AppSettings.Default.Hyphenation
                       ? UISettings.SettingPage_Formatting_Hyphenation_On
                       : UISettings.SettingPage_Formatting_Hyphenation_Off;
        }

        public string GetFlippingModeName(FlippingMode flippingMode)
        {
            switch (flippingMode)
            {
                case FlippingMode.TouchOrSlide:
                    return UISettings.SettingsPage_FlippingMode_TouchOrSlide;
                case FlippingMode.Touch:
                    return UISettings.SettingsPage_FlippingMode_Touch;
                case FlippingMode.Slide:
                    return UISettings.SettingsPage_FlippingMode_Slide;
            }
            throw new NotSupportedException();
        }

        public string GetFlippingStyleName(FlippingStyle flippingStyle)
        {
            switch (flippingStyle)
            {
                case FlippingStyle.None:
                    return UISettings.SettingsPage_FlippingStyle_None;
                case FlippingStyle.Shift:
                    return UISettings.SettingsPage_FlippingStyle_Shift;
                case FlippingStyle.Overlap:
                    return UISettings.SettingsPage_FlippingStyle_Overlap;
            }
            throw new NotSupportedException();
        }

        public string GetFlippingValue()
        {
            string flippingMode;
            switch (AppSettings.Default.FlippingMode)
            {
                case FlippingMode.TouchOrSlide:
                    flippingMode = UISettings.SettingsPage_FlippingMode_TouchOrSlide_Full;
                    break;
                case FlippingMode.Touch:
                    flippingMode = UISettings.SettingsPage_FlippingMode_Touch;
                    break;
                case FlippingMode.Slide:
                    flippingMode = UISettings.SettingsPage_FlippingMode_Slide;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return string.Format(UISettings.SettingsPage_Formatting_FlippingMode_Format, flippingMode.ToLowerInvariant());
        }
    }
}
