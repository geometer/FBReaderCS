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

using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FBReader.Common.ExtensionMethods;

namespace FBReader.Tokenizer.Parsers.Txt
{
    public class TxtSummaryParser : BookSummaryParserBase
    {
        private readonly string _name;
        private readonly StreamReader _reader;

        public TxtSummaryParser(Stream stream, string name)
        {
            if (!stream.CheckUTF8())
                stream = stream.Win1251ToUTF8();
            _reader = new StreamReader(stream);
            _name = Regex.Replace(name, "(\\.fb2|\\.txt)(\\.zip)?$", string.Empty);
        }

        public override void SaveImages(Stream stream)
        {
            var xdocument = new XDocument();
            var xelement = new XElement("images");
            xdocument.Add(xelement);
            xdocument.Save(stream);
        }

        public override bool SaveCover(string bookID)
        {
            return true;
        }

        public override BookSummary GetBookPreview()
        {
            return new BookSummary
                   {
                       Title = _name
                   };
        }

        public override ITokenParser GetTokenParser()
        {
            return new TxtTokenParser(_reader);
        }
    }
}