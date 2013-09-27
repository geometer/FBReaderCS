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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Live;

namespace FBReader.Common
{
    public class LiveLogin : ILiveLogin
    {
        private const string LiveClientId = "000000004010423E";
        private static readonly string[] Scopes =
            new[]
                {
                    "wl.signin",
                    "wl.offline_access",
                    "wl.skydrive",
                    "wl.skydrive_update"
                };

        private readonly Lazy<LiveAuthClient> _lazyAuthClient = new Lazy<LiveAuthClient>(() => new LiveAuthClient(LiveClientId));
        private AsyncContext _context;

        private LiveAuthClient AuthClient
        {
            get { return _lazyAuthClient.Value; }
        }

        public async Task<LiveConnectClient> Login()
        {
            LiveLoginResult result = await AuthClient.InitializeAsync(Scopes);
            if (result.Status == LiveConnectSessionStatus.Connected)
            {
                return new LiveConnectClient(result.Session);
            }
            _context = new AsyncContext
                                {
                                    WaitHandle = new AutoResetEvent(false)
                                };
            Deployment.Current.Dispatcher.BeginInvoke(() => Login(_context));
            await Task.Factory.StartNew(() => _context.WaitHandle.WaitOne());

            if (_context.Error != null)
            {
                throw _context.Error;
            }
            result = _context.LoginResult;

            if (result.Status == LiveConnectSessionStatus.Connected)
            {
                return new LiveConnectClient(result.Session);
            }
            return null;
            
        }

        public void Logout()
        {
            Deployment.Current.Dispatcher.BeginInvoke(AuthClient.Logout);
        }

        private async void Login(AsyncContext context)
        {
            try
            {
                context.LoginResult = await AuthClient.LoginAsync(Scopes);
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

        private class AsyncContext
        {
            public EventWaitHandle WaitHandle { get; set; }
            public LiveLoginResult LoginResult { get; set; }
            public Exception Error { get; set; }
        }

    }
}
