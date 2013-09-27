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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FBReader.WebClient
{
    public class WebClient : IWebClient
    {
        private readonly List<CancellationTokenSource> _cancellationTokens = new List<CancellationTokenSource>();

        public void Cancel()
        {
            lock (((ICollection)_cancellationTokens).SyncRoot)
            {
                try
                {
                    foreach (var source in _cancellationTokens)
                    {
                        if (!source.Token.CanBeCanceled)
                        {
                            continue;
                        }

                        source.Cancel();
                        source.Dispose();
                    }
                }
                finally
                {
                    _cancellationTokens.Clear();
                }
            }
        }

        public async Task<HttpResponseMessage> DoPostAsync(string url, Dictionary<string, string> postParameters)
        {
            var taskSource = new TaskCompletionSource<HttpResponseMessage>();
            var cancellationToken = CreateCancellationToken();

            if (cancellationToken.IsCancellationRequested)
            {
                taskSource.SetCanceled();
            }
            else
            {
                HttpClient httpClient = CreateHttpClient();
                HttpContent httpContent = CreateHttpContent(postParameters);

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    HttpResponseMessage response = await httpClient.PostAsync(url, httpContent, cancellationToken);
                    taskSource.SetResult(response);
                }
                catch (OperationCanceledException)
                {
                    taskSource.SetCanceled();
                }
                catch (Exception exp)
                {
                    taskSource.SetException(exp);
                }
                finally
                {
                    RemoveCancellationToken(cancellationToken);
                }

                return await taskSource.Task;
            }

            return await taskSource.Task;
        }

        public async Task<HttpResponseMessage> DoGetAsync(string url, string authorizationString)
        {
            var taskSource = new TaskCompletionSource<HttpResponseMessage>();
            var cancellationToken = CreateCancellationToken();

            if (cancellationToken.IsCancellationRequested)
            {
                taskSource.SetCanceled();
            }
            else
            {
                HttpClient httpClient = CreateHttpClient(authorizationString);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    taskSource.SetResult(response);
                }
                catch (OperationCanceledException)
                {
                    taskSource.SetCanceled();
                }
                catch (HttpRequestException e)
                {
                    if (e.InnerException is WebException)
                        taskSource.SetException(e.InnerException);
                    else
                        taskSource.SetException(e);
                }
                catch (Exception exp)
                {
                    taskSource.SetException(exp);
                }
                finally
                {
                    RemoveCancellationToken(cancellationToken);
                }

                return await taskSource.Task;
            }

            return await taskSource.Task;
        }

        public async Task<HttpResponseMessage> DoHeadAsync(string url)
        {
            var taskSource = new TaskCompletionSource<HttpResponseMessage>();
            var cancellationToken = CreateCancellationToken();

            if (cancellationToken.IsCancellationRequested)
            {
                taskSource.SetCanceled();
            }
            else
            {
                try
                {
                    HttpClient httpClient = CreateHttpClient();
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Head, url);
                    cancellationToken.ThrowIfCancellationRequested();
                    HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    taskSource.SetResult(response);
                }
                catch (OperationCanceledException)
                {
                    taskSource.SetCanceled();
                }
                catch (Exception exp)
                {
                    taskSource.SetException(exp);
                }
                finally
                {
                    RemoveCancellationToken(cancellationToken);
                }

                return await taskSource.Task;
            }

            return await taskSource.Task;
        }

        private static HttpClient CreateHttpClient(string authorizationString = null)
        {
            var httpClientHandler = new HttpClientHandler();
            if (httpClientHandler.SupportsAutomaticDecompression)
            {
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var httpClient = new HttpClient(httpClientHandler);
            if (!string.IsNullOrEmpty(authorizationString))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationString);
            }
            return httpClient;
        }

        private static HttpContent CreateHttpContent(Dictionary<string, string> postParams)
        {
            return postParams != null ? new FormUrlEncodedContent(postParams) : null;
        }

        protected CancellationToken CreateCancellationToken()
        {
            lock (((ICollection)_cancellationTokens).SyncRoot)
            {
                var source = new CancellationTokenSource();
                _cancellationTokens.Add(source);
                return source.Token;
            }
        }

        protected void RemoveCancellationToken(CancellationToken token)
        {
            lock (((ICollection)_cancellationTokens).SyncRoot)
            {
                _cancellationTokens.Remove(_cancellationTokens.SingleOrDefault(s => s.Token.Equals(token)));
            }
        }
    }
}