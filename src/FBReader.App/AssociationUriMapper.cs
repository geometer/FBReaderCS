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
using System.Windows.Navigation;
using Windows.Phone.Storage.SharedAccess;
using Caliburn.Micro;
using FBReader.AppServices;
using FBReader.AppServices.ViewModels.Pages;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;

namespace FBReader.App
{
    class AssociationUriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            var navigationService = IoC.Get<INavigationService>();
            var catalogRepository = IoC.Get<ICatalogRepository>();

            string tempUri = uri.ToString();

            // File association launch
            if (tempUri.Contains("/FileTypeAssociation"))
            {
                // Get the file ID (after "fileToken=").
                int fileIDIndex = tempUri.IndexOf("fileToken=", StringComparison.InvariantCulture) + 10;
                string fileID = tempUri.Substring(fileIDIndex);

                // Get the file name.
                string incomingFileName = SharedStorageAccessManager.GetSharedFileName(fileID);

                // Get the file extension.
// ReSharper disable PossibleNullReferenceException
                string incomingFileType = Path.GetExtension(incomingFileName).ToLower();
// ReSharper restore PossibleNullReferenceException


                var catalog = catalogRepository.GetAll().Single(c => c.Type == CatalogType.StorageFolder);

                var bookItemModel = new CatalogBookItemModel
                {
                    Title = Path.GetFileNameWithoutExtension(incomingFileName),
                    Description = string.Empty,
                    Author = string.Empty,
                    Links = new List<BookDownloadLinkModel>
                            {
                                new BookDownloadLinkModel {Type = incomingFileType, Url = fileID}
                            },
                    Id = fileID
                };

                return navigationService
                    .UriFor<BookInfoPageViewModel>()
                    .WithParam(vm => vm.Title, bookItemModel.Title.ToUpper())
                    .WithParam(vm => vm.Description, bookItemModel.Description)
                    .WithParam(vm => vm.CatalogId, catalog.Id)
                    .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(bookItemModel))
                    .BuildUri();
            }
            // Otherwise perform normal launch.
            return uri;
        }
    }
}
