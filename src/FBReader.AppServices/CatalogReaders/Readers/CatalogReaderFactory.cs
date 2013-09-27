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
using FBReader.AppServices.Controller;
using FBReader.AppServices.Tombstone.StateSaving;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.PhoneServices;
using FBReader.WebClient;

namespace FBReader.AppServices.CatalogReaders.Readers
{
    public class CatalogReaderFactory : ICatalogReaderFactory
    {
        private readonly IWebClient _webClient;
        private readonly ISdCardStorage _sdStorage;
        private readonly IStorageStateSaver _storageStateSaver;
        private readonly ILiveLogin _liveLogin;
        private readonly DownloadController _downloadController;
        private readonly ICatalogRepository _catalogRepository;

        public CatalogReaderFactory(
            ISdCardStorage sdStorage, 
            IStorageStateSaver storageStateSaver,
            ILiveLogin liveLogin,
            DownloadController downloadController,
            ICatalogRepository catalogRepository,
            IWebClient webClient)
        {
            _sdStorage = sdStorage;
            _storageStateSaver = storageStateSaver;
            _liveLogin = liveLogin;
            _downloadController = downloadController;
            _catalogRepository = catalogRepository;
            _webClient = webClient;
        }

        public ICatalogReader Create(CatalogModel catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            if (catalog.Type == CatalogType.SDCard)
            {
                return new SdCardCatalogReader(_sdStorage, catalog);
            }
            if (catalog.Type == CatalogType.SkyDrive)
            {
                return new SkyDriveCatalogReader(catalog, _liveLogin, _storageStateSaver, _downloadController, _catalogRepository);
            }
            if (catalog.Type == CatalogType.Litres)
            {
                return new LitresCatalogReader(catalog, _storageStateSaver, _webClient);
            }

            return new OpdsCatalogReader(catalog, _storageStateSaver, _webClient);
        }
    }
}