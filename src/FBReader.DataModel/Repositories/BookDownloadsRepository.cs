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

using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using FBReader.DataModel.Model;

namespace FBReader.DataModel.Repositories
{
    public class BookDownloadsRepository : IBookDownloadsRepository
    {
        private static object _locker = new object();

        public void Add(BookDownloadModel item)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                context.Downloads.InsertOnSubmit(item);
                context.SubmitChanges();
            }
        }

        public IEnumerable<BookDownloadModel> GetItems()
        {
            lock (_locker)
            {
                using (BookDataContext context = BookDataContext.Connect())
                {
                    return (from t in context.Downloads
                        orderby t.DownloadID
                        select t).ToList<BookDownloadModel>();
                }
            }
        }

        public void Remove(int downloadId)
        {
            lock (_locker)
            {
                using (BookDataContext context = BookDataContext.Connect())
                {
                    try
                    {
                        BookDownloadModel model = context.Downloads.FirstOrDefault(t => t.DownloadID == downloadId);
                        if (model != null)
                        {
                            context.Downloads.DeleteOnSubmit(model);
                            context.SubmitChanges(ConflictMode.ContinueOnConflict);
                        }
                    }
                    catch (ChangeConflictException)
                    {
                        context.ChangeConflicts.ResolveAll(RefreshMode.KeepChanges);
                        context.SubmitChanges();
                    }
                }
            }
        }
    }
}