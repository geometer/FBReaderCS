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

using System.IO;
using FBReader.Tokenizer.Fonts;

namespace FBReader.Tokenizer.Styling
{
    public class TextVisualProperties
    {
        public TextVisualProperties()
        {
            Tag = string.Empty;
            LinkID = string.Empty;
            TextAlign = string.Empty;
        }

        public FontSizeType FontSizeType { get; set; }

        public double FontSize { get; set; }

        public string TextAlign { get; set; }

        public string Tag { get; set; }

        public bool Inline { get; set; }

        public bool Bold { get; set; }

        public bool Italic { get; set; }

        public double MarginLeft { get; set; }

        public double MarginRight { get; set; }

        public double MarginBottom { get; set; }

        public double TextIndent { get; set; }

        public bool SupOption { get; set; }

        public bool SubOption { get; set; }

        public string LinkID { get; set; }

        public TextVisualProperties Clone()
        {
            return new TextVisualProperties
                   {
                       FontSize = FontSize,
                       FontSizeType = FontSizeType,
                       TextAlign = TextAlign,
                       Tag = Tag,
                       Inline = Inline,
                       Bold = Bold,
                       Italic = Italic,
                       LinkID = LinkID,
                       SupOption = SupOption,
                       SubOption = SubOption
                   };
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(FontSize);
            writer.Write((int) FontSizeType);
            writer.Write(TextAlign);
            writer.Write(Tag);
            writer.Write(Inline);
            writer.Write(Italic);
            writer.Write(Bold);
            writer.Write(MarginLeft);
            writer.Write(MarginRight);
            writer.Write(MarginBottom);
            writer.Write(TextIndent);
            writer.Write(SubOption);
            writer.Write(SupOption);
            writer.Write(LinkID);
        }

        public static TextVisualProperties Load(BinaryReader reader)
        {
            return new TextVisualProperties
                   {
                       FontSize = reader.ReadDouble(),
                       FontSizeType = (FontSizeType) reader.ReadInt32(),
                       TextAlign = reader.ReadString(),
                       Tag = reader.ReadString(),
                       Inline = reader.ReadBoolean(),
                       Italic = reader.ReadBoolean(),
                       Bold = reader.ReadBoolean(),
                       MarginLeft = reader.ReadDouble(),
                       MarginRight = reader.ReadDouble(),
                       MarginBottom = reader.ReadDouble(),
                       TextIndent = reader.ReadDouble(),
                       SubOption = reader.ReadBoolean(),
                       SupOption = reader.ReadBoolean(),
                       LinkID = reader.ReadString()
                   };
        }
    }
}