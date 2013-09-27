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

namespace FBReader.Common.Exceptions
{
    public class CatalogNotEnoughMoneyException : Exception
    {
        public CatalogNotEnoughMoneyException()
        {
        }

        public CatalogNotEnoughMoneyException(CatalogType catalogType, string payMoneyUrl)
        {
            CatalogType = catalogType;
            PayMoneyUrl = payMoneyUrl;
        }

        public CatalogNotEnoughMoneyException(string message)
            : base(message)
        {
        }

        public CatalogNotEnoughMoneyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CatalogNotEnoughMoneyException(string message, Exception innerException, CatalogType catalogType)
            : base(message, innerException)
        {
            CatalogType = catalogType;
        }

        public CatalogNotEnoughMoneyException(string message, Exception innerException, CatalogType catalogType, string payMoneyUrl)
            : base(message, innerException)
        {
            CatalogType = catalogType;
            PayMoneyUrl = payMoneyUrl;
        }

        public CatalogType CatalogType { get; private set; }

        public string PayMoneyUrl { get; private set; }
    }
}