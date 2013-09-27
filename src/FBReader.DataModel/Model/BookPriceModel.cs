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

using System.Runtime.Serialization;

namespace FBReader.DataModel.Model
{
    [DataContract]
    public class BookPriceModel
    {
        [DataMember]
        public string CurrencyCode
        {
            get; 
            set;
        }

        [DataMember]
        public string Price
        {
            get; 
            set;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Price, CurrencyCode);
        }

        public static BookPriceModel Parse(string data)
        {
            var parts = data.Split(':');
            return new BookPriceModel{Price = parts[0], CurrencyCode = parts[1]};
        }
    }
}