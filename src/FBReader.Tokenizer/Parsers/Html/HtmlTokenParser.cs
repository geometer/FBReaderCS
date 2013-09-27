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
using FBReader.Tokenizer.Styling;
using FBReader.Tokenizer.Tokens;
using HtmlAgilityPack;

namespace FBReader.Tokenizer.Parsers.Html
{
    public class HtmlTokenParser : TokenParserBase
    {
        private readonly TextReader _reader;
        private CSS _css;
        private readonly Dictionary<string, int> _anchors;

        public HtmlTokenParser(TextReader reader, Dictionary<string, int> anchors)
        {
            _reader = reader;
            _anchors = anchors;
        }

        public override IEnumerable<TokenBase> GetTokens()
        {
            _css = new CSS();

            var html = new HtmlDocument
                            {
                                OptionOutputAsXml = true,
                                OptionReadEncoding = false
                            };
            html.Load(_reader);
            var root  = html.DocumentNode.SelectSingleNode("//body");
            
            var cssNode = html.DocumentNode.SelectSingleNode("//style");
            if (cssNode != null)
            {
                _css.Analyze(cssNode.InnerText);
            }

            var stack = new Stack<TextVisualProperties>();
            var item = new TextVisualProperties();
            stack.Push(item);

            return ParseTokens(root, stack, new TokenIndex());
        }

        private IEnumerable<TokenBase> ParseTokens(HtmlNode container, Stack<TextVisualProperties> propertiesStack, TokenIndex top, int parentID = -1)
        {
            foreach (HtmlNode child in container.ChildNodes)
            {
                var node = child as HtmlTextNode;
                if (node != null)
                {
                    if (!string.IsNullOrEmpty(node.Text))
                    {
                        foreach (TokenBase token in ParseText(node.Text, top))
                        {
                            yield return token;
                        }
                    }
                }
                else
                {
                    TextVisualProperties properties = propertiesStack.Peek().Clone().Update(child, _css);
                    properties.LinkID = string.Empty;

                    if (child.Name == "a" || child.Name == "span")
                    {
                        ParseAnchors(top, child);
                    }

                    if (child.Name == "a")
                    {
                        string attributeValue = child.GetAttributeValue("href", string.Empty);
                        if (!string.IsNullOrEmpty(attributeValue))
                        {
                            properties.LinkID = attributeValue;
                        }
                    }
                    //TODO: add images support
                    //if (string.Equals(child.Name, imageParser.ImageTag))
                    //{
                    //    int oldTopIndex;
                    //    HtmlAttributeCollection attributes = child.Attributes;
                    //    string imagePath = attributes.Contains("src") ? attributes["src"].Value : string.Empty;
                    //    top.Index = (oldTopIndex = top.Index) + 1;
                    //    var pictureToken = new PictureToken(oldTopIndex)
                    //    {
                    //        ImageID = imagePath
                    //    };
                    //    yield return pictureToken;
                    //}
                    //else
                    {
                        if (child is HtmlCommentNode)
                            continue;

                        var tagOpen = new TagOpenToken(top.Index++, child, properties, parentID);
                        yield return tagOpen;
                        propertiesStack.Push(properties);
                        foreach (TokenBase token in ParseTokens(child, propertiesStack, top, tagOpen.ID))
                        {
                            yield return token;
                        }
                        propertiesStack.Pop();
                        yield return new TagCloseToken(top.Index++, parentID);
                    }
                }
            }
        }


        private void ParseAnchors(TokenIndex top, HtmlNode node)
        {
            foreach (string str in new[] { "id", "name" })
            {
                string attributeValue = node.GetAttributeValue(str, string.Empty);
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    AddAnchor(top, attributeValue);
                }
            }
        }

        private void AddAnchor(TokenIndex top, string path)
        {
            _anchors[path] = top.Index;
        }
    }
}
