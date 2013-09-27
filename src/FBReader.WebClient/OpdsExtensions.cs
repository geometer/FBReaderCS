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
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.WebClient.DTO;
using FBReader.WebClient.DTO.OpenSearchDescription;
using FBReader.WebClient.Exceptions;

namespace FBReader.WebClient
{
    public static class OpdsExtensions
    {
        private const string REL_ACQUISITION_PREFIX = "http://opds-spec.org/acquisition";
        private const string REL_ACQUISITION_BUY_PREFIX = "http://opds-spec.org/acquisition/buy";
        private const string REL_IMAGE_PREFIX = "http://opds-spec.org/image";
        private const string LITRES_REL_BOOKSHELF_PREFIX = "http://data.fbreader.org/rel/bookshelf";
        private const string LITRES_REL_TOPUP_PREFIX = "http://data.fbreader.org/rel/topup";
        private const string LITRES_PUT_MONEY_LINK_FORMAT = "http://www.litres.ru/pages/put_money_on_account/?sid={0}";
        private const string LITRES_TRIAL_LINK_FORMAT = "http://robot.litres.ru/static/trials/{0}/{1}/{2}/{3}.fb2.zip";

        private const string APPLICATION_PREFIX = "application/";
        private const string CATALOG_LINK_TYPE_PREFIX = "application/atom+xml";
        private const string LITRES_CATALOG_LINK_TYPE_ = "application/litres+xml";
        private const string CATALOG_LINK_TYPE = "profile=opds-catalog";
        private const string OPEN_SEARCH_LINK_TYPE = "application/opensearchdescription+xml";
        private const string HTML_TEXT_LINK_TYPE = "text/html";
        private const string IMAGE_LINK_TYPE = "image/jpeg";
        private const string SEARCH_TERMS = "{searchTerms}";

        private const string LITRES_REL_TOPUP_TITLE = "Пополнить счёт";

        private static readonly HtmlToText HtmlToText = new HtmlToText();
        private static readonly string[] FormatConstants = new[] { "fb2", "epub", "html", "txt" };
        private static readonly string[] ApparentlyIgnoredFormatConstants = new[] {"xhtml"};
        
        public static CatalogModel ToCatalogModel(this CatalogDto catalogDto, string url = null)
        {
            var catalogModel = new CatalogModel { Title = catalogDto.Title};

            if (!string.IsNullOrEmpty(catalogDto.SubTitle))
            {
                var desc = catalogDto.SubTitle;
                for (var i = 0; i < desc.Length; ++i)
                {
                    if (desc[i].Equals('\n') || desc[i].Equals('\t') || desc[i].Equals(' '))
                    {
                        continue;
                    }
                    desc = desc.Substring(i);
                    break;
                }
                catalogModel.Description = desc;
            }

            catalogModel.IconLocalPath = "DesignBookCover.jpg"; // is for test. Remove or change when some stub for opds catalog will be ready.

            if (url != null)
            {
                catalogModel.Url = url;
            }
            else
            {
                var selfLink = catalogDto.Links.SingleOrDefault(l => l.Rel.Equals("self"));
                if (selfLink != null)
                {
                    catalogModel.Url = selfLink.Href;
                }
            }

            var searchLink = catalogDto.Links.SingleOrDefault(l => "search".Equals(l.Rel) && !string.IsNullOrEmpty(l.Href) && OPEN_SEARCH_LINK_TYPE.Equals(l.Type));
            if (searchLink != null)
            {
                Uri searchDescriptionUri;
                if (Uri.TryCreate(new Uri(catalogModel.Url, UriKind.RelativeOrAbsolute), searchLink.Href, out searchDescriptionUri))
                {
                    catalogModel.OpenSearchDescriptionUrl = searchDescriptionUri.ToString();
                }
                else
                {
                    catalogModel.OpenSearchDescriptionUrl = GetValidUrl(searchLink.Href, catalogModel.Url);
                }
                return catalogModel;
            }

            searchLink = catalogDto.Links.SingleOrDefault(l => "search".Equals(l.Rel));
            if (searchLink != null)
            {
                catalogModel.SearchUrl = GetValidUrl(ValidateSearchUrlTemplate(searchLink.Href), catalogModel.Url);
            }

            return catalogModel;
        }

