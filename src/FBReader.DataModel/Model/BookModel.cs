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

using System;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq.Mapping;

namespace FBReader.DataModel.Model
{
    [Index(Columns = "Author", Name = "IX_Books_Author")]
    [Table(Name = "Books")]
    public class BookModel : BaseTable
    {
        [Column(DbType = "nvarchar(150) not null", IsDbGenerated = false, IsPrimaryKey = true)]
        public string BookID { get; set; }

        [Column(DbType = "nvarchar(16) not null")]
        public string Type { get; set; }

        [Column(DbType = "nvarchar(1024) not null")]
        public string Title { get; set; }

        [Column(DbType = "nvarchar(1024) not null")]
        public string Author { get; set; }

        [Obsolete]
        [Column(CanBeNull = false, DbType = "int not null default(0)")]
        public int AuthorHash { get; set; }

        [Column(CanBeNull = false)]
        public int CurrentTokenID { get; set; }

        [Column(CanBeNull = false)]
        public int TokenCount { get; set; }

        [Column(CanBeNull = false)]
        public bool Hidden { get; set; }

        [Column(CanBeNull = false, DbType = "bit not null default(0)")]
        public bool Trial { get; set; }

        [Column(CanBeNull = false, UpdateCheck = UpdateCheck.Never)]
        public long LastUsage { get; set; }

        [Column(CanBeNull = true)]
        public int? WordCount { get; set; }

        [Column(CanBeNull = false, DbType = "bit not null default(0)")]
        public bool Deleted { get; set; }

        [Column(CanBeNull = true)]
        public long? CreatedDate { get; set; }

        [Column(DbType = "nvarchar(1024) null")]
        public string UniqueID { get; set; }

        [Column(DbType = "nvarchar(1024) null")]
        public string Url { get; set; }

        [Column(CanBeNull = false)]
        public bool IsFavourite { get; set; }

        [Column(DbType = "nvarchar(1024) null")]
        public string CatalogItemId { get; set; }

        [Column(DbType = "nvarchar(10) null")]
        public string Language { get; set; }

        [Column(DbType = "ntext null", UpdateCheck = UpdateCheck.Never, Name = "Annotation")]
        public string Description { get; set; }
    }
}