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

using FBReader.AppServices.DataModels;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.Localization;

namespace FBReader.AppServices.Controller
{
    public class CatalogController
    {

        public CatalogDataModel ToCatalogDataModel(CatalogModel catalog)
        {
            var dataModel = new CatalogDataModel();
            dataModel.Catalog = catalog;
            dataModel.Icon = catalog.IconLocalPath;
            dataModel.Title = catalog.Title;
            dataModel.Description = catalog.Description;

            switch (catalog.Type)
            {
                case CatalogType.SDCard:
                    dataModel.Title = UIStrings.SDCard_Catalog_Title;
                    dataModel.Description = UIStrings.SDCard_Catalog_Description;
                    break;
                case CatalogType.SkyDrive:
                    dataModel.Title = UIStrings.SkyDrive_Catalog_Title;
                    dataModel.Description = UIStrings.SkyDrive_Catalog_Description;
                    break;
            }
            switch (catalog.Title)
            {
                case "FBReader_Litres":
                    dataModel.Title = UIStrings.Litres_Catalog_Title;
                    dataModel.Description = UIStrings.Litres_Catalog_Descritption;
                    break;
                case "FBReader_Flibusta":
                    dataModel.Title = UIStrings.Flibusta_Catalog_Title;
                    dataModel.Description = UIStrings.Flibusta_Catalog_Description;
                    break;
                case "FBReader_Manybooks":
                    dataModel.Title = UIStrings.Catalog_Manybooks_Title;
                    dataModel.Description = UIStrings.Catalog_Manybooks_Description;
                    break;
                case "FBReader_Prochtenie":
                    dataModel.Title = UIStrings.Catalog_Prochtenie_Title;
                    dataModel.Description = UIStrings.Catalog_Prochtenie_Description;
                    break;
            }
            return dataModel;
        }

        public string GetCatalogTitle(CatalogModel catalog)
        {
            switch (catalog.Type)
            {
                case CatalogType.SDCard:
                    return UIStrings.SDCard_Catalog_Title;
                case CatalogType.SkyDrive:
                    return UIStrings.SkyDrive_Catalog_Title;
            }
            switch (catalog.Title)
            {
                case "FBReader_Litres":
                    return UIStrings.Litres_Catalog_Title;
                case "FBReader_Flibusta":
                    return UIStrings.Flibusta_Catalog_Title;
                case "FBReader_Manybooks":
                    return UIStrings.Catalog_Manybooks_Title;
                case "FBReader_Prochtenie":
                    return UIStrings.Catalog_Prochtenie_Title;
            }
            return catalog.Title;
        }

        public string GetCatalogDescription(CatalogModel catalog)
        {
            switch (catalog.Type)
            {
                case CatalogType.SDCard:
                    return UIStrings.SDCard_Catalog_Description;
                case CatalogType.SkyDrive:
                    return UIStrings.SkyDrive_Catalog_Description;
            }
            switch (catalog.Title)
            {
                case "FBReader_Litres":
                    return UIStrings.Litres_Catalog_Descritption;
                case "FBReader_Flibusta":
                    return UIStrings.Flibusta_Catalog_Description;
                case "FBReader_Manybooks":
                    return UIStrings.Catalog_Manybooks_Description;
                case "FBReader_Prochtenie":
                    return UIStrings.Catalog_Prochtenie_Description;
            }
            return catalog.Description;
        }
    }
}
