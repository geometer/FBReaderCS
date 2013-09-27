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

using System.Globalization;
using System.Xml.Linq;

namespace FBReader.Common.ExtensionMethods
{
    public static class XElementExtentions
    {
        public static string GetStringAttribute(this XElement element, XName name)
        {
            XAttribute xattribute = element.Attribute(name);
            if (xattribute != null)
                return xattribute.Value;
            return string.Empty;
        }

        public static int GetIntAttribute(this XElement element, XName name, int defaultValue = 0)
        {
            XAttribute xattribute = element.Attribute(name);
            int number;
            if (xattribute != null && int.TryParse(xattribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                return number;
            return defaultValue;
        }
    }
}