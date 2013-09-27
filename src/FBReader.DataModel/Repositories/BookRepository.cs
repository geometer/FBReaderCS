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
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using FBReader.DataModel.Model;

namespace FBReader.DataModel.Repositories
{
    public class BookRepository : IBookRepository
    {
        public void Add(BookModel book)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                context.Books.InsertOnSubmit(book);
                context.SubmitChanges();
            }
        }

        public void Save(BookModel book)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                context.Books.Attach(book);
                context.Refresh(0, book);
                context.SubmitChanges();
            }
        }

        public void Remove(string bookId)
        {
            MarkAsDeleted(bookId);
            ThreadPool.QueueUserWorkItem(delegate
                                         {
                                             RemoveFromDataBase(bookId);
                                             DeleteFolder(bookId);
                                         });
        }

        public BookModel Get(string bookId, bool updateTime = true)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                BookModel model = context.Books.FirstOrDefault(t => t.BookID == bookId);
                if ((model != null) && updateTime)
                {
                    model.LastUsage = DateTime.Now.ToFileTimeUtc();
                    context.SubmitChanges();
                }
                return model;
            }
        }

        public IEnumerable<BookModel> GetAll()
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
               return (from t in context.Books
                            where !t.Hidden && !t.Deleted
                            orderby t.Title
                            select t).ToList();
            }
        }

        public IEnumerable<BookModel> GetRecent(int count)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                return
                    context.Books.Where(b => !b.Hidden && !b.Deleted)
                           .OrderByDescending(b => b.LastUsage)
                           .ThenByDescending(b => b.CreatedDate)
                           .Take(count).ToList();
            }
        }

        public IEnumerable<BookModel> GetFavourites()
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                return (from t in context.Books
                        where !t.Hidden && t.IsFavourite && !t.Deleted
                        orderby t.Title
                        select t).ToList<BookModel>();
            }
        }
        
        public IEnumerable<BookModel> GetTrials()
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                return context.Books.Where(b => b.Trial).ToList();
            }
        }

        public bool CheckUniqueId(string uniqueId)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                return !context.Books.Any(t => t.UniqueID == uniqueId && !t.Deleted);
            }
        }

        public BookModel GetBookByUniqueId(string uniqueId)
        {
            if (string.IsNullOrEmpty(uniqueId))
                return null;

            using (BookDataContext context = BookDataContext.Connect())
            {
                return context.Books.FirstOrDefault(t => t.UniqueID == uniqueId);
            }
        }


        public void RemoveFromDataBase(string bookID)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                BookModel model = context.Books.FirstOrDefault(t => t.BookID == bookID);
                if (model != null)
                {
                    context.Books.DeleteOnSubmit(model);
                    context.SubmitChanges();
                }
            }
        }

        private static void DeleteFolder(string bookId)
        {
            try
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string path = ModelExtensions.GetBookCoverPath(bookId);
                    if (file.FileExists(path))
                    {
                        file.DeleteFile(path);
                    }
                    string coverPath = ModelExtensions.GetBookFullCoverPath(bookId);
                    if (file.FileExists(coverPath))
                    {
                        file.DeleteFile(coverPath);
                    }
                    if (file.DirectoryExists(bookId))
                    {
                        string[] fileNames = file.GetFileNames(bookId + @"\*.*");
                        bool flag = false;
                        foreach (string fileName in fileNames)
                        {
                            try
                            {
                                file.DeleteFile(Path.Combine(bookId, fileName));
                            }
                            catch (Exception)
                            {
                                flag = true;
                            }
                        }
                        if (!flag)
                        {
                            file.DeleteDirectory(bookId);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static void MarkAsDeleted(string bookID)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                BookModel model = context.Books.FirstOrDefault(t => t.BookID == bookID);
                if (model != null)
                {
                    model.Deleted = true;
                    context.SubmitChanges();
                }
            }       
        }
    }
}