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
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.IO;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.Parsers;
using FBReader.Tokenizer.Tokens;

namespace FBReader.Render.Tools
{
    public static class TokensTool
    {
        public static List<int> GetTokens(string id)
        {
            var tokensCache = new Cache<string, List<int>>(delegate
                {
                    using (var file = FileStorage.Instance.GetFile(Path.Combine(id, ModelConstants.BOOK_FILE_DATA_REF_PATH)))
                    {
                        using (file.Lock())
                        {
                            file.Seek(0, SeekOrigin.Begin);
                            var capacity = file.Reader.ReadInt32();
                            var list = new List<int>(capacity);
                            for (var index = 0; index < capacity; ++index)
                            {
                                list.Add(file.Reader.ReadInt32());
                            }
                            return list;
                        }
                    }
                });

            return tokensCache.Get(id);
        }

        public static void SaveTokens(BookModel book, IBookSummaryParser parser)
        {
            var tokens = parser.GetTokenParser().GetTokens().ToList();
            var positions = SaveTokensWithPosition(book.GetTokensPath(), tokens);

            SaveTokenPosition(book.GetTokensRefPath(), positions);

            book.TokenCount = positions.Count;
            book.WordCount = tokens.Count(t => t is TextToken);
            book.CurrentTokenID = Math.Min(tokens.Count() - 1, book.CurrentTokenID);

            parser.BuildChapters();

            SaveAnchors(book.BookID, parser.Anchors, tokens);
            SaveChapters(book.BookID, parser.Chapters, tokens);
        }

        private static void SaveAnchors(string bookId, Dictionary<string, int> anchors, IList<TokenBase> tokens)
        {
            var anchModels = anchors.Select(anchor => CreateAnchor(bookId, anchor, tokens));
            ToolsRepository.SaveAnchors(anchModels);
        }

        private static void SaveChapters(string bookId, IEnumerable<BookChapter> chapters, IList<TokenBase> tokens)
        {
            var chapModels = chapters.Select(chapter => CreateChapter(bookId, chapter, tokens));
            ToolsRepository.SaveChapters(chapModels);
        }

        private static ChapterModel CreateChapter(string bookId, BookChapter chapter, IList<TokenBase> tokens)
        {
            return new ChapterModel
                {
                    BookID = bookId,
                    Level = chapter.Level,
                    Title = chapter.Title.Length > 1024 ? chapter.Title.Substring(0, 1024) : chapter.Title,
                    TokenID = GetUIToken(chapter.TokenID, tokens),
                    MinTokenID = GetMinToken(chapter.TokenID, tokens)
                };
        }

        private static AnchorModel CreateAnchor(string bookId, KeyValuePair<string, int> anchor, IList<TokenBase> tokens)
        {
            var key = anchor.Key;
            if (key.Length > 1024)
            {
                key = key.Substring(0, 1024);
            }

            return new AnchorModel
                {
                    BookID = bookId,
                    NameHash = key.GetHashCode(),
                    Name = key,
                    TokenID = GetUIToken(anchor.Value, tokens)
                };
        }

        private static int GetUIToken(int tokenId, IList<TokenBase> tokens)
        {
            for (var i = tokenId; i < tokens.Count; i++)
            {
                if (tokens[i] is TextToken || tokens[i] is PictureToken)
                {
                    return i;
                }
            }

            return tokenId;
        }

        private static int GetMinToken(int tokenId, IList<TokenBase> tokens)
        {
            for (var i = tokenId; i >= 0; i--)
            {
                if (tokens[i] is TextToken || tokens[i] is PictureToken)
                {
                    return i;
                }
            }

            return 0;
        }

        private static List<int> SaveTokensWithPosition(string tokensPath, IEnumerable<TokenBase> tokens)
        {
            var list = new List<int>();

            using (var file = FileStorage.Instance.GetFile(tokensPath))
            {
                using (file.Lock())
                {
                    foreach (var baseToken in tokens)
                    {
                        list.Add(file.Position);
                        TokenSerializer.Save(file.Writer, baseToken);
                    }
                }
            }

            return list;
        }

        private static void SaveTokenPosition(string path, List<int> positions)
        {
            using (var file = FileStorage.Instance.GetFile(path))
            {
                using (file.Lock())
                {
                    var writer = file.Writer;
                    writer.Write(positions.Count);
                    if (positions.Count <= 0)
                    {
                        return;
                    }

                    var buffer = new byte[positions.Count * 4];
                    Buffer.BlockCopy(positions.ToArray(), 0, buffer, 0, buffer.Length);
                    writer.Write(buffer);
                }
            }
        }
    }
}