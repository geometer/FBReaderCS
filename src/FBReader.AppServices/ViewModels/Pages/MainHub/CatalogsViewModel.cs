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
using System.Threading;
using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.AppServices.DataModels;
using FBReader.AppServices.Events;
using FBReader.AppServices.ViewModels.Pages.Catalogs;
using FBReader.Common;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.PhoneServices;
using Screen = Caliburn.Micro.Screen;

namespace FBReader.AppServices.ViewModels.Pages.MainHub
{
    public class CatalogsViewModel: Screen, IHandle<UILanguageChanged>
    {
        private readonly INavigationService _navigationService;
        private readonly CatalogController _catalogController;
        private readonly ISdCardStorage _sdCardStorage;
        private readonly ICatalogRepository _catalogRepository;
        private bool _isEmpty;

        public CatalogsViewModel(
            ICatalogRepository catalogRepository, 
            INavigationService navigationService, 
            IEventAggregator eventAggregator,
            CatalogController catalogController,
            ISdCardStorage sdCardStorage)
        {
            _catalogRepository = catalogRepository;
            _navigationService = navigationService;
            _catalogController = catalogController;
            _sdCardStorage = sdCardStorage;

            DisplayName = UIStrings.CatalogsView_Title;
            
            eventAggregator.Subscribe(this);
        }

        public bool IsEmpty
        {
            get
            {
                return _isEmpty;
            }
            set
            {
                _isEmpty = value;
                NotifyOfPropertyChange(() => IsEmpty);
            }
        }

        public IObservableCollection<CatalogDataModel> Catalogs { get; set; }

        public void LoadData()
        {
            ThreadPool.QueueUserWorkItem(async callback =>
                                             {
                    bool isSdCardAvaliable = await _sdCardStorage.GetIsAvailableAsync();
                    Catalogs = new BindableCollection<CatalogDataModel>(_catalogRepository
                        .GetAll()
                        .Where(c => c.Type != CatalogType.SDCard || (c.Type == CatalogType.SDCard && isSdCardAvaliable))
                        .Where(c => c.Type != CatalogType.StorageFolder)
                        .Select(_catalogController.ToCatalogDataModel));
                });
        }
        public void NavigateToCatalog(CatalogDataModel model)
        {
            if (model == null)
            {
                return;
            }
            var catalog = model.Catalog;

            _navigationService.UriFor<CatalogPageViewModel>()
                              .WithParam(vm => vm.CatalogId, catalog.Id)
                              .WithParam(vm => vm.DisplayName, model.Title.ToUpper())
                              .WithParam(vm => vm.IsSearchEnabled, 
                                                    !string.IsNullOrEmpty(catalog.SearchUrl) 
                                                    || catalog.Type == CatalogType.SDCard
                                                    || catalog.Type == CatalogType.SkyDrive)
                              .WithParam(vm => vm.CanRefresh, catalog.Type != CatalogType.SDCard)
                              .Navigate();
        }

        public void Handle(UILanguageChanged message)
        {
            DisplayName = UIStrings.CatalogsView_Title;
            LoadData();
        }

        public void RemoveCatalog(CatalogDataModel catalogModel)
        {
            _catalogRepository.Remove(catalogModel.Catalog);
            Catalogs.Remove(catalogModel);
        }

        public bool CanRemoveCatalog(CatalogDataModel dataModel)
        {
            var catalog = dataModel.Catalog;
            if (catalog.Type != CatalogType.OPDS)
                return false;

            if (catalog.IconLocalPath != null && catalog.IconLocalPath.StartsWith("/Resources/Icons/"))
                return false;

            return true;
        }
    }
}