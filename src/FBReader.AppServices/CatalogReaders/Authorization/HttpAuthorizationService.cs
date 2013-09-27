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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.WebClient;

namespace FBReader.AppServices.CatalogReaders.Authorization
{
    public class HttpAuthorizationService : OpdsAuthorizationServiceBase
    {
        private readonly IWebClient _webClient;

        public HttpAuthorizationService(IWebClient webClient, ICatalogRepository catalogRepository, CatalogModel catalogModel)
            : base(catalogRepository, catalogModel)
        {
            _webClient = webClient;
        }

        public override async Task<string> Authorize(string userName, string password, string path)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("userName");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }

            var authorizationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Concat(userName, ":", password)));

            try
            {
                var response = await _webClient.DoGetAsync(path, authorizationString);
                return response.StatusCode == HttpStatusCode.OK ? authorizationString : null;
            }
            catch (WebException webException)
            {
                var statusCode = ((HttpWebResponse) webException.Response).StatusCode;
                if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
                {
                    return null;
                }

                throw;
            }
        }
    }
}