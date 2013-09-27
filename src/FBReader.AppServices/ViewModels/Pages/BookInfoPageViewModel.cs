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
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FBReader.AppServices.CatalogReaders.AcquisitionServices;
using FBReader.AppServices.Controller;
using FBReader.AppServices.DataModels;
using FBReader.AppServices.Events;
using FBReader.AppServices.Services;
using FBReader.AppServices.ViewModels.Pages.Catalogs;
using FBReader.AppServices.ViewModels.Base;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.Render.Downloading.Model;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class BookInfoPageViewModel : BusyViewModel, IHandle<BookDownloaded>
    {
        private const string AUTHORIZATION_URL = "http://robot.litres.ru/pages/catalit_authorise/";
        
        private readonly INotificationsService _notificationsService;
        private readonly CatalogController _catalogController;
        private readonly IAcquisitionServiceFactory _acquisitionServiceFactory;
        private readonly INavigationService _navigationService;
        private readonly IBusyOverlayManager _busyOverlay;
        private readonly IBookRepository _bookService;
        private readonly DownloadController _downloadController;
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IDownloadsContainer _container;
        private readonly IBookDownloadsRepository _bookDownloadsRepository;
        private bool _updateBook;

        public BookInfoPageViewModel(
            IBookRepository bookService,
            DownloadController downloadController,
            IEventAggregator eventAggregator, 
            IBookmarkRepository bookmarkRepository,
            INavigationService navigationService,
            SharingDataModel sharingDataModel,
            IBusyOverlayManager busyOverlay, 
            ICatalogRepository catalogRepository, 
            IAcquisitionServiceFactory acquisitionServiceFactory, 
            INotificationsService notificationsService,
            CatalogController catalogController, 
            IDownloadsContainer container, 
            IBookDownloadsRepository bookDownloadsRepository)
        {
            SharingDataModel = sharingDataModel;
            _bookService = bookService;
            _downloadController = downloadController;
            _bookmarkRepository = bookmarkRepository;
            _navigationService = navigationService;
            _busyOverlay = busyOverlay;
            _catalogRepository = catalogRepository;
            _acquisitionServiceFactory = acquisitionServiceFactory;
            _notificationsService = notificationsService;
            _catalogController = catalogController;
            _container = container;
            _bookDownloadsRepository = bookDownloadsRepository;

            eventAggregator.Subscribe(this);

            _busyOverlay.Content = UIStrings.BookInfoPage_Loading;
            _busyOverlay.Closable = true;
        }

        public bool IsFavouriteBook { get; set; }

        public bool CanDownloadTrial { get; set; }

        public string BookId { get; set; }

        public BookModel Book { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public Uri ImageUrl { get; set; }
        public ImageSource CoverSource { get; set; }

        public string CatalogBookItemKey { get; set; }
        public CatalogBookItemModel CatalogBookItemModel { get; set; }

        public bool IsBookFree { get; set; }

        public bool IsBookPaid
        {
            get { return !IsBookFree; }
        }

        public bool HasTrial { get; set; }

        public int ApplicationBarIndex { get; set; }

        public int CatalogId { get; set; }

        public SharingDataModel SharingDataModel { get; set; }

        public bool StartDownload { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (TransientStorage.Contains(CatalogBookItemKey))
                CatalogBookItemModel = TransientStorage.Get<CatalogBookItemModel>(CatalogBookItemKey);

            if (!string.IsNullOrEmpty(BookId))
            {
                InitializeBook();
            }
            else
            {
                if (CatalogBookItemModel != null)
                {
                    if(CheckIsDownloaded())
                        return;

                    if (CheckForExistingTrial())
                    {
                        InitializeBook();
                    }
                    else
                    {
                        Title = !string.IsNullOrEmpty(CatalogBookItemModel.Title) ? CatalogBookItemModel.Title : null;
                        Author = CatalogBookItemModel.Author;
                        Description = CatalogBookItemModel.Description;
                        IsBookFree = CatalogBookItemModel.AcquisitionLink == null;
                        ApplicationBarIndex = 1;
                    }
                }
                ShowOverlayIfBookDownloading();
            }
        }

        private async void ShowOverlayIfBookDownloading()
        {
            if (CheckIsDownloading())
            {
                _busyOverlay.Closing -= GoBack;
                _busyOverlay.Closing += GoBack;
                await _busyOverlay.Start(false);
                IsBusy = true;
            }
            else if (StartDownload)
            {
                StartDownload = false;
                DownloadBook(true);
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (_updateBook)
            {
                InitializeBook();
                StopOverlay();
                _updateBook = false;
            }
            else
            {
                IsBusy = false;
            }
        }

        private void InitializeBook()
        {
            Book = _bookService.Get(BookId, false);

            Title = Book.Title;
            Author = Book.Author;
            Description = Book.Description;
            Type = Book.Type;

            IsFavouriteBook = Book.IsFavourite;
            ApplicationBarIndex = Book.Trial ? 2 : 0;
            
            if (IsBusy)
            {
                IsBusy = false;
            }

            try
            {
                var culture = new CultureInfo(Book.Language);
                Language = culture.NativeName;
            }
            catch (Exception)
            {
            }

        }

        private bool CheckForExistingTrial()
        {
            if (CatalogBookItemModel.TrialLink != null)
            {
                var book = _bookService.GetTrials().SingleOrDefault(b => b.Url.Equals(CatalogBookItemModel.TrialLink.Url));
                if (book != null)
                {
                    BookId = book.BookID;
                    return true;
                }

                CanDownloadTrial = true;
                HasTrial = true;
            }

            return false;
        }

        private bool CheckIsDownloaded()
        {
            if (CatalogBookItemModel == null)
                return false;


            var books = _bookService.GetAll();
            foreach (var book in books)
            {
                if (CatalogBookItemModel.Id == book.CatalogItemId)
                {
                    BookId = book.BookID;
                    InitializeBook();
                    NotifyOfPropertyChange("DataContext");
                    return true;
                }
            }
            return false;
        }

        private bool CheckIsDownloading()
        {
            if(CatalogBookItemModel == null)
                return false;

            var bookDownloadModel = _bookDownloadsRepository.GetItems().FirstOrDefault(i => i.CatalogItemId == CatalogBookItemModel.Id);
            if (bookDownloadModel == null)
            {
                return false;
            }
            
            int index = _container.GetDataModelIndex(new DownloadItemDataModel(bookDownloadModel));
            if (index > -1)
            {
                var bookDownloadViewModel = _container.Items[index];
                return bookDownloadViewModel.Status != DownloadStatus.Error;
            }
            return false;
        }

        public async void DownloadBook(bool fullBook)
        {
            _busyOverlay.Closable = true;
            _busyOverlay.Closing += GoBack;
            await _busyOverlay.Start(false);
            IsBusy = true;

            if (!_downloadController.DownloadBook(CatalogBookItemModel, CatalogId, fullBook))
            {
                _busyOverlay.Stop();
                _busyOverlay.Closing -= GoBack;
            }
        }

        private void GoBack()
        {
            _navigationService.GoBack();
            StopOverlay();
        }

        public void Handle(BookDownloaded message)
        {
            if (message.Book.Status == DownloadStatus.Error)
            {
                StopOverlay();
                IsBusy = false;
                return;
            }

            if (message.Book.CatalogItemId != CatalogBookItemModel.Id)
            {
                return;
            }

            StopOverlay();

            CanDownloadTrial = false;
            BookId = message.Book.BookID.ToString();
            InitializeBook();
            NotifyOfPropertyChange("DataContext");
            

            IsBusy = false;
        }

        public void Edit()
        {
            _updateBook = true;
            
            _navigationService.UriFor<EditBookPageViewModel>()
                              .WithParam(vm => vm.BookId, BookId)
                              .WithParam(vm => vm.PageTitle, Title)
                              .WithParam(vm => vm.Title, Title)
                              .Navigate();
        }

        public void AddRemoveToFavourites()
        {
            var book = _bookService.Get(BookId);
            book.IsFavourite = !book.IsFavourite;
            _bookService.Save(book);
            IsFavouriteBook = book.IsFavourite;
        }

        public async void ShareAsync()
        {

            if(!await SharingDataModel.ShowMessage())
                return;

            _busyOverlay.Closable = true;
            _busyOverlay.Closing += Cancel;

            IsBusy = true;
            using (await _busyOverlay.Start(false))
            {
                await SharingDataModel.UploadBook(Book);
                IsBusy = false;
            }
        }

        private void Cancel()
        {
            SharingDataModel.Cancel();
            StopOverlay();
        }

        public void Read()
        {
            if(string.IsNullOrEmpty(BookId) || Book == null)
                return;

            _navigationService
                .UriFor<ReadPageViewModel>()
                .WithParam(vm => vm.BookId, BookId)
                .WithParam(vm => vm.ToLastReadPage, true)
                .WithParam(vm => vm.CatalogId, CatalogId)
                .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                .Navigate();
        }

        private void StopOverlay()
        {
            _busyOverlay.Stop();
            _busyOverlay.Closing -= GoBack;
            _busyOverlay.Closing -= Cancel;
        }

        public void DeleteTrial()
        {
            _bookService.Remove(Book.BookID);
            _bookmarkRepository.DeleteBookmarks(Book.BookID);
            IsBusy = false;
            CanDownloadTrial = true;
            IsBookFree = false;
            ApplicationBarIndex = 1;

            ClearBackStack();
        }

        private void ClearBackStack()
        {
            if (RemoveFromBackStack("ReadPage.xaml"))
                RemoveFromBackStack("BookInfoPage.xaml");
        }

        private bool RemoveFromBackStack(string pageName)
        {
            var lastEntry = _navigationService.BackStack.FirstOrDefault();
            if (lastEntry == null)
                return false;

            if (lastEntry.Source.ToString().Contains(pageName))
            {
                _navigationService.RemoveBackEntry();
                return true;
            }
            return false;
        }

        public async void Buy()
        {
            IsBusy = true;

            var catalog = _catalogRepository.Get(CatalogId);

            if (catalog.Type != CatalogType.Litres)
            {
                _navigationService.UriFor<WebBrowserPageViewModel>()
                                  .WithParam(vm => vm.WebBrowserUrl, CatalogBookItemModel.AcquisitionLink.Url)
                                  .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                                  .WithParam(vm => vm.CatalogId, CatalogId)
                                  .Navigate();
                return;
            }

            if (string.IsNullOrEmpty(catalog.AuthorizationString))
            {
                _navigationService.UriFor<AuthorizationPageViewModel>()
                                  .WithParam(vm => vm.CatalogTitle, _catalogController.GetCatalogTitle(catalog).ToLower())
                                  .WithParam(vm => vm.Path, AUTHORIZATION_URL)
                                  .WithParam(vm => vm.CatalogId, catalog.Id)
                                  .WithParam(vm => vm.CatalogType, catalog.Type)
                                  .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                                  .Navigate();
                return;
            }
            
            await PerformLitresAcqusition(catalog);
        }

        private async Task PerformLitresAcqusition(CatalogModel catalog)
        {
            try
            {
                _busyOverlay.Closable = true;
                _busyOverlay.Closing += GoBack;
                await _busyOverlay.Start(false);

                // only for litres
                var buyService = _acquisitionServiceFactory.GetAcquisitionService(catalog.Type);
                var downloadLink = await buyService.BuyBook(CatalogBookItemModel, catalog.AuthorizationString);

                CatalogBookItemModel.Links = new List<BookDownloadLinkModel>
                    {
                        new BookDownloadLinkModel {Type = ".fb2.zip", Url = downloadLink}
                    };

                if (!_downloadController.DownloadBook(CatalogBookItemModel, CatalogId))
                {
                    _busyOverlay.Stop();
                    _busyOverlay.Closing -= GoBack;
                    IsBusy = false;
                }
            }
            catch (CatalogAuthorizationException exception)
            {
                _navigationService.UriFor<AuthorizationPageViewModel>()
                                  .WithParam(vm => vm.CatalogTitle, _catalogController.GetCatalogTitle(catalog).ToLower())
                                  .WithParam(vm => vm.Path, exception.Path)
                                  .WithParam(vm => vm.CatalogId, catalog.Id)
                                  .WithParam(vm => vm.CatalogType, catalog.Type)
                                  .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                                  .Navigate();
            }
            catch (ReadCatalogException)
            {
                ShowReadCatalogError();
                StopOverlay();
                IsBusy = false;
            }
            catch (CatalogNotEnoughMoneyException exception)
            {
                StopOverlay();
                IsBusy = false;
                _navigationService.UriFor<WebBrowserPageViewModel>()
                                  .WithParam(vm => vm.WebBrowserUrl, exception.PayMoneyUrl)
                                  .Navigate();
            }
            catch (CatalogBookAlreadyBoughtException)
            {
                _notificationsService.ShowMessage(UINotifications.General_AttentionCaption,
                                                  UINotifications.BuyBook_AlradyBoughtMessage);
                StopOverlay();
                IsBusy = false;
            }
        }

        private void ShowReadCatalogError()
        {
            _notificationsService.ShowAlert(UINotifications.General_ErrorCaption, UINotifications.Read_Catalog_Error_Message);
        }
    }
}