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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using FBReader.AppServices.CatalogReaders.Readers;
using FBReader.AppServices.Controller;
using FBReader.AppServices.Services;
using FBReader.AppServices.Tombstone.StateSaving;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;

namespace FBReader.AppServices.ViewModels.Pages.Catalogs
{
    public class CatalogSearchPageViewModel : CatalogPageViewModelBase, IRestorable
    {
        public CatalogSearchPageViewModel(
            INotificationsService notificationsService, 
            ICatalogRepository catalogRepository, 
            ICatalogReaderFactory catalogReaderFactory,
            INavigationService navigationService,
            CatalogController catalogController) 
            : base(catalogReaderFactory, catalogRepository, notificationsService, navigationService, catalogController)
        {
        }

        public string PageTitle
        {
            get; 
            set;
        }

        public string SearchQuery { get; set; }

        public bool IsEmpty { get; set; }

        public bool StartSearch { get; set; }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            if (CatalogId <= 0)
            {
                throw new ArgumentException("CatalogId is not set in CatalogPageViewModel.");
            }

            var catalog = CatalogRepository.Get(CatalogId);
            if (catalog == null)
            {
                return;
            }

            CatalogReader = CatalogReaderFactory.Create(catalog);

            if (StartSearch && SavedInTombstone)
            {
                LoadState(ToString());
                if (!string.IsNullOrEmpty(SearchQuery))
                {
                    Search();
                }
            }
        }

        public async void Search()
        {
            if (IsBusy || string.IsNullOrEmpty(SearchQuery))
            {
                return;
            }

            StartSearch = true;

            var searchableCatalog = CatalogReader as ISearchableCatalogReader;
            if (searchableCatalog == null)
            {
                return;
            }

            StartBusiness();

            try
            {
                var result = await searchableCatalog.SearchAsync(SearchQuery);
                //if (!result.Any())
                //{
                    //NotificationsService.ShowAlert(UINotifications.General_AttentionCaption, UINotifications.CatalogSearchPage_EmptySearchResult);
                //}

                FolderItems = new ObservableCollection<CatalogItemModel>(result.OfType<CatalogBookItemModel>());
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

        protected override void StartBusiness()
        {
            base.StartBusiness();

            IsEmpty = false;
        }

        protected override void StopBusiness()
        {
            base.StopBusiness();

            if (!FolderItems.Any())
            {
                IsEmpty = true;
            }
        }
    }
}