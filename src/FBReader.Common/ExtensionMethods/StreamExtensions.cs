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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FBReader.Common.ExtensionMethods
{
    public static class StreamExtentions
    {
        public static string ToBase64String(this Stream stream)
        {
            var numArray = new byte[stream.Length];
            stream.Position = 0L;
            stream.Read(numArray, 0, numArray.Length);
            return Convert.ToBase64String(numArray);
        }

        public static bool CheckUTF8(this Stream stream)
        {
            Func<bool> func = () =>
                              {
                                  int b = stream.ReadByte();
                                  if (b < 0)
                                      return false;
                                  return (b & 192) == 128;
                              };
            long position = stream.Position;
            try
            {
                int num;
                while ((num = stream.ReadByte()) >= 0)
                {
                    if ((num & 128) != 0 && ((num & 224) == 192 && !func() || (num & 240) == 224 && (!func() || !func()) || (num & 248) == 240 && (!func() || !func() || !func())))
                        return false;
                }
                return true;
            }
            finally
            {
                stream.Position = position;
            }
        }

        public static Stream Win1251ToUTF8(this Stream source)
        {
            var list = new List<char>
                       {
                           char.MinValue,
                           '\x0001',
                           '\x0002',
                           '\x0003',
                           '\x0004',
                           '\x0005',
                           '\x0006',
                           '\a',
                           '\b',
                           '\t',
                           '\n',
                           '\v',
                           '\f',
                           '\r',
                           '\x000E',
                           '\x000F',
                           '\x0010',
                           '\x0011',
                           '\x0012',
                           '\x0013',
                           '\x0014',
                           '\x0015',
                           '\x0016',
                           '\x0017',
                           '\x0018',
                           '\x0019',
                           '\x001A',
                           '\x001B',
                           '\x001C',
                           '\x001D',
                           '\x001E',
                           '\x001F',
                           ' ',
                           '!',
                           '"',
                           '#',
                           '$',
                           '%',
                           '&',
                           '\'',
                           '(',
                           ')',
                           '*',
                           '+',
                           ',',
                           '-',
                           '.',
                           '/',
                           '0',
                           '1',
                           '2',
                           '3',
                           '4',
                           '5',
                           '6',
                           '7',
                           '8',
                           '9',
                           ':',
                           ';',
                           '<',
                           '=',
                           '>',
                           '?',
                           '@',
                           'A',
                           'B',
                           'C',
                           'D',
                           'E',
                           'F',
                           'G',
                           'H',
                           'I',
                           'J',
                           'K',
                           'L',
                           'M',
                           'N',
                           'O',
                           'P',
                           'Q',
                           'R',
                           'S',
                           'T',
                           'U',
                           'V',
                           'W',
                           'X',
                           'Y',
                           'Z',
                           '[',
                           '\\',
                           ']',
                           '^',
                           '_',
                           '`',
                           'a',
                           'b',
                           'c',
                           'd',
                           'e',
                           'f',
                           'g',
                           'h',
                           'i',
                           'j',
                           'k',
                           'l',
                           'm',
                           'n',
                           'o',
                           'p',
                           'q',
                           'r',
                           's',
                           't',
                           'u',
                           'v',
                           'w',
                           'x',
                           'y',
                           'z',
                           '{',
                           '|',
                           '}',
                           '~',
                           '\x007F',
                           'Ђ',
                           'Ѓ',
                           '‚',
                           'ѓ',
                           '„',
                           '…',
                           '†',
                           '‡',
                           '€',
                           '‰',
                           'Љ',
                           '‹',
                           'Њ',
                           'Ќ',
                           'Ћ',
                           'Џ',
                           'ђ',
                           '‘',
                           '’',
                           '“',
                           '”',
                           '•',
                           '–',
                           '—',
                           '\x0098',
                           '™',
                           'љ',
                           '›',
                           'њ',
                           'ќ',
                           'ћ',
                           'џ',
                           ' ',
                           'Ў',
                           'ў',
                           'Ј',
                           '¤',
                           'Ґ',
                           '¦',
                           '§',
                           'Ё',
                           '©',
                           'Є',
                           '«',
                           '¬',
                           '\x00AD',
                           '®',
                           'Ї',
                           '°',
                           '±',
                           'І',
                           'і',
                           'ґ',
                           'µ',
                           '¶',
                           '·',
                           'ё',
                           '№',
                           'є',
                           '»',
                           'ј',
                           'Ѕ',
                           'ѕ',
                           'ї',
                           'А',
                           'Б',
                           'В',
                           'Г',
                           'Д',
                           'Е',
                           'Ж',
                           'З',
                           'И',
                           'Й',
                           'К',
                           'Л',
                           'М',
                           'Н',
                           'О',
                           'П',
                           'Р',
                           'С',
                           'Т',
                           'У',
                           'Ф',
                           'Х',
                           'Ц',
                           'Ч',
                           'Ш',
                           'Щ',
                           'Ъ',
                           'Ы',
                           'Ь',
                           'Э',
                           'Ю',
                           'Я',
                           'а',
                           'б',
                           'в',
                           'г',
                           'д',
                           'е',
                           'ж',
                           'з',
                           'и',
                           'й',
                           'к',
                           'л',
                           'м',
                           'н',
                           'о',
                           'п',
                           'р',
                           'с',
                           'т',
                           'у',
                           'ф',
                           'х',
                           'ц',
                           'ч',
                           'ш',
                           'щ',
                           'ъ',
                           'ы',
                           'ь',
                           'э',
                           'ю',
                           'я'
                       };
            var memoryStream = new MemoryStream();
            var buffer = new byte[4096];
            var streamWriter = new StreamWriter(memoryStream);
            int num1;
            while ((num1 = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int index = 0; index < num1; ++index)
                {
                    byte num2 = buffer[index];
                    streamWriter.Write(list[num2]);
                }
            }
            streamWriter.Flush();
            memoryStream.Position = 0L;
            return memoryStream;
        }

        public static XDocument GetXmlDocument(this Stream source, bool removeDocType = false)
        {
            long position = source.Position;
            string input = new StreamReader(source).ReadLine();
            source.Position = position;
            Encoding encoding = Encoding.UTF8;
            if (!string.IsNullOrEmpty(input))
            {
                Match match = new Regex("<\\?xml[^>]*encoding=\"(?<encoding>[^\"]+)\"[^>]*\\?>").Match(input);
                if (match.Success)
                {
                    string name = match.Groups["encoding"].Value.ToLower();
                    if (name == "windows-1251" || name == "cp-1251" || name == "win-1251")
                        source = source.Win1251ToUTF8();
                    else
                        encoding = Encoding.GetEncoding(name);
                }
            }
            string str = new StreamReader(source, encoding).ReadToEnd();
            if (removeDocType)
                str = Regex.Replace(str, "<!DOCTYPE[^>]*>", string.Empty);
            try
            {
                return XDocument.Parse(str.Trim());
            }
            catch (Exception ex)
            {
                throw new Exception("Can't parse XML", ex);
            }
        }
    }
}