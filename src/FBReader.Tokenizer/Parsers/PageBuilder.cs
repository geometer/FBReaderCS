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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FBReader.Settings;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.TextStructure;
using FBReader.Tokenizer.Tokens;
using FBReader.Tokenizer.Extensions;

namespace FBReader.Tokenizer.Parsers
{
    public class PageBuilder
    {
        private readonly IEnumerable<BookImage> _images;
        private readonly PageInfo _page;
        private readonly Size _pageSize;
        private ParagraphInfo _paragraph;
        private double _height;

        public PageBuilder(Size pageSize, IEnumerable<BookImage> images)
        {
            _images = images;
            _pageSize = pageSize;
            _height = 0.0;
            _paragraph = null;
            _page = new PageInfo
            {
                FirstTokenID = -1
            };
        }

        public PageInfo GetPage(IEnumerable<TokenBlockBase> lines)
        {
            foreach (TokenBlockBase tokenBlock in lines)
            {
                if (!AppendToPage(tokenBlock))
                    return _page;
            }
            return _page;
        }

        private bool AppendToPage(TokenBlockBase tokenBlock)
        {
            if (tokenBlock is PageBreakBlock)
                return BreakPage();

            if (tokenBlock is ImageTokenBlock)
                return AppendToPage((ImageTokenBlock)tokenBlock);

            if (tokenBlock is TextTokenBlock)
                return AppendToPage((TextTokenBlock)tokenBlock);

            if (tokenBlock is SeparatorTokenBlock)
                return AppendToPage((SeparatorTokenBlock)tokenBlock);

            return true;
        }

        private bool AppendToPage(TextTokenBlock block)
        {
            if (_page.FirstTokenID < 0)
                _page.FirstTokenID = block.FirstTokenID;

            double height = block.Height * (double) AppSettings.Default.FontSettings.FontInterval;
            if (_height + height <= _pageSize.Height)
            {
                _height += height;
                _page.LastTokenID = block.LastTokenID;
                _page.LastTextPart = block.GetLastPart();
                if (_paragraph == null)
                {
                    _paragraph = CreateParagraph(block);
                    _page.Paragraphs.Add(_paragraph);
                }
                AddInlineToParagraph(block);
                return true;
            }
            return false;
        }

        private bool AppendToPage(SeparatorTokenBlock block)
        {
            if (!_page.Paragraphs.Any())
                return true;

            _page.Paragraphs.Last().MarginBottom += block.Height;
            _height += block.Height * (double) AppSettings.Default.FontSettings.FontInterval;
            return true;
        }

        private bool AppendToPage(ImageTokenBlock block)
        {
            BookImage bookImage = _images.FirstOrDefault(t => t.ID == block.ImageID);
            if (bookImage == null)
                return true;

            Size size = bookImage.FitToSize(_pageSize);
            if (_height + size.Height <= _pageSize.Height)
            {
                _height += size.Height;
                if (_page.FirstTokenID < 0)
                    _page.FirstTokenID = block.FirstTokenID;

                _page.LastTokenID = block.LastTokenID;
                _page.LastTextPart = block.GetLastPart();
                AddImageParagraph(block, size);
                return true;
            }
            return false;
        }

        private bool BreakPage()
        {
            if (!_page.Paragraphs.Any())
                return true;

            return false;
        }

        private void AddImageParagraph(ImageTokenBlock block, Size size)
        {
            var paragraphInfo = new ImageParagraphInfo();
            paragraphInfo.Inlines.Add(new ImageElement
                                      {
                                          ImageID = block.ImageID,
                                          Width = (int) size.Width,
                                          Height = (int) size.Height
                                      });
            _page.Paragraphs.Add(paragraphInfo);
            _page.Lines.Add(block);
        }

        private ParagraphInfo CreateParagraph(TextTokenBlock block)
        {
            return new ParagraphInfo
                   {
                       TextAlign = block.TextAlign,
                       MarginLeft = block.MarginLeft,
                       MarginRight = block.MarginRight,
                       TextIndent = block.TextIndent
                   };
        }

        private void AddInlineToParagraph(TextTokenBlock block)
        {
            _page.Lines.Add(block);
            foreach (TextElementBase baseInlineItem in block.Inlines)
            {
                if (!(baseInlineItem is EOPElement))
                {
                    _paragraph.Inlines.Add(baseInlineItem);
                }
                else
                {
                    _paragraph = null;
                    break;
                }
            }
        }
    }
}