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
using System.Linq;

namespace FBReader.Tokenizer.Parsers.Epub
{
    public class EpubPath
    {
        private readonly string[] _directories;

        public EpubPath(string currentFilePath)
        {
            CurrentFilePath = currentFilePath;
            List<string> list = currentFilePath.Split(new[] {'/'}).ToList();
            list.RemoveAt(list.Count - 1);
            _directories = list.ToArray();
        }

        public string CurrentFilePath { get; private set; }

        private string Combine(string link)
        {
            string[] strArray = link.Replace('\\', '/').Split(new[] {'/'});
            List<string> values = _directories.ToList();
            foreach (string str in strArray)
            {
                if (str == "..")
                {
                    values.RemoveAt(values.Count - 1);
                }
                else
                {
                    values.Add(str);
                }
            }
            return string.Join("/", values);
        }

        public static string operator +(EpubPath path, string link)
        {
            return path.Combine(link);
        }

        public static implicit operator EpubPath(string path)
        {
            return new EpubPath(path);
        }

        public override string ToString()
        {
            return CurrentFilePath;
        }
    }
}