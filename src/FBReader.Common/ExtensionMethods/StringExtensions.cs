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
using System.Net;
using System.Text;

namespace FBReader.Common.ExtensionMethods
{
    public static class StringExtentions
    {
        private const char NBSP = ' ';

        public static IEnumerable<string> BreakToWords(this string text)
        {
            var stringBuilder = new StringBuilder();
            bool flag = true;
            foreach (char ch in text)
            {
                switch (ch)
                {
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                    case NBSP:
                        if (flag)
                        {
                            if (stringBuilder.Length > 0)
                            {
                                yield return stringBuilder.ToString();
                                stringBuilder.Remove(0, stringBuilder.Length);
                            }
                            yield return string.Empty;
                            flag = false;
                        }
                        break;
                    default:
                        flag = true;
                        stringBuilder.Append(ch);
                        break;
                }
            }
            if (stringBuilder.Length > 0)
            {
                yield return stringBuilder.ToString();
            }
        }

        public static string SafeSubstring(this string @this, int length)
        {
            if (@this.Length > length)
            {
                return @this.Substring(0, length);
            }
            return @this;
        }

        public static string HtmlDecode(this string text)
        {
            return HttpUtility.HtmlDecode(text);
        }
    }
}