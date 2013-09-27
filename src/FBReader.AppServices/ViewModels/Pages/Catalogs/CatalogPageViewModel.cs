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
using System.ComponentModel;
using Caliburn.Micro;
using FBReader.AppServices.CatalogReaders.Authorization;
using FBReader.AppServices.CatalogReaders.Readers;
using FBReader.AppServices.Controller;
using FBReader.AppServices.Services;
using FBReader.AppServices.Tombstone.StateSaving;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;

namespace FBReader.AppServices.ViewModels.Pages.Catalogs
{
    public class CatalogPageViewModel : CatalogPageViewModelBase, IRestorable
    {
        private readonly INavigationService _navigationService;
        private readonly CatalogController _catalogController;
        private readonly ICatalogAuthorizationFactory _catalogAuthorizationFactory;
        private CatalogModel _catalog;
        private ICatalogAuthorizationService _catalogAuthorizationService;

        public CatalogPageViewModel(
            ICatalogReaderFactory catalogReaderFactory, 
            ICatalogRepository catalogRepository, 
            INotificationsService notificationsService, 
            INavigationService navigationService,
            CatalogController catalogController,
            ICatalogAuthorizationFactory catalogAuthorizationFactory) 
            : base(catalogReaderFactory, catalogRepository, notificationsService, navigationService, catalogController)
        {
            _navigationService = navigationService;
            _catalogController = catalogController;
            _catalogAuthorizationFactory = catalogAuthorizationFactory;
        }

        public bool IsSearchEnabled
        {
            get; 
            set;
        }

        public bool IsAuthorized
        {
            get { return _catalogAuthorizationService != null && _catalogAuthorizationService.IsAuthorized; }
        }

        public bool CanRefresh { get; set; }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            if (CatalogId <= 0)
            {
                throw new ArgumentException("CatalogId is not set in CatalogPageViewModel.");
            }

            _catalog = CatalogRepository.Get(CatalogId);
            if (_catalog == null)
            {
                return;
            }

            _catalogAuthorizationService = _catalogAuthorizationFactory.GetAuthorizationService(_catalog);
            NotifyOfPropertyChange("IsAuthorized");

            CatalogReader = CatalogReaderFactory.Create(_catalog);

            if (SavedInTombstone)
            {
                LoadState(ToString());
            }

            LoadItems();
        }

        public void NavigateToSearch()
        {
            _navigationService.UriFor<CatalogSearchPageViewModel>()
                            .WithParam(vm => vm.CatalogId, CatalogId)
                            .WithParam(vm => vm.PageTitle, string.Format(UIStrings.CatalogSearchPage_Title, _catalogController.GetCatalogTitle(_catalog)).ToUpper())
                            .Navigate();
        }

        public void Logout()
        {
            _catalogAuthorizationService.Deauthorize();
            NotifyOfPropertyChange("IsAuthorized");
            _navigationService.GoBack();
        }

        public void SaveState(string ownerKey = null)
        {
            var restorableCatalog = CatalogReader as IRestorable;
            if (restorableCatalog != null)
            {
                restorableCatalog.SaveState(ownerKey);
            }
        }

        public void LoadState(string ownerKey = null)
        {
            var restorableCatalog = CatalogReader as IRestorable;
            if (restorableCatalog != null)
            {
                restorableCatalog.LoadState(ownerKey);
            }
        }

        public void Update()
        {
            if(IsBusy)
                return;

            var catalogReader = CatalogReader as ITreeCatalogReader;
            if(catalogReader == null)
                return;

            FolderItems = null;
            catalogReader.Refresh();
            LoadItems();
        }
    }
}