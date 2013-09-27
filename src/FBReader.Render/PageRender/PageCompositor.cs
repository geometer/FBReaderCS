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
using System.Threading.Tasks;
using System.Windows;
using FBReader.DataModel.Model;
using FBReader.Render.Parsing;
using FBReader.Render.Tools;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.Parsers;
using FBReader.Tokenizer.TextStructure;

namespace FBReader.Render.PageRender
{
    public class PageCompositor
    {
        private readonly BookModel _book;
        private readonly int _fontSize;
        private readonly IList<BookImage> _images;
        private readonly Size _pageSize;

        public PageCompositor(BookModel book, int fontSize, Size pageSize, IList<BookImage> images)
        {
            _book = book;
            _fontSize = fontSize;
            _pageSize = pageSize;
            _images = images;
        }

        public Task<PageInfo> GetPageAsync(int tokenID, string startText)
        {
            return Task.Factory.StartNew(
                delegate
                    {
                        using (var tokens = new BookTokenIterator(_book.GetTokensPath(), TokensTool.GetTokens(_book.BookID)))
                        {
                            PageInfo page = BookFactory
                                .GetBookParser(_book.Type, tokens, _fontSize, _pageSize, _images)
                                .GetPage(tokenID, startText);
                            return page;
                        }
                    });
        }

        public Task<PageInfo> GetPreviousPageAsync(int tokenID, string startText)
        {
            return Task.Factory.StartNew(
                delegate
                    {
                        using (var tokens = new BookTokenIterator(_book.GetTokensPath(), TokensTool.GetTokens(_book.BookID)))
                        {
                            PageInfo previousPage = BookFactory
                                .GetBookParser(_book.Type, tokens, _fontSize, _pageSize, _images)
                                .GetPreviousPage(tokenID, startText);

                            return previousPage;
                        }
                    });
        }  
    }
}