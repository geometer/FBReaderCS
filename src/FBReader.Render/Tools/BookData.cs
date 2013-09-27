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
using System.Linq;
using FBReader.Tokenizer.Data;

namespace FBReader.Render.Tools
{
    public class BookData
    {
        private readonly object _syncObject = new object();

        private readonly string _bookId;
        private IEnumerable<BookImage> _imagesCache;

        public BookData(string bookId)
        {
            _bookId = bookId;
        }

        public bool HasTableOfContents
        {
            get
            {
                var chapters = ToolsRepository.GetChapters(_bookId);
                return chapters != null && chapters.Any();
            }
        }

        public IEnumerable<BookImage> LoadImages()
        {
            lock (_syncObject)
            {
                return _imagesCache ?? (_imagesCache = ToolsRepository.GetImages(_bookId));
            }
        }

        public int GetAnchorsTokenId(string linkId)
        {
            return ToolsRepository.GetAnchorsTokenId(linkId, _bookId);
        }
    }
}