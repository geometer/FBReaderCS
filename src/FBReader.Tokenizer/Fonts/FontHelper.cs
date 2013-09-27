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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FBReader.Common.ExtensionMethods;

namespace FBReader.Tokenizer.Fonts
{
    public class FontHelper : IFontHelper
    {
        private readonly Dictionary<long, Size> _cache = new Dictionary<long, Size>();
        private readonly FontFamily _fontFamily;

        public FontHelper(string family)
        {
            _fontFamily = new FontFamily(family);
        }

        public Size GetSize(char c, double fontSize, bool bold = false, bool italic = false)
        {
            long hash = GetHash(c, fontSize, bold, italic);
            if (!_cache.ContainsKey(hash))
            {
                lock (this)
                {
                    if (!_cache.ContainsKey(hash))
                        _cache[hash] = InternalGetSize(c, fontSize, bold, italic);
                }
            }
            return _cache[hash];
        }

        private static long GetHash(char c, double fontSize, bool bold, bool italic)
        {
            return ((long)c << 16) + ((((long)(fontSize * 5) << 1) + (bold ? 1L : 0L) << 1) + (italic ? 1L : 0L));
        }

        private Size InternalGetSize(char c, double fontSize, bool bold, bool italic)
        {
            var @event = new AutoResetEvent(false);
            var size = new Size();
            ((Action)(() =>
                {
                    var textBlock = new TextBlock
                                    {
                                        Text = Convert.ToString(c),
                                        FontSize = fontSize,
                                        FontFamily = _fontFamily,
                                        FontStyle = italic ? FontStyles.Italic : FontStyles.Normal,
                                        FontWeight = bold ? FontWeights.Bold : FontWeights.Normal
                                    };
                    textBlock.Measure(new Size(1024.0, 1024.0));
                    size = new Size(textBlock.ActualWidth, textBlock.ActualHeight);
                    @event.Set();
                })).OnUIThread();

            @event.WaitOne();
            return size;
        }
    }
}