        public static OpenSearchDescriptionModel ToDescription(this OpenSearchDescriptionDto dto, string authorityUrl)
        {
            var searchUrl = dto.Urls.SingleOrDefault(l => !string.IsNullOrEmpty(l.Type) && l.Type.Contains(CATALOG_LINK_TYPE_PREFIX) && 
                                                          !string.IsNullOrEmpty(l.Template) && l.Template.Contains(SEARCH_TERMS));
            if (searchUrl == null)
            {
                throw new OpdsFormatException("Open search description does not contain valid search template url.");
            }

            var template = GetValidUrl(ValidateSearchUrlTemplate(searchUrl.Template), authorityUrl);

            return new OpenSearchDescriptionModel
                {
                    Description = dto.Description, ShortName = dto.ShortName, SearchTemplateUrl = template
                };
        }

        private static string ValidateSearchUrlTemplate(string template)
        {
            if (string.IsNullOrEmpty(template))
            {
                return template;
            }
            
            if (template.Contains("{atom:author}"))
            {
                var pattern = new Regex("(?<AuthorParam>&.*={atom:author})");
                var matchCollection = pattern.Matches(template);
                template = matchCollection.Cast<Match>()
                                          .Select(match => match.Groups["AuthorParam"].Value)
                                          .Where(foundString => !string.IsNullOrEmpty(foundString))
                                          .Aggregate(template, (current, foundString) => current.Replace(foundString, string.Empty));
            }
            if (template.Contains("{startPage?}"))
            {
                var pattern = new Regex("(?<StartPageParam>&.*={startPage\\?})");
                var matchCollection = pattern.Matches(template);
                template = matchCollection.Cast<Match>()
                                          .Select(match => match.Groups["StartPageParam"].Value)
                                          .Where(foundString => !string.IsNullOrEmpty(foundString))
                                          .Aggregate(template, (current, foundString) => current.Replace(foundString, string.Empty));
            }

            template = template.Replace(SEARCH_TERMS, "{0}");

            return template;
        }

