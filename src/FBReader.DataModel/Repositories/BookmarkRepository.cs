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

using System.Collections.Generic;
using System.Linq;
using FBReader.DataModel.Model;

namespace FBReader.DataModel.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        public void DeleteBookmark(BookmarkModel data)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                BookmarkModel model = context.Bookmarks.FirstOrDefault(t => t.BookmarkID == data.BookmarkID);
                context.Bookmarks.DeleteOnSubmit(model);
                context.SubmitChanges();
            }
        }

        public void DeleteBookmarks(string bookId)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                var bookmarks = context.Bookmarks.Where(t => t.BookID == bookId);
                context.Bookmarks.DeleteAllOnSubmit(bookmarks);
                context.SubmitChanges();
            }
        }

        public IEnumerable<BookmarkModel> GetBookmarks()
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                var books = context.Books
                    .Where(t => !t.Hidden && !t.Deleted && !t.Trial)
                    .Select(b => b.BookID)
                    .ToArray();

                return context.Bookmarks
                    .Where(b => books.Contains(b.BookID))
                    .OrderBy(b => b.BookID)
                    .ThenBy(b => b.TokenID)
                    .ToList();
            }
        }

        public IList<BookmarkModel> GetBookmarks(string bookId)
        {
            using (BookDataContext bookDataContext = BookDataContext.Connect())
                return bookDataContext.Bookmarks.Where(t => t.BookID == bookId).OrderBy(t => t.TokenID).ToList();
        }

        public BookmarkModel AddBookmark(string bookId, ICollection<BookmarkModel> bookmarks, string text, int color, int startTokenID, int endTokenID = -1)
        {
            using (BookDataContext bookDataContext = BookDataContext.Connect())
            {
                var entity = new BookmarkModel
                {
                    BookID = bookId,
                    TokenID = startTokenID,
                    EndTokenID = endTokenID,
                    Text = text,
                    Color = color
                };
                bookDataContext.Bookmarks.InsertOnSubmit(entity);
                bookDataContext.SubmitChanges();
                bookmarks.Add(entity);
                return entity;
            }
        }
    }
}