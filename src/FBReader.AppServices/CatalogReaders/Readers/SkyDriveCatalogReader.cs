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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FBReader.AppServices.CatalogReaders.SkyDrive;
using FBReader.AppServices.Controller;
using FBReader.AppServices.Tombstone.StateSaving;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using Microsoft.Live;

namespace FBReader.AppServices.CatalogReaders.Readers
{
    public class SkyDriveCatalogReader : ISearchableCatalogReader, IRestorable
    {
        private const string CURRENT_FOLDER_KEY = "skydrive_current_folder";
        private const string NAVIGATION_STACK_KEY = "skydrive_navigation_stack";
        
        private const string ROOT_FOLDER = "me/skydrive/files";
        private const string SEARCH_FORMAT = "me/skydrive/search?q={0}";

        private readonly CatalogModel _catalog;
        private readonly ILiveLogin _liveLogin;
        private readonly IStorageStateSaver _storageStateSaver;
        private readonly DownloadController _downloadController;
        private readonly ICatalogRepository _catalogRepository;

        private CatalogFolderModel _currentFolder = new CatalogFolderModel
                                                      {
                                                          BaseUrl = ROOT_FOLDER,
                                                          Items = new List<CatalogItemModel>()
                                                      };
        private LiveConnectClient _skyDrive;
        private Stack<CatalogFolderModel> _navigationStack = new Stack<CatalogFolderModel>();
        private CancellationTokenSource _cancelSource = new CancellationTokenSource();

        public SkyDriveCatalogReader(
            CatalogModel catalog, 
            ILiveLogin liveLogin, 
            IStorageStateSaver storageStateSaver,
            DownloadController downloadController,
            ICatalogRepository catalogRepository)
        {
            _catalog = catalog;
            _liveLogin = liveLogin;
            _storageStateSaver = storageStateSaver;
            _downloadController = downloadController;
            _catalogRepository = catalogRepository;
            CatalogId = catalog.Id;
        }

        public bool CanReadNextPage 
        { 
            get
            {
                return false;
            } 
        }

        public bool CanGoBack 
        { 
            get
            {
                return _navigationStack.Any();
            }
        }

        public int CatalogId { get; private set; }

        public async Task<IEnumerable<CatalogItemModel>> ReadAsync()
        {
            if (_currentFolder.Items != null && _currentFolder.Items.Any())
            {
                return _currentFolder.Items;
            }

            _currentFolder.Items = (await ReadAsync(_currentFolder.BaseUrl)).ToList();
            return _currentFolder.Items;
        }

        public void GoTo(CatalogItemModel model)
        {

            //cancel active requests
            _cancelSource.Cancel();

            _navigationStack.Push(_currentFolder);
            _currentFolder = new CatalogFolderModel
            {
                Items = new List<CatalogItemModel>(),
                BaseUrl = model.OpdsUrl + "/files"
            };
        }

        public void GoBack()
        {
            if (!_navigationStack.Any())
            {
                throw new ReadCatalogException("Unable go back");
            }

            //cancel active requests
            _cancelSource.Cancel();

            _currentFolder = _navigationStack.Pop();
        }

        public void Refresh()
        {
            _currentFolder.Items.Clear();
            foreach (CatalogFolderModel folderModel in _navigationStack)
            {
                folderModel.Items.Clear();
            }
        }

        public Task<IEnumerable<CatalogItemModel>> SearchAsync(string query)
        {
            return ReadAsync(string.Format(SEARCH_FORMAT, query));
        }

        public Task<IEnumerable<CatalogItemModel>> ReadNextPageAsync()
        {
            throw new NotImplementedException();
        }

        public void SaveState(string ownerKey = null)
        {
            _storageStateSaver.Save(_navigationStack.ToList(), CreateStorageKey(NAVIGATION_STACK_KEY, ownerKey));
            _storageStateSaver.Save(_currentFolder, CreateStorageKey(CURRENT_FOLDER_KEY, ownerKey));
        }

