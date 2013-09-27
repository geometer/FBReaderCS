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
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.Render.Downloading.Model;
using ICSharpCode.SharpZipLib.Core;
using Microsoft.Live;

namespace FBReader.Render.Downloading.Loaders
{
    public class SkyDriveFileLoader : BaseFileLoader
    {
        private readonly ILiveLogin _liveLogin;

        public SkyDriveFileLoader()
        {
            _liveLogin = new LiveLogin();
        }

        public override Stream LoadFile(string fileID, bool isZipFile)
        {
            var context = new AsyncContext
                              {
                                  WaitHandle = new AutoResetEvent(false)
                              };

            LoadFileAsync(fileID, context);
            context.WaitHandle.WaitOne();

            if (context.Error != null)
            {
                Thread.MemoryBarrier();
                throw context.Error;
            }
            if (!isZipFile)
            {
                return context.Stream;
            }

            Stream source = UnZip(context.Stream);
            var buffer = new byte[4096];
            var memoryStream = new MemoryStream();
            StreamUtils.Copy(source, memoryStream, buffer);
            memoryStream.Position = 0L;
            return memoryStream;
        }

        private async void LoadFileAsync(string fileID, AsyncContext context)
        {
            try
            {
                LiveConnectClient skyDrive = await _liveLogin.Login();
                if (skyDrive == null)
                {
                    context.Error = new Exception("Login Required");
                    return;
                }

                LiveOperationResult fileData = await skyDrive.GetAsync(fileID);

                string path = FixSkyDriveUrl((string) fileData.Result["source"]);

                LiveDownloadOperationResult downloadResult = await skyDrive.DownloadAsync(path);

                var buffer = new byte[4096];
                var memoryStream = new MemoryStream();
                StreamUtils.Copy(downloadResult.Stream, memoryStream, buffer);
                memoryStream.Position = 0L;
                context.Stream = memoryStream;

            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.RequestCanceled)
                {
                    context.Error = new RestartException(e.Message, e);
                    return;
                }
                context.Error = e;
            }
            catch (TaskCanceledException e)
            {
                context.Error = new RestartException(e.Message, e);
            }
            catch (Exception e)
            {
                context.Error = e;
            }
            finally
            {
                context.WaitHandle.Set();
            }
        }


        private string FixSkyDriveUrl(string s)
        {
            Match match = new Regex("http://storage.live.com/([^/]*)/.*:Binary").Match(s);
            if (match.Success)
                return match.Value;
            return s;
        }
    }
}
