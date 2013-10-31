/*
 * Author: Vitaly Leschenko, CactusSoft (http://cactussoft.biz/), 2013
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
using FBReader.DataModel.Repositories;
using FBReader.Render.Downloading.Loaders;

namespace FBReader.Render.Downloading
{
    public class FileLoadingFactory : IFileLoadingFactory
    {
        private readonly ICatalogRepository _catalogRepository = new CatalogRepository();

        public FileLoadingFactory(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public BaseFileLoader GetFileLoader(int dataSourceId, bool trial)
        {
            var source = _catalogRepository.Get(dataSourceId);
            if (source == null)
            {
                return null;
            }

            switch (source.Type)
            {
                case CatalogType.OPDS:
                    return new DirectFileLoader();

                case CatalogType.Litres:
                    if (trial)
                    {
                        return new DirectFileLoader();
                    }
                    return new LitresFileLoader();

                case CatalogType.SkyDrive:
                    return new SkyDriveFileLoader();

                case CatalogType.SDCard:
                    return new SdCardFileLoader();
                case CatalogType.StorageFolder:
                    return new StorageFolderFileLoader();
                default:
                    return null;
            }
        }
    }
}