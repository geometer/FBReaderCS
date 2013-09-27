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
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.WebClient;
using FBReader.WebClient.DTO.Litres;

namespace FBReader.AppServices.CatalogReaders.Authorization
{
    public class LitresAuthorizationService : OpdsAuthorizationServiceBase
    {
        private readonly IWebClient _webClient;

        public LitresAuthorizationService(IWebClient webClient, ICatalogRepository catalogRepository, CatalogModel catalogModel)
            : base(catalogRepository, catalogModel)
        {
            _webClient = webClient;
        }

        public override async Task<string> Authorize(string userName, string password, string path)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("userName");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }

            var postParams = CreatePostParams(userName, password);
            var taskComplete = new TaskCompletionSource<string>();

            try
            {
                var response = await _webClient.DoPostAsync(path, postParams);
                var stream = await response.Content.ReadAsStreamAsync();

                var xmlSerializer = new XmlSerializer(typeof (LitresAuthorizationDto));
                var authorizationDto = (LitresAuthorizationDto) xmlSerializer.Deserialize(stream);
                taskComplete.SetResult(authorizationDto.Token);
            }
            catch (InvalidOperationException exp)
            {
                if (exp is WebException)
                {
                    taskComplete.SetException(exp);
                }
                else
                {
                    taskComplete.SetResult(null);
                }
            }

            return await taskComplete.Task;
        }

        private static Dictionary<string, string> CreatePostParams(string userName, string password)
        {
            var dictionary = new Dictionary<string, string>
                {
                    {"login", userName}, 
                    {"pwd", password}
                };
            return dictionary;
        }
    }
}