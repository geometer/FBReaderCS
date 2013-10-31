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

using System.Data.Linq.Mapping;

namespace FBReader.DataModel.Model
{
    [Table(Name = "Downloads")]
    public class BookDownloadModel : BaseTable
    {
        [Column(CanBeNull = false)]
        public int DataSourceID { get; set; }

        [Column(IsPrimaryKey = true, DbType = "int not null identity", IsDbGenerated = true)]
        public int DownloadID { get; set; }

        [Column(CanBeNull = true, DbType = "nvarchar(1024)")]
        public string CatalogItemId { get; set; }

        [Column(CanBeNull = false)]
        public bool IsZip { get; set; }

        [Column(CanBeNull = false, DbType = "nvarchar(1024)")]
        public string Name { get; set; }

        [Column(CanBeNull = false, DbType = "nvarchar(1024)")]
        public string Path { get; set; }

        [Column(CanBeNull = false, DbType = "nvarchar(1024)")]
        public string Type { get; set; }

        [Column(CanBeNull = false)]
        public bool IsTrial { get; set; }

        [Column(CanBeNull = true, DbType = "nvarchar(1024)")]
        public string Author { get; set; }

        [Column(CanBeNull = true, DbType = "ntext null", UpdateCheck = UpdateCheck.Never)]
        public string Description { get; set; }

        [Column(CanBeNull = true, DbType = "nvarchar(1024)")]
        public string AcquisitionUrl { get; set; }

        [Column(CanBeNull = true, DbType = "nvarchar(1024)")]
        public string AcquisitionType { get; set; }

        [Column(CanBeNull = true, DbType = "ntext null", UpdateCheck = UpdateCheck.Never)]
        public string AcquisitionPrices { get; set; }
    }
}