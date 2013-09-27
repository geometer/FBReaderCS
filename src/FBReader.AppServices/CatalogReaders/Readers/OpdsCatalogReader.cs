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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FBReader.AppServices.CatalogReaders.OpdsFilters;
using FBReader.AppServices.Services;
using FBReader.AppServices.Tombstone.StateSaving;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.WebClient;
using FBReader.WebClient.DTO;

namespace FBReader.AppServices.CatalogReaders.Readers
{
    public class OpdsCatalogReader: ISearchableCatalogReader, IRestorable
    {
        private const string CURRENT_FOLDER_KEY = "opds_current_folder";
        private const string NAVIGATION_STACK_KEY = "navigation_stack";

        private readonly IStorageStateSaver _storageStateSaver;
        private readonly IEnumerable<IOpdsBadFormatFilter> _opdsFilters;
        protected readonly Stack<CatalogFolderModel> NavigationStack = new Stack<CatalogFolderModel>();
        protected readonly IWebClient WebClient;
        protected readonly CatalogModel CatalogModel;
        protected CatalogFolderModel CurrentFolder;

        private string SearchUrl
        {
            get
            {
                return string.IsNullOrEmpty(CatalogModel.SearchUrl) ? string.Empty : CatalogModel.SearchUrl;
            }
        }

        public OpdsCatalogReader(CatalogModel catalogModel, IStorageStateSaver storageStateSaver, 
            IWebClient webClient)
        {
            CatalogModel = catalogModel;
            _storageStateSaver = storageStateSaver;
            WebClient = webClient;

            _opdsFilters = new IOpdsBadFormatFilter[]
                               {
                                   new UnescapedAmpersandsFilter(), 
                                   new UnescapedCdataFilter(),
                                   new UnescapedQuotesFilter(), 
                                   new UnescapedSignsFilter(),
                                   new UnescapedHtmlDescriptionFilter(),
                                   new AcquisitionBuyFilter()
                               };

            CurrentFolder = new CatalogFolderModel
                                 {
                                     BaseUrl = catalogModel.Url,
                                     Items = new List<CatalogItemModel>()
                                 };
        }

        #region Properties

        public bool CanReadNextPage
        {
            get
            {
                return CurrentFolder != null && !string.IsNullOrEmpty(CurrentFolder.NextPageUrl);
            }
        }

        public bool CanGoBack
        {
            get
            {
                return NavigationStack.Any();
            }
        }

        public int CatalogId
        {
            get
            {
                return CatalogModel.Id;
            }
        }

        #endregion

        #region ISearchableCatalogReader implementation

        public async Task<IEnumerable<CatalogItemModel>> ReadAsync()
        {
            if (CurrentFolder.Items != null && CurrentFolder.Items.Any())
            {
                return CurrentFolder.Items;
            }

            return await ReadCatalogAsync();
        }

        protected virtual async Task<IEnumerable<CatalogItemModel>> ReadCatalogAsync()
        {
            return await LoadItemsAsync(CurrentFolder.BaseUrl);
        }

        public async Task<IEnumerable<CatalogItemModel>> ReadNextPageAsync()
        {
            if (CurrentFolder == null || string.IsNullOrEmpty(CurrentFolder.NextPageUrl))
            {
                return Enumerable.Empty<CatalogItemModel>();
            }

            CurrentFolder.BaseUrl = CurrentFolder.NextPageUrl;
            return await LoadItemsAsync(CurrentFolder.NextPageUrl);
        }

        public async Task<IEnumerable<CatalogItemModel>> SearchAsync(string query)
        {
            NavigationStack.Clear();
            CurrentFolder = new CatalogFolderModel
                {
                    BaseUrl = string.Format(SearchUrl, HttpUtility.HtmlEncode(query)),
                    Items = new List<CatalogItemModel>()
                };

            if (string.IsNullOrEmpty(SearchUrl))
            {
                return Enumerable.Empty<CatalogItemModel>();
            }

            return await LoadItemsAsync(string.Format(SearchUrl, HttpUtility.HtmlEncode(query)));
        }

        public void GoTo(CatalogItemModel catalogItem)
        {
            //cancel active requests
            //WebClient.Cancel();
            WebClient.Cancel();

            NavigationStack.Push(CurrentFolder);
            CurrentFolder = new CatalogFolderModel
                                    {
                                        Items = new List<CatalogItemModel>(),
                                        BaseUrl = catalogItem.OpdsUrl,
                                        CurrentRepresentationItem =  catalogItem
                                    };
        }

        public void GoBack()
        {
            if (!NavigationStack.Any())
            {
                throw new ReadCatalogException("Unable go back");
            }

            //cancel active requests
            WebClient.Cancel();
            
            CurrentFolder = NavigationStack.Pop();
        }

        public void Refresh()
        {
            CurrentFolder.Items.Clear();
            foreach (CatalogFolderModel folderModel in NavigationStack)
            {
                folderModel.Items.Clear();
            }
        }

        #endregion

        #region Data loading and parsing

