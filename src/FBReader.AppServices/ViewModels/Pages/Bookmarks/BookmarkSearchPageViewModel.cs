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
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.AppServices.DataModels;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.PhoneServices;

namespace FBReader.AppServices.ViewModels.Pages.Bookmarks
{
    public class BookmarkSearchPageViewModel : Screen
    {
        private readonly INavigationService _navigationService;
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly BookmarksController _bookmarksController;
        private readonly IBookRepository _bookRepository;
        private readonly IBusyIndicatorManager _busyIndicatorManager;

        private List<BookmarkSearchResultDaraModel> _bookmarks;

        public BookmarkSearchPageViewModel(
            INavigationService navigationService,
            IBookmarkRepository bookmarkRepository,
            BookmarksController bookmarksController,
            IBookRepository bookRepository,
            IBusyIndicatorManager busyIndicatorManager)
        {
            _navigationService = navigationService;
            _bookmarkRepository = bookmarkRepository;
            _bookmarksController = bookmarksController;
            _bookRepository = bookRepository;
            _busyIndicatorManager = busyIndicatorManager;
        }

        public string Query { get; set; }

        public List<BookmarkSearchResultDaraModel> SearchResult { get; set; }

        public string BookId { get; set; }

        public string SearchBookSubtitle { get; set; }

        public int CatalogId { get; set; }

        public CatalogBookItemModel CatalogBookItemModel { get; set; }

        public string CatalogBookItemKey { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (TransientStorage.Contains(CatalogBookItemKey))
                CatalogBookItemModel = TransientStorage.Get<CatalogBookItemModel>(CatalogBookItemKey);
        }

        public async void SearchAsync()
        {
            _busyIndicatorManager.Start();
            try
            {
                SearchResult = null;
                await InitBookmarks();

                SearchResult = await Task<List<BookmarkSearchResultDaraModel>>.Factory.StartNew(() => _bookmarks
                    .Where(b => b.BookmarkText.IndexOf(Query, StringComparison.InvariantCultureIgnoreCase) > -1)
                    .Select(b => 
                        { 
                            b.SearchQuery = Query;
                            return b;
                        })
                    .ToList());
            }
            finally
            {
                _busyIndicatorManager.Stop();
            }
        }

        private async Task InitBookmarks()
        {
            if (_bookmarks != null)
                return;
            
            if (string.IsNullOrEmpty(BookId))
            {
                var bookmarks = await Task<IEnumerable<BookmarkModel>>.Factory.StartNew(() => _bookmarkRepository.GetBookmarks());
                var books = await Task<IEnumerable<BookModel>>.Factory.StartNew(() => _bookRepository.GetAll());

                var bookmarksWithBooks = Enumerable.Join(bookmarks, books, bm => bm.BookID, b => b.BookID,
                                                            (bookmark, book) => new { Bookmark = bookmark, Book = book });
                _bookmarks = bookmarksWithBooks.Select(b => _bookmarksController.CreateBookmarkSearchDataModel(b.Bookmark, Query, b.Book.Title)).ToList();
            }
            else
            {
                var bookmarks = await Task<IEnumerable<BookmarkModel>>.Factory.StartNew(() => _bookmarkRepository.GetBookmarks(BookId));

                _bookmarks = bookmarks.Select(b => _bookmarksController.CreateBookmarkSearchDataModel(b, Query)).ToList();
            }
        }

        public void OnBookmarkClick(BookmarkSearchResultDaraModel itemDataModel)
        {
            _navigationService
                .UriFor<ReadPageViewModel>()
                .WithParam(vm => vm.BookId, itemDataModel.BookId)
                .WithParam(vm => vm.TokenOffset, itemDataModel.TokenOffset)
                .WithParam(vm => vm.CatalogId, CatalogId)
                .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                .Navigate();
        }

    }
}
