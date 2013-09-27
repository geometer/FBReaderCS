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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Caliburn.Micro;
using FBReader.AppServices.CatalogReaders.Readers;
using FBReader.AppServices.Controller;
using FBReader.AppServices.Services;
using FBReader.AppServices.ViewModels.Base;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;

namespace FBReader.AppServices.ViewModels.Pages.Catalogs
{
    public abstract class CatalogPageViewModelBase : BusyViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly CatalogController _catalogController;
        protected readonly INotificationsService NotificationsService;
        protected readonly ICatalogReaderFactory CatalogReaderFactory;
        protected readonly ICatalogRepository CatalogRepository;
        protected ICatalogReader CatalogReader;

        protected CatalogPageViewModelBase(
            ICatalogReaderFactory catalogReaderFactory, 
            ICatalogRepository catalogRepository, 
            INotificationsService notificationsService,
            INavigationService navigationService,
            CatalogController catalogController)
        {
            FolderItems = new ObservableCollection<CatalogItemModel>();
            CatalogReaderFactory = catalogReaderFactory;
            CatalogRepository = catalogRepository;
            NotificationsService = notificationsService;
            _navigationService = navigationService;
            _catalogController = catalogController;
        }

        public event EventHandler<bool> CatalogNavigated;

        public bool SavedInTombstone
        {
            get; 
            set;
        }

        public int CatalogId
        {
            get;
            set;
        }

        public bool CanLoadMore { get; set; }

        public ObservableCollection<CatalogItemModel> FolderItems { get; set; }

        public bool CanGoToPreviousLevel
        {
            get
            {
                var treeReader = CatalogReader as ITreeCatalogReader;
                return treeReader != null && treeReader.CanGoBack;
            }
        }

        

        protected override void StartBusiness()
        {
            base.StartBusiness();
            CanLoadMore = false;
        }

        protected override void StopBusiness()
        {
            base.StopBusiness();
            UpdateCanLoadMore();
        }

        public async void GoBack()
        {
            StartBusiness();

            var treeCatalog = CatalogReader as ITreeCatalogReader;
            if (treeCatalog == null)
            {
                StopBusiness();
                return;
            }

            treeCatalog.GoBack();
            IEnumerable<CatalogItemModel> items;

            try
            {
                items = await CatalogReader.ReadAsync();
            }
            catch (ReadCatalogException)
            {
                ShowReadCatalogError();
                return;
            }
            catch (TaskCanceledException)
            {
                //skip taks cancelled exception
                return;
            }
            finally
            {
                StopBusiness();
            }

            FolderItems = new ObservableCollection<CatalogItemModel>(items);
            OnCatalogNavigated(false);
        }

        public void NavigateToItem(CatalogItemModel model)
        {
            if (IsBusy)
            {
                return;
            }

            // hack for improving performance. If we skip this thing, app will take a lot of time to generate exception and catch it.
            if (CheckForUnauthorizedLitresAccess(model))
            {
                NavigateToAuthorizationPage("http://robot.litres.ru/pages/catalit_authorise/");
                return;
            }
            
            var treeReader = CatalogReader as ITreeCatalogReader;
            if (treeReader == null)
                return;


            if (model is CatalogBookItemModel)
            {
                var bookItemModel = model as CatalogBookItemModel;

                _navigationService
                    .UriFor<BookInfoPageViewModel>()
                    .WithParam(vm => vm.Title, bookItemModel.Title.ToUpper())
                    .WithParam(vm => vm.Description, bookItemModel.Description)
                    .WithParam(vm => vm.ImageUrl, bookItemModel.ImageUrl)
                    .WithParam(vm => vm.CatalogId, treeReader.CatalogId)
                    .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(bookItemModel))
                    .Navigate();

                return;
            }

