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

using Caliburn.Micro;
using FBReader.AppServices.ViewModels.Pages.Catalogs;
using FBReader.AppServices.ViewModels.Pages.MainHub;
using FBReader.Localization;
using FBReader.AppServices.ViewModels.Pages.Settings;
using FBReader.Settings;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class MainPageViewModel: Conductor<Screen>.Collection.OneActive
    {
        private int _appBarSelectedIndex;
        private readonly INavigationService _navigationService;
        private readonly DataBaseInitializer _dbInitialzier;

        public MainPageViewModel(INavigationService navigationService, UserLibraryViewModel userLibaryViewModel, CatalogsViewModel catalogsViewModel, DataBaseInitializer dbInitialzier)
        {
            _navigationService = navigationService;
            _dbInitialzier = dbInitialzier;
            UserLibraryViewModel = userLibaryViewModel;
            CatalogsViewModel = catalogsViewModel;
        }

        public UserLibraryViewModel UserLibraryViewModel
        {
            get;
            set;
        }

        public CatalogsViewModel CatalogsViewModel
        {
            get;
            set;
        }

        public int AppBarSelectedIndex
        {
            get
            {
                return _appBarSelectedIndex;
            }

            set
            {
                _appBarSelectedIndex = value;
                NotifyOfPropertyChange(() => AppBarSelectedIndex);
            }
        }

        protected async override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            if (AppSettings.Default.IsFirstTimeRunning)
            {
                await _dbInitialzier.ExecuteAsync();
                AppSettings.Default.IsFirstTimeRunning = false;
            }
            
            CatalogsViewModel.LoadData();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (AppBarSelectedIndex == 1)
            {
                CatalogsViewModel.LoadData();
            }

            UserLibraryViewModel.LoadData();
        }

        public void AddCatalog()
        {
            _navigationService.UriFor<AddCatalogPageViewModel>().Navigate();
        }

        public void NavigateToSearchCatalogs()
        {
            _navigationService.UriFor<AllCatalogsSearchPageViewModel>().Navigate();
        }

        public void NavigateToDownloads()
        {
            _navigationService.UriFor<DownloadListPageViewModel>().Navigate();
        }

        public void NavigateToSettings()
        {
            _navigationService.UriFor<SettingsPivotViewModel>().Navigate();
        }

        public void NavigateToFavourites()
        {
            _navigationService.UriFor<UserLibraryPageViewModel>()
                              .WithParam(vm => vm.ShowOnlyFavourites, true)
                              .WithParam(vm => vm.PageTitle, UIStrings.UserLibraryPage_FavsTitle)
                              .Navigate();
        }

        public void NavigateToSearch()
        {
            _navigationService.UriFor<SearchPageViewModel>().Navigate();
        }
    }
}