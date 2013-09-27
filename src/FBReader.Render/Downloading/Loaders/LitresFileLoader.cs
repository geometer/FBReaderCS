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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FBReader.Common.Exceptions;
using FBReader.Render.Downloading.Model;

namespace FBReader.Render.Downloading.Loaders
{
    public class LitresFileLoader : BaseFileLoader
    {
        public override Stream LoadFile(string pathFile, bool isZipFile)
        {
            var asyncContext = new AsyncContext
                {
                    Stream = new MemoryStream(),
                    WaitHandle = new AutoResetEvent(false),
                    IsZip = isZipFile
                };

            LoadFileAsync(pathFile, asyncContext);
            asyncContext.WaitHandle.WaitOne();

           if (asyncContext.Error != null)
           {
               throw asyncContext.Error;
           }
           return asyncContext.Stream;
        }

        private async void LoadFileAsync(string pathFile, AsyncContext context)
        {
            try
            {
                HttpClient httpClient = CreateHttpClient();
                HttpContent httpContent = CreateHttpContent(pathFile);

                HttpResponseMessage response = await httpClient.PostAsync(pathFile.Split('?')[0], httpContent);
                var stream = await response.Content.ReadAsStreamAsync();

                if (context.IsZip)
                {
                    stream = UnZip(stream);
                }
                stream.CopyTo(context.Stream);
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

        private static HttpClient CreateHttpClient()
        {
            var httpClientHandler = new HttpClientHandler();
            if (httpClientHandler.SupportsAutomaticDecompression)
            {
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var httpClient = new HttpClient(httpClientHandler);
            return httpClient;
        }

        private static HttpContent CreateHttpContent(string path)
        {
            var urlParts = path.Split('?')[1];
            var bodyParts = urlParts.Split('&');

            var postParams = bodyParts.Select(bodyPart => bodyPart.Split('='))
                                      .ToDictionary(keyValue => keyValue[0], keyValue => keyValue[1]);

            return new FormUrlEncodedContent(postParams);
        }
    }
}
