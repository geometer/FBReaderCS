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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using FBReader.Settings;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.Fonts;
using FBReader.Tokenizer.Parsers;
using FBReader.Tokenizer.Parsers.Epub;
using FBReader.Tokenizer.Parsers.Fb2;
using FBReader.Tokenizer.Parsers.Html;
using FBReader.Tokenizer.Parsers.Txt;

namespace FBReader.Render.Parsing
{
    public static class BookFactory
    {
        private static readonly Dictionary<string, IFontHelper> FontMetrics = new Dictionary<string, IFontHelper>();

        static BookFactory()
        {
        }

        private static IFontHelper GetActiveFontMetrics(string family)
        {
            lock (FontMetrics)
            {
                if (!FontMetrics.ContainsKey(family))
                    FontMetrics[family] = new FontHelper(family);
                return FontMetrics[family];
            }
        }

        public static IBookSummaryParser GetPreviewGenerator(string bookType, string fileName, Stream file)
        {
            switch (bookType)
            {
                case "txt":
                    return new TxtSummaryParser(file, fileName);
                case "html":
                    return new HtmlSummaryParser(file, fileName);
                case "fb2":
                    return new Fb2SummaryParser(file);
                case "epub":
                    return new EpubSummaryParser(file);
                default:
                    throw new NotSupportedException("Book type '" + bookType + "' is not supported!");
            }
        }

        public static IBookBuilder GetBookParser(string bookType, BookTokenIterator bookTokens, int fontSize, Size pageSize, IEnumerable<BookImage> images)
        {
            var headerSizes = new ReadOnlyCollection<double>(new List<double> {24, 32, 42});
            IFontHelper activeFontHelper = GetActiveFontMetrics(AppSettings.Default.FontSettings.FontFamily.Source);
            switch (bookType)
            {
                case "fb2":
                case "txt":
                case "epub":
                case "html":
                    return new BookBuilder(bookTokens, images, headerSizes, activeFontHelper, pageSize, fontSize , AppSettings.Default.Hyphenation, AppSettings.Default.UseCSSFontSize);
                default:
                    throw new NotSupportedException("Book type '" + bookType + "' is not supported!");
            }
        }
    }
}
