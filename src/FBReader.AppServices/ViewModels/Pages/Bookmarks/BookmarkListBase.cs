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

using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using FBReader.AppServices.DataModels;
using FBReader.AppServices.Events;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;

namespace FBReader.AppServices.ViewModels.Pages.Bookmarks
{
    public abstract class BookmarkListBase : Screen, IHandle<BookmarkRemoved>
    {
        private readonly INavigationService _navigationService;
        private readonly IBookmarkRepository _bookmarkService;
        private readonly IEventAggregator _eventAggregator;

        protected BookmarkListBase(INavigationService navigationService, IBookmarkRepository bookmarkService, IEventAggregator eventAggregator)
        {
            _navigationService = navigationService;
            _bookmarkService = bookmarkService;
            _eventAggregator = eventAggregator;

            _eventAggregator.Subscribe(this);
        }

        public IObservableCollection<BookmarkItemDataModel> Bookmarks { get; set; }

        public int CatalogId { get; set; }

        public CatalogBookItemModel CatalogBookItemModel { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            GetBookmarksAsync();
        }

        protected abstract void GetBookmarksAsync();

        public void RemoveBookmark(object bookmark)
        {
            var bookmarkDataModel = bookmark as BookmarkItemDataModel;
            if (bookmarkDataModel == null)
                return;

            _bookmarkService.DeleteBookmark(bookmarkDataModel.Bookmark);

            _eventAggregator.Publish(new BookmarkRemoved(bookmarkDataModel.Bookmark));
        }

        public async void AddBookmarkAsync(BookmarkItemDataModel bookmarkDataModel)
        {
            if(Bookmarks == null)
                return;

            int index = await Task<int>.Factory.StartNew(() =>
                {
                    int i = 0;
                    var bookmark = Bookmarks
                        .Where(b => b.Bookmark.BookID == bookmarkDataModel.Bookmark.BookID)
                        .LastOrDefault(b => b.Bookmark.TokenID <= bookmarkDataModel.Bookmark.TokenID);

                    if (bookmark == null)
                    {
                        bookmark = Bookmarks
                            .LastOrDefault(b => System.String.Compare(
                                b.Bookmark.BookID, 
                                bookmarkDataModel.Bookmark.BookID, 
                                System.StringComparison.Ordinal) < 0);
                    }

                    if (bookmark != null)
                    {
                        i = Bookmarks.IndexOf(bookmark) + 1;
                    }
                    return i;
                });

            Bookmarks.Insert(index, bookmarkDataModel);
        }

        public void OnBookmarkClick(BookmarkItemDataModel itemDataModel)
        {
            _navigationService
                .UriFor<ReadPageViewModel>()
                .WithParam(vm => vm.BookId, itemDataModel.Bookmark.BookID)
                .WithParam(vm => vm.TokenOffset, itemDataModel.Bookmark.TokenID)
                .WithParam(vm => CatalogId, CatalogId)
                .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                .Navigate();
        }

        public void Handle(BookmarkRemoved message)
        {
            if(Bookmarks == null)
                return;

            var bookmark = Bookmarks.SingleOrDefault(b => b.Bookmark.BookmarkID == message.Bookmark.BookmarkID);
            if(bookmark == null)
                return;

            Bookmarks.Remove(bookmark);
        }
    }
}
