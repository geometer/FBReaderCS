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
using System.Threading;
using FBReader.Common.Exceptions;
using FBReader.Render.Downloading.Model;

namespace FBReader.Render.Downloading.Loaders
{
    public class DirectFileLoader : BaseFileLoader
    {
        public override Stream LoadFile(string uri, bool isZipFile)
        {
            var asyncContext = new AsyncContext
                               {
                                   Stream = new MemoryStream(),
                                   WaitHandle = new AutoResetEvent(false),
                                   IsZip = isZipFile
                               };
            
            var webClient = new WebClient();
            webClient.OpenReadCompleted += WebClientOnOpenReadCompleted;
            webClient.OpenReadAsync(new Uri(uri), asyncContext);
            asyncContext.WaitHandle.WaitOne();

            if (asyncContext.Error != null)
                throw asyncContext.Error;
            return asyncContext.Stream;
        }

        private void WebClientOnOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            var asyncContext = (AsyncContext)e.UserState;   
            try
            {
                if (e.Error != null)
                {
                    var webException = e.Error as WebException;
                    if (webException != null)
                    {
                        if (webException.Status == WebExceptionStatus.RequestCanceled)
                        {
                            asyncContext.Error = new RestartException(e.Error.Message, e.Error);
                            return;
                        }
                    }

                    asyncContext.Error = e.Error;
                    return;
                }

                Stream stream = e.Result;
                if (asyncContext.IsZip)
                    stream = UnZip(stream);
                stream.CopyTo(asyncContext.Stream);
            }
            catch (Exception ex)
            {
                asyncContext.Error = ex;
            }
            finally
            {
                asyncContext.WaitHandle.Set();
            }
        }
    }
}