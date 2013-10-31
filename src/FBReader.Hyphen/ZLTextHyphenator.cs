/*
 * Copyright (C) 2007-2013 Geometer Plus <contact@geometerplus.com>
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

namespace FBReader.Hyphen
{
    public abstract class ZLTextHyphenator
    {
        private static readonly Lazy<ZLTextHyphenator> Instance =
            new Lazy<ZLTextHyphenator>(() => new ZLTextTeXHyphenator());

        public static ZLTextHyphenator Default
        {
            get
            {
                return Instance.Value;
            }
        }

        public abstract Dictionary<ZLTextTeXHyphenationPattern, ZLTextTeXHyphenationPattern> Patterns { get; }

        public ZLTextHyphenationInfo GetInfo(string word, int offset = 0)
        {
            int len = word.Length;
            var isLetter = new bool[len];
            var pattern = new char[len + 2];
            char[] data = word.ToCharArray();
            pattern[0] = ' ';
            for (int i = 0, j = offset; i < len; ++i, ++j)
            {
                char character = data[j];
                if (character == '\'' || character == '^' || Char.IsLetter(character))
                {
                    isLetter[i] = true;
                    pattern[i + 1] = char.ToLower(character);
                }
                else
                {
                    pattern[i + 1] = ' ';
                }
            }
            pattern[len + 1] = ' ';

            var info = new ZLTextHyphenationInfo(len + 2);
            bool[] mask = info.Mask;
            Hyphenate(pattern, mask, len + 2);
            for (int i = 0, j = offset - 1; i <= len; ++i, ++j)
            {
                if ((i < 2) || (i > len - 2))
                {
                    mask[i] = false;
                }
                else
                {
                    switch (data[j])
                    {
                        case (char) 0xAD: // soft hyphen
                            mask[i] = true;
                            break;
                        case '-':
                            mask[i] = (i >= 3)
                                      && isLetter[i - 3]
                                      && isLetter[i - 2]
                                      && isLetter[i]
                                      && isLetter[i + 1];
                            break;
                        default:
                            mask[i] = mask[i]
                                      && isLetter[i - 2]
                                      && isLetter[i - 1]
                                      && isLetter[i]
                                      && isLetter[i + 1];
                            break;
                    }
                }
            }

            return info;
        }

        protected abstract void Hyphenate(char[] stringToHyphenate, bool[] mask, int length);
    }
}