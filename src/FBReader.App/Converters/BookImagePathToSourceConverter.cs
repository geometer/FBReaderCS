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
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using FBReader.App.Interaction;
using FBReader.DataModel.Model;

namespace FBReader.App.Converters
{
    public class BookImagePathToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            
            var bookModel = (BookModel) value;
            var fullCover = System.Convert.ToBoolean(parameter);

            var path = fullCover ? ModelExtensions.GetBookFullCoverPath(bookModel.BookID) : ModelExtensions.GetBookCoverPath(bookModel.BookID);

            var task = Task.Run(async () =>
                {
                    var taskCompletionSource = new TaskCompletionSource<BitmapImage>();
                    MemoryStream memoryStream = null;

                    using (var storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        try
                        {
                            if (!storeForApplication.FileExists(path))
                            {
                                throw new FileNotFoundException(path);
                            }
                            using (var storageFileStream = storeForApplication.OpenFile(path, FileMode.Open))
                            {
                                memoryStream = new MemoryStream((int)storageFileStream.Length);
                                storageFileStream.CopyTo(memoryStream, 4096);
                            }
                        }
                        catch (Exception)
                        {
                            taskCompletionSource.SetResult(null);
                        }
                    }

                    Execute.OnUIThread(() =>
                        {
                            var bitmapImage = new BitmapImage();
                            bitmapImage.SetSource(memoryStream);
                            taskCompletionSource.SetResult(bitmapImage);
                        });

                    return await taskCompletionSource.Task;
                });

            return new TaskCompletionNotifier<BitmapImage>(task);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }
}