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

using FBReader.Common.ExtensionMethods;
using FBReader.IO;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.Extensions;
using FBReader.Tokenizer.Styling;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Xml.Linq;

namespace FBReader.Tokenizer.Parsers.Fb2
{
    public class Fb2SummaryParser : BookSummaryParserBase
    {
        private readonly XNamespace _ns;
        private readonly XElement _root;

        public Fb2SummaryParser(Stream source)
        {
            try
            {
                XDocument xmlDocument = source.GetXmlDocument();
                _root = xmlDocument.Root;
            }
            catch
            {
                source.Position = 0;
                var zip = ZipContainer.Unzip(source);
                source = zip.Files.First().Stream;

                source.Position = 0;
                XDocument xmlDocument = source.GetXmlDocument();
                _root = xmlDocument.Root; 
            }
                
            if (_root == null)
            {
                throw new DataException("Can't load book.");
            }
            XAttribute attribute = _root.Attribute("xmlns");
            if (attribute == null)
            {
                throw new DataException("Can't load book.");
            }
            _ns = attribute.Value;
        }

        public override ITokenParser GetTokenParser()
        {
            Chapters.Clear();
            Anchors.Clear();
            return new Fb2TokenParser(_ns, _root, GetCss(), Chapters, Anchors);
        }

        public override BookSummary GetBookPreview()
        {
            XElement info = _root
                .Elements(_ns + "description")
                .Elements(_ns + "title-info")
                .FirstOrDefault();
            var preview = new BookSummary();
            if (info == null)
                return preview;
            
            preview.Title = PrepareBookTitle(info);
            preview.AuthorName = PrepareBookAuthor(info);
            preview.Description = PrepareAnnotation(info);
            preview.Language = PrepareLanguage(info);
            preview.UniqueID = PrepareUniqueID();
            return preview;
        }


        public override bool SaveCover(string bookID)
        {
            string cover = GetCoverImageID();
            if (string.IsNullOrEmpty(cover))
                return false;
            
            XElement xelement = _root.Elements(_ns + "binary")
                                     .Select(b => new { b = b, attr = b.Attribute("id") })
                                     .Where(b => b.attr != null && b.attr.Value == cover)
                                     .Select(t => t.b)
                                     .FirstOrDefault();

            if (xelement == null)
                return false;

            MemoryStream stream;
            try
            {
                stream = new MemoryStream(Convert.FromBase64String(
                    xelement.Value
                            .Replace(" ", string.Empty)
                            .Replace("\n", string.Empty)));
            }
            catch (Exception)
            {
                return false;
            }
            return SaveCoverImages(bookID, stream);
        }

        public override void SaveImages(Stream output)
        {
            var document = new XDocument();
            var images = new XElement("images");
            document.Add(images);
            List<BookImage> xmlImages = GetXmlImages().ToList();
            var @event = new AutoResetEvent(false);
            Execute.OnUIThread(() =>
            {
                foreach (BookImage bookImage in xmlImages)
                {
                    try
                    {
                        Stream streamSource = bookImage.CreateStream();
                        Size imageSize = streamSource.GetImageSize();
                        bookImage.Width = (int)imageSize.Width;
                        bookImage.Height = (int)imageSize.Height;
                        images.Add(bookImage.Save());
                    }
                    catch (Exception)
                    {
                    }
                }
                @event.Set();
            });
            @event.WaitOne();

            document.Save(output);
        }

        private string PrepareAnnotation(XContainer info)
        {
            XElement element = info.Element(_ns + "annotation");
            if (element == null)
                return string.Empty;
            
            var sb = new StringBuilder();
            var textWriter = new StringWriter(sb);
            element.Save(textWriter, SaveOptions.DisableFormatting);
            return Regex.Replace(Regex.Replace(sb.ToString().Replace("\n", string.Empty), "</p>", "\n"), "<[^>]*>", "");
            
        }

        private string PrepareBookAuthor(XContainer info)
        {
            XElement author = info.Elements(_ns + "author").FirstOrDefault();
            string fullName = string.Empty;
            if (author == null)
                return string.Empty;
            
            AddFullName(ref fullName, author, "first-name");
            AddFullName(ref fullName, author, "middle-name");
            AddFullName(ref fullName, author, "last-name");
            
            return fullName.Trim();
        }

        private string PrepareBookTitle(XContainer info)
        {
            XElement element = info.Elements(_ns + "book-title").FirstOrDefault();
            if (element == null)
                return string.Empty;
            
            return element.Value;
        }

        private string PrepareLanguage(XElement info)
        {
            XElement element = info.Elements(_ns + "lang").FirstOrDefault();
            if (element == null)
                return null;
            
            return element.Value;
        }


        private string PrepareUniqueID()
        {
            XElement isbn = _root
                .Elements(_ns + "description")
                .Elements(_ns + "publish-info")
                .Elements(_ns + "isbn")
                .FirstOrDefault();
            if (isbn == null)
                return string.Empty;

            string str = isbn.Value.SafeSubstring(1000);
            return ("isbn://" + str);
        }

        private void AddFullName(ref string fullName, XElement author, string name)
        {
            XElement element = author.Elements(_ns + name).FirstOrDefault();
            if (element != null)
            {
                fullName = fullName.Trim() + " " + element.Value.Trim();
            }
        }

        private string GetCoverImageID()
        {
            XAttribute attribute = _root
                .Elements(_ns + "description")
                .Descendants(_ns + "coverpage")
                .Elements(_ns + "image")
                .Attributes()
                .FirstOrDefault(t => t.Name.LocalName == "href");

            if (attribute == null)
                return null;
            
            return attribute.Value.TrimStart('#');
        }

        private IEnumerable<BookImage> GetXmlImages()
        {
            return (from binary in _root.Elements(_ns + "binary")
                let id = binary.Attribute("id")
                where id != null
                let data = binary.Value.Replace(" ", string.Empty).Replace("\n", string.Empty)
                select new BookImage {ID = id.Value, Data = data});
        }

        private CSS GetCss()
        {
            CSS sheet = new CSS();
            sheet.Analyze(DefaultFb2Css);
            return sheet;
        }

        private const string DefaultFb2Css =
@"sup{vertical-align:super;}
sub{vertical-align:sub;}
p{font-size:1em;text-indent:32px;margin:0px;}
image{margin:8px;}
sup,sub,a,b,i,u,strong,em,emphasis,style,strikethrough,span {display:inline;margin:0px;}
b,strong,epigraphAuthor {font-weight: bold;}
i,em,emphasis,cite,epigraph,epigraphAuthor,style {font-style: italic;}
v,cite {margin-left:25px;margin-bottom:0px;}
title,epigraph,stanza {margin-bottom:12px;}
epigraph,text-author {text-align:right;}
title {text-align:center;}";

    }
}
