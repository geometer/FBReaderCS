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

namespace FBReader.Tokenizer.TextStructure
{
    public class TextElement : TextElementBase
    {
        public TextElement()
        {
            LinkID = string.Empty;
        }

        public string Text { get; set; }

        public bool Bold { get; set; }

        public bool Italic { get; set; }

        public double Size { get; set; }

        public bool SupOption { get; set; }

        public bool SubOption { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public string Part { get; set; }

        public string LinkID { get; set; }

        public int TokenID { get; set; }
    }
}