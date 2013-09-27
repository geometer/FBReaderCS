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
using System.IO;
using System.Xml.Linq;
using FBReader.Tokenizer.Styling;
using FBReader.Tokenizer.Tokens;

namespace FBReader.Tokenizer.Parsers.Txt
{
    public class TxtTokenParser : TokenParserBase
    {
        private readonly TextReader _reader;

        public TxtTokenParser(TextReader reader)
        {
            _reader = reader;
        }

        public override IEnumerable<TokenBase> GetTokens()
        {
            var top = new TokenIndex();
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                if(string.IsNullOrEmpty(line))
                    continue;
                
                var p = new XElement("p");
                var property = new TextVisualProperties
                                {
                                    TextIndent = 32.0,
                                    Inline = false
                                };

                yield return new TagOpenToken(top.Index++, p, property, -1);
                
                foreach (TokenBase baseToken in ParseText(line, top))
                    yield return baseToken;
                
                yield return new TagCloseToken(top.Index++, -1);
            }
        }
    }
}