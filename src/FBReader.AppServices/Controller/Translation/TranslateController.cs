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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace FBReader.AppServices.Controller.Translation
{
    public class TranslateController
    {
        private const string ClientID = "FBReader_WP8";
        private const string ClientSecret = "cr8f5s0RdT8VAQXEHRtjG37GHQMUXF6UpcUzGiIb2PI=";

        private readonly AdmAuthentication _auth;

        public TranslateController()
        {
            _auth = new AdmAuthentication(ClientID, ClientSecret);
        }

        public async Task<string> Translate(string text, string toLang)
        {
            AdmAccessToken authToken = await GetAccessToken();

            string uri = string.Format("http://api.microsofttranslator.com/v2/Http.svc/Translate?text={0}&to={1}&contentType={2}", text, toLang, "text/plain");

            using (Stream stream = await MakeRequest(uri, authToken))
            {
                var serializer = new DataContractSerializer(typeof (String));
                var translation = (string) serializer.ReadObject(stream);

                return translation;
            }
        }

        public async Task<List<string>> GetLanguages()
        {
            AdmAccessToken authToken = await GetAccessToken();

            string uri = string.Format("http://api.microsofttranslator.com/V2/Http.svc/GetLanguagesForTranslate");

            using (Stream stream = await MakeRequest(uri, authToken))
            {
               
                var serializer = new DataContractSerializer(typeof(string[]));
                var translationLangs = (string[])serializer.ReadObject(stream);

                return translationLangs.ToList();
            }
        }

        private Task<Stream> MakeRequest(string uri, AdmAccessToken authToken)
        {
            var task = new TaskCompletionSource<Stream>();

            if (authToken == null)
            {
                task.SetException(new TranslationException(new NullReferenceException("authToken")));
                return task.Task;
            }

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers["Authorization"] = "Bearer " + authToken.access_token;

            IAsyncResult ar = null;
            ar = httpWebRequest.BeginGetResponse(delegate
                                                 {
                                                     try
                                                     {
                                                         Stream stream = httpWebRequest.EndGetResponse(ar).GetResponseStream();
                                                         task.SetResult(stream);
                                                     }
                                                     catch (Exception e)
                                                     {
                                                         task.SetException(new TranslationException(e));
                                                     }
                                                     
                                                 }, null);

            return task.Task;
        }

        private async Task<AdmAccessToken> GetAccessToken()
        {
            try
            {
                return await _auth.GetAccessToken();
            }
            catch (Exception e)
            {
                throw new TranslationException(e);
            }
        }
    }
}