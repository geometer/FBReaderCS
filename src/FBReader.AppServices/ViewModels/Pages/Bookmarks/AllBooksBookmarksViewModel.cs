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
using System.Threading.Tasks;
using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.AppServices.DataModels;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.PhoneServices;

namespace FBReader.AppServices.ViewModels.Pages.Bookmarks
{
    public class AllBooksBookmarksViewModel : BookmarkListBase
    {
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly BookmarksController _bookmarksController;
        private readonly IBookRepository _bookRepository;
        private readonly IBusyIndicatorManager _busyIndicatorManager;

        public AllBooksBookmarksViewModel(
            INavigationService navigationService,
            IBookmarkRepository bookmarkRepository, 
            BookmarksController bookmarksController,
            IBookRepository bookRepository,
            IBusyIndicatorManager busyIndicatorManager,
            IEventAggregator eventAggregator)
            : base(navigationService, bookmarkRepository, eventAggregator)
        {
            _bookmarkRepository = bookmarkRepository;
            _bookmarksController = bookmarksController;
            _bookRepository = bookRepository;
            _busyIndicatorManager = busyIndicatorManager;
            DisplayName = UIStrings.BookmarksPivot_AllBooks;
        }

        protected override async void GetBookmarksAsync()
        {
            _busyIndicatorManager.Start();

            var bookmarks = await Task<IEnumerable<BookmarkModel>>.Factory.StartNew(() => _bookmarkRepository.GetBookmarks());
            var books = await Task<IEnumerable<BookModel>>.Factory.StartNew(() => _bookRepository.GetAll());

            var bookmarksWithBooks = Enumerable.Join(bookmarks, books, bm => bm.BookID, b => b.BookID,
                            (bookmark, book) => new { Bookmark = bookmark, Book = book });

            Bookmarks = new BindableCollection<BookmarkItemDataModel>(bookmarksWithBooks
                .Select(b => _bookmarksController.CreateBookmarkDataModel(b.Bookmark, b.Book != null ? b.Book.Title : null)));

            _busyIndicatorManager.Stop();
        }
    }
}
