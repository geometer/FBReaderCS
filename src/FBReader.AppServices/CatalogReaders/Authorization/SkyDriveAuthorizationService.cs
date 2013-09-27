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
using System.Threading.Tasks;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;

namespace FBReader.AppServices.CatalogReaders.Authorization
{
    public class SkyDriveAuthorizationService : ICatalogAuthorizationService
    {
        private readonly ILiveLogin _liveLogin;
        private CatalogModel _catalog;
        private readonly ICatalogRepository _catalogRepository;

        public SkyDriveAuthorizationService(ILiveLogin liveLogin, ICatalogRepository catalogRepository,
                                            CatalogModel catalog)
        {
            _liveLogin = liveLogin;
            _catalog = catalog;
            _catalogRepository = catalogRepository;
        }

        public bool IsAuthorized
        {
            get { return true; }
        }

        public Task<string> Authorize(string userName, string password, string path)
        {
            throw new NotSupportedException();
        }

        public void Deauthorize()
        {
            _liveLogin.Logout();
            var catalog = _catalogRepository.Get(_catalog.Id);
            catalog.AccessDenied = true;
            _catalogRepository.Save(catalog);
            _catalog = catalog;
        }
    }
}
