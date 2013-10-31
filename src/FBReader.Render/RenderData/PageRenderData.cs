/*
 * Author: Vitaly Leschenko, CactusSoft (http://cactussoft.biz/), 2013
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FBReader.DataModel.Model;
using FBReader.Tokenizer.TextStructure;

namespace FBReader.Render.RenderData
{
    public class PageRenderData
    {
        public PageRenderData()
        {
            Images = new List<ImageRenderData>();
            Words = new List<WordRenderData>();
        }

        public Panel Panel { get; set; }

        public PageInfo Page { get; set; }

        public BookModel Book { get; set; }

        public IList<BookmarkModel> Bookmarks { get; set; }

        public List<WordRenderData> Words { get; private set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public IList<ImageRenderData> Images { get; private set; }

        public ICollection<LinkRenderData> Links { get; set; }

        public ICollection<TextRenderData> Texts { get; set; }

        public Size PageSize { get; set; }

        public Brush BackgroundBrush { get; set; }
        
        public Color LinkBrush { get; set; }
    }
}