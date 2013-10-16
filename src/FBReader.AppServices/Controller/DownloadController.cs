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
using System.Linq;
using Caliburn.Micro;
using FBReader.AppServices.Events;
using FBReader.AppServices.Services;
using FBReader.AppServices.ViewModels.Pages;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.Render.Downloading;
using FBReader.Render.Downloading.Model;

namespace FBReader.AppServices.Controller
{
    public class DownloadController
    {
        private readonly IBookDownloader _bookDownloader;
        private readonly INavigationService _navigationService;
        private readonly INotificationsService _notificationsService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDownloadsContainer _container;
        private readonly IBookDownloadsRepository _bookDownloadsRepository;
        private static readonly string[] FormatPriority = new[] {"fb2", "epub", "html", "txt"};

        public DownloadController(
            IBookDownloader bookDownloader, 
            INavigationService navigationService,
            INotificationsService notificationsService,
            IEventAggregator eventAggregator, 
            IDownloadsContainer container, 
            IBookDownloadsRepository bookDownloadsRepository)
        {
            _bookDownloader = bookDownloader;
            _navigationService = navigationService;
            _notificationsService = notificationsService;
            _eventAggregator = eventAggregator;
            _container = container;
            _bookDownloadsRepository = bookDownloadsRepository;

            _bookDownloader.DownloadCompleted += BookDownloaderOnDownloadCompleted;
            _bookDownloader.DownloadError += BookDownloaderOnDownloadError;
        }

        private void BookDownloaderOnDownloadCompleted(object sender, DownloadItemDataModel downloadItemDataModel)
        {
            _eventAggregator.Publish(new BookDownloaded(downloadItemDataModel));


            var catalogBookItemModel = new CatalogBookItemModel();
            catalogBookItemModel.Id = downloadItemDataModel.CatalogItemId;
            catalogBookItemModel.Title = downloadItemDataModel.Name;
            catalogBookItemModel.Author = downloadItemDataModel.Author;
            catalogBookItemModel.Description = downloadItemDataModel.Description;
            if (!string.IsNullOrWhiteSpace(downloadItemDataModel.AcquisitionUrl))
            {
                catalogBookItemModel.AcquisitionLink = new BookAcquisitionLinkModel
                                                           {
                                                               Url = downloadItemDataModel.AcquisitionUrl,
                                                               Type = downloadItemDataModel.AcquisitionType,
                                                               Prices = BookAcquisitionLinkModel.ParsePrices(downloadItemDataModel.AcquisitionPrices)
                                                           };
            }
            if (downloadItemDataModel.IsTrial)
            {
                catalogBookItemModel.TrialLink = new BookDownloadLinkModel
                                                     {
                                                         Url = downloadItemDataModel.Path,
                                                         Type = downloadItemDataModel.Type
                                                     };
            }

            _notificationsService.ShowToast(UIStrings.BookDownloader_Success, downloadItemDataModel.Name, _navigationService.UriFor<ReadPageViewModel>()
                                                                      .WithParam(vm => vm.BookId, downloadItemDataModel.BookID.ToString())
                                                                      .WithParam(vm => vm.CatalogId, downloadItemDataModel.DataSourceID)
                                                                      .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(catalogBookItemModel))
                                                                      .WithParam(vm => vm.TokenOffset, 0).BuildUri());

        }

        private void BookDownloaderOnDownloadError(object sender, DownloadItemDataModel downloadItemDataModel)
        {
            _eventAggregator.Publish(new BookDownloaded(downloadItemDataModel));

            _notificationsService.ShowToast(
                UIStrings.BookDownloader_Error,
                downloadItemDataModel.Name, 
                _navigationService.UriFor<DownloadListPageViewModel>().BuildUri());
        }


        public BookDownloadLinkModel GetDownloadLink(params BookDownloadLinkModel[] links)
        {
            if(links == null)
                return null;

            foreach (var type in FormatPriority)
            {
                var link = links.FirstOrDefault(l => GetBookType(l.Type) == type);
                if (link != null)
                    return link;
            }

            return null;
        }

        public BookDownloadLinkModel GetDownloadLink(IEnumerable<BookDownloadLinkModel> links)
        {
            if (links == null)
                return null;
            return GetDownloadLink(links.ToArray());
        }


        public void StartDownload()
        {
            if (!_bookDownloadsRepository.GetItems().Any())
            {
                return;
            }

            _bookDownloader.Start();
        }

        public string GetBookType(string type)
        {
            type = type.ToLower();
            bool isFb2 = 
                type ==  "application/fb2+zip" || 
                type.EndsWith(".fb2") || 
                type.EndsWith(".fb2.zip") ||
                type == "fb2";
            if (isFb2)
                return "fb2";

            bool isHtml = 
                type ==  "application/html+zip" || 
                type.EndsWith(".html") ||
                type.EndsWith(".html.zip") ||
                type == "html";
            if (isHtml)
                return "html";

            bool isTxt = 
                type == "application/txt+zip" || 
                type.EndsWith(".txt") ||
                type.EndsWith(".txt.zip") ||
                type == "txt";
            if (isTxt)
                return "txt";

            bool isEpub = 
                type == "application/epub+zip" || 
                type == "application/epub" ||
                type.EndsWith(".epub") ||
                type.EndsWith(".epub.zip") ||
                type == "epub";
            if (isEpub)
                return "epub";

            return string.Empty;
        }

        public bool CheckIsZip(string type)
        {
            if (type.EndsWith(".zip"))
                return true;

            return 
                type.EndsWith("fb2+zip") || 
                type.EndsWith("html+zip") || 
                type.EndsWith("txt+zip");
        }

        public bool DownloadBook(CatalogBookItemModel catalogBookItemModel, int catalogId, bool fullBook = true)
        {
            var link = fullBook
                           ? GetDownloadLink(catalogBookItemModel.Links.ToArray())
                           : GetDownloadLink(catalogBookItemModel.TrialLink);
            
            if (link == null)
                return false;

            if (!_bookDownloader.IsStarted)
            {
                _bookDownloader.Start();
            }

            var downloadModel = new BookDownloadModel
            {
                Path = link.Url,
                Type = GetBookType(link.Type),

                IsZip = CheckIsZip(link.Type),
                Name = catalogBookItemModel.Title,
                Author = catalogBookItemModel.Author,
                Description = catalogBookItemModel.Description,
                AcquisitionUrl = catalogBookItemModel.AcquisitionLink != null ? catalogBookItemModel.AcquisitionLink.Url : null,
                AcquisitionType = catalogBookItemModel.AcquisitionLink != null ? catalogBookItemModel.AcquisitionLink.Type : null,
                AcquisitionPrices = catalogBookItemModel.AcquisitionLink != null ? catalogBookItemModel.AcquisitionLink.ToString() : null,
                DataSourceID = catalogId,
                IsTrial = !fullBook,
                CatalogItemId = catalogBookItemModel.Id
            };

            int oldQueueCount = _container.Count;
            _bookDownloadsRepository.Add(downloadModel);

            var downloadViewModel = new DownloadItemDataModel(downloadModel);
            _container.Enqueue(downloadViewModel);

            if (_container.Count == oldQueueCount)
            {
                int index = _container.GetDataModelIndex(downloadViewModel);
                if (index > -1)
                {
                    var viewModel = _container.Items[index];
                    if (viewModel.Status == DownloadStatus.Error)
                    {
                        viewModel.Status = DownloadStatus.Pending;
                        _bookDownloadsRepository.Remove(downloadModel.DownloadID);
                    }
                }
            }
            return true;
        }

    }
}
