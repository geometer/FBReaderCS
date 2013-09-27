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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using FBReader.DataModel.Model;
using FBReader.WebClient.DTO;
using FBReader.WebClient.DTO.OpenSearchDescription;
using FBReader.WebClient.Exceptions;

namespace FBReader.WebClient
{
    public class WebDataGateway : IWebDataGateway
    {
        private readonly IWebClient _webClient;
        
        public WebDataGateway(IWebClient webClient)
        {
            _webClient = webClient;
        }

        public async Task<CatalogModel> GetCatalogInfo(string path)
        {
            return await DoRequest<CatalogDto, CatalogModel>(path, dto => dto.ToCatalogModel(path), exception =>
                {
                    if (exception.InnerException is XmlException || exception is WebException)
                    {
                        throw new OpdsFormatException(exception.Message, exception.InnerException);
                    }

                    throw exception;
                });
        }

        public async Task<OpenSearchDescriptionModel> GetOpenSearchDescriptionModel(string path, string authorityUrl)
        {
            return await DoRequest<OpenSearchDescriptionDto, OpenSearchDescriptionModel>(path, dto => dto.ToDescription(authorityUrl), exception =>
                {
                    if (exception.InnerException is XmlException || exception is WebException)
                    {
                        throw new OpdsFormatException(exception.Message, exception.InnerException);
                    }
                    throw exception;
                });
        }

        private async Task<TModel> DoRequest<TDto, TModel>(string path, Func<TDto, TModel> toModel, Action<Exception> exceptionHandler) where TDto: class 
                                                                                                                                        where TModel: class
        {
            try
            {
                var response = await _webClient.DoGetAsync(path, null);
                var stream = await response.Content.ReadAsStreamAsync();

                var dto = DeserializeData<TDto>(stream);

                var model = toModel(dto);
                return model;
            }
            catch (Exception exception)
            {
                if (exceptionHandler != null)
                {
                    exceptionHandler(exception);
                }
            }

            return null;
        }

        private static T DeserializeData<T>(Stream stream) where T: class
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            var validatedString = ValidateServerResponse(new StreamReader(stream).ReadToEnd());
            var byteArray = Encoding.UTF8.GetBytes(validatedString);
            stream = new MemoryStream(byteArray);
            
            var catalogDto = xmlSerializer.Deserialize(new StreamReader(stream, Encoding.UTF8)) as T;
            return catalogDto;
        }

        private static string ValidateServerResponse(string responseString)
        {
            responseString = ValidateForUnescapedCdata(responseString);
            responseString = ValidateForUnescapedQuotesInTitle(responseString);
            responseString = ValidateForUnescapedAmpersands(responseString);
            responseString = ValidateForUnescapedSigns(responseString);

            return responseString;
        }

        private static string ValidateForUnescapedCdata(string responseString)
        {
            var pattern = new Regex("<!\\[CDATA\\[(.*)\\]\\]>");
            var matchCollection = pattern.Matches(responseString);

            foreach (Match match in matchCollection)
            {
                var foundString = match.Value;
                var newString = foundString.Replace("<![CDATA[", string.Empty).Replace("]]>", string.Empty);
                responseString = responseString.Replace(foundString, newString);
            }

            return responseString;
        }

        private static string ValidateForUnescapedAmpersands(string responseString)
        {
            responseString = responseString.Replace("&", "&amp;");
            return responseString;
        }

        private static string ValidateForUnescapedSigns(string responseString)
        {
            var pattern = new Regex("<title>(?<TargetValue>(?=.*?[<>].*?</title>)(.*?))</title>");
            var matchCollection = pattern.Matches(responseString); 

            foreach (Match match in matchCollection)
            {
                var foundString = match.Groups["TargetValue"].Value;
                if (string.IsNullOrEmpty(foundString))
                {
                    continue;
                }
                var newString = foundString.Replace("<", "&lt;").Replace(">", "&gt;");
                responseString = responseString.Replace(foundString, newString);
            }

            return responseString;
        }

        private static string ValidateForUnescapedQuotesInTitle(string responseString)
        {
            var pattern = new Regex("title=\"(?<TargetTitle>(?=(.*)\"(.*)\").*)\" ((.*)=\"|(/))");
            var matchCollection = pattern.Matches(responseString);

            foreach (Match match in matchCollection)
            {
                var foundString = match.Groups["TargetTitle"].Value;
                var newString = foundString.Replace("\"", "&quot;");
                responseString = responseString.Replace(foundString, newString);
            }

            return responseString;
        }
    }
}