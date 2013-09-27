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
using System.Linq;
using System.Threading;
using FBReader.Render.Downloading.Model;
using ICSharpCode.SharpZipLib.Core;
using Microsoft.Phone.Storage;

namespace FBReader.Render.Downloading.Loaders
{
    public class SdCardFileLoader : BaseFileLoader
    {
        public override Stream LoadFile(string pathFile, bool isZipFile)
        {
            var context = new AsyncContext
                              {
                                  WaitHandle = new AutoResetEvent(false),
                                  IsZip = isZipFile
                              };

            LoadFileAsync(pathFile, context);
            context.WaitHandle.WaitOne();

            if (context.Error != null)
            {
                throw context.Error;
            }

            if (!context.IsZip)
            {
                return context.Stream;
            }

            Stream source = UnZip(context.Stream);
            return CopyStream(source);
        }

        private async void LoadFileAsync(string fileId, AsyncContext context)
        {
            try
            {
                var sdCardStorage = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();
                if (sdCardStorage == null)
                {
                    context.Error = new Exception("There are no external storage devices found.");
                    context.WaitHandle.Set();
                    return;
                }

                var file = await sdCardStorage.GetFileAsync(fileId);
                var stream = await file.OpenForReadAsync();

                context.Stream = CopyStream(stream);
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