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
using System.Windows.Data;
using FBReader.Localization;
using FBReader.Render.Downloading.Model;

namespace FBReader.App.Converters
{
    public class DownloadStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var downladStatus = (DownloadStatus) value;
            switch (downladStatus)
            {
                case DownloadStatus.Pending:
                    return UIStrings.DownloadStatus_Pending;
                case DownloadStatus.Downloading:
                    return UIStrings.DownloadStatus_Downloading;
                case DownloadStatus.Parsing:
                    return UIStrings.DownloadStatus_Parsing;
                case DownloadStatus.Completed:
                    return UIStrings.DownloadStatus_Completed;
                case DownloadStatus.Error:
                    return UIStrings.DownloadStatus_Error;
                default:
                    throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
