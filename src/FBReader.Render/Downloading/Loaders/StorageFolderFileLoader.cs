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
using System.IO;
using System.Threading;
using Windows.Phone.Storage.SharedAccess;
using Windows.Storage;
using FBReader.Render.Downloading.Model;
using ICSharpCode.SharpZipLib.Core;

namespace FBReader.Render.Downloading.Loaders
{
    public class StorageFolderFileLoader : BaseFileLoader
    {
        public override Stream LoadFile(string pathFile, bool isZipFile)
        {
            var context = new AsyncContext
            {
                WaitHandle = new AutoResetEvent(false)
            };

            LoadFileAsync(pathFile, context);
            context.WaitHandle.WaitOne();

            if (context.Error != null)
            {
                throw context.Error;
            }

            return context.Stream;
        }

        private async void LoadFileAsync(string fileId, AsyncContext context)
        {
            try
            {
                var file = await SharedStorageAccessManager.CopySharedFileAsync(ApplicationData.Current.LocalFolder,
                    fileId + ".tmp", NameCollisionOption.ReplaceExisting,
                    fileId);

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    context.Stream = CopyStream(stream);
                }
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception exception)
            {
                context.Error = exception;
            }
            finally
            {
                context.WaitHandle.Set();
            }
        }

        private Stream CopyStream(Stream source)
        {
            var buffer = new byte[4096];
            var memoryStream = new MemoryStream();
            StreamUtils.Copy(source, memoryStream, buffer);
            memoryStream.Position = 0L;

            return memoryStream;
        }
    }
}
