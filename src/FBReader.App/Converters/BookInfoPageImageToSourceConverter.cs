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
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using FBReader.App.Interaction;
using FBReader.AppServices.ViewModels.Pages;

namespace FBReader.App.Converters
{
    public class BookInfoPageImageToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModel = (BookInfoPageViewModel) value;
            if (viewModel.Book != null)
            {
                var converter = new BookImagePathToSourceConverter();
                return converter.Convert(viewModel.Book, targetType, true, culture);
            }

            return new TaskCompletionNotifier<BitmapImage>(Task.Run(async () =>
                {
                    var taskCompletionSource = new TaskCompletionSource<BitmapImage>();
                    Execute.OnUIThread(() => taskCompletionSource.SetResult(viewModel.ImageUrl != null
                                                                                ? new BitmapImage(viewModel.ImageUrl)
                                                                                : null));
                    return await taskCompletionSource.Task;
                }));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}