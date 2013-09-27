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
using System.Data.Linq;
using System.Linq;
using FBReader.DataModel.Changesets;
using Microsoft.Phone.Data.Linq;

namespace FBReader.DataModel.Model
{
    public class BookDataContext : DataContext
    {
        private static readonly object _locker = new object();

        private BookDataContext(): base("Data Source='isostore:/Books.sdf'")
        {
            
        }

        public static BookDataContext Connect()
        {
            lock (_locker)
            {
                using (var tempContext = new BookDataContext())
                {
                    CreateDatabase(tempContext);
                }

                return new BookDataContext();
            }
        }

        private static void CreateDatabase(BookDataContext db)
        {
            var changesets = GetChangesets();

            if (!db.DatabaseExists())
            {
                db.CreateDatabase();

                DatabaseSchemaUpdater databaseSchemaUpdater = db.CreateDatabaseSchemaUpdater();
                foreach (BaseChangeset baseChangeset in changesets.Where(t => t.IsDataInitializer).OrderBy(t => t.Version))
                {
                    baseChangeset.Update(db, databaseSchemaUpdater);
                }
                databaseSchemaUpdater.DatabaseSchemaVersion = changesets.Any() ? changesets.Max(t => t.Version) : 0;
                databaseSchemaUpdater.Execute();
                return;
            }

            foreach (BaseChangeset baseChangeset in changesets.OrderBy(t => t.Version))
            {
                try
                {
                    DatabaseSchemaUpdater databaseSchemaUpdater = db.CreateDatabaseSchemaUpdater();
                    if (databaseSchemaUpdater.DatabaseSchemaVersion < baseChangeset.Version)
                    {
                        baseChangeset.Update(db, databaseSchemaUpdater);
                        databaseSchemaUpdater.DatabaseSchemaVersion = baseChangeset.Version;
                        databaseSchemaUpdater.Execute();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static List<BaseChangeset> GetChangesets()
        {
            return new List<BaseChangeset>
                       {
                           new Changeset1(),
                           new Changeset2(),
                           new Changeset3(),
                           new Changeset4(),
                           new Changeset5(),
                           new Changeset6()
                       };
        }

        public Table<AnchorModel> Anchors;
        public Table<BookmarkModel> Bookmarks;
        public Table<BookModel> Books;
        public Table<ChapterModel> Chapters;
        public Table<BookDownloadModel> Downloads;
        public Table<CatalogModel> Catalogs;
    }
}