        public static CatalogFolderModel ToFolder(this CatalogContentDto catalogContentDto, string authorityUrl, CatalogType type, int catalogId)
        {
            var folderModel = new CatalogFolderModel();
            var folderItems = new List<CatalogItemModel>();

            if (catalogContentDto.Links != null)
            {
                // pagination, next page
                var nextPageLink = catalogContentDto.Links.SingleOrDefault(l => !string.IsNullOrEmpty(l.Rel) && l.Rel.Equals("next")
                                                                            && !string.IsNullOrEmpty(l.Type) &&
                                                                            (l.Type.Contains(CATALOG_LINK_TYPE_PREFIX) || (l.Type.Contains(CATALOG_LINK_TYPE))));
                if (nextPageLink != null)
                {
                    folderModel.NextPageUrl = GetValidUrl(nextPageLink.Href, authorityUrl);
                }
            }

            if (catalogContentDto.Entries != null)
            {
                foreach (var entryDto in catalogContentDto.Entries)
                {
                    CatalogItemModel model = null;

                    // book or just folder
                    var links = entryDto.Links.Where(e =>
                        {
                            if (string.IsNullOrEmpty(e.Rel) || (!e.Rel.StartsWith(REL_ACQUISITION_PREFIX)))
                            {
                                if (string.IsNullOrEmpty(e.Type) 
                                    || !(e.Type.StartsWith(APPLICATION_PREFIX) && FormatConstants.Any(fc => e.Type.Contains(fc)) && !ApparentlyIgnoredFormatConstants.Any(fc => e.Type.Contains(fc))))
                                {
                                    return false;
                                }
                                return true;
                            }

                            return !string.IsNullOrEmpty(e.Type); // && FormatConstants.Any(formatConstant => e.Type.Contains(formatConstant));
                        });

                    if (links.Count() != 0)
                    {
                        // links with price
                        var priceLink = links.SingleOrDefault(l => REL_ACQUISITION_BUY_PREFIX.Equals(l.Rel) && !string.IsNullOrEmpty(l.Href)
                                                                   && l.Prices != null && l.Prices.Any(p => !p.Price.Equals("0.00")));
                        
                        // download links for diff. formats
                        var downloadLinks = (from linkDto in links
                                             where !string.IsNullOrEmpty(linkDto.Type) && !string.IsNullOrEmpty(linkDto.Href) && !REL_ACQUISITION_BUY_PREFIX.Equals(linkDto.Rel)
                                             select new BookDownloadLinkModel
                                                 {
                                                     Type = linkDto.Type, Url = GetValidUrl(linkDto.Href, authorityUrl, true)
                                                 }).Where(dl => FormatConstants.Any(fc => dl.Type.Contains(fc))).ToList();

                        // if there are now supported formats in downloadLinks, but there were some acquisition links with another formats => skip this book.
                        if (!downloadLinks.Any() && priceLink == null)
                        {
                            var htmlBuyLink = links.SingleOrDefault(l => REL_ACQUISITION_BUY_PREFIX.Equals(l.Rel));
                            if (htmlBuyLink == null)
                            {
                                continue;
                            }
                            model = new CatalogItemModel {HtmlUrl = GetValidUrl(htmlBuyLink.Href, authorityUrl)};
                        }
                        else
                        {
                            // this is book
                            BookAcquisitionLinkModel acquisitionLink = null;
                            if (priceLink != null && priceLink.Prices.Any(p => !p.Price.Equals("0.00")))
                            {
                                acquisitionLink = new BookAcquisitionLinkModel
                                {
                                    Type = !string.IsNullOrEmpty(priceLink.DcFormat) ? priceLink.DcFormat : priceLink.Type,
                                    Prices = priceLink.Prices != null ? priceLink.Prices.Select(p => new BookPriceModel { CurrencyCode = p.CurrencyCode, Price = p.Price })
                                                                                        .ToList() : null,
                                    Url = GetValidUrl(priceLink.Href, authorityUrl)
                                };
                            }

                            var id = string.IsNullOrEmpty(entryDto.Id) ? string.Concat(catalogId, "-", entryDto.Title) : entryDto.Id;

                            model = new CatalogBookItemModel
                            {
                                AcquisitionLink = acquisitionLink,
                                Links = downloadLinks,
                                Id = id,
                                TrialLink = type == CatalogType.Litres ? CreateTrialLink(entryDto.Id) : null
                            };
                        }
                    }
                    // check for Litres bookshelf
                    else if (entryDto.Links.Any(l => LITRES_REL_BOOKSHELF_PREFIX.Equals(l.Rel)))
                    {
                        model = new LitresBookshelfCatalogItemModel();
                    }
                    // check for Litres topup
                    else if (entryDto.Links.Any(l => LITRES_REL_TOPUP_PREFIX.Equals(l.Rel)))
                    {
                        model = new LitresTopupCatalogItemModel
                            {
                                HtmlUrl = LITRES_PUT_MONEY_LINK_FORMAT,
                                Title = LITRES_REL_TOPUP_TITLE
                            };
                    }
                    else
                    {
                        // this is default folder
                        if (model == null)
                        {
                            model = new CatalogItemModel();
                        }
                    }

                    // title
                    if (string.IsNullOrEmpty(model.Title))
                    {
                        if (entryDto.Title == null)
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(entryDto.Title.Text))
                        {
                            model.Title = entryDto.Title.Text;
                        }
                        else if (!string.IsNullOrEmpty(entryDto.Title.DivValue))
                        {
                            model.Title = entryDto.Title.DivValue;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // description
                    model.Description = entryDto.Content != null && !string.IsNullOrEmpty(entryDto.Content.Value)
                                            ? entryDto.Content.Value
                                            : string.Empty;

                    // author
                    model.Author = entryDto.Author != null && !string.IsNullOrEmpty(entryDto.Author.Name)
                                        ? entryDto.Author.Name
                                        : string.Empty;

                    // opds catalog url
                    if (!(model is LitresTopupCatalogItemModel) && string.IsNullOrEmpty(model.HtmlUrl))
                    {
                        var catalogLink = entryDto.Links.FirstOrDefault(l => !string.IsNullOrEmpty(l.Type) && (l.Type.Contains(CATALOG_LINK_TYPE_PREFIX) || l.Type.Contains(CATALOG_LINK_TYPE) || l.Type.Contains(LITRES_CATALOG_LINK_TYPE_)));
                        if (catalogLink != null)
                        {
                            model.OpdsUrl = GetValidUrl(catalogLink.Href, authorityUrl);
                        }
                    }

                    // html url, to open in browser
                    var htmlLink = entryDto.Links.FirstOrDefault(l => HTML_TEXT_LINK_TYPE.Equals(l.Type) && !string.IsNullOrEmpty(l.Href));
                    if (htmlLink != null)
                    {
                        if (string.IsNullOrEmpty(model.HtmlUrl))
                        {
                            model.HtmlUrl = GetValidUrl(htmlLink.Href, authorityUrl);
                        }
                    }

                    //image
                    var imageLinks = entryDto.Links.Where(l => !string.IsNullOrEmpty(l.Type) && l.Type.Equals(IMAGE_LINK_TYPE));
                    if (imageLinks.Any())
                    {
                        if (imageLinks.Count() > 1)
                        {
                            var imageLink = imageLinks.SingleOrDefault(l => l.Rel.Equals(REL_IMAGE_PREFIX));
                            if (imageLink != null)
                            {
                                model.ImageUrl = new Uri(GetValidUrl(imageLink.Href, authorityUrl, true));
                            }
                        }
                        else
                        {
                            model.ImageUrl = new Uri(GetValidUrl(imageLinks.First().Href, authorityUrl, true));
                        }
                    }

                    // decoding of description & title
                    model.Description = HttpUtility.HtmlDecode(model.Description);
                    model.Title = HttpUtility.HtmlDecode(model.Title);
                    model.Author = HttpUtility.HtmlDecode(model.Author);

                    if (model.Description.Contains("<") && model.Description.Contains(">"))
                    {
                        model.Description = HtmlToText.Convert(model.Description);
                    }
                    model.Description = model.Description.Trim();
                    if (model.Author.Contains("<") && model.Author.Contains(">"))
                    {
                        model.Author = HtmlToText.Convert(model.Author);
                    }

                    folderItems.Add(model);
                }
            }

            folderModel.Items = folderItems;
            return folderModel;
        }

        private static string GetValidUrl(string url, string authorityUrl, bool useAbsoluteUrl = false)
        {
            var authority = HttpUtility.HtmlDecode(authorityUrl);
            url = HttpUtility.HtmlDecode(url);

            if (string.IsNullOrEmpty(url))
            {
                return authority;
            }

            if (useAbsoluteUrl)
            {
                authority = string.Concat("http://", new Uri(authority).Authority);
            }

            if (url.Contains(authority))
            {
                return url;
            }

            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return url;
            }

            var queryParts = url.Split(new[] { '/' }).ToList();
            if (string.IsNullOrEmpty(queryParts[0]))
            {
                queryParts.RemoveAt(0);
            }

            string targetUrl;
            if (!useAbsoluteUrl)
            {
                targetUrl = authority;
                if (targetUrl[0].Equals('/'))
                {
                    targetUrl = targetUrl.Substring(1, targetUrl.Length - 1);
                }
                for (var i = 0; i < queryParts.Count; ++i)
                {
                    var index = targetUrl.LastIndexOf(queryParts[i], StringComparison.Ordinal);
                    if (index == -1 || !targetUrl[index - 1].Equals('/'))
                    {
                        continue;
                    }

                    if (index + queryParts[i].Length < targetUrl.Length)
                    {
                        if (!targetUrl[index + queryParts[i].Length].Equals('/'))
                        {
                            continue;
                        }
                    }

                    targetUrl = targetUrl.Substring(0, index);
                    targetUrl = string.Concat(targetUrl, queryParts[i]);
                    for (var j = i + 1; j < queryParts.Count; ++j)
                    {
                        targetUrl = string.Concat(targetUrl, '/' + queryParts[j]);
                    }

                    return targetUrl;
                }
            }

            targetUrl = authority;

            // case with http://some.ru/index.xml

            var targetUri = new Uri(targetUrl);
            var path = targetUri.AbsolutePath;

            if (path.Contains(".") && url.Contains("."))
            {
                var pathLastIndex = path.LastIndexOf(".", StringComparison.Ordinal);
                var sLastIndex = url.LastIndexOf(".", StringComparison.Ordinal);

                if (path.Length - pathLastIndex == url.Length - sLastIndex)
                {
                    if (url[0] != '/')
                    {
                        url = string.Concat("/", url);
                    }

                    var slashLastIndex = path.LastIndexOf("/", StringComparison.Ordinal);
                    if (slashLastIndex != -1)
                    {
                        path = path.Substring(slashLastIndex, path.Length - slashLastIndex);
                    }

                    targetUrl = targetUrl.Replace(path, url);
                    return targetUrl;
                }

                string biggerString;
                string smallerString;
                if (url.Length - sLastIndex > path.Length - pathLastIndex)
                {
                    biggerString = url;
                    smallerString = path;
                }
                else
                {
                    biggerString = path;
                    smallerString = url;
                }

                var possibleExt = smallerString.Substring(pathLastIndex, smallerString.Length - pathLastIndex);
                if (biggerString.Contains(possibleExt) && biggerString.Contains("?") && biggerString.Contains("=") &&
                    biggerString.LastIndexOf("?", StringComparison.Ordinal) - biggerString.LastIndexOf(possibleExt, StringComparison.Ordinal) - possibleExt.Length == 0)
                {
                    if (url[0] != '/')
                    {
                        url = string.Concat("/", url);
                    }

                    var slashLastIndex = path.LastIndexOf("/", StringComparison.Ordinal);
                    if (slashLastIndex != -1)
                    {
                        path = path.Substring(slashLastIndex, path.Length - slashLastIndex);
                    }

                    targetUrl = targetUrl.Replace(path, url);
                    return targetUrl;
                }
            }

            if (!targetUrl[targetUrl.Length - 1].Equals('/'))
            {
                targetUrl = string.Concat(targetUrl, '/');
            }
            if (url[0].Equals('/'))
            {
                url = url.Substring(1, url.Length - 1);
            }

            return string.Concat(targetUrl, url);
        }

        private static BookDownloadLinkModel CreateTrialLink(string id)
        {
            var missedZerosCount = 8 - id.Length;
            if (missedZerosCount > 0)
            {
                for (int i = 0; i < missedZerosCount; ++i)
                {
                    id = string.Concat("0", id);
                }
            }

            var firstPart = id.Substring(0, 2);
            var secondPart = id.Substring(2, 2);
            var thirdPart = id.Substring(4, 2);

            var downloadLink = string.Format(LITRES_TRIAL_LINK_FORMAT, firstPart, secondPart, thirdPart, id);

            return new BookDownloadLinkModel
            {
                Type = ".fb2.zip",
                Url = downloadLink
            };
        }
    }
}