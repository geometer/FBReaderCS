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

using System.Threading;
using Caliburn.Micro;
using FBReader.AppServices.DataModels;
using FBReader.AppServices.Services;
using FBReader.AppServices.ViewModels.Pages.Bookmarks;
using FBReader.AppServices.ViewModels.Pages.Settings;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.Settings;
using Screen = Caliburn.Micro.Screen;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class ReadPageViewModel : Screen
    {
        private readonly INavigationService _navigationService;
        private readonly ITileManager _tileManager;
        private readonly IBookRepository _bookRepository;
        private readonly IBusyOverlayManager _busyOverlay;
        private IReadView _readView;
        private string _bookId;
        private bool _navigateForward;
        
        public ReadPageViewModel(
            INavigationService navigationService,
            ITileManager tileManager, 
            IBookRepository bookRepository,
            SharingDataModel sharingDataModel,
            IBusyOverlayManager busyOverlay)
        {
            SharingDataModel = sharingDataModel;
            _navigationService = navigationService;
            _tileManager = tileManager;
            _bookRepository = bookRepository;
            _busyOverlay = busyOverlay;
        }

        public string BookId
        {
            get { return _bookId; }
            set
            {
                _bookId = value;
                //UpdateIsFavouriteBook();
            }
        }

        public bool IsFavouriteBook { get; set; }

        public int TokenOffset { get; set; }

        public SharingDataModel SharingDataModel { get; set; }

        public bool ToLastReadPage { get; set; }

        private BookModel Book { get; set; }


        public int CatalogId { get; set; }

        public string CatalogBookItemKey { get; set; }

        public CatalogBookItemModel CatalogBookItemModel { get; set; }

        public void OpenSettings()
        {
            _navigationService.UriFor<SettingsPivotViewModel>()
                              .Navigate();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Book = _bookRepository.Get(BookId);
            if (Book == null)
            {
                _navigationService.GoBack();
                return;
            }

            if (ToLastReadPage)
                TokenOffset = Book.CurrentTokenID;

            if (TransientStorage.Contains(CatalogBookItemKey))
                CatalogBookItemModel = TransientStorage.Get<CatalogBookItemModel>(CatalogBookItemKey);

            UpdateIsFavouriteBook();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            _readView = view as IReadView;
        }


        protected override void OnActivate()
        {
            base.OnActivate();

            if (_navigateForward)
            {
                _navigateForward = false;
                UpdateIsFavouriteBook();
            }
        }

        public void GoToColorSchemeSelection()
        {
            _navigationService.UriFor<ThemeSettingPageViewModel>()
                              .Navigate();
        }

        public void GoToSettings()
        {
            _navigationService.UriFor<SettingsPivotViewModel>()
                              .Navigate();
        }

        public void ZoomIn()
        {
            var currentFontSize = AppSettings.Default.FontSettings.FontSize;
            var currentIndex = AppSettings.Default.FontSettings.Sizes.IndexOf(currentFontSize);
            if (currentIndex < AppSettings.Default.FontSettings.Sizes.Count - 1)
            {
                AppSettings.Default.FontSettings.FontSize = AppSettings.Default.FontSettings.Sizes[currentIndex + 1];

                RedrawBook();
            }
        }

        public void ZoomOut()
        {
            var currentFontSize = AppSettings.Default.FontSettings.FontSize;
            var currentIndex = AppSettings.Default.FontSettings.Sizes.IndexOf(currentFontSize);
            if (currentIndex > 0)
            {
                AppSettings.Default.FontSettings.FontSize = AppSettings.Default.FontSettings.Sizes[currentIndex - 1];

                RedrawBook();
            }
        }


        public void GoToBookmarks()
        {
            _navigationService.UriFor<BookmarksPivotViewModel>()
                              .WithParam(vm => vm.BookId, BookId)
                              .WithParam(vm => CatalogId, CatalogId)
                              .WithParam(vm => CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                              .Navigate();
        }

        public void GoToTableOfContents()
        {
            _navigationService.UriFor<ContentsPageViewModel>()
                              .WithParam(vm => vm.BookId, BookId)
                              .WithParam(vm => CatalogId, CatalogId)
                              .WithParam(vm => CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                              .Navigate();
        }

        public void GoToSearch()
        {
            _navigationService.UriFor<SearchInBookPageViewModel>()
                              .WithParam(vm => vm.BookId, BookId)
                              .WithParam(vm => CatalogId, CatalogId)
                              .WithParam(vm => CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                              .Navigate();
        }

        public void GoToBookInfo()
        {
            _navigationService.UriFor<BookInfoPageViewModel>()
                              .WithParam(vm => vm.BookId, BookId)
                              .WithParam(vm => vm.CatalogId, CatalogId)
                              .WithParam(vm => CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                              .Navigate();
        }

        public void GoToBrightness()
        {
            _navigationService.UriFor<ScreenBrightnessPageViewModel>().Navigate();
        }

        public void AddRemoveToFavourites()
        {
            if (string.IsNullOrEmpty(_bookId))
            {
                return;
            }
            
            ThreadPool.QueueUserWorkItem(waitCallback =>
                {
                    var book = _bookRepository.Get(_bookId);
                    book.IsFavourite = !book.IsFavourite;
                    _bookRepository.Save(book);
                    IsFavouriteBook = book.IsFavourite;
                });
        }

        private void RedrawBook()
        {
            _readView.Redraw();                        
        }

        private void UpdateIsFavouriteBook()
        {
            if (string.IsNullOrEmpty(_bookId))
            {
                return;
            }
            
            var book = _bookRepository.Get(_bookId);
            if (!book.Trial)
            {
                IsFavouriteBook = book.IsFavourite;
            }
        }

        public bool CanPinToDesktop
        {
            get
            {
                return (!_tileManager.IsPinnedToDesktop(BookId)) && (!Book.Hidden);
            }
        }

        public void PinToDesktop()
        {
            _tileManager.CreateTileForBook(BookId);            
        }

        public async void ShareAsync()
        {
            if (Book == null)
                return;

            if(!await SharingDataModel.ShowMessage())
                return;

            _busyOverlay.Closable = true;
            _busyOverlay.Content = UIStrings.BookInfoPage_Loading;
            _busyOverlay.Closing += Cancel;
            using (await _busyOverlay.Start())
            {
                await SharingDataModel.UploadBook(Book);
            }
        }

        public void ShareText(string text)
        {
            if (Book == null)
                return;

            SharingDataModel.ShareText(Book.Title, text);
        }

        private void Cancel()
        {
            SharingDataModel.Cancel();
            _busyOverlay.Closing -= Cancel;
            _busyOverlay.Stop();
        }
    }
}
