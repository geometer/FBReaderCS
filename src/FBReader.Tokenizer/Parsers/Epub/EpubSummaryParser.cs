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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using FBReader.Common.ExtensionMethods;
using FBReader.IO;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.Extensions;
using FBReader.Tokenizer.Styling;

namespace FBReader.Tokenizer.Parsers.Epub
{
    public class EpubSummaryParser : BookSummaryParserBase
    {
        private const string InvalidEpubMetaInfo = "Invalid epub meta info.";
        private const string ManifestDataNotFound = "Manifest data not found.";
        private readonly string _oebps;
        private readonly XDocument _opf;
        private readonly EpubPath _opfPath;
        private readonly XElement _opfRoot;
        private readonly XNamespace _opfdc;
        private readonly XNamespace _opfns;
        private readonly ZipContainer _zip;
        private readonly EpubCoverHelper _coverHelper;

        public EpubSummaryParser(Stream source)
        {
            _zip = ZipContainer.Unzip(source);
            XDocument xmlDocument = _zip.GetFileStream("META-INF/container.xml").GetXmlDocument();
            XElement root = xmlDocument.Root;
            if (root == null)
                throw new DataException(InvalidEpubMetaInfo);

            XAttribute attribute = root.Attribute("xmlns");
            XNamespace xmlns = (attribute != null) ? XNamespace.Get(attribute.Value) : XNamespace.None;
            XAttribute fullPath = xmlDocument.Descendants(xmlns + "rootfile").First().Attribute("full-path");
            if (fullPath == null)
                throw new DataException(InvalidEpubMetaInfo);

            string path = fullPath.Value;
            _opfPath = path;
            _opf = _zip.GetFileStream(path).GetXmlDocument();
            _opfRoot = _opf.Root;
            if (_opfRoot == null)
                throw new DataException(InvalidEpubMetaInfo);

            _oebps = GetPath(path);
            _opfns = XNamespace.Get("http://www.idpf.org/2007/opf");
            _opfdc = XNamespace.Get("http://purl.org/dc/elements/1.1/");

            _coverHelper = new EpubCoverHelper(_zip, _opfns, _opfRoot, _oebps);
        }

        public override void BuildChapters()
        {
            Chapters.Clear();
            string ncxPath = GetNcxPath();
            if (!string.IsNullOrEmpty(ncxPath))
            {
                string path = _opfPath + ncxPath;
                XDocument xmlDocument = _zip.GetFileStream((_opfPath) + ncxPath, true, true).GetXmlDocument(true);
                XNamespace ns = XNamespace.Get("http://www.daisy.org/z3986/2005/ncx/");
                XElement root = xmlDocument.Root;
                if (root != null)
                {
                    XElement navMap = root.Element(ns + "navMap");
                    ParseItems(navMap, 0, ns, path);
                }
            }
        }

        public override ITokenParser GetTokenParser()
        {
            Anchors.Clear();
            return new EpubTokenParser(_opf, _opfPath, _zip, GetCss(), Anchors);
        }

        public override BookSummary GetBookPreview()
        {
            XElement metadata = _opfRoot.Element(_opfns + "metadata");
            if (metadata == null)
                throw new DataException("Incorect opf data: Metadata not found.");
            
            XElement title = metadata.Element(_opfdc + "title");
            XElement language = metadata.Element(_opfdc + "language");
            return new BookSummary
                       {
                           AuthorName = GetAuthorName(metadata),
                           Title = (title != null) ? title.Value.Trim() : string.Empty,
                           Description = GetAnnotation(metadata),
                           Language = (language != null) ? language.Value : null,
                           UniqueID = GetUniqueID(metadata)
                       };
        }

        private string GetAnnotation(XElement metadata)
        {
            XElement description = metadata.Element(_opfdc + "description");
            if (description == null)
                return string.Empty;

            return Regex.Replace(Regex.Replace(description.Value.Replace("\n", string.Empty), "</p>", "\n"), "<[^>]*>", "");
        }

        private string GetAuthorName(XElement metadata)
        {
            XElement creator = metadata.Element(_opfdc + "creator");
            if (creator == null)
                return string.Empty;

            string[] strArray = creator.Value.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", strArray);
        }

        private string GetUniqueID(XElement metadata)
        {
            XElement identifier = metadata.Element(_opfdc + "identifier");
            if (identifier == null)
                return string.Empty;

            string str = identifier.Value;
            XAttribute attribute = identifier.Attribute("scheme");
            if (attribute != null)
            {
                str = attribute.Value.ToLower() + "://" + str;
            }
            return str.SafeSubstring(1000);
        }

        private static string CleanText(string value)
        {
            value = value.HtmlDecode().Trim();
            if (value.IndexOf("  ", StringComparison.InvariantCulture) >= 0)
            {
                value = value.Replace("  ", " ");
            }
            return value;
        }

