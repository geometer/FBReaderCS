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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FBReader.AppServices.Services
{
    public static class HttpAuthorizationService
    {
        public static Task<string> CheckHttpAuthorization(string userName, string password, string path)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(path);
            webRequest.Method = "HEAD";

            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Concat(userName, ":", password)));
            var str = string.Concat("Basic ", authString);
            webRequest.Headers["Authorization"] = str;

            var taskComplete = new TaskCompletionSource<string>();
            webRequest.BeginGetResponse(asyncResponse =>
            {
                try
                {
                    var responseRequest = (HttpWebRequest)asyncResponse.AsyncState;
                    var response = (HttpWebResponse)responseRequest.EndGetResponse(asyncResponse);
                    taskComplete.SetResult(response.StatusCode == HttpStatusCode.OK ? authString : null);
                }
                catch (WebException webExc)
                {
                    var statusCode = ((HttpWebResponse) webExc.Response).StatusCode;
                    if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
                    {
                        taskComplete.TrySetResult(null);
                        return;
                    }

                    throw;
                }
            }, webRequest);

            return taskComplete.Task;
        }
    }
}