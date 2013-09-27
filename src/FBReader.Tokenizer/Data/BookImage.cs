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
using System.IO;
using System.Xml.Linq;
using FBReader.Common.ExtensionMethods;

namespace FBReader.Tokenizer.Data
{
    public class BookImage
    {
        public BookImage()
        {
        }

        public BookImage(XElement root)
        {
            ID = root.GetStringAttribute("id");
            Width = root.GetIntAttribute("width");
            Height = root.GetIntAttribute("height");
            Data = root.Value;
        }

        public string ID { get; set; }

        public string Data { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public Stream CreateStream()
        {
            return new MemoryStream(Convert.FromBase64String(Data));
        }

        public XElement Save()
        {
            return new XElement("image", new object[]
                                         {
                                             new XAttribute("id", ID),
                                             new XAttribute("width", Width),
                                             new XAttribute("height", Height),
                                             new XText(Data)
                                         });
        }
    }
}