        public void LoadState(string ownerKey = null)
        {
            var navigationStack = _storageStateSaver.Restore<List<CatalogFolderModel>>(CreateStorageKey(NAVIGATION_STACK_KEY, ownerKey));
            if (navigationStack != null && navigationStack.Count > 0)
            {
                _navigationStack.Clear();
                for (var i = navigationStack.Count - 1; i >= 0; --i)
                {
                    _navigationStack.Push(navigationStack[i]);
                }
            }
            if(_navigationStack == null)
                _navigationStack = new Stack<CatalogFolderModel>();

            _currentFolder = _storageStateSaver.Restore<CatalogFolderModel>(CreateStorageKey(CURRENT_FOLDER_KEY, ownerKey));
        }

        private string CreateStorageKey(string keyFormat, string ownerKey = null)
        {
            return string.Concat(keyFormat, '_', _catalog.Id.ToString(CultureInfo.InvariantCulture),
                     !string.IsNullOrEmpty(ownerKey) ? "_" + ownerKey : string.Empty);
        }

        private async Task<IEnumerable<CatalogItemModel>> ReadAsync(string path)
        {
            List<CatalogItemModel> items = null;
            try
            {
                if (_skyDrive == null)
                    _skyDrive = await _liveLogin.Login();

                if (_skyDrive == null)
                {
                    throw new CatalogAuthorizationException(CatalogType.SkyDrive, path);
                }

                ChangeAccessToCatalog();

                _cancelSource = new CancellationTokenSource();
                var e = await _skyDrive.GetAsync(path, _cancelSource.Token);

                var skyDriveItems = new List<SkyDriveItem>();
                var data = (List<object>) e.Result["data"];
                foreach (IDictionary<string, object> content in data)
                {
                    var type = (string) content["type"];
                    SkyDriveItem item;
                    if (type == "folder")
                    {
                        item = new SkyDriveFolder
                                   {
                                       Id = (string) content["id"],
                                       Name = (string) content["name"]
                                   };
                    }
                    else if (type == "file")
                    {
                        var name = (string) content["name"];

                        if (string.IsNullOrEmpty(_downloadController.GetBookType(name)))
                            continue;

                        item = new SkyDriveFile
                                   {
                                       Id = (string) content["id"],
                                       Name = name
                                   };
                    }
                    else
                    {
                        continue;
                    }

                    skyDriveItems.Add(item);
                }

                var folders = skyDriveItems
                    .OfType<SkyDriveFolder>()
                    .Select(i => new CatalogItemModel
                                     {
                                         Title = i.Name,
                                         OpdsUrl = i.Id
                                     });

                var files = skyDriveItems
                    .OfType<SkyDriveFile>()
                    .Select(file => new CatalogBookItemModel
                                        {
                                            Title = file.Name,
                                            OpdsUrl = file.Id,
                                            Links = new List<BookDownloadLinkModel>
                                                        {
                                                            new BookDownloadLinkModel
                                                                {
                                                                    Url = file.Id,
                                                                    Type = file.Name
                                                                }
                                                        },
                                            Id = file.Id
                                        });

                items = Enumerable.Union(folders, files).ToList();
            }
            catch (OperationCanceledException)
            {
            }
            catch (LiveAuthException e)
            {
                if (e.ErrorCode == "access_denied")
                {
                    throw new CatalogAuthorizationException(e.Message, e, CatalogType.SkyDrive);
                }
                throw new ReadCatalogException(e.Message, e);
            }
            catch (Exception e)
            {
                throw new ReadCatalogException(e.Message, e);
            }

            return items ?? new List<CatalogItemModel>();
        }

        private void ChangeAccessToCatalog()
        {
            if (_catalog.AccessDenied)
            {
                _catalog.AccessDenied = false;
                _catalogRepository.Save(_catalog);
            }
        }
    }
}
