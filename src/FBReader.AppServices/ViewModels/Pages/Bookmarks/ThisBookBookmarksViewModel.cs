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
    public class ThisBookBookmarksViewModel : BookmarkListBase
    {
        private readonly BookmarksController _bookmarksController;
        private readonly IBookmarkRepository _bookmarkService;
        private readonly IBookRepository _bookRepository;
        private readonly IBusyIndicatorManager _busyIndicatorManager;

        public ThisBookBookmarksViewModel(
            INavigationService navigationService,
            BookmarksController bookmarksController, 
            IBookmarkRepository bookmarkService,
            IBookRepository bookService,
            IBusyIndicatorManager busyIndicatorManager,
            IEventAggregator eventAggregator) : base(navigationService, bookmarkService, eventAggregator)
        {
            _bookmarksController = bookmarksController;
            _bookmarkService = bookmarkService;
            _bookRepository = bookService;
            _busyIndicatorManager = busyIndicatorManager;
            DisplayName = UIStrings.BookmarksPivot_ThisBook;
        }

        public string BookId { get; set; }

        public string EmptyContent { get; set; }

        protected override async void GetBookmarksAsync()
        {
            _busyIndicatorManager.Start();

            var book = await Task<BookModel>.Factory.StartNew(() => _bookRepository.Get(BookId, false));
            if (book != null)
            {
                EmptyContent = string.Format("{0} \"{1}\"", UIStrings.BookmarksPivot_ThisBook_EmptyList, book.Title);
            }
            else
            {
                EmptyContent = UIStrings.BookmarksPivot_ThisBook_EmptyList;
            }

            var bookmarks =
                await Task<IEnumerable<BookmarkModel>>.Factory.StartNew(() => _bookmarkService.GetBookmarks(BookId));

            Bookmarks = new BindableCollection<BookmarkItemDataModel>(bookmarks
                .Select(bm => _bookmarksController.CreateBookmarkDataModel(bm)));

            _busyIndicatorManager.Stop();
        }
    }
}