        protected async Task<IEnumerable<CatalogItemModel>> LoadItemsAsync(string url)
        {
            var folder = await LoadFolderAsync(url);
            UpdateCurrentFolder(folder);
            return CurrentFolder.Items;
        }
        
        private async Task<CatalogFolderModel> LoadFolderAsync(string url)
        {
            var opdsSource = await GetOpdsDataAsync(url, WebClient);

            ApplyBadFormatFilters(opdsSource);
            return ConvertToFolder(opdsSource, url);
        }

        private async Task<StringBuilder> GetOpdsDataAsync(string url, IWebClient webClient)
        {
            try
            {
                var baseUri = new UriBuilder(url);
                string queryToAppend = "rand=" + Guid.NewGuid();
                baseUri.Query = baseUri.Query.Length > 1 ? baseUri.Query.Substring(1) + "&" + queryToAppend : queryToAppend;

                string authorizationString = null;
                if (!string.IsNullOrEmpty(CatalogModel.AuthorizationString))
                {
                    authorizationString = EncryptService.Decrypt(CatalogModel.AuthorizationString);
                }

                var response = await webClient.DoGetAsync(baseUri.ToString(), authorizationString);

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new CatalogAuthorizationException(CatalogType.OPDS, url);
                }

                var stream = await response.Content.ReadAsStreamAsync();

                using (var reader = new StreamReader(stream))
                {
                    return new StringBuilder(reader.ReadToEnd());
                }
            }
            catch (WebException exception)
            {
                if (exception.Status != WebExceptionStatus.RequestCanceled)
                {
                    if (NavigationStack.Any())
                    {
                        NavigationStack.Pop();
                    }
                
                    var statusCode = ((HttpWebResponse) exception.Response).StatusCode;
                    if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
                    {
                        throw new CatalogAuthorizationException(CatalogModel.Type, url);
                    }
           
                    throw new ReadCatalogException(string.Format("Unable read catalog {0}", url));
                }
            }
            catch (DataCorruptedException)
            {
                //skip DataCorruptedException
            }

            return await GetOpdsDataAsync(url, webClient);
        }

        protected void UpdateCurrentFolder(CatalogFolderModel folder)
        {
            if (folder.BaseUrl != null && folder.BaseUrl != CurrentFolder.BaseUrl)
            {
                return;
            }
            CurrentFolder.Items = folder.Items;
            CurrentFolder.NextPageUrl = folder.NextPageUrl;
        }

        private CatalogFolderModel ConvertToFolder(StringBuilder opdsSource, string url)
        {
            try
            {
                CatalogContentDto dto;
                using (var stringReader = new StringReader(opdsSource.ToString()))
                {
                    var xmlSerializer = new XmlSerializer(typeof(CatalogContentDto));
                    dto = (CatalogContentDto)xmlSerializer.Deserialize(stringReader);
                }

                var folder = dto.ToFolder(CatalogModel.Url, CatalogModel.Type, CatalogId);
                folder.BaseUrl = url;
                return folder;
            }
            catch (InvalidOperationException exp)
            {
                if (ValidateForHtmlContent(exp))
                {
                    throw new WrongCatalogFormatException(exp.Message, url);
                }
                throw new ReadCatalogException("Unable convert OPDS data to folder", exp);
            }
        }

        #endregion

        #region Validation

        private void ApplyBadFormatFilters(StringBuilder opdsSource)
        {
            foreach (var editor in _opdsFilters)
            {
                editor.Apply(opdsSource);
            }
        }

        private bool ValidateForHtmlContent(InvalidOperationException exp)
        {
            return exp.ToString().Contains("<html xmlns=''> was not expected");
        }
        
        #endregion

        #region IRestorable implementation

        public void SaveState(string ownerKey = null)
        {
            _storageStateSaver.Save(NavigationStack.ToList(), CreateStorageKey(NAVIGATION_STACK_KEY, ownerKey));
            _storageStateSaver.Save(CurrentFolder, CreateStorageKey(CURRENT_FOLDER_KEY, ownerKey));
        }

        public void LoadState(string ownerKey = null)
        {
            var navigationStack = _storageStateSaver.Restore<List<CatalogFolderModel>>(CreateStorageKey(NAVIGATION_STACK_KEY, ownerKey));
            if (navigationStack != null && navigationStack.Count > 0)
            {
                NavigationStack.Clear();
                for (var i = navigationStack.Count - 1; i >= 0; --i)
                {
                    NavigationStack.Push(navigationStack[i]);
                }
            }

            CurrentFolder = _storageStateSaver.Restore<CatalogFolderModel>(CreateStorageKey(CURRENT_FOLDER_KEY, ownerKey));
        }

        private string CreateStorageKey(string keyFormat, string ownerKey = null)
        {
            return string.Concat(keyFormat, '_', CatalogModel.Id.ToString(CultureInfo.InvariantCulture),
                                 !string.IsNullOrEmpty(ownerKey) ? "_" + ownerKey : string.Empty);
        }
        
        #endregion
    }
}