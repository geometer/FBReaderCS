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
using System.Threading.Tasks;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.PhoneServices;
using FBReader.Render.Parsing;

namespace FBReader.AppServices.CatalogReaders.Readers
{
    public class SdCardCatalogReader : ISearchableCatalogReader
    {
        private readonly CatalogModel _catalogModel;
        private readonly ISdCardStorage _sdCardStorage;
        private readonly string[] _supportedFileTypes = new []{ ".epub", ".fb2" };

        public SdCardCatalogReader(ISdCardStorage sdCardStorage, CatalogModel catalogModel)
        {
            _sdCardStorage = sdCardStorage;
            _catalogModel = catalogModel;
        }

        public async Task<IEnumerable<CatalogItemModel>> ReadAsync()
        {
            try
            {
                var files = await _sdCardStorage.GetFilesAsync(_supportedFileTypes);
                
                //extract book summary
                var items = new List<CatalogBookItemModel>();

                foreach (var file in files)
                {
                    var stream = await file.OpenForReadAsync();

// ReSharper disable PossibleNullReferenceException
                    var type = Path.GetExtension(file.Path).TrimStart('.');
// ReSharper restore PossibleNullReferenceException
                    
                    var parser = BookFactory.GetPreviewGenerator(type, file.Name, stream);
                    var preview = parser.GetBookPreview();
                    items.Add(new CatalogBookItemModel
                        {
                            Title = preview.Title,
                            Description = preview.Description,
                            Author = preview.AuthorName,
                            Links = new List<BookDownloadLinkModel>
                                        {
                                            new BookDownloadLinkModel {Type = '.' + type, Url = file.Path}
                                        },
                            Id = file.Path
                        });
                }

                return items.OrderBy(i => i.Title);
            }
            catch (SdCardNotSupportedException)
            {
                return Enumerable.Empty<CatalogItemModel>();
            }
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
                return false;
            }
        }

        public int CatalogId
        {
            get
            {
                return _catalogModel.Id;
            }
        }

        public Task<IEnumerable<CatalogItemModel>> ReadNextPageAsync()
        {
            throw new NotSupportedException();
        }

        public void GoTo(CatalogItemModel model)
        {
            throw new NotSupportedException();
        }

        public void GoBack()
        {
            throw new NotSupportedException();
        }

        public void Refresh()
        {
        }

        public async Task<IEnumerable<CatalogItemModel>> SearchAsync(string query)
        {
            var items = await ReadAsync();
            return items.Where(i => i.Title.ToLower().Contains(query.ToLower()));
        }
    }
}
