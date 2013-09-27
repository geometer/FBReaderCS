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
using System.Data;
using System.Linq;
using System.Xml.Linq;
using FBReader.IO;
using FBReader.Tokenizer.Styling;
using FBReader.Tokenizer.Tokens;
using HtmlAgilityPack;

namespace FBReader.Tokenizer.Parsers.Epub
{
    public class EpubTokenParser : TokenParserBase
    {
        private readonly Dictionary<string, int> _anchors;
        private readonly XDocument _opf;
        private readonly EpubPath _opfPath;
        private readonly XNamespace _opfns;
        private readonly CSS _css;
        private readonly ZipContainer _zip;
        private readonly XElement _opfRoot;

        public EpubTokenParser(XDocument opf, EpubPath opfPath, ZipContainer zip, CSS css, Dictionary<string, int> anchors)
        {
            _opf = opf;
            _opfPath = opfPath;
            _zip = zip;
            _css = css;
            _anchors = anchors;
            _opfns = XNamespace.Get("http://www.idpf.org/2007/opf");

            _opfRoot = _opf.Root;
            if (_opfRoot == null)
                throw new DataException("Invalid epub meta info.");
        }

        #region ITokenParser Members

        public override IEnumerable<TokenBase> GetTokens()
        {
            var propertiesStack = new Stack<TextVisualProperties>();
            var item = new TextVisualProperties();
            propertiesStack.Push(item);
            var top = new TokenIndex();
            foreach (EpubSpineItem spineItem in GetSpineItems())
            {
                yield return new NewPageToken(top.Index++);

                AddAnchor(top, (_opfPath) + spineItem.Path);
                foreach (TokenBase token in ParseSpineItem(spineItem, propertiesStack, top))
                {
                    yield return token;
                }
            }
        }

        #endregion

        private void AddAnchor(TokenIndex top, string path)
        {
            _anchors[path] = top.Index;
        }

        private IEnumerable<string> GetItemIDs()
        {
            IEnumerable<XElement> refs = _opfRoot.Elements((_opfns + "spine")).Elements((_opfns + "itemref"));
            return from reference in refs 
                   let linear = reference.Attribute("linear") 
                   where (linear == null) || (linear.Value != "no") 
                   select reference.Attribute("idref") into idref 
                   where idref != null 
                   select idref.Value;
        }

        private static HtmlNode GetPageBody(EpubSpineItem item)
        {
            try
            {
                var document = new HtmlDocument
                               {
                                   OptionOutputAsXml = true
                               };
                document.Load(item.Stream);
                return document.DocumentNode.SelectSingleNode("//body");
            }
            catch (Exception exception)
            {
                throw new Exception("XHTML parsing error.", exception);
            }
        }

        private string GetPath(string itemID, XElement manifest)
        {
            return (from i in manifest.Elements((_opfns + "item"))
                let id = i.Attribute("id")
                let href = i.Attribute("href")
                where ((id != null) && (id.Value == itemID)) && (href != null)
                select href.Value).FirstOrDefault<string>();
        }

        private IEnumerable<EpubSpineItem> GetSpineItems()
        {
            XElement manifest = _opfRoot.Element((_opfns + "manifest"));
            return from itemId in GetItemIDs() 
                   select GetPath(itemId, manifest) into path 
                   where !string.IsNullOrEmpty(path) 
                   let spineStream = _zip.GetFileStream(_opfPath + path, false, true) 
                   where spineStream != null 
                   select new EpubSpineItem
                                {
                                    Stream = spineStream,
                                    Path = path
                                };
        }

        private void ParseAnchors(TokenIndex top, EpubPath path, HtmlNode node)
        {
            foreach (string str in new[] {"id", "name"})
            {
                string attributeValue = node.GetAttributeValue(str, string.Empty);
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    AddAnchor(top, path.CurrentFilePath + "#" + attributeValue);
                }
            }
        }

        private IEnumerable<TokenBase> ParseNodes(HtmlNode container, Stack<TextVisualProperties> propertiesStack, TokenIndex top, EpubPath path, int parentID = -1)
        {
            foreach (HtmlNode child in container.ChildNodes)
            {
                var asText = child as HtmlTextNode;
                if (asText != null && !string.IsNullOrEmpty(asText.Text))
                {
                    foreach (TokenBase text in ParseText(asText.Text, top))
                    {
                        yield return text;
                    }
                }
                else
                {
                    TextVisualProperties properties = propertiesStack.Peek().Clone().Update(child, _css);
                    properties.LinkID = string.Empty;

                    if (child.Name == "a" || child.Name == "span")
                    {
                        ParseAnchors(top, path, child);
                    }

                    if (child.Name == "a")
                    {
                        string href = child.GetAttributeValue("href", string.Empty);
                        if (!string.IsNullOrEmpty(href))
                        {
                            if (href.StartsWith("#"))
                            {
                                properties.LinkID = path.CurrentFilePath + href;
                            }
                            else
                            {
                                properties.LinkID = path + href;
                            }
                        }
                    }
                    if (string.Equals(child.Name, "img"))
                    {
                        HtmlAttributeCollection attributes = child.Attributes;
                        string src = attributes.Contains("src") ? attributes["src"].Value : string.Empty;
                        var pictureToken = new PictureToken(top.Index++, (path) + src);
                        yield return pictureToken;
                    }
                    else
                    {
                        if(child is HtmlCommentNode)
                            continue;

                        var tagOpenToken = new TagOpenToken(top.Index++, child, properties, parentID);
                        yield return tagOpenToken;
                        propertiesStack.Push(properties);
                        foreach (TokenBase token in ParseNodes(child, propertiesStack, top, path, tagOpenToken.ID))
                        {
                            yield return token;
                        }
                        propertiesStack.Pop();
                        yield return new TagCloseToken(top.Index++, parentID);
                    }
                }
            }
        }

        private IEnumerable<TokenBase> ParseSpineItem(EpubSpineItem item, Stack<TextVisualProperties> propertiesStack, TokenIndex top)
        {
            HtmlNode pageBody = GetPageBody(item);
            return ParseNodes(pageBody, propertiesStack, top, _opfPath + item.Path);
        }
    }
}