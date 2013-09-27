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

using System.IO;

namespace FBReader.DataModel.Model
{
    public static class ModelExtensions
    {
        public static string GetTokensPath(this BookModel model)
        {
            return Path.Combine(model.BookID, ModelConstants.BOOK_TOKENS_PATH);
        }

        public static string GetTokensRefPath(this BookModel model)
        {
            return Path.Combine(model.BookID, ModelConstants.BOOK_FILE_DATA_REF_PATH);
        }

        public static string GetBookPath(this BookModel model)
        {
            return Path.Combine(model.BookID, ModelConstants.BOOK_FILE_DATA_PATH);
        }

        public static string GetBookFullCoverPath(string bookId)
        {
            return GetBookCover(bookId, true);
        }

        public static string GetBookCoverPath(string bookId)
        {
            return GetBookCover(bookId, false);
        }

        private static string GetBookCover(string bookId, bool fullCover)
        {
            return string.Concat("Shared/ShellContent/", bookId, fullCover ? ".cover" : string.Empty, ".jpg");
        }
    }
}