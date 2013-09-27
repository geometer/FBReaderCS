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
using System.Text;
using FBReader.WebClient;

namespace FBReader.AppServices.CatalogReaders.OpdsFilters
{
    public class UnescapedHtmlDescriptionFilter : IOpdsBadFormatFilter
    {
        private const string OPEN_TAG = "<content type=\"xhtml\">";
        private const string CLOSE_TAG = "</content>";
        
        public void Apply(StringBuilder opdsSource)
        {
            var strExists = true;
            var opds = opdsSource.ToString();
            var stringsToReplace = new List<string>();

            while (strExists)
            {
                var firstIndex = opds.IndexOf(OPEN_TAG, StringComparison.Ordinal);
                var closeFirstIndex = firstIndex + OPEN_TAG.Length;

                var secondIndex = opds.IndexOf(CLOSE_TAG, StringComparison.Ordinal);
                var closeSecondIndex = secondIndex + CLOSE_TAG.Length;

                if (firstIndex < 0 || secondIndex < 0)
                {
                    strExists = false;
                    continue;
                }

                var possibleHtml = opds.Substring(closeFirstIndex, secondIndex - closeFirstIndex);
                stringsToReplace.Add(possibleHtml);
                opds = opds.Substring(closeSecondIndex, opds.Length - closeSecondIndex);
            }

            var htmlToText = new HtmlToText();
            foreach (var s in stringsToReplace)
            {
                var normalContent = htmlToText.Convert(s);
                opdsSource.Replace(s, normalContent);
            }
        }
    }
}