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
using FBReader.Common;

namespace FBReader.DataModel.Model
{
    [Table(Name = "Catalogs")]
    public class CatalogModel
    {
        //[Column(DbType = "nvarchar(150) not null", IsDbGenerated = false, IsPrimaryKey = true)]
        [Column(DbType = "int not null identity", IsDbGenerated = true, IsPrimaryKey = true)]
        public int Id
        {
            get;
            set;
        }

        [Column(DbType = "nvarchar(1024) null")]
        public string Url
        {
            get;
            set;
        }

        [Column(DbType = "nvarchar(1024) null")]
        public string OpenSearchDescriptionUrl
        {
            get;
            set;
        }

        [Column(DbType = "nvarchar(1024) null")]
        public string SearchUrl
        {
            get;
            set;
        }

        [Column(DbType = "nvarchar(1024) not null")]
        public string Title
        {
            get;
            set;
        }

        [Column(DbType = "nvarchar(1024) null")]
        public string Description
        {
            get;
            set;
        }

        [Column(DbType = "nvarchar(1024) null")]
        public string IconLocalPath
        {
            get;
            set;
        }

        [Column(DbType = "tinyint not null")]
        public CatalogType Type
        {
            get;
            set;
        }

        [Column(CanBeNull = false, DbType = "bit not null default(0)")]
        public bool AccessDenied
        {
            get;
            set;
        }

        [Column(DbType = "nvarchar(1024) null")]
        public string AuthorizationString
        {
            get; 
            set;
        }
    }
}