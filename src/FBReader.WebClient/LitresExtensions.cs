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
using FBReader.DataModel.Model;
using FBReader.WebClient.DTO.Litres;

namespace FBReader.WebClient
{
    public static class LitresExtensions
    {
        private const string DOWNLOAD_URL = "http://robot.litres.ru/pages/catalit_download_book/?";
        
        public static CatalogFolderModel ToFolder(this CatalitFb2BooksDto booksDto, string authorizationString)
        {
            var folderModel = new CatalogFolderModel {Items = new List<CatalogItemModel>()};

            if (booksDto.Books == null || !booksDto.Books.Any())
            {
                return folderModel;
            }

            foreach (var fb2BookDto in booksDto.Books)
            {
                var bookCatalogItem = new CatalogBookItemModel();

                //TODO: change buy url and download links
                //bookCatalogItem.AcquisitionLink = new BookAcquisitionLinkModel
                //    {
                //        Type = ".fb2",
                //        Prices = new List<BookPriceModel> {new BookPriceModel {CurrencyCode = "RUB", Price = fb2BookDto.Price}},
                //        Url = "http://www.someurl.com/"
                //    };
                bookCatalogItem.Links = new List<BookDownloadLinkModel>
                    {
                        new BookDownloadLinkModel
                            {
                                Type = ".fb2.zip", 
                                Url = CreateDownloadUrl(fb2BookDto, authorizationString)
                            }
                    };

                bookCatalogItem.Id = fb2BookDto.Id.ToString(CultureInfo.InvariantCulture);
                //bookCatalogItem.Id = string.Concat(fb2BookDto.Id.ToString(CultureInfo.InvariantCulture), "|",
                //                                   fb2BookDto.Description.Hidden.DocumentInfo.Id);

                // title
                bookCatalogItem.Title = fb2BookDto.Description.Hidden.TitleInfo.BookTitle;

                // author
                bookCatalogItem.Author = CreateAuthorFullName(fb2BookDto.Description.Hidden.TitleInfo.Author);

                // cover image
                bookCatalogItem.ImageUrl = new Uri(fb2BookDto.ImageCover);

                folderModel.Items.Add(bookCatalogItem);
            }

            return folderModel;
        }

        private static string CreateDownloadUrl(Fb2BookDto fb2BookDto, string authorizationString)
        {
            return string.Concat(DOWNLOAD_URL, string.Format("sid={0}&art={1}&uuid={2}", authorizationString, fb2BookDto.Id, fb2BookDto.Description.Hidden.DocumentInfo.Id));
        }

        private static string CreateAuthorFullName(AuthorLitresDto author)
        {
            var name = author.FirstName;
            if (!string.IsNullOrEmpty(author.MiddleName))
            {
                name = string.Concat(name, " ", author.MiddleName);
            }
            if (!string.IsNullOrEmpty(author.LastName))
            {
                name = string.Concat(name, " ", author.LastName);
            }

            return name;
        }
    }
}