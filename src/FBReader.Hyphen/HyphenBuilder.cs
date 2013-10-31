/*
 * Author: Vitaly Leschenko, CactusSoft (http://cactussoft.biz/), 2013
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

namespace FBReader.Hyphen
{
    public class HyphenBuilder
    {
        public string[] GetPartsOfWord(string word, bool hyphenation = true)
        {
            if (!hyphenation)
            {
                return new[] { word };
            }

            if (word.Contains("-"))
            {
                string[] strArray = word.Split(new []{'-'});
                var result = new List<string>();
                for (int i = 0; i < strArray.Length; ++i)
                {
                    string[] partsOfWord = GetPartsOfWord(strArray[i]);
                    result.AddRange(partsOfWord);
                    if (i < strArray.Length - 1)
                    {
                        result[result.Count - 1] = result[result.Count - 1] + "-";
                    }
                }
                return result.ToArray();
            }
            
            var hyphenInfo = ZLTextHyphenator.Default.GetInfo(word);
            var list = new List<string>();
            int from = 0;
            for(int i = 0; i < word.Length; ++i)
            {
                if(hyphenInfo.Mask[i])
                {
                    list.Add(word.Substring(from, i - from));
                    from = i;
                }
            }
            list.Add(word.Substring(from));

            return list.ToArray();
        }
    }
}
