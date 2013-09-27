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
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.Localization;

namespace FBReader.AppServices
{
    public class DataBaseInitializer 
    {
        public async Task ExecuteAsync()
        {
            

            using (var db = BookDataContext.Connect())
            {
                AddDefaultCatalogs(db);
            }
        }
        
        private void AddDefaultCatalogs(BookDataContext dataContext)
        {
            var catalog = new CatalogModel
            {
                Url = "http://data.fbreader.org/catalogs/litres2/index.php5",
                Title = "FBReader_Litres",
                Description = "description",
                IconLocalPath = "/Resources/Icons/litres_icon.jpg",
                SearchUrl = "http://data.fbreader.org/catalogs/litres2/search.php5?query={0}",
                Type = CatalogType.Litres
            };
            dataContext.Catalogs.InsertOnSubmit(catalog);
            
//            catalog = new CatalogModel
//            {
//                Url = "http://flibusta.net/opds",
//                Title = "FBReader_Flibusta",
//                Description = "description",
//                IconLocalPath = "/Resources/Icons/flibusta_icon.jpg",
//                SearchUrl = "http://flibusta.net/opds/search?searchTerm={0}&searchType=books",
//                Type = CatalogType.OPDS
//            };
//            dataContext.Catalogs.InsertOnSubmit(catalog);

            catalog = new CatalogModel
            {
                Url = "http://manybooks.net/opds/index.php",
                Title = "FBReader_Manybooks",
                Description = "description",
                IconLocalPath = "/Resources/Icons/Manybooks.png",
                Type = CatalogType.OPDS
            };
            dataContext.Catalogs.InsertOnSubmit(catalog);

            catalog = new CatalogModel
            {
                Url = "http://data.fbreader.org/catalogs/prochtenie/index.xml",
                Title = "FBReader_Prochtenie",
                Description = "description",
                IconLocalPath = "/Resources/Icons/prochtenie_catalog.png",
                Type = CatalogType.OPDS
            };
            dataContext.Catalogs.InsertOnSubmit(catalog);

            catalog = new CatalogModel
                          {
                              Title = UIStrings.SkyDrive_Catalog_Title,
                              Description = UIStrings.SkyDrive_Catalog_Description,
                              IconLocalPath = "/Resources/Icons/skydrive.png",
                              Type = CatalogType.SkyDrive,
                              AccessDenied = true
                          };
            dataContext.Catalogs.InsertOnSubmit(catalog);

           
            catalog = new CatalogModel
            {
                Title = UIStrings.SDCard_Catalog_Title,
                Description = UIStrings.SDCard_Catalog_Description,
                IconLocalPath = "/Resources/Icons/sd_card.png",
                Type = CatalogType.SDCard
            };

            dataContext.Catalogs.InsertOnSubmit(catalog);
            

            dataContext.SubmitChanges();
        }
    }
}
