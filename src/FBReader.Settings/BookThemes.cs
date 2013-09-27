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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;

namespace FBReader.Settings
{
    public class BookThemes : IEnumerable<Scheme>
    {
        private static readonly Lazy<BookThemes> LazyInstance = new Lazy<BookThemes>(() => new BookThemes());     

        private readonly Dictionary<ColorSchemes, Scheme> _schemes = new Dictionary<ColorSchemes, Scheme>(8);

        private BookThemes()
        {
            InitSchemes();
        }

        public static BookThemes Default
        {
            get
            {
                return LazyInstance.Value;
            }
        }

        public Scheme this[ColorSchemes scheme]
        {
            get
            {
                if (!_schemes.ContainsKey(scheme))
                    throw new NotSupportedException();

                return _schemes[scheme];
            }
        }

        public IEnumerator<Scheme> GetEnumerator()
        {
            return _schemes.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void InitSchemes()
        {
            _schemes.Add(
                ColorSchemes.Day,
                new Scheme
                (
                    colorScheme :                   ColorSchemes.Day,
                    backgroundBrush :               Colors.White,
                    titleForegroundBrush :          Color.FromArgb(0xFF, 0x7D, 0x7D, 0x7D),
                    textForegroundBrush :           Colors.Black,
                    linkForegroundBrush :           Color.FromArgb(0xFF, 0x2D, 0x8E, 0xCC),
                    selectionBrush :                Color.FromArgb(0x26, 0x00, 0x00, 0x00),
                    applicationBarBackgroundBrush : Color.FromArgb(0xF2, 0x34, 0x2E, 0x2B),
                    progressBarBrush :              Color.FromArgb(0xFF, 0xF0, 0x96, 0x09),
                    systemTrayForegroundColor :     Colors.Black
                ));

            _schemes.Add(
                ColorSchemes.Night,
                new Scheme
                (
                    colorScheme :                   ColorSchemes.Night,
                    backgroundBrush :               Colors.Black,
                    titleForegroundBrush :          Color.FromArgb(0xFF, 0xA8, 0xA8, 0xA8),
                    textForegroundBrush :           Colors.White,
                    linkForegroundBrush :           Color.FromArgb(0xFF, 0xF0, 0x96, 0x09),
                    selectionBrush :                Color.FromArgb(0x26, 0x00, 0x00, 0x00),
                    applicationBarBackgroundBrush : Color.FromArgb(0xF2, 0x2C, 0x2C, 0x2C),
                    progressBarBrush :              Color.FromArgb(0xFF, 0x71, 0x71, 0x71),
                    systemTrayForegroundColor :     Colors.Black
                ));

            _schemes.Add(
                ColorSchemes.GrayOne,
                new Scheme
                (
                    colorScheme :                   ColorSchemes.GrayOne,
                    backgroundBrush :               Color.FromArgb(0xFF, 0xCF, 0xCF, 0xCF),
                    titleForegroundBrush :          Color.FromArgb(0xFF, 0x60, 0x60, 0x60),
                    textForegroundBrush :           Colors.Black,
                    linkForegroundBrush :           Color.FromArgb(0xFF, 0x16, 0x78, 0xCA),
                    selectionBrush :                Color.FromArgb(0x26, 0x00, 0x00, 0x00),
                    applicationBarBackgroundBrush : Color.FromArgb(0xF2, 0x3E, 0x3E, 0x3E),
                    progressBarBrush :              Color.FromArgb(0xFF, 0x3C, 0x3C, 0x3C),
                    systemTrayForegroundColor :     Colors.Black
                ));

            _schemes.Add(
                ColorSchemes.GrayTwo,
                new Scheme
                (
                    colorScheme :                   ColorSchemes.GrayTwo,
                    backgroundBrush :               Color.FromArgb(0xFF, 0x32, 0x32, 0x32),
                    titleForegroundBrush :          Color.FromArgb(0xFF, 0xE3, 0xE3, 0xE3),
                    textForegroundBrush :           Color.FromArgb(0xFF, 0xB4, 0xB4, 0xB4),
                    linkForegroundBrush :           Color.FromArgb(0xFF, 0xF0, 0x96, 0x09),
                    selectionBrush :                Color.FromArgb(0x26, 0x00, 0x00, 0x00),
                    applicationBarBackgroundBrush : Color.FromArgb(0xF2, 0x18, 0x18, 0x18),
                    progressBarBrush :              Color.FromArgb(0xFF, 0x71, 0x71, 0x71),
                    systemTrayForegroundColor :     Color.FromArgb(0xFF, 0xB4, 0xB4, 0xB4)
                ));

            _schemes.Add(
                ColorSchemes.Sepia,
                new Scheme
                (
                    colorScheme :                   ColorSchemes.Sepia,
                    backgroundBrush :               Color.FromArgb(0xFF, 0xF3, 0xF1, 0xCF),
                    titleForegroundBrush :          Color.FromArgb(0xFF, 0x77, 0x70, 0x52),
                    textForegroundBrush :           Colors.Black,
                    linkForegroundBrush :           Color.FromArgb(0xFF, 0xD7, 0x83, 0x00),
                    selectionBrush :                Color.FromArgb(0x26, 0x00, 0x00, 0x00),
                    applicationBarBackgroundBrush : Color.FromArgb(0xF2, 0x34, 0x2E, 0x2B),
                    progressBarBrush :              Color.FromArgb(0xFF, 0xF0, 0x96, 0x09),
                    systemTrayForegroundColor :     Colors.Black
                ));

            _schemes.Add(
                ColorSchemes.Coffee,
                new Scheme
                (
                    colorScheme :                   ColorSchemes.Coffee,
                    backgroundBrush :               Color.FromArgb(0xFF, 0x36, 0x34, 0x2B),
                    titleForegroundBrush :          Color.FromArgb(0xFF, 0xF3, 0xD3, 0xA0),
                    textForegroundBrush :           Color.FromArgb(0xFF, 0xE6, 0xE4, 0xC8),
                    linkForegroundBrush :           Color.FromArgb(0xFF, 0xD7, 0x83, 0x00),
                    selectionBrush :                Color.FromArgb(0x26, 0x00, 0x00, 0x00),
                    applicationBarBackgroundBrush : Color.FromArgb(0xF2, 0x23, 0x21, 0x19),
                    progressBarBrush :              Color.FromArgb(0xFF, 0xF0, 0x96, 0x09),
                    systemTrayForegroundColor :     Color.FromArgb(0xFF, 0xE6, 0xE4, 0xC8)
                ));

            _schemes.Add(
                ColorSchemes.Sky,
                new Scheme
                (
                    colorScheme :                   ColorSchemes.Sky,
                    backgroundBrush :               Color.FromArgb(0xFF, 0xCF, 0xE2, 0xE6),
                    titleForegroundBrush :          Color.FromArgb(0xFF, 0x54, 0x6D, 0x81),
                    textForegroundBrush :           Color.FromArgb(0xFF, 0x28, 0x2D, 0x2E),
                    linkForegroundBrush :           Color.FromArgb(0xFF, 0x1C, 0x6D, 0xB9),
                    selectionBrush :                Color.FromArgb(0x26, 0x00, 0x00, 0x00),
                    applicationBarBackgroundBrush : Color.FromArgb(0xF2, 0x25, 0x34, 0x41),
                    progressBarBrush :              Color.FromArgb(0xFF, 0x90, 0xB0, 0xB7),
                    systemTrayForegroundColor :     Colors.Black
                ));

            _schemes.Add(
                ColorSchemes.Asphalt,
                new Scheme
                (
                    colorScheme :                   ColorSchemes.Asphalt,
                    backgroundBrush :               Color.FromArgb(0xFF, 0x6D, 0x75, 0x80),
                    titleForegroundBrush :          Color.FromArgb(0xFF, 0xD1, 0xED, 0xFF),
                    textForegroundBrush :           Color.FromArgb(0xFF, 0xE7, 0xE8, 0xE9),
                    linkForegroundBrush :           Color.FromArgb(0xFF, 0x75, 0xCD, 0xDD),
                    selectionBrush :                Color.FromArgb(0x26, 0x00, 0x00, 0x00),
                    applicationBarBackgroundBrush : Color.FromArgb(0xF2, 0x2F, 0x35, 0x3E),
                    progressBarBrush :              Color.FromArgb(0xFF, 0x69, 0x7B, 0x95),
                    systemTrayForegroundColor :     Colors.White
                ));

        }        
    }
}
