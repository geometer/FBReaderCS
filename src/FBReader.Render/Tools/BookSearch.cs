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
using System.Threading.Tasks;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.Tokenizer.Parsers;
using FBReader.Tokenizer.Tokens;

namespace FBReader.Render.Tools
{
    public class BookSearchResult
    {
        public List<TextToken> PreviousContext { get; set; }
        public List<TextToken> SearchResult { get; set; }
        public List<TextToken> NextContext { get; set; }
    }

    public class BookSearch : IDisposable
    {
        private BookTokenIterator _bookTokenIterator;
        private List<string> _query;
        private BookModel _book;

        public void Init()
        {
            if (_bookTokenIterator != null && _book != null)
            {
                int tokenId = _bookTokenIterator.Current.ID;
                _bookTokenIterator.Dispose();
                _bookTokenIterator = new BookTokenIterator(_book.GetTokensPath(), TokensTool.GetTokens(_book.BookID));
                _bookTokenIterator.MoveTo(tokenId);
                _bookTokenIterator.MoveNext();
            }
        }

        public Task<List<BookSearchResult>> Search(BookModel book, string query, int count)
        {
            if (string.IsNullOrEmpty(query) || book == null)
                return Task<List<BookSearchResult>>.Factory.StartNew(() => new List<BookSearchResult>());

            if (_bookTokenIterator != null)
            {
                _bookTokenIterator.Dispose();
            }

            _book = book;
            _bookTokenIterator = new BookTokenIterator(_book.GetTokensPath(), TokensTool.GetTokens(_book.BookID));

            _query = PrepareQuery(query);

            return Task<List<BookSearchResult>>
                .Factory.StartNew(() => Load(_bookTokenIterator, _query, count));
        }

        public Task<List<BookSearchResult>> SearchNext(int count)
        {
            return Task<List<BookSearchResult>>
                .Factory.StartNew(() => Load(_bookTokenIterator, _query, count));
        }

        private List<BookSearchResult> Load(BookTokenIterator bookTokenIterator, List<string> query, int count)
        {
            var result = new List<BookSearchResult>();

            try
            {
                if (query.Count == 1)
                {
                    result = SearchOneWord(bookTokenIterator, query[0], count);
                }

                if (query.Count > 1)
                {
                    result = SearchGroupWords(bookTokenIterator, query, count);
                }
            }
            catch (TokenIteratorUnableMoveNextException tokenExp)
            {
                throw new SearchInBookInterruptedException("Book tokenizer exception has occured", tokenExp);
            }
            
            return result;
        }

        private List<BookSearchResult> SearchOneWord(BookTokenIterator bookTokenIterator, string query, int count)
        {
            var result = new List<BookSearchResult>();
            while (bookTokenIterator.MoveNext())
            {
                var textToken = bookTokenIterator.Current as TextToken;
                if (textToken == null)
                    continue;

                if (textToken.Text.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    var previousContext = GetSearchBeforeContext(bookTokenIterator, textToken.ID);
                    var afterContext = GetSearchAfterContext(bookTokenIterator, textToken.ID);

                    result.Add(new BookSearchResult
                                   {
                                       PreviousContext = previousContext,
                                       SearchResult = new List<TextToken>{textToken},
                                       NextContext = afterContext
                                   });

                    if (result.Count >= count)
                        break;
                }
            }
            return result;
        }

        private List<BookSearchResult> SearchGroupWords(BookTokenIterator bookTokenIterator, List<string> query, int count)
        {
            var result = new List<BookSearchResult>();

            var firstWordQuery = query[0];
            var lastWordQuery = query.Last();
            TextToken firstWordToken;
            while ((firstWordToken = FindFirstWord(bookTokenIterator, firstWordQuery)) != null)
            {
                var resultSequence = new List<TextToken>();
                resultSequence.Add(firstWordToken);

                bool findNextSequence = false;
                for (int i = 1; i < query.Count - 1; i++)
                {
                    TextToken intermediateToken;
                    if (CheckIntermediateWord(bookTokenIterator, query[i], out intermediateToken))
                    {
                        resultSequence.Add(intermediateToken);
                    }
                    else
                    {
                        findNextSequence = true;
                        break;
                    }
                }

                if (findNextSequence)
                    continue;

                TextToken lastToken;
                if (CheckLastWord(bookTokenIterator, lastWordQuery, out lastToken))
                {
                    resultSequence.Add(lastToken);

                    var previousContext = GetSearchBeforeContext(bookTokenIterator, firstWordToken.ID);
                    var afterContext = GetSearchAfterContext(bookTokenIterator, lastToken.ID);

                    result.Add(new BookSearchResult
                                   {
                                       PreviousContext = previousContext, 
                                       SearchResult = resultSequence, 
                                       NextContext = afterContext
                                   });

                    if (result.Count >= count)
                        break;
                }
            }
            return result;
        }

        private TextToken FindFirstWord(BookTokenIterator bookTokenIterator, string query)
        {
            while (bookTokenIterator.MoveNext())
            {
                var textToken = bookTokenIterator.Current as TextToken;
                if (textToken == null)
                    continue;

                if (textToken.Text.EndsWith(query, StringComparison.InvariantCultureIgnoreCase))
                {
                    return textToken;
                }
            }
            return null;
        }

        private bool CheckIntermediateWord(BookTokenIterator bookTokenIterator, string query, out TextToken result)
        {
            result = null;
            while (bookTokenIterator.MoveNext())
            {
                var textToken = bookTokenIterator.Current as TextToken;
                if (textToken == null)
                    continue;

                if (textToken.Text.Equals(query, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = textToken;
                    return true;
                }
                return false;
            }
            return false;
        }

        private bool CheckLastWord(BookTokenIterator bookTokenIterator, string query, out TextToken result)
        {
            result = null;
            while (bookTokenIterator.MoveNext())
            {
                var textToken = bookTokenIterator.Current as TextToken;
                if (textToken == null)
                    continue;

                if (textToken.Text.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = textToken;
                    return true;
                }
                return false;
            }
            return false;
        }

        private List<TextToken> GetSearchBeforeContext(BookTokenIterator bookTokenIterator, int startTokenId, int count = 8)
        {
            var result = new List<TextToken>();
            var tokenId = startTokenId;

            while (--tokenId >= 0 && result.Count < count)
            {
                bookTokenIterator.MoveTo(tokenId);
                bookTokenIterator.MoveNext();

                if (bookTokenIterator.Current is NewPageToken)
                    break;

                var textToken = bookTokenIterator.Current as TextToken;
                if (textToken == null)
                    continue;

                result.Insert(0, textToken);
            }

            bookTokenIterator.MoveTo(startTokenId);
            bookTokenIterator.MoveNext();
            return result;
        }

        private List<TextToken> GetSearchAfterContext(BookTokenIterator bookTokenIterator, int endTokenId, int count = 8)
        {
            var result = new List<TextToken>();
            var tokenId = endTokenId;

            while (++tokenId < bookTokenIterator.Count && result.Count < count)
            {
                bookTokenIterator.MoveTo(tokenId);
                bookTokenIterator.MoveNext();

                if (bookTokenIterator.Current is NewPageToken)
                    break;

                var textToken = bookTokenIterator.Current as TextToken;
                if (textToken == null)
                    continue;

                result.Add(textToken);
            }

            bookTokenIterator.MoveTo(endTokenId);
            bookTokenIterator.MoveNext();
            return result;
        }

        public void Dispose()
        {
            if(_bookTokenIterator != null)
            {
                _bookTokenIterator.Dispose();
            }
        }


        public static List<string> PrepareQuery(string query)
        {
            return query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
