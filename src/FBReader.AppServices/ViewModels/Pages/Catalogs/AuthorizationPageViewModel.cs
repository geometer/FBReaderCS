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
using Caliburn.Micro;
using FBReader.AppServices.CatalogReaders.Authorization;
using FBReader.AppServices.Controller;
using FBReader.AppServices.Services;
using FBReader.AppServices.ViewModels.Base;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;

namespace FBReader.AppServices.ViewModels.Pages.Catalogs
{
    public class AuthorizationPageViewModel : BusyViewModel
    {
        private readonly ICatalogAuthorizationFactory _authorizationFactory;
        private readonly CatalogController _catalogController;
        private readonly ICatalogRepository _catalogRepository;
        private readonly INavigationService _navigationService;
        private readonly INotificationsService _notificationsService;
        private readonly IErrorHandler _errorHandler;
        private string _userName;
        private string _password;

        public AuthorizationPageViewModel(
            ICatalogRepository catalogRepository, 
            INavigationService navigationService, 
            INotificationsService notificationsService, 
            IErrorHandler errorHandler, 
            ICatalogAuthorizationFactory authorizationFactory,
            CatalogController catalogController)
        {
            _catalogRepository = catalogRepository;
            _navigationService = navigationService;
            _notificationsService = notificationsService;
            _errorHandler = errorHandler;
            _authorizationFactory = authorizationFactory;
            _catalogController = catalogController;
        }

        public string CatalogTitle
        {
            get; 
            set;
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyOfPropertyChange(() => UserName);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyOfPropertyChange(() => Password);
            }
        }

        public int CatalogId
        {
            get; 
            set;
        }

        public string Path
        {
            get; 
            set;
        }

        public CatalogType CatalogType
        {
            get; 
            set;
        }

        public string CatalogBookItemKey
        {
            get; 
            set;
        }

        public async void Login()
        {
            try
            {
                if (string.IsNullOrEmpty(_userName) || string.IsNullOrEmpty(_password))
                {
                    _notificationsService.ShowAlert(UINotifications.General_AttentionCaption, UINotifications.AuthorizationPage_WrongPasswordOrUserName);
                    return;
                }

                StartBusiness();

                var authorizationService = _authorizationFactory.GetAuthorizationService(new CatalogModel{Id = CatalogId, Type = CatalogType});
                var authorizationString = await authorizationService.Authorize(_userName, _password, Path);

                if (!string.IsNullOrEmpty(authorizationString))
                {
                    var catalog = _catalogRepository.Get(CatalogId);
                    catalog.AuthorizationString = EncryptService.Encrypt(authorizationString);
                    _catalogRepository.Save(catalog);

                    if (!string.IsNullOrEmpty(CatalogBookItemKey))
                    {
                        var catalogBookItemModel = TransientStorage.Get<CatalogBookItemModel>(CatalogBookItemKey);
                        
                        _navigationService
                            .UriFor<BookInfoPageViewModel>()
                            .WithParam(vm => vm.Title, catalogBookItemModel.Title.ToUpper())
                            .WithParam(vm => vm.Description, catalogBookItemModel.Description)
                            .WithParam(vm => vm.ImageUrl, catalogBookItemModel.ImageUrl)
                            .WithParam(vm => vm.CatalogId, CatalogId)
                            .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(catalogBookItemModel))
                            .Navigate();
                    }
                    else
                    {
                        _navigationService.UriFor<CatalogPageViewModel>()
                                      .WithParam(vm => vm.CatalogId, catalog.Id)
                                      .WithParam(vm => vm.DisplayName, _catalogController.GetCatalogTitle(catalog).ToUpper())
                                      .WithParam(vm => vm.IsSearchEnabled, !string.IsNullOrEmpty(catalog.SearchUrl)
                                                                           || catalog.Type == CatalogType.SDCard
                                                                           || catalog.Type == CatalogType.SkyDrive)
                                      .WithParam(vm => vm.CanRefresh, catalog.Type != CatalogType.SDCard)
                                      .Navigate();
                    }
                }
                else
                {
                    StopBusiness();
                    _notificationsService.ShowAlert(UINotifications.General_AttentionCaption, UINotifications.AuthorizationPage_WrongPasswordOrUserName);
                }
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
    }
}