            if (!string.IsNullOrEmpty(model.OpdsUrl))
            {
                treeReader.GoTo(model);
                LoadItems();
            }
            else if (!string.IsNullOrEmpty(model.HtmlUrl))
            {
                NavigateToWebBrowser(model);
            }
        }



        private void NavigateToWebBrowser(CatalogItemModel model)
        {
            // another hack for improving performance.
            if (model is LitresTopupCatalogItemModel)
            {
                var catalog = CatalogRepository.Get(CatalogId);
                model.HtmlUrl = string.Format(model.HtmlUrl, EncryptService.Decrypt(catalog.AuthorizationString));
            }

            _navigationService.UriFor<WebBrowserPageViewModel>()
                                  .WithParam(vm => vm.WebBrowserUrl, model.HtmlUrl)
                                  .WithParam(vm => vm.CatalogId, CatalogId)
                                  .WithParam(vm => vm.Title, model.Title)
                                  .Navigate();
        }

        private bool CheckForUnauthorizedLitresAccess(CatalogItemModel model)
        {
            if (model is LitresBookshelfCatalogItemModel || model is LitresTopupCatalogItemModel)
            {
                var catalog = CatalogRepository.Get(CatalogId);
                if (string.IsNullOrEmpty(catalog.AuthorizationString))
                {
                    return true;
                }
            }

            return false;
        }

        protected async void LoadItems()
        {
            StartBusiness();

            try
            {
                OnCatalogNavigated(true);
                var result = await CatalogReader.ReadAsync();
                FolderItems = new ObservableCollection<CatalogItemModel>(result);
                UpdateCanLoadMore();
            }
            catch (ReadCatalogException)
            {
                ShowReadCatalogError();
            }
            catch (TaskCanceledException)
            {
                //skip taks cancelled exception
            }
            catch (WrongCatalogFormatException exception)
            {
                // open browser
                _navigationService.UriFor<WebBrowserPageViewModel>()
                                  .WithParam(vm => vm.WebBrowserUrl, exception.WrongFormatUrl)
                                  .Navigate();
            }
            catch (CatalogAuthorizationException exception)
            {
                HandleAuthorizationException(exception);
            }
            finally
            {
                StopBusiness();
            }
        }

        public async void LoadNextPage()
        {
            var multiPagesCatalogReader = CatalogReader as ITreeCatalogReader;
            if (multiPagesCatalogReader == null)
            {
                return;
            }

            StartBusiness();

            try
            {
                var items = await multiPagesCatalogReader.ReadNextPageAsync();
                AddItems(items);
                UpdateCanLoadMore();
            }
            catch (ReadCatalogException)
            {
                ShowReadCatalogError();
            }
            catch (TaskCanceledException)
            {
                //skip taks cancelled exception
            }
            finally
            {
                StopBusiness();
            }
        }

        protected void UpdateCanLoadMore()
        {
            var multiPageReader = CatalogReader as ITreeCatalogReader;
            if (multiPageReader != null)
            {
                CanLoadMore = multiPageReader.CanReadNextPage && FolderItems.Count != 0;
            }
        }

        private void HandleAuthorizationException(CatalogAuthorizationException exception)
        {
            switch (exception.CatalogType)
            {
                case CatalogType.OPDS:
                case CatalogType.Litres:
                    NavigateToAuthorizationPage(exception.Path);
                    break;
                case CatalogType.SkyDrive:
                    _navigationService.GoBack();
                    break;
            }
        }

        private void NavigateToAuthorizationPage(string path)
        {
            var treeReader = CatalogReader as ITreeCatalogReader;
            if (treeReader == null)
            {
                return;
            }

            try
            {
                treeReader.GoBack();
            }
            catch (ReadCatalogException)
            {
            }

            var catalog = CatalogRepository.Get(treeReader.CatalogId);

            _navigationService.UriFor<AuthorizationPageViewModel>()
                              .WithParam(vm => vm.CatalogTitle, _catalogController.GetCatalogTitle(catalog).ToLower())
                              .WithParam(vm => vm.Path, path)
                              .WithParam(vm => vm.CatalogId, catalog.Id)
                              .WithParam(vm => vm.CatalogType, catalog.Type)
                              .Navigate();
        }

        private void AddItems(IEnumerable<CatalogItemModel> items)
        {
            foreach (var item in items)
            {
                FolderItems.Add(item);
            }
        }

        private void OnCatalogNavigated(bool e)
        {
            var handler = CatalogNavigated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void ShowReadCatalogError()
        {
            NotificationsService.ShowAlert(UINotifications.General_ErrorCaption, UINotifications.Read_Catalog_Error_Message);
        }
    }
}