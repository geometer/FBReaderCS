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

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FBReader.Common.ExtensionMethods;
using FBReader.IO;

namespace FBReader.Tokenizer.Parsers.Epub
{
    public class EpubCoverHelper
    {
        private readonly string _oebps;
        private readonly XElement _opfRoot;
        private readonly XNamespace _opfns;
        private readonly ZipContainer _zip;

        public EpubCoverHelper(ZipContainer zip, XNamespace opfns, XElement opfRoot, string oebps)
        {
            _zip = zip;
            _opfns = opfns;
            _opfRoot = opfRoot;
            _oebps = oebps;
        }

        public string GetCoverPath()
        {
            string coverHref = (TryFindCoverInMetadata()
                                ?? TryFindCoverInGuide())
                               ?? TryFindCoverByFileName();

            if (coverHref == null)
                return null;

            string ext = Path.GetExtension(coverHref);
            if (ext == ".xhtml" || ext == ".xml")
            {
                coverHref = FindImageInXml(coverHref);
                if (coverHref == null)
                    return null;
            }

            switch (Path.GetExtension(coverHref))
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                    return coverHref;
                default:
                    return null;
            }
        }

        private string TryFindCoverInMetadata()
        {
            XElement metadata = _opfRoot.Element(_opfns + "metadata");
            if (metadata == null)
                return null;

            XElement coverElement = metadata
                .Elements()
                .FirstOrDefault(e =>
                                    {
                                        XAttribute name = e.Attribute("name");
                                        if (name == null) return false;
                                        return name.Value == "cover";
                                    });

            if (coverElement == null)
                return null;

            XAttribute coverIdAttr = coverElement.Attribute("content");
            if (coverIdAttr == null)
                return null;

            string coverId = coverIdAttr.Value;

            XElement manifest = _opfRoot.Element(_opfns + "manifest");
            if (manifest == null)
                return null;

            var cover = (from item in manifest.Elements(_opfns + "item")
                         let id = item.Attribute("id")
                         where (id != null) && (id.Value == coverId)
                         select item).FirstOrDefault<XElement>();

            if (cover == null)
                return null;

            XAttribute coverHref = cover.Attribute("href");
            if (coverHref == null)
                return null;

            return coverHref.Value;
        }

        private string TryFindCoverByFileName()
        {
            XElement manifest = _opfRoot.Element(_opfns + "manifest");
            if (manifest == null)
                return null;

            var cover = (from item in manifest.Elements(_opfns + "item")
                         let href = item.Attribute("href")
                         let mediaType = item.Attribute("media-type")
                         where (href != null) && (href.Value.IndexOf("cover", StringComparison.InvariantCultureIgnoreCase) >= 0)
                         select item).FirstOrDefault<XElement>();

            if (cover != null)
                return cover.Attribute("href").Value;

            return null;
        }

        private string TryFindCoverInGuide()
        {
            XElement guide = _opfRoot.Element(_opfns + "guide");
            if (guide == null)
                return null;

            var cover = (from item in guide.Elements(_opfns + "reference")
                         let type = item.Attribute("type")
                         let mediaType = item.Attribute("media-type")
                         where (type != null) && (type.Value == "cover" || type.Value == "other.ms-coverimage-standard")
                         select item).FirstOrDefault<XElement>();

            if (cover == null)
                return null;

            XAttribute href = cover.Attribute("href");
            if (href == null)
                return null;

            return href.Value;
        }

        private string FindImageInXml(string path)
        {
            XDocument coverFile = _zip.GetFileStream(_oebps + path).GetXmlDocument();
            XElement coverRoot = coverFile.Root;
            if (coverRoot == null)
                return null;

            XElement image = coverRoot.DescendantNodes().OfType<XElement>().FirstOrDefault(n => n.Name.LocalName == "image");

            if (image != null)
            {
                XAttribute href = image.Attributes().FirstOrDefault(a => a.Name.LocalName == "href");
                if (href != null)
                    return href.Value;
            }
            XElement img = coverRoot.DescendantNodes().OfType<XElement>().FirstOrDefault(n => n.Name.LocalName == "img");
            if (img != null)
            {
                XAttribute src = img.Attributes().FirstOrDefault(a => a.Name.LocalName == "src");
                if (src != null)
                {
                    return src.Value;
                }
            }
            return null;
        }
    }
}