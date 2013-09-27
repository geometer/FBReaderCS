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

using System.Windows.Media;
using FBReader.AppServices.DataModels;
using FBReader.Common;
using FBReader.DataModel.Model;

namespace FBReader.AppServices.Controller
{
    public class BookmarksController
    {
        public BookmarkItemDataModel CreateBookmarkDataModel(BookmarkModel bookmark, string bookTitle = null)
        {
            var dataModel = new BookmarkItemDataModel();
            dataModel.BookmarkText = bookmark.Text;
            dataModel.Color = new SolidColorBrush(ColorHelper.ToColor(bookmark.Color));
            dataModel.Bookmark = bookmark;
            dataModel.BookTitle = bookTitle;
            return dataModel;
        }

        public BookmarkSearchResultDaraModel CreateBookmarkSearchDataModel(BookmarkModel bookmark, string searchQuery, string bookTitle = null)
        {
            var dataModel = new BookmarkSearchResultDaraModel();
            dataModel.BookmarkText = bookmark.Text;
            dataModel.SearchQuery = searchQuery;
            dataModel.BookId = bookmark.BookID;
            dataModel.TokenOffset = bookmark.TokenID;
            dataModel.BookTitle = bookTitle;
            dataModel.Color = new SolidColorBrush(ColorHelper.ToColor(bookmark.Color));
            return dataModel;
        }
    }
}
