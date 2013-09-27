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
using System.Diagnostics;
using System.Linq;
using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.AppServices.DataModels;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.PhoneServices;
using FBReader.Render.Tools;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class SearchInBookPageViewModel : Screen
    {
        private const int LOAD_COUNT = 20;

        private readonly INavigationService _navigationService;
        private readonly IBookRepository _bookRepository;
        private readonly SearchInBookController _searchController;
        private readonly IBusyIndicatorManager _busyIndicatorManager;
        private readonly BookSearch _bookSearch;
        private BookModel _book;
        private string _searchQuery;

        public SearchInBookPageViewModel(
            INavigationService navigationService,
            IBookRepository bookRepository, 
            SearchInBookController searchController,
            IBusyIndicatorManager busyIndicatorManager,
            BookSearch bookSearch)
        {
            _navigationService = navigationService;
            _bookRepository = bookRepository;
            _searchController = searchController;
            _busyIndicatorManager = busyIndicatorManager;
            _bookSearch = bookSearch;
        }

        public string BookId { get; set; }

        public string Query { get; set; }

        public bool CanLoadMore { get; set; }

        public bool StartSearch { get; set; }

        public IObservableCollection<SearchInBookResultItemDataModel> SearchResult { get; set; }

        public int CatalogId { get; set; }

        public CatalogBookItemModel CatalogBookItemModel { get; set; }

        public string CatalogBookItemKey { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (TransientStorage.Contains(CatalogBookItemKey))
                CatalogBookItemModel = TransientStorage.Get<CatalogBookItemModel>(CatalogBookItemKey);

            _book = _bookRepository.Get(BookId, false);

            DisplayName = string.Format(UIStrings.SearchInBookPage_Title, _book.Title).ToUpper();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            _bookSearch.Init();

            if(StartSearch && !string.IsNullOrWhiteSpace(Query) && SearchResult == null)
            {
                SearchAsync();
                Debug.WriteLine("New search is started");
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            if (_bookSearch != null)
            {
                _bookSearch.Dispose();
            }
        }

        public void OnItemClick(SearchInBookResultItemDataModel item)
        {
            _navigationService
                .UriFor<ReadPageViewModel>()
                .WithParam(vm => vm.BookId, item.BookId)
                .WithParam(vm => vm.TokenOffset, item.TokenId)
                .WithParam(vm => vm.CatalogId, CatalogId)
                .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                .Navigate();
        }

        public async void SearchAsync()
        {
            StartSearch = true;
            SearchResult = null;
            CanLoadMore = false;
            _searchQuery = Query;
            _busyIndicatorManager.Start();

            List<BookSearchResult> items;

            try
            {
                items = await _bookSearch.Search(_book, _searchQuery, LOAD_COUNT);
            }
            catch (SearchInBookInterruptedException)
            {
                //skip search an interrupted exception (it has occurred after FAS), search will be repeated at OnActivate
                return;
            }
            finally
            {
                _busyIndicatorManager.Stop();
            }

            SearchResult = new BindableCollection<SearchInBookResultItemDataModel>(
                items.Select(result => _searchController.ToDataModel(result, _searchQuery, BookId)));

            CanLoadMore = items.Count >= LOAD_COUNT;
        }

        public async void LoadMoreAsync()
        {

            _busyIndicatorManager.Start();

            List<SearchInBookResultItemDataModel> items;
            try
            {
                items = (await _bookSearch.SearchNext(LOAD_COUNT)).Select(result => _searchController.ToDataModel(result, _searchQuery, BookId)).ToList();
            }
            finally
            {
                _busyIndicatorManager.Stop();
            }

            foreach (var item in items)
            {
                SearchResult.Add(item);
            }

            CanLoadMore = items.Count >= LOAD_COUNT;
        }
    }
}