        private CSS GetCss()
        {
            var sheet = new CSS();
            sheet.Analyze(DefaultEpubCss);
            
            var hrefs = _opfRoot
                .Elements(_opfns + "manifest")
                .Elements(_opfns + "item")
                .Where(e =>
                           {
                               var mediaType = e.Attribute("media-type");
                               return mediaType != null && mediaType.Value == "text/css";
                           })
                .Attributes("href").Select(h => h.Value);

            foreach (var href in hrefs)
            {
                Stream stream = _zip.GetFileStream(_opfPath + href, false, true);
                if (stream != null)
                {
                    sheet.Analyze(new StreamReader(stream).ReadToEnd());
                }
            }
            return sheet;
        }

        private string GetNcxPath()
        {
            XAttribute tocAttr = _opfRoot
                .Elements(_opfns + "spine")
                .Attributes("toc")
                .FirstOrDefault();
            if (tocAttr == null)
                return string.Empty;

            string toc = tocAttr.Value;

            var ncxHref = _opfRoot
                .Elements(_opfns + "manifest")
                .Elements(_opfns + "item")
                .Where(i =>
                           {
                               var id = i.Attribute("id");
                               return id != null && id.Value == toc;
                           })
                .Attributes("href")
                .FirstOrDefault();

            if (ncxHref != null)
                return ncxHref.Value;

            return string.Empty;
        }

        private string GetPath(string filePath)
        {
            List<string> values = filePath.Split(new[] {'/'}).ToList();
            if (values.Count > 1)
            {
                values.RemoveAt(values.Count - 1);
                return (string.Join("/", values) + "/");
            }
            return string.Empty;
        }


        private void ParseItems(XElement root, int level, XNamespace ns, EpubPath path)
        {
            foreach (XElement navPoint in root.Elements(ns + "navPoint"))
            {
                XElement text = navPoint.Elements(ns + "navLabel").Elements(ns + "text").FirstOrDefault();
                if (text == null)
                    continue;
                
                XAttribute srcAttr = navPoint.Elements(ns + "content").Attributes("src").FirstOrDefault();
                if (srcAttr == null)
                    continue;
                            
                string cleanSource = srcAttr.Value;
                int length = srcAttr.Value.IndexOf("#", StringComparison.Ordinal);
                length = length > -1 ? length : cleanSource.Length;
                cleanSource = cleanSource.Substring(0, length);
                                
                string key = path + cleanSource;
                if (Anchors.ContainsKey(key))
                {
                    int num = Anchors[key];
                    var item = new BookChapter
                                {
                                    Level = level,
                                    Title = CleanText(text.Value),
                                    TokenID = num
                                };
                    Chapters.Add(item);
                    ParseItems(navPoint, level + 1, ns, path);
                }
            }
        }

        private bool SaveCoverImage(string path, string bookId)
        {
            Stream stream = _zip.GetFileStream(_oebps + path, false);
            if (stream == null)
            {
                return false;
            }
            return SaveCoverImages(bookId, stream);
        }

        public override bool SaveCover(string bookId)
        {
            string coverHref = _coverHelper.GetCoverPath();
            if (string.IsNullOrEmpty(coverHref))
                return false;

            return SaveCoverImage(coverHref, bookId);
        }

        public override void SaveImages(Stream output)
        {
            var document = new XDocument();
            var images = new XElement("images");
            document.Add(images);
            XElement manifest = _opfRoot.Element(_opfns + "manifest");
            if(manifest == null)
                throw new DataException(ManifestDataNotFound);

            IEnumerable<XElement> items = manifest.Elements(_opfns + "item");

            var @event = new AutoResetEvent(false);
            Execute.OnUIThread(() => 
            {
                foreach (XElement element in items)
                {
                    XAttribute mediaTypeAttr = element.Attributes().FirstOrDefault(t => (t.Name.LocalName == "media-type"));
                    if (mediaTypeAttr == null)
                        continue;

                    switch (mediaTypeAttr.Value)
                    {
                        case "image/png":
                        case "image/jpg":
                        case "image/jpeg":
                            try
                            {
                                string href = element.Attributes().First(t => (t.Name.LocalName == "href")).Value;
                                string path = _oebps + href;
                                Stream imageStream = _zip.GetFileStream(path, false);
                                if (imageStream == null)
                                    continue;

                                Size imageSize = imageStream.GetImageSize();
                                images.Add(
                                    new BookImage
                                    {
                                        ID = path,
                                        Width = (int) imageSize.Width,
                                        Height = (int) imageSize.Height,
                                        Data = imageStream.ToBase64String()
                                    }.Save());
                                
                            }
                            catch
                            {
                            }
                            break;
                    }
                }
                @event.Set();
            });
            
            @event.WaitOne();
            document.Save(output);
        }

        private const string DefaultEpubCss =
@"sup{vertical-align:super}
sub{vertical-align:sub}
p,div{font-size:1em;text-indent:32px;margin-bottom:0px;display:block}
div,h1,h2,h3,h4,h5,h6{margin-bottom:12px}
h1,h2,h3,h4,h5,h6,center{text-align:center}
sup,sub,a,b,i,u,span,label,strong,em,emphasis{display:inline;margin-bottom: 0px;}
b,strong {font-weight: bold}
i,em,emphasis {font-style: italic}
h1{font-size:35px}
h2{font-size:32px}
h3{font-size:29px}
h4{font-size:26px}
h5{font-size:23px}
h6{font-size:20px}";
    }
}