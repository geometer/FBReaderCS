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
using System.Linq;
using FBReader.DataModel.Model;

namespace FBReader.DataModel.Repositories
{
    public class CatalogRepository : ICatalogRepository
    {
        public void Add(CatalogModel catalog)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                context.Catalogs.InsertOnSubmit(catalog);
                context.SubmitChanges();
            }
        }

        public void Remove(CatalogModel catalog)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                CatalogModel model = context.Catalogs.FirstOrDefault(t => t.Id == catalog.Id);
                if (model != null)
                {
                    context.Catalogs.DeleteOnSubmit(model);
                    context.SubmitChanges();
                }
            }
        }

        public void Save(CatalogModel catalog)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                context.Catalogs.Attach(catalog);
                context.Refresh(0, catalog);
                context.SubmitChanges();
            }
        }

        public int Count()
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                return context.Catalogs.Count();
            }
        }

        public CatalogModel Get(int catalogId)
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                CatalogModel model = context.Catalogs.FirstOrDefault(t => t.Id == catalogId);
                return model;
            }
        }

        public IEnumerable<CatalogModel> GetAll()
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                return context.Catalogs.ToList();
            }
        }
    }
}