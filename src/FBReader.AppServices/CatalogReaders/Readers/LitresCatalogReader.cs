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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FBReader.AppServices.Services;
using FBReader.AppServices.Tombstone.StateSaving;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.WebClient;
using FBReader.WebClient.DTO.Litres;

namespace FBReader.AppServices.CatalogReaders.Readers
{
    public class LitresCatalogReader : OpdsCatalogReader
    {
        public LitresCatalogReader(CatalogModel catalogModel, IStorageStateSaver storageStateSaver, IWebClient webClient) 
            : base(catalogModel, storageStateSaver, webClient)
        {
        }

        protected override async Task<IEnumerable<CatalogItemModel>> ReadCatalogAsync()
        {
            if (CurrentFolder.CurrentRepresentationItem is LitresBookshelfCatalogItemModel)
            {
                return await LoadLitresBookShelfItems();
            }
            
            return await LoadItemsAsync(CurrentFolder.BaseUrl);
        }

        private async Task<IEnumerable<CatalogItemModel>> LoadLitresBookShelfItems()
        {
            var litresBookShelfData = (await GetLitresBookShelfDataAsync()).ToString();
            if (!ValidateForLitresAuthorization(litresBookShelfData))
            {
                throw new CatalogAuthorizationException(CatalogType.Litres, LitresApiConstants.AUTHORIZATION_URL);
            }

            var litresFolder = ConvertToLitresFolder(litresBookShelfData);
            UpdateCurrentFolder(litresFolder);

            return litresFolder.Items;
        }

        private CatalogFolderModel ConvertToLitresFolder(string litresBookShelfData)
        {
            try
            {
                CatalitFb2BooksDto dto;
                using (var stringReader = new StringReader(litresBookShelfData))
                {
                    var xmlSerializer = new XmlSerializer(typeof(CatalitFb2BooksDto));
                    dto = (CatalitFb2BooksDto)xmlSerializer.Deserialize(stringReader);
                }

                return dto.ToFolder(EncryptService.Decrypt(CatalogModel.AuthorizationString));
            }
            catch (InvalidOperationException exp)
            {
                throw new ReadCatalogException("Unable convert OPDS data to folder", exp);
            }
        }

        private static bool ValidateForLitresAuthorization(string litresBookShelfData)
        {
            return !litresBookShelfData.Contains(LitresApiConstants.AUTHORIZATION_FAILED_MESSAGE);
        }

        private async Task<StringBuilder> GetLitresBookShelfDataAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CatalogModel.AuthorizationString))
                {
                    throw new CatalogAuthorizationException(CatalogType.Litres, LitresApiConstants.AUTHORIZATION_URL);
                }

                var authorizationString = EncryptService.Decrypt(CatalogModel.AuthorizationString);
                var postParams = CreatePostParamsForBookShelf(authorizationString);
                var response = await WebClient.DoPostAsync(LitresApiConstants.BOOK_SHELF_URL, postParams);
                var stream = await response.Content.ReadAsStreamAsync();

                using (var reader = new StreamReader(stream))
                {
                    return new StringBuilder(reader.ReadToEnd());
                }
            }
            catch (WebException)
            {
                if (NavigationStack.Any())
                {
                    NavigationStack.Pop();
                }

                throw new ReadCatalogException("Unable read Litres catalog");
            }
        }

        private static Dictionary<string, string> CreatePostParamsForBookShelf(string authorizationString)
        {
            var dictionary = new Dictionary<string, string>
                {
                    {"sid", authorizationString}, 
                    {"my", "1"}
                };
            return dictionary;
        }

        private static class LitresApiConstants
        {
            public const string AUTHORIZATION_URL = "http://robot.litres.ru/pages/catalit_authorise/";
            public const string BOOK_SHELF_URL = "http://robot.litres.ru/pages/catalit_browser/";
            public const string AUTHORIZATION_FAILED_MESSAGE = "catalit-authorization-failed";
        }
    }
}