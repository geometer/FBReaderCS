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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FBReader.App.Converters
{
    public class ImageNameToUriConverter: IValueConverter
    {
        private const string WVGA_IMAGE_HOME_FOLDER = "/Resources/Images/WVGA/";
        private const string WXGA_IMAGE_HOME_FOLDER = "/Resources/Images/WVGA/";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageName = value as string;
            
            if (string.IsNullOrEmpty(imageName))
            {
                return string.Empty;
            }

            var imagePath = string.Concat(WVGA_IMAGE_HOME_FOLDER, imageName);
            if (Application.Current.Host.Content.ScaleFactor > 100)
            {
                imagePath = string.Concat(WXGA_IMAGE_HOME_FOLDER, imageName);
            }

            return new Uri(imagePath, UriKind.Relative);
        } 

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
