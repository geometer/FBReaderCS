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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBReader.AppServices.CatalogReaders.OpdsFilters
{
    class UrlWithoutProtocolFilter : IOpdsBadFormatFilter
    {
        public void Apply(StringBuilder opdsSource)
        {
            var pattern = new Regex("href=\"(?<TargetHref>//(|(/)|(.*?)))\"");
            var matchCollection = pattern.Matches(opdsSource.ToString());

            int offset = 0;
            foreach (Match match in matchCollection)
            {
                Group foundHref = match.Groups["TargetHref"];
                if(foundHref == null)
                    continue;
                
                opdsSource.Insert(foundHref.Index + offset, "http:");

                offset += 5; // "http:".Length;
            }
        }
    }
}
