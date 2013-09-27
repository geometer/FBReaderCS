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
using System.Linq;
using System.Xml.Linq;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.Styling;
using FBReader.Tokenizer.Tokens;

namespace FBReader.Tokenizer.Parsers.Fb2
{
    public class Fb2TokenParser : TokenParserBase
    {
        // Fields
        private readonly Dictionary<string, int> _anchors;
        private readonly List<BookChapter> _chapters;
        private readonly XNamespace _ns;
        private readonly XElement _root;
        private readonly CSS _styleSheet;

        // Methods
        public Fb2TokenParser(XNamespace ns, XElement root, CSS styleSheet, List<BookChapter> chapters, Dictionary<string, int> anchors)
        {
            _ns = ns;
            _root = root;
            _styleSheet = styleSheet;
            _chapters = chapters;
            _anchors = anchors;
        }

        public override IEnumerable<TokenBase> GetTokens()
        {
            var propertiesStack = new Stack<TextVisualProperties>();
            var item = new TextVisualProperties();
            propertiesStack.Push(item);
            var top = new TokenIndex();
            return _root.Elements(_ns + "body").SelectMany(b => ParseNodes(b, propertiesStack, top, 0));
        }

        private string GetText(XElement xelement)
        {
            string str = string.Empty;
            foreach (XNode node in xelement.Nodes())
            {
                string text = string.Empty;
                if (node is XText)
                {
                    text = ((XText) node).Value;
                }
                else if (node is XElement)
                {
                    text = GetText((XElement) node);
                }
                if (!string.IsNullOrEmpty(text))
                {
                    if (!str.EndsWith(" "))
                    {
                        str = str + " ";
                    }
                    str = str + text;
                }
            }
            return str;
        }

        private IEnumerable<TokenBase> ParseNodes(XContainer container, Stack<TextVisualProperties> propertiesStack, TokenIndex top, int bookLevel, int parentID = -1)
        {
            foreach (XNode node in container.Nodes())
            {
                var text = node as XText;
                if ((text != null) && !string.IsNullOrEmpty(text.Value))
                {
                    foreach (TokenBase token in ParseText(text.Value, top))
                    {
                        yield return token;
                    }
                }
                var element = node as XElement;
                if(element == null)
                    continue;

                TextVisualProperties properties = propertiesStack.Peek().Clone().Update(element, _styleSheet);
                
                string localName = element.Name.LocalName;
                int level = bookLevel;

                if (localName == "a")
                {
                    ProcessLinks(properties, element);
                }
                ProcessAnchors(top, element);

                if (localName == "section")
                {
                    yield return new NewPageToken(top.Index++);
                    level++;
                }

                if (localName == "title")
                {
                    ProcessTitleData(top, element, level);
                }

                if (localName == "image")
                {
                    XAttribute hrefAttr = element.Attributes().FirstOrDefault(t => (t.Name.LocalName == "href"));
                    string href = ((hrefAttr != null) ? hrefAttr.Value : string.Empty).TrimStart('#');
                    var pictureToken = new PictureToken(top.Index++, href);
                    yield return pictureToken;
                }
                else
                {
                    var tagOpen = new TagOpenToken(top.Index++, element, properties, parentID);
                    yield return tagOpen;

                    propertiesStack.Push(properties);
                    foreach (TokenBase token in ParseNodes(element, propertiesStack, top, level, tagOpen.ID))
                    {
                        yield return token;
                    }
                    propertiesStack.Pop();

                    yield return new TagCloseToken(top.Index++, parentID);
                }
                
            }
        }

        private void ProcessAnchors(TokenIndex top, XElement xelement)
        {
            XAttribute attribute = xelement.Attributes().FirstOrDefault(t => (t.Name.LocalName == "id"));
            if (attribute != null)
            {
                _anchors[attribute.Value] = top.Index;
            }
        }

        private static void ProcessLinks(TextVisualProperties properties, XElement xelement)
        {
            properties.LinkID = string.Empty;
            XAttribute attribute = xelement.Attributes().FirstOrDefault(t => (t.Name.LocalName == "href"));
            string str = ((attribute != null) ? attribute.Value : string.Empty).TrimStart('#');
            if (!string.IsNullOrEmpty(str))
            {
                properties.LinkID = str;
            }
        }

        private void ProcessTitleData(TokenIndex top, XElement xelement, int bookLevel)
        {
            var item = new BookChapter
                           {
                               Level = bookLevel,
                               Title = GetText(xelement),
                               TokenID = top.Index
                           };
            _chapters.Add(item);
        }
    }
}