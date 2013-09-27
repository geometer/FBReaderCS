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
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FBReader.AppServices.Controller.Translation
{
    public class AdmAuthentication
    {
        private const int RefreshTokenDuration = 9;
        private readonly string _clientId;
        private readonly string _clientSecret;

        private Timer _renewTimer;
        private AdmAccessToken _token;
        private DateTime _lastTokenTimestamp = DateTime.MaxValue;

        public AdmAuthentication(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task<AdmAccessToken> GetAccessToken()
        {
            if (DateTime.Now + TimeSpan.FromMinutes(RefreshTokenDuration) > _lastTokenTimestamp && _token != null)
                return _token;

            await RequestNewToken();

            _renewTimer = new Timer(OnTokenExpiredCallback, this, TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));

            return _token;
        }

        private async void OnTokenExpiredCallback(object stateInfo)
        {
            try
            {
                await RequestNewToken();
            }
            catch (Exception ex)
            {
                _token = null;
            }
            finally
            {
                _renewTimer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
            }
        }


        private async Task RequestNewToken()
        {
            const string datamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";

            var requestData = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", 
                HttpUtility.UrlEncode(_clientId),
                HttpUtility.UrlEncode(_clientSecret));

            byte[] bytes = Encoding.UTF8.GetBytes(requestData);
            
            var http = new HttpClient();
            var content = new ByteArrayContent(bytes);

            HttpResponseMessage response = await http.PostAsync(datamarketAccessUri, content);

            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                var serializer = new DataContractJsonSerializer(typeof (AdmAccessToken));
                
                _token = (AdmAccessToken)serializer.ReadObject(responseStream);

                _lastTokenTimestamp = DateTime.Now;
            }
        }
    }
}