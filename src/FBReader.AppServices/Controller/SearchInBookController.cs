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
using System.Linq;
using FBReader.AppServices.DataModels;
using FBReader.Render.Tools;

namespace FBReader.AppServices.Controller
{
    public class SearchInBookController
    {
        public SearchInBookResultItemDataModel ToDataModel(
            BookSearchResult searchResult, 
            string queryString,
            string bookId)
        {

            var query = BookSearch.PrepareQuery(queryString);

            var item = new SearchInBookResultItemDataModel();
            if (searchResult.PreviousContext.Any())
            {
                item.TokenId = searchResult.PreviousContext[0].ID;
            }
            else
            {
                item.TokenId = searchResult.SearchResult[0].ID;
            }

            item.BookId = bookId;

            var firstWord = query.First();
            var lastWord = query.Last();

            var firstToken = searchResult.SearchResult.First();
            var lastToken = searchResult.SearchResult.Last();

            var beforeWords = searchResult.PreviousContext.Select(r => r.Text).ToList();
            var afterWords = searchResult.NextContext.Select(r => r.Text).ToList();

            var intermediateWords = searchResult.SearchResult.Skip(1).Take(searchResult.SearchResult.Count - 2).Select(r => r.Text).ToList();

            var firstWordIndex = firstToken.Text.IndexOf(firstWord, StringComparison.InvariantCultureIgnoreCase);
            beforeWords.Add(firstToken.Text.Substring(0, firstWordIndex));
            intermediateWords.Insert(0, firstToken.Text.Substring(firstWordIndex, firstWord.Length));

            var lastWordIndex = lastToken.Text.IndexOf(lastWord, StringComparison.InvariantCultureIgnoreCase);
            afterWords.Insert(0, lastToken.Text.Substring(lastWordIndex + lastWord.Length));
            if (query.Count > 1)
                intermediateWords.Add(lastToken.Text.Substring(lastWordIndex, lastWord.Length));

            item.TextBefore = string.Join(" ", beforeWords);
            item.SearchedText = string.Join(" ", intermediateWords);
            item.TextAfter = string.Join(" ", afterWords);

            return item;
        }
    }
}
