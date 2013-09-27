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

using System.Threading.Tasks;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;

namespace FBReader.AppServices.CatalogReaders.Authorization
{
    public abstract class OpdsAuthorizationServiceBase : ICatalogAuthorizationService
    {
        private readonly ICatalogRepository _catalogRepository;
        private CatalogModel _catalogModel;

        protected OpdsAuthorizationServiceBase(ICatalogRepository catalogRepository, CatalogModel catalogModel)
        {
            _catalogRepository = catalogRepository;
            _catalogModel = catalogModel;
        }

        public bool IsAuthorized { get { return !string.IsNullOrEmpty(_catalogModel.AuthorizationString); } }

        public abstract Task<string> Authorize(string userName, string password, string path);

        public void Deauthorize()
        {
            var catalog = _catalogRepository.Get(_catalogModel.Id);
            catalog.AuthorizationString = null;
            _catalogRepository.Save(catalog);
            _catalogModel = catalog;
        }
    }
}
