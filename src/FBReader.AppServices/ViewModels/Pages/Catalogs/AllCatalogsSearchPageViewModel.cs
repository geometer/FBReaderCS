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
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using FBReader.AppServices.CatalogReaders;
using FBReader.AppServices.CatalogReaders.Readers;
using FBReader.AppServices.Services;
using FBReader.AppServices.Tombstone.StateSaving;
using FBReader.AppServices.ViewModels.Base;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;

namespace FBReader.AppServices.ViewModels.Pages.Catalogs
{
    public class AllCatalogsSearchPageViewModel : BusyViewModel, IRestorable
    {
        private readonly INavigationService _navigationService;
        private readonly INotificationsService _notificationsService;
        private readonly ICatalogReaderFactory _catalogReaderFactory;
        private readonly ICatalogRepository _catalogRepository;
        private readonly List<ICatalogReader> _catalogReaders = new List<ICatalogReader>();
        private readonly List<int> _ignoredCatalogsIds = new List<int>();
        private readonly Dictionary<int, List<int>> _itemsMap = new Dictionary<int, List<int>>();

        public AllCatalogsSearchPageViewModel(INotificationsService notificationsService, ICatalogReaderFactory catalogReaderFactory, ICatalogRepository catalogRepository, 
            INavigationService navigationService)
        {
            Items = new ObservableCollection<CatalogItemModel>();
            _notificationsService = notificationsService;
            _catalogReaderFactory = catalogReaderFactory;
            _catalogRepository = catalogRepository;
            _navigationService = navigationService;
        }

        public string PageTitle
        {
            get
            {
                return UIStrings.AllCatalogsSearchPageViewModel_Title.ToUpper();
            }
        }

        public bool SavedInTombstone
        {
            get; 
            set;
        }

        public bool IsEmpty { get; set; }

        public string SearchQuery { get; set; }

        public bool CanLoadMore { get; set; }

        public ObservableCollection<CatalogItemModel> Items { get; set; }

        public bool StartSearch { get; set; }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            var catalogs = _catalogRepository.GetAll().Where(c => !c.AccessDenied);

            foreach (var catalogModel in catalogs)
            {
                var catalogReader = _catalogReaderFactory.Create(catalogModel);
                _catalogReaders.Add(catalogReader);
            }

            if (StartSearch && SavedInTombstone)
            {
                Search();
            }
        }

        public void Search()
        {
            if (IsBusy || string.IsNullOrEmpty(SearchQuery))
            {
                return;
            }
            StartSearch = true;
            Items.Clear();
            foreach (var catalogReader in _catalogReaders)
            {
                LoadCatalogItems(catalogReader as ISearchableCatalogReader);
            }
        }

        private async void LoadCatalogItems(ISearchableCatalogReader catalogReader)
        {
            if (catalogReader == null)
            {
                return;
            }
            
            StartBusiness();

            try
            {
                var result = await catalogReader.SearchAsync(SearchQuery);
                if (result != null)
                {
                    var books = result.OfType<CatalogBookItemModel>();
                    if (!books.Any())
                    {
                        _ignoredCatalogsIds.Add(catalogReader.CatalogId);
                        return;
                    }
                    AddItems(books, catalogReader.CatalogId);
                }
            }
            catch (ReadCatalogException)
            {
                //ShowReadCatalogError();
                // ignore exception. Show message about empty search result instead.
            }
            catch (TaskCanceledException)
            {
                //skip taks cancelled exception
            }
            catch (Exception)
            {
                // ignore exception. Show message about empty search result instead.
            }
            finally
            {
                StopBusiness();
            }
        }

        public async void LoadNextPage()
        {
            var catalog = _catalogReaders.OfType<ITreeCatalogReader>()
                                         .Where(c => _ignoredCatalogsIds.All(ic => ic != c.CatalogId))
                                         .First(c => c.CanReadNextPage);
            if (catalog == null)
            {
                return;
            }

            StartBusiness();

            try
            {
                var items = await catalog.ReadNextPageAsync();
                var books = items.OfType<CatalogBookItemModel>();
                if (!books.Any())
                {
                    _ignoredCatalogsIds.Add(catalog.CatalogId);
                    return;
                }

                AddItems(books, catalog.CatalogId);
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

        public void NavigateToItem(CatalogBookItemModel model)
        {
            _navigationService
                    .UriFor<BookInfoPageViewModel>()
                    .WithParam(vm => vm.Title, model.Title.ToUpper())
                    .WithParam(vm => vm.Description, model.Description)
                    .WithParam(vm => vm.ImageUrl, model.ImageUrl)
                    .WithParam(vm => vm.CatalogId, GetCatalogId(model.GetHashCode()))
                    .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(model))
                    .Navigate();
        }

        protected override void StartBusiness()
        {
            base.StartBusiness();
            CanLoadMore = false;
            IsEmpty = false;
        }

        protected override void StopBusiness()
        {
            base.StopBusiness();

            if (IsBusy)
            {
                return;
            }

            UpdateCanLoadMore();
            if (Items.Count == 0)
            {
                //_notificationsService.ShowAlert(UINotifications.General_AttentionCaption, UINotifications.CatalogSearchPage_EmptySearchResult);
                IsEmpty = true;
            }
        }

        public void SaveState(string ownerKey = null)
        {
        }

        public void LoadState(string ownerKey = null)
        {
        }

        private void UpdateCanLoadMore()
        {
            CanLoadMore = _catalogReaders.OfType<ITreeCatalogReader>()
                                         .Where(c => _ignoredCatalogsIds.All(ci => ci != c.CatalogId))
                                         .Any(c => c.CanReadNextPage);
        }

        private void AddItems(IEnumerable<CatalogBookItemModel> items, int catalogId)
        {
            foreach (var item in items)
            {
                Items.Add(item);
            }

            List<int> bookIds;
            if (_itemsMap.TryGetValue(catalogId, out bookIds))
            {
                bookIds.AddRange(items.Select(i => i.GetHashCode()));
                _itemsMap.Remove(catalogId);
                _itemsMap.Add(catalogId, bookIds);
            }
            else
            {
                bookIds = new List<int>(items.Select(i => i.GetHashCode()));
                _itemsMap.Add(catalogId, bookIds);
            }
        }

        protected void ShowReadCatalogError()
        {
            _notificationsService.ShowAlert(UINotifications.General_ErrorCaption, UINotifications.Read_Catalog_Error_Message);
        }

        private int GetCatalogId(int hashCode)
        {
            return (from keyValuePair in _itemsMap where keyValuePair.Value.Contains(hashCode) select keyValuePair.Key).FirstOrDefault();
        }
    }
}