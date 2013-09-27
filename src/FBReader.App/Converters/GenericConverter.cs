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
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace FBReader.App.Converters
{
    public class GenericCase
    {
        public object Value { get; set; }
        public string Case { get; set; }
    }

    [ContentProperty("Cases")]
    public class GenericSwitchConverter : DependencyObject, IValueConverter
    {
        public GenericSwitchConverter()
        {
            Cases = new List<GenericCase>();
        }

        public object Other { get; set; }
        public List<GenericCase> Cases { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strVal = value.ToString();

            foreach (var genericCase in Cases)
            {
                if (genericCase.Case.Equals(strVal, StringComparison.InvariantCultureIgnoreCase))
                    return genericCase.Value;
            }

            return Other;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
