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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Render.PageRender;
using FBReader.Render.Tools;
using FBReader.Settings;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.TextStructure;

namespace FBReader.AppServices.Controller
{
    public class ReadController
    {
        private readonly BookModel _bookModel;
        private readonly PageCompositor _pageLoader;
        private readonly IBookRepository _bookRepository;

        private PageInfo _currentPage;
        private PageInfo _prevPage;
        private PageInfo _nextPage;
        private readonly IBookView _bookView;
        private readonly int _offset;
        private readonly BookData _data;
        private readonly List<BookImage> _images;

        public ReadController(IBookView page, string bookId, IBookRepository bookRepository, int offset = 0)
        {
            _data = new BookData(bookId);
            _bookView = page;
            _offset = offset;
            _bookRepository = bookRepository;
            _bookModel = _bookRepository.Get(bookId);
            _images = _data.LoadImages().ToList();
            _pageLoader = new PageCompositor(_bookModel, (int)(AppSettings.Default.FontSettings.FontSize), new Size(page.GetSize().Width - AppSettings.Default.Margin.Left - AppSettings.Default.Margin.Right, page.GetSize().Height - AppSettings.Default.Margin.Top - AppSettings.Default.Margin.Bottom), _images);
            BookId = bookId;
        }

        public string BookId { get; private set; }

        public bool IsFirst
        {
            get { return _prevPage == null; }
        }

        public bool IsLast
        {
            get { return _nextPage == null; }
        }

        public int CurrentPage
        {
            get
            {
                return (int) Math.Ceiling((double) (_currentPage.FirstTokenID + 1)/Constants.WORDS_PER_PAGE);
            }
        }

        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling((double)_bookModel.TokenCount / Constants.WORDS_PER_PAGE);
            }
        }

        public int Offset
        {
            get { return _currentPage.FirstTokenID; }
        }

        public async Task ShowNextPage()
        {
            var page = _nextPage ?? await PrepareNextPage();
            if (page != null)
            {
                _prevPage = _currentPage;
                _currentPage = page;
                _nextPage = null;
                _bookView.SwapNextWithCurrent();

                await PrepareNextPage();

                if (_prevPage == null)
                    await PreparePrevPage();
            }
        }

        public async Task ShowPrevPage()
        {            
            var page = _prevPage ?? await PreparePrevPage();
            if (page != null)
            {
                _nextPage = _currentPage;
                _currentPage = page;
                _prevPage = null;
                _bookView.SwapPrevWithCurrent();

                await PreparePrevPage();
            }
        }

        public async Task<PageInfo> PreparePrevPage()
        {
            if (_currentPage == null)
                return null;

            string startText = string.Empty;
            if (_currentPage != null)
            {
                startText = _currentPage.StartText;
            }

            var page = await _pageLoader.GetPreviousPageAsync(_currentPage.FirstTokenID, startText);
            if (page == null || page.FirstTokenID < 0)
                return null;

            _prevPage = page;

            var bgBuilder = new PageRenderer(_images);
            _bookView.PreviousTexts.Clear();
            _bookView.PreviousLinks.Clear();
            bgBuilder.RenderPageAsync(new RenderPageRequest()
            {
                Page = page,
                Panel = _bookView.GetPrevPagePanel(),
                Texts = _bookView.PreviousTexts,
                Links=  _bookView.PreviousLinks,
                Book = _bookModel,
                Bookmarks = _bookView.Bookmarks
            });

            Log.Write("Preparing prev page - @" + page.FirstTokenID + " - " + page.LastTokenID);

            return page;
        }

        public async Task<PageInfo> PrepareNextPage()
        {
            string lastText = string.Empty;
            if (_currentPage != null)
            {
                lastText = _currentPage.LastTextPart;
            }

            int nextTokenId = _currentPage == null ? _offset : _currentPage.LastTokenID + 1;

            var page = await _pageLoader.GetPageAsync(nextTokenId, lastText);
            if (page == null || page.FirstTokenID < 0)
                return null;

            _nextPage = page;

            var bgBuilder = new PageRenderer(_images);
            _bookView.NextTexts.Clear();
            _bookView.NextLinks.Clear();
            bgBuilder.RenderPageAsync(new RenderPageRequest()
            {
                Page = page,
                Panel = _bookView.GetNextPagePanel(),
                Texts = _bookView.NextTexts,
                Book = _bookModel,
                Links = _bookView.NextLinks,
                Bookmarks = _bookView.Bookmarks
            });

            Log.Write("Preparing next page - @" + page.FirstTokenID + " - " + page.LastTokenID);

            return page;
        }
    }
}
