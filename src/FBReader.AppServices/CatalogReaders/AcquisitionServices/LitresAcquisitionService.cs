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
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using FBReader.AppServices.Services;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.WebClient;

namespace FBReader.AppServices.CatalogReaders.AcquisitionServices
{
    public class LitresAcquisitionService : IAcquisitionService
    {
        private const string AUTHORIZATION_URL = "http://robot.litres.ru/pages/catalit_authorise/";
        private const string BUY_BOOK_URL = "https://robot.litres.ru/pages/purchase_book/";
        private const string DOWNLOAD_BOOK_URL = "http://robot.litres.ru/pages/catalit_download_book/?";
        private const string NOT_ENOUGH_MONEY_URL = "http://www.litres.ru/pages/put_money_on_account/";
        private readonly IWebClient _webClient;
        
        public LitresAcquisitionService(IWebClient webClient)
        {
            _webClient = webClient;
        }

        public async Task<string> BuyBook(CatalogBookItemModel book, string authorizationString)
        {
            if (string.IsNullOrEmpty(authorizationString))
            {
                throw new CatalogAuthorizationException(CatalogType.Litres, AUTHORIZATION_URL);
            }

            try
            {
                var art = string.Empty;
                var uuid = string.Empty;

                if (book.Id.Contains("|"))
                {
                    var idParts = book.Id.Split('|');
                    art = idParts[0];
                    uuid = idParts[1];
                }
                else
                {
                    art = book.Id;
                }

                var response = await _webClient.DoPostAsync(BUY_BOOK_URL, CreateAcquisitionParams(art, uuid, EncryptService.Decrypt(authorizationString)));

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ReadCatalogException("Unable to read catalog");
                }

                var responseStream = await response.Content.ReadAsStreamAsync();

                var xDoc = XDocument.Load(responseStream);
                if (xDoc.Root.Name == "catalit-purchase-ok")
                {
                    var format = string.Format("sid={0}&art={1}", EncryptService.Decrypt(authorizationString), art);
                    if (!string.IsNullOrEmpty(uuid))
                    {
                        format = string.Concat(format, string.Format("uuid={0}", uuid));
                    }

                    return string.Concat(DOWNLOAD_BOOK_URL, format);
                }

                if (xDoc.Root.Name == "catalit-purchase-failed")
                {
                    if (xDoc.Root.Attribute("error").Value == "1")
                    {
                        throw new CatalogNotEnoughMoneyException(CatalogType.Litres, string.Concat(NOT_ENOUGH_MONEY_URL, string.Format("?sid={0}", EncryptService.Decrypt(authorizationString))));
                    }
                    if (xDoc.Root.Attribute("error").Value == "3")
                    {
                        //throw new CatalogBookAlreadyBoughtException(CatalogType.Litres, book.Id);
                        var format = string.Format("sid={0}&art={1}", EncryptService.Decrypt(authorizationString), art);
                        if (!string.IsNullOrEmpty(uuid))
                        {
                            format = string.Concat(format, string.Format("uuid={0}", uuid));
                        }

                        return string.Concat(DOWNLOAD_BOOK_URL, format);
                    }
                }

                if (xDoc.Root.Name == "catalit-authorization-failed")
                {
                    throw new CatalogAuthorizationException(CatalogType.Litres, AUTHORIZATION_URL);
                }
            }
            catch (WebException)
            {
                throw new ReadCatalogException("Unable to read catalog");
            }

            throw new ReadCatalogException("Unable to read catalog");
        }

        private static Dictionary<string, string> CreateAcquisitionParams(string art, string uuid, string authorizationString)
        {
            var dictionary = new Dictionary<string, string>
                {
                    {"sid", authorizationString}, 
                    {"art", art}
                };
            if (!string.IsNullOrEmpty(uuid))
            {
                dictionary.Add("uuid", uuid);
            }
            dictionary.Add("lfrom", "51");

            return dictionary;
        }
    }
}