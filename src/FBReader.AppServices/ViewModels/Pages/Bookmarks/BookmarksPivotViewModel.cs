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
using System.Windows.Media;
using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Render.Tools;

namespace FBReader.AppServices.ViewModels.Pages.Bookmarks
{
    public class BookmarksPivotViewModel : Conductor<BookmarkListBase>.Collection.OneActive
    {
        private readonly ThisBookBookmarksViewModel _thisBookBookmarksViewModel;
        private readonly AllBooksBookmarksViewModel _allBooksBookmarksViewModel;
        private readonly INavigationService _navigationService;
        private readonly IBookRepository _bookRepository;
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly BookmarksController _bookmarksController;
        private readonly BookTool _bookTool;

        public BookmarksPivotViewModel(
            ThisBookBookmarksViewModel thisBookBookmarksViewModel, 
            AllBooksBookmarksViewModel allBooksBookmarksViewModel,
            INavigationService navigationService,
            IBookRepository bookRepository,
            IBookmarkRepository bookmarkRepository,
            BookmarksController bookmarksController,
            BookTool bookTool)
        {
            _thisBookBookmarksViewModel = thisBookBookmarksViewModel;
            _allBooksBookmarksViewModel = allBooksBookmarksViewModel;
            _navigationService = navigationService;
            _bookRepository = bookRepository;
            _bookmarkRepository = bookmarkRepository;
            _bookmarksController = bookmarksController;
            _bookTool = bookTool;
        }

        public int CatalogId { get; set; }

        public CatalogBookItemModel CatalogBookItemModel { get; set; }

        public string CatalogBookItemKey { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (TransientStorage.Contains(CatalogBookItemKey))
                CatalogBookItemModel = TransientStorage.Get<CatalogBookItemModel>(CatalogBookItemKey);

            _thisBookBookmarksViewModel.BookId = BookId;
            _thisBookBookmarksViewModel.CatalogId = CatalogId;
            _thisBookBookmarksViewModel.CatalogBookItemModel = CatalogBookItemModel;
            Items.Add(_thisBookBookmarksViewModel);
            Items.Add(_allBooksBookmarksViewModel);
        }

        public string BookId { get; set; }

        public void GoToSearch()
        {
            var navigateUri = _navigationService
                .UriFor<BookmarkSearchPageViewModel>();

            if (ActiveItem is ThisBookBookmarksViewModel)
            {
                var activeItem = (ThisBookBookmarksViewModel) ActiveItem;
                navigateUri.WithParam(vm => vm.BookId, activeItem.BookId);
            }
            navigateUri.WithParam(vm => vm.SearchBookSubtitle, ActiveItem.DisplayName)
                       .WithParam(vm => vm.CatalogId, CatalogId)
                       .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                       .Navigate();
        }

        public void AddBookmark()
        {
            var book = _bookRepository.Get(BookId);
            int lastTokenId;
            string text = _bookTool.GetText(book, book.CurrentTokenID, 20, out lastTokenId);
            var bookmark = _bookmarkRepository.AddBookmark(BookId, new List<BookmarkModel>(), text, ColorHelper.ToInt32(Color.FromArgb(0xFF, 0xE5, 0x14, 0x00)), book.CurrentTokenID, lastTokenId);
            var bookmarkDataModel = _bookmarksController.CreateBookmarkDataModel(bookmark);
            var bookmarkDataModelWithTitle = _bookmarksController.CreateBookmarkDataModel(bookmark, book.Title);
            _thisBookBookmarksViewModel.AddBookmarkAsync(bookmarkDataModel);
            _allBooksBookmarksViewModel.AddBookmarkAsync(bookmarkDataModelWithTitle);
        }
    }
}
