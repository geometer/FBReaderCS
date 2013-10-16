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

using System.Text;
using System.Text.RegularExpressions;

namespace FBReader.AppServices.CatalogReaders.OpdsFilters
{
    class UnescapedQuotesFilter: IOpdsBadFormatFilter
    {
        public void Apply(StringBuilder opdsSource)
        {
            var pattern = new Regex("title=\"(?<TargetTitle>(?=(.*)\"(.*)\").*)\" (href=\"|(/)|(.*)=\")");
            var matchCollection = pattern.Matches(opdsSource.ToString());
            
            foreach (Match match in matchCollection)
            {
                var foundString = match.Groups["TargetTitle"].Value;
                if (string.IsNullOrEmpty(foundString))
                {
                    continue;
                }

                var hrefIndex = foundString.IndexOf("\" href=\"", System.StringComparison.Ordinal);
                if (hrefIndex != -1)
                {
                    foundString = foundString.Substring(0, hrefIndex);
                }

                var newString = foundString.Replace("\"", "&quot;");
                if (!string.IsNullOrEmpty(foundString))
                {
                    opdsSource.Replace(foundString, newString);
                }
            }
        }
    }
}