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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using FBReader.PhoneServices;

namespace FBReader.Settings
{
    public class FontSettings
    {
        private const decimal DEFAULT_FONT_SIZE = 24;
        private const decimal DEFAULT_FONT_INTERVAL = 1.0M;
        private const string DEFAULT_FONT_FAMILY = "Segoe WP";

        private readonly SettingsStorage _settingsStorage;

        private readonly List<FontFamily> _fonts = new List<string>
                                                     {
                                                        "Arial Black", "Arial Bold", "Arial Italic", "Arial", "Calibri Bold",
                                                        "Calibri Italic", "Calibri", "Comic Sans MS Bold", "Comic Sans MS",
                                                        "Courier New Bold", "Courier New Italic", "Courier New", "Georgia Bold",
                                                        "Georgia Italic", "Georgia", "Lucida Sans Unicode", "Malgun Gothic",
                                                        "Meiryo UI", "Microsoft YaHei", "Segoe UI Bold", "Segoe UI", "Segoe WP Black",
                                                        "Segoe WP Bold", "Segoe WP Light", "Segoe WP Semibold", "Segoe WP SemiLight",
                                                        "Segoe WP", "Tahoma Bold", "Tahoma", "Times New Roman Bold", "Times New Roman Italic",
                                                        "Times New Roman", "Trebuchet MS Bold", "Trebuchet MS Italic", "Trebuchet MS",
                                                        "Verdana Bold", "Verdana Italic", "Verdana",
                                                     }.Select(f => new FontFamily(f)).ToList();

        private readonly List<decimal> _sizes = new List<decimal> { 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 32, 36, 40, 42 };

        private readonly List<decimal> _intervals = Enumerable.Range(8, 13).Select(n => (decimal)n / 10).ToList();

        public FontSettings(SettingsStorage settingsStorage)
        {
            _settingsStorage = settingsStorage;
        }

        public List<FontFamily> Fonts
        {
            get { return _fonts; }
        }

        public List<decimal> Sizes
        {
            get { return _sizes; }
        }

        public List<decimal> Intervals
        {
            get { return _intervals; }
        }

        public decimal FontSize
        {
            get
            {
                return _settingsStorage.GetValueWithDefault("FontSize", DEFAULT_FONT_SIZE);
            }
            set
            {
                _settingsStorage.SetValue("FontSize", value);
            }
        }

        public decimal FontInterval
        {
            get
            {
                return _settingsStorage.GetValueWithDefault("FontInterval", DEFAULT_FONT_INTERVAL);
            }
            set
            {
                _settingsStorage.SetValue("FontInterval", value);
            }
        }

        public FontFamily FontFamily
        {
            get
            {
                return new FontFamily(_settingsStorage.GetValueWithDefault("FontFamily", DEFAULT_FONT_FAMILY));
            }
            set
            {
                _settingsStorage.SetValue("FontFamily", value.Source);
            }
        }
    }
}
