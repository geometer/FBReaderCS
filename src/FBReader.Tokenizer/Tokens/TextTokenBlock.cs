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
using System.Windows;
using FBReader.Tokenizer.Styling;
using FBReader.Tokenizer.TextStructure;

namespace FBReader.Tokenizer.Tokens
{
    public class TextTokenBlock : TokenBlockBase
    {
        public TextTokenBlock()
        {
            Inlines = new List<TextElementBase>();
        }

        public List<TextElementBase> Inlines { get; set; }

        public string TextAlign { get; set; }

        public double MarginLeft { get; set; }

        public double MarginRight { get; set; }

        public double TextIndent { get; set; }

        public void AddText(string text, TextVisualProperties properties, double fontSize, Size size, string part = null, int tokenID = -1)
        {
            part = part ?? string.Empty;
            Inlines.Add(new TextElement
                        {
                            Text = text,
                            Width = size.Width,
                            Height = size.Height,
                            Bold = properties.Bold,
                            Italic = properties.Italic,
                            Size = fontSize,
                            SupOption = properties.SupOption,
                            SubOption = properties.SubOption,
                            Part = part,
                            LinkID = properties.LinkID,
                            TokenID = tokenID
                        });
        }

        public void UpdateHeight(double height)
        {
            Height = Math.Max(Height, height);
        }

        public void EndParagraph()
        {
            Inlines.Add(new EOPElement());
        }

        public void EndLine()
        {
            Inlines.Add(new EOLElement());
        }

        public override string GetLastPart()
        {
            TextElement textElement = Inlines.OfType<TextElement>().LastOrDefault();
            if (textElement == null)
                return string.Empty;
            return textElement.Part;
        }
    }
}