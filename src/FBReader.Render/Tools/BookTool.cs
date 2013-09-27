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

using System.Collections.Generic;
using System.IO;
using FBReader.DataModel.Model;
using FBReader.IO;
using FBReader.Tokenizer.Parsers;
using FBReader.Tokenizer.Tokens;

namespace FBReader.Render.Tools
{
    public class BookTool
    {
        public string GetText(BookModel book, int tokenOffset, int wordsCount, out int lastTokenId)
        {
            lastTokenId = -1;
            var result = new List<string>();

            using (var tokenIterator = new BookTokenIterator(book.GetTokensPath(), TokensTool.GetTokens(book.BookID)))
            {
                int words = 0;
                tokenIterator.MoveTo(tokenOffset);
                while (tokenIterator.MoveNext() && words < wordsCount)
                {
                    if(tokenIterator.Current is NewPageToken && result.Count > 0)
                        break;

                    var textToken = tokenIterator.Current as TextToken;
                    if(textToken == null)
                        continue;
                    lastTokenId = textToken.ID;
                    result.Add(textToken.Text);
                    words++;
                }
            }
            return string.Join(" ", result);
        }

        public Stream GetOriginalBook(BookModel book)
        {
            var destinationStream = new MemoryStream();
            using (var file = FileStorage.Instance.GetFile(book.GetBookPath()))
            {
                using (file.Lock())
                {
                    file.Reader.BaseStream.CopyTo(destinationStream);
                }
            }
            destinationStream.Seek(0, SeekOrigin.Begin);
            return destinationStream;
        }
    }
}
