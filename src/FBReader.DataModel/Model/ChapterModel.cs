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

using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq.Mapping;

namespace FBReader.DataModel.Model
{
    [Table(Name = "Chapters"), Index(Name = "IX_Chapters_BookID", Columns = "BookID")]
    public class ChapterModel : BaseTable
    {
        [Column(DbType = "nvarchar(36) not null")]
        public string BookID { get; set; }

        [Column(IsPrimaryKey = true, DbType = "int not null identity", IsDbGenerated = true)]
        public int ItemID { get; set; }

        [Column(DbType = "int not null")]
        public int Level { get; set; }

        [Column(DbType = "int null")]
        public int? MinTokenID { get; set; }

        [Column(DbType = "nvarchar(1024) not null")]
        public string Title { get; set; }

        [Column(DbType = "int not null")]
        public int TokenID { get; set; }
    }
}