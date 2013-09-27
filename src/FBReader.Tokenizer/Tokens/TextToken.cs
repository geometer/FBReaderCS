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

using System.IO;

namespace FBReader.Tokenizer.Tokens
{
    public class TextToken : TokenBase
    {
        public TextToken(int id, string text)
            : base(id)
        {
            Text = text;
            Part = string.Empty;
        }

        public string Part { get; set; }

        public string Text { get; private set; }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(Text);
        }

        public static TokenBase Load(BinaryReader reader, int id)
        {
            string text = reader.ReadString();
            return new TextToken(id, text);
        }
    }
}