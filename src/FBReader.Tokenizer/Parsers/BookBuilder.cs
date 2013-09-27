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
using System.Windows;
using FBReader.Settings;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.Fonts;
using FBReader.Tokenizer.TextStructure;
using FBReader.Tokenizer.Tokens;
using FBReader.Tokenizer.Extensions;

namespace FBReader.Tokenizer.Parsers
{
    public class BookBuilder : IBookBuilder
    {
        private readonly BookTokenIterator _bookTokens;
        private readonly IEnumerable<BookImage> _images;
        private readonly IList<double> _headerSizes;
        private readonly IFontHelper _helper;
        private readonly Size _pageSize;
        private readonly double _textSize;
        private readonly bool _hyphenation;
        private readonly bool _useCssFontSize;

        public BookBuilder(
            BookTokenIterator bookTokens,
            IEnumerable<BookImage> images, 
            IList<double> headerSizes,
            IFontHelper helper,
            Size pageSize,
            double textSize, 
            bool hyphenation,
            bool useCssFontSize)
        {
            _bookTokens = bookTokens;
            _images = images;
            _headerSizes = headerSizes;
            _helper = helper;
            _pageSize = pageSize;
            _textSize = textSize;
            _hyphenation = hyphenation;
            _useCssFontSize = useCssFontSize;
        }

        public PageInfo GetPage(int firstTokenID, string lastText)
        {
            if (firstTokenID >= _bookTokens.Count)
                return null;

            LineBuilder lineBuilder = CreateLineParser();
            IEnumerable<TokenBlockBase> lines = lineBuilder.GetLines(_bookTokens, lastText, firstTokenID);
            var pageParser = new PageBuilder(_pageSize, _images);
            string startText = lastText;
            PageInfo pageInfo = pageParser.GetPage(lines);
            if (pageInfo == null)
                return null;

            pageInfo.StartText = startText;
            return pageInfo;
        }

        public PageInfo GetPreviousPage(int firstTokenID, string stopText)
        {
            string startText = string.Empty;
            var tokenLines = new List<TokenBlockBase>();
            int stopTokenID = firstTokenID;
            LineBuilder lineBuilder = CreateLineParser();
            foreach (int blockStartId in FindBlockTokenID(firstTokenID))
            {
                List<TokenBlockBase> linesInBlock = lineBuilder
                    .GetLines(_bookTokens, string.Empty, blockStartId, stopTokenID, stopText)
                    .ToList();

                EndTokenBlock(linesInBlock);

                InsertTokenBlocks(linesInBlock, tokenLines);

                // Remove all page breaks at the end
                TokenBlockBase pageBreak;
                while ((pageBreak = tokenLines.LastOrDefault()) != null && pageBreak is PageBreakBlock)
                {
                    tokenLines.RemoveAt(tokenLines.Count - 1);
                }

                if (RemovePageBreaks(tokenLines, ref stopTokenID, ref startText)) 
                    break;

                stopTokenID = blockStartId;
                stopText = string.Empty;
            }

            if (!tokenLines.Any())
                return null;

            PageInfo pageInfo = new PageBuilder(_pageSize, _images).GetPage(tokenLines);
            if (pageInfo == null)
                return null;

            pageInfo.StartText = startText;
            pageInfo.FirstTokenID = stopTokenID;
            return pageInfo;
        }

        private bool RemovePageBreaks(List<TokenBlockBase> tokenLines, ref int stopTokenID, ref string startText)
        {
            if (!tokenLines.Any()) 
                return false;

            int itemsAfterPageBreakCount = tokenLines.AsEnumerable().Reverse().TakeWhile((t => !(t is PageBreakBlock))).Count();
            if (itemsAfterPageBreakCount == tokenLines.Count)
            {
                // No page breaks found
                itemsAfterPageBreakCount = 0;
                double acumulativeHeight = 0.0;
                foreach (TokenBlockBase tokenLine in tokenLines.AsEnumerable().Reverse())
                {
                    double heightIncrement = tokenLine.Height * (double) AppSettings.Default.FontSettings.FontInterval;
                    if (acumulativeHeight + heightIncrement <= _pageSize.Height)
                    {
                        acumulativeHeight += heightIncrement;
                        ++itemsAfterPageBreakCount;
                    }
                    else
                    {
                        stopTokenID = tokenLine.LastTokenID + 1;

                        string lastPart = tokenLine.GetLastPart();
                        if (!string.IsNullOrEmpty(lastPart))
                            startText = lastPart;

                        break;
                    }
                }
            }

            if (itemsAfterPageBreakCount < tokenLines.Count)
            {
                tokenLines.RemoveRange(0, tokenLines.Count - itemsAfterPageBreakCount);
                return true;
            }
            return false;
        }

        private void InsertTokenBlocks(List<TokenBlockBase> linesInBlock, List<TokenBlockBase> tokenLines)
        {
            for (int index = 0; index < linesInBlock.Count; ++index)
            {
                TokenBlockBase baseTokenBlock = linesInBlock[index];
                if (baseTokenBlock.Height < 0.0)
                {
                    var imageTokenLine = baseTokenBlock as ImageTokenBlock;
                    if (imageTokenLine != null)
                        baseTokenBlock.Height = GetImageHeight(imageTokenLine.ImageID);
                }
                tokenLines.Insert(index, baseTokenBlock);
            }
        }

        private static void EndTokenBlock(List<TokenBlockBase> linesInBlock)
        {
            if (!linesInBlock.Any()) 
                return;
            
            var textTokenLine = linesInBlock.Last() as TextTokenBlock;
            if (textTokenLine == null) 
                return;

            TextElementBase textElementBase = textTokenLine.Inlines.LastOrDefault();
            
            if (!(textElementBase is EOPElement) && !(textElementBase is EOLElement))
            {
                if (!string.IsNullOrEmpty(textTokenLine.GetLastPart()))
                    textTokenLine.EndLine();
                else
                    textTokenLine.EndParagraph();
            }
        }

        protected virtual LineBuilder CreateLineParser()
        {
            return new LineBuilder(_pageSize.Width, _headerSizes, _textSize, _helper, _hyphenation, _useCssFontSize);
        }

        private double GetImageHeight(string imageID)
        {
            BookImage bookImage = _images.FirstOrDefault(t => t.ID == imageID);
            if (bookImage == null)
                return 0.0;
            return bookImage.FitToSize(_pageSize).Height;
        }

        private IEnumerable<int> FindBlockTokenID(int tokenID)
        {
            while (tokenID >= 0)
            {
                _bookTokens.MoveTo(tokenID);
                _bookTokens.MoveNext();
                TokenBase current = _bookTokens.Current;
                if (current is PictureToken)
                    yield return tokenID;

                var asOpenTagToken = current as TagOpenToken;
                if ((asOpenTagToken != null) && !asOpenTagToken.TextProperties.Inline)
                    yield return tokenID;

                tokenID--;
            }
        }
    }
}