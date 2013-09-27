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

using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.WebClient;

namespace FBReader.AppServices.CatalogReaders.Authorization
{
    public class CatalogAuthorizationFactory : ICatalogAuthorizationFactory
    {
        private readonly IWebClient _webClient;
        private readonly ILiveLogin _liveLogin;
        private readonly ICatalogRepository _catalogRepository;

        public CatalogAuthorizationFactory(IWebClient webClient, ILiveLogin liveLogin, ICatalogRepository catalogRepository)
        {
            _webClient = webClient;
            _liveLogin = liveLogin;
            _catalogRepository = catalogRepository;
        }

        public ICatalogAuthorizationService GetAuthorizationService(CatalogModel catalog)
        {
            switch (catalog.Type)
            {
                case CatalogType.OPDS:
                    return new HttpAuthorizationService(_webClient, _catalogRepository, catalog);
                case CatalogType.Litres:
                    return new LitresAuthorizationService(_webClient, _catalogRepository, catalog);
                case CatalogType.SkyDrive:
                    return new SkyDriveAuthorizationService(_liveLogin, _catalogRepository, catalog);
            }

            return new DummyAuthorizationService();
        }
    }
}