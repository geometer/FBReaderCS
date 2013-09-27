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
using System.Linq;
using Caliburn.Micro;
using FBReader.AppServices.Services;
using FBReader.AppServices.ViewModels.Base;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.WebClient;

namespace FBReader.AppServices.ViewModels.Pages.Catalogs
{
    public class AddCatalogPageViewModel : BusyViewModel
    {
        private const string HTTP_PREFIX = "http://";
        private const string WWW_PREFIX = "www.";
        private readonly INavigationService _navigationService;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IWebDataGateway _dataGateway;
        private readonly INotificationsService _notificationsService;
        private readonly IErrorHandler _errorHandler;
        private string _catalogUrl;

        public AddCatalogPageViewModel(IWebDataGateway dataGateway, INotificationsService notificationsService, IErrorHandler errorHandler, 
            INavigationService navigationService, ICatalogRepository catalogRepository)
        {
            _dataGateway = dataGateway;
            _notificationsService = notificationsService;
            _errorHandler = errorHandler;
            _navigationService = navigationService;
            _catalogRepository = catalogRepository;

            CatalogUrl = HTTP_PREFIX;
        }

        public string CatalogUrl
        {
            get { return _catalogUrl; }
            set
            {
                _catalogUrl = value;
                NotifyOfPropertyChange(() => CatalogUrl);
                NotifyOfPropertyChange(() => CanAddCatalog);
            }
        }

        public bool CanAddCatalog
        {
            get
            {
                return IsNotBusy && !string.IsNullOrEmpty(_catalogUrl);
            }
        }

        public async void AddCatalog()
        {
            if (!ValidateUrl())
            {
                return;
            }

            try
            {
                StartBusiness();

                var catalogModel = await _dataGateway.GetCatalogInfo(CatalogUrl);
                if (catalogModel == null)
                {
                    return;
                }

                try
                {
                    if (string.IsNullOrEmpty(catalogModel.SearchUrl))
                    {
                        if (!string.IsNullOrEmpty(catalogModel.OpenSearchDescriptionUrl))
                        {
                            var openSearchDescription = await _dataGateway.GetOpenSearchDescriptionModel(catalogModel.OpenSearchDescriptionUrl, catalogModel.Url);
                            if (openSearchDescription != null && !string.IsNullOrEmpty(openSearchDescription.SearchTemplateUrl))
                            {
                                catalogModel.SearchUrl = openSearchDescription.SearchTemplateUrl;
                            }
                        }
                    }
                }
                catch
                {
                    catalogModel.SearchUrl = string.Empty;
                }
                    
                var catalogs = _catalogRepository.GetAll();
                if (catalogs.Any(c => !string.IsNullOrEmpty(c.Url) && c.Url.Equals(_catalogUrl)))
                {
                    _notificationsService.ShowAlert(UINotifications.General_AttentionCaption, UINotifications.AddCatalogPage_CatalogAlreadyExists);
                    return;
                }
                    
                catalogModel.Type = CatalogType.OPDS;
                _catalogRepository.Add(catalogModel);
                    
                _notificationsService.ShowAlert(UINotifications.General_AttentionCaption, UINotifications.AddCatalogPage_CatalogAddedSuccess);
                _navigationService.GoBack();
            }
            catch (DataCorruptedException)
            {
                AddCatalog();
            }
            catch (Exception exception)
            {
                _errorHandler.Handle(exception);
            }
            finally
            {
                StopBusiness();
            }
        }

        protected override void StartBusiness()
        {
            base.StartBusiness();

            NotifyOfPropertyChange(() => CanAddCatalog);
        }

        protected override void StopBusiness()
        {
            base.StopBusiness();

            NotifyOfPropertyChange(() => CanAddCatalog);
        }

        private bool ValidateUrl()
        {
            if (!CatalogUrl.StartsWith(HTTP_PREFIX) && !CatalogUrl.StartsWith(WWW_PREFIX))
            {
                _notificationsService.ShowAlert(UINotifications.General_AttentionCaption, UINotifications.AddCatalogPage_InvalidUrlMessage);
                return false;
            }

            if (CatalogUrl.StartsWith(WWW_PREFIX))
            {
                _catalogUrl = string.Concat(HTTP_PREFIX, _catalogUrl);
            }

            Uri uri;
            if (!Uri.TryCreate(CatalogUrl, UriKind.RelativeOrAbsolute, out uri))
            {
                _notificationsService.ShowAlert(UINotifications.General_AttentionCaption, UINotifications.AddCatalogPage_InvalidUrlMessage);
                return false;
            }
            
            return true;
        }
    }
}