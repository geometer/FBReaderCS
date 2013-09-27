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
using System.Xml.Linq;
using FBReader.DataModel.Model;
using FBReader.Tokenizer.Data;

namespace FBReader.Render.Tools
{
    internal static class ToolsRepository
    {
        internal static void SaveAnchors(IEnumerable<AnchorModel> anchors)
        {
            using (var bookDataContext = BookDataContext.Connect())
            {
                foreach (var anchorModel in anchors)
                {
                    bookDataContext.Anchors.InsertOnSubmit(anchorModel);
                }

                bookDataContext.SubmitChanges();
            }
        }

        internal static void SaveChapters(IEnumerable<ChapterModel> chapters)
        {
            using (var bookDataContext = BookDataContext.Connect())
            {
                foreach (var chapterModel in chapters)
                {
                    bookDataContext.Chapters.InsertOnSubmit(chapterModel);
                }

                bookDataContext.SubmitChanges();
            }
        }

        internal static IEnumerable<ChapterModel> GetChapters(string bookId)
        {
            try
            {
                using (var bookDataContext = BookDataContext.Connect())
                {
                    return bookDataContext.Chapters.Where(c => c.BookID == bookId).ToList();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static int GetAnchorsTokenId(string linkId, string bookId)
        {
            try
            {
                using (var bookDataContext = BookDataContext.Connect())
                {
                    var hash = linkId.GetHashCode();
                    var statement = new Func<AnchorModel, bool>(t => t.BookID == bookId && t.NameHash == hash && t.Name == linkId);

                    var anchorModel = bookDataContext.Anchors.FirstOrDefault(statement);
                    return anchorModel != null ? anchorModel.TokenID : -1;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        internal static IEnumerable<BookImage> GetImages(string bookId)
        {
            try
            {
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var imagesFilePath = Path.Combine(bookId, ModelConstants.BOOK_IMAGES_FILE_NAME);
                    using (var imagesFileStream = storage.OpenFile(imagesFilePath, FileMode.Open, FileAccess.Read))
                    {
                        var imagesXml = XDocument.Load(imagesFileStream).Root;

                        if (imagesXml == null)
                        {
                            throw new Exception("Can't load images. Something wrong with document");
                        }
                        return imagesXml.Elements("image").Select(t => new BookImage(t)).ToList();
                    }
                }
            }
            catch (Exception)
            {
                return new List<BookImage>();
            }
        }
    }
}