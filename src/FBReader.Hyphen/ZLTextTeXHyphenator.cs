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
using System.IO;
using System.Linq;
using System.Reflection;

namespace FBReader.Hyphen
{
    public sealed class ZLTextTeXHyphenator : ZLTextHyphenator
    {
        private readonly Lazy<Dictionary<ZLTextTeXHyphenationPattern, ZLTextTeXHyphenationPattern>> _lazyLoadPatterns = 
            new Lazy<Dictionary<ZLTextTeXHyphenationPattern, ZLTextTeXHyphenationPattern>>(GetPatterns); 

        public override Dictionary<ZLTextTeXHyphenationPattern, ZLTextTeXHyphenationPattern> Patterns
        {
            get
            {
                return _lazyLoadPatterns.Value;
            }
        }

        public static Dictionary<ZLTextTeXHyphenationPattern, ZLTextTeXHyphenationPattern> GetPatterns()
        {
            var patterns = new List<string>();
            IEnumerable<string> ru = LoadPatterns(".Resources.ru.pattern");
            IEnumerable<string> en = LoadPatterns(".Resources.en.pattern");

            patterns.AddRange(ru);
            patterns.AddRange(en);

            return patterns
                .Select(pattern => new ZLTextTeXHyphenationPattern(pattern.ToCharArray(), 0, pattern.Length, true))
                .ToDictionary(p => p);
        }

        private static IEnumerable<string> LoadPatterns(string resource)
        {
            Assembly assembly = typeof(ZLTextTeXHyphenator).Assembly;
            string name = assembly.GetManifestResourceNames().FirstOrDefault(t => t.EndsWith(resource));
            if (!string.IsNullOrEmpty(name))
            {
                using (Stream manifestResourceStream = assembly.GetManifestResourceStream(name))
                {
                    if (manifestResourceStream != null)
                        return new StreamReader(manifestResourceStream)
                                .ReadToEnd()
                                .Split(new[] {'\n', '\r'},StringSplitOptions.RemoveEmptyEntries)
                                .ToArray();
                }
            }
            return new string[0];
        }

        protected override void Hyphenate(char[] stringToHyphenate, bool[] mask, int length)
        {
            if (!Patterns.Any())
            {
                for (int i = 0; i < length - 1; i++)
                {
                    mask[i] = false;
                }
                return;
            }

            var values = new byte[length + 1];

            var pattern = new ZLTextTeXHyphenationPattern(stringToHyphenate, 0, length, false);
            for (int offset = 0; offset < length - 1; offset++)
            {
                int len = length - offset + 1;
                pattern.Update(stringToHyphenate, offset, len - 1);
                while (--len > 0)
                {
                    pattern.Length = len;
                    ZLTextTeXHyphenationPattern toApply;
                    Patterns.TryGetValue(pattern, out toApply);
                    if (toApply != null)
                    {
                        toApply.Apply(values, offset);
                    }
                }
            }

            for (int i = 0; i < length - 1; i++)
            {
                mask[i] = (values[i + 1] % 2) == 1;
            }
        }
    }
}