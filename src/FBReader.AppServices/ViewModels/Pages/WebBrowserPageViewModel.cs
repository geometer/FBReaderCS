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
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.AppServices.Events;
using FBReader.AppServices.ViewModels.Base;
using FBReader.DataModel.Model;
using FBReader.WebClient;
using Microsoft.Phone.Controls;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class WebBrowserPageViewModel : BusyViewModel, IHandle<BookDownloaded>
    {
        private string _catalogItemId;
        private readonly IWebClient _webClient;
        private readonly INavigationService _navigationService;
        private readonly DownloadController _downloadController;
        private string _webBrowserUrl;
        private bool _navigated;

        public WebBrowserPageViewModel(IWebClient webClient, INavigationService navigationService, DownloadController downloadController)
        {
            _webClient = webClient;
            _navigationService = navigationService;
            _downloadController = downloadController;
            BrowserHistoryUrls = new List<string>();
        }

        public string WebBrowserUrl
        {
            get
            {
                return _webBrowserUrl;
            }
            set
            {
                _webBrowserUrl = value;
                NotifyOfPropertyChange(() => WebBrowserUrl);
            }
        }

        public List<string> BrowserHistoryUrls
        {
            get; 
            set;
        }

        public string CatalogBookItemKey
        {
            get; 
            set;
        }

        public string Title
        {
            get; 
            set;
        }

        public int CatalogId
        {
            get; 
            set;
        }

        public void OnBrowserNavigated(NavigationEventArgs args)
        {
            StopBusiness();
        }

        public void OnBrowserNavigationFailed(NavigationFailedEventArgs args)
        {
            StopBusiness();
        }

        public async void OnBrowserNavigating(NavigatingEventArgs args)
        {
            if (!IsActive || _navigated)
            {
                return;
            }
            
            if (!IsBusy)
            {
                StartBusiness();
            }

            await CheckForDownloadItem(args.Uri.ToString());
        }

        private async Task CheckForDownloadItem(string url)
        {
            var link = await GetDownloadLink(url);
            if (link != null)
            {
                if (!string.IsNullOrEmpty(CatalogBookItemKey) && TransientStorage.Contains(CatalogBookItemKey))
                {
                    var catalogBookItem = TransientStorage.Get<CatalogBookItemModel>(CatalogBookItemKey);
                    catalogBookItem.Links = new List<BookDownloadLinkModel> { link };

                    _navigationService
                        .UriFor<BookInfoPageViewModel>()
                        .WithParam(vm => vm.Title, catalogBookItem.Title.ToUpper())
                        .WithParam(vm => vm.Description, catalogBookItem.Description)
                        .WithParam(vm => vm.ImageUrl, catalogBookItem.ImageUrl)
                        .WithParam(vm => vm.CatalogId, CatalogId)
                        .WithParam(vm => vm.StartDownload, true)
                        .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(catalogBookItem))
                        .Navigate();
                    _navigated = true;
                }
                else
                {
                    var catalogBookItemModel = new CatalogBookItemModel
                        {
                            Links = new List<BookDownloadLinkModel> { link },
                            Title = Title,
                            Id = Guid.NewGuid().ToString()
                        };
                    _downloadController.DownloadBook(catalogBookItemModel, CatalogId);
                    _catalogItemId = catalogBookItemModel.Id;
                }
            }
        }

        private async Task<BookDownloadLinkModel> GetDownloadLink(string url)
        {
            HttpResponseMessage response;
            try
            {
                response = await _webClient.DoHeadAsync(url);
            }
            catch (Exception)
            {
                return null;
            }

            if (response == null || response.Content.Headers.ContentType == null)
                return null;

            var contentType = response.Content.Headers.ContentType.MediaType;

            if (contentType.Contains("application/epub"))
            {
                var type = contentType.EndsWith("zip") ? "application/epub+zip" : "application/epub";
                return new BookDownloadLinkModel
                    {
                        Type = type,
                        Url = url
                    };
            }
            if (contentType.Contains("application/fb2"))
            {
                var type = contentType.EndsWith("zip") ? "application/fb2+zip" : "application/fb2";
                return new BookDownloadLinkModel
                {
                    Type = type,
                    Url = url
                };
            }
            if (contentType.Contains("application/html"))
            {
                var type = contentType.EndsWith("zip") ? "application/html+zip" : "application/html";
                return new BookDownloadLinkModel
                {
                    Type = type,
                    Url = url
                };
            }
            if (contentType.Contains("application/txt"))
            {
                var type = contentType.EndsWith("zip") ? "application/txt+zip" : "application/txt";
                return new BookDownloadLinkModel
                {
                    Type = type,
                    Url = url
                };
            }
            if (contentType.Contains("application/zip"))
            {
                return new BookDownloadLinkModel
                {
                    Type = "application/fb2+zip",
                    Url = url
                };
            }

            return null;
        }

        public void Handle(BookDownloaded message)
        {
            if (message.Book.CatalogItemId == _catalogItemId)
            {
                StopBusiness();
                _navigationService.GoBack();
            }
        }
    }
}