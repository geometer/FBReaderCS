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
using System.Runtime.Serialization;

namespace FBReader.DataModel.Model
{
    [KnownType(typeof(CatalogBookItemModel))]
    [KnownType(typeof(LitresBookshelfCatalogItemModel))]
    [KnownType(typeof(LitresTopupCatalogItemModel))]
    [DataContract]
    public class CatalogItemModel
    {
        [DataMember]
        public string Title
        {
            get;
            set;
        }

        [DataMember]
        public string Description
        {
            get;
            set;
        }

        [DataMember]
        public string Author
        {
            get; 
            set;
        }

        [DataMember]
        public Uri ImageUrl
        {
            get;
            set;
        }

        [DataMember]
        public string OpdsUrl
        {
            get;
            set;
        }

        [DataMember]
        public string HtmlUrl
        {
            get;
            set;
        }

        protected bool Equals(CatalogItemModel other)
        {
            return string.Equals(Title, other.Title) && string.Equals(OpdsUrl, other.OpdsUrl) && string.Equals(HtmlUrl, other.HtmlUrl);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CatalogItemModel)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OpdsUrl != null ? OpdsUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (HtmlUrl != null ? HtmlUrl.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}