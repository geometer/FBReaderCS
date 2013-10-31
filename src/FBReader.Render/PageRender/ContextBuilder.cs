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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using FBReader.Render.RenderData;
using FBReader.Settings;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.TextStructure;

namespace FBReader.Render.PageRender
{
    public class ContextBuilder
    {
        private readonly List<BookImage> _images;

        public ContextBuilder(List<BookImage> images)
        {
            _images = images;
        }

        public void BuildContexts(PageRenderData context)
        {
            double top = 0.0;
            int paragraphID = 0;

            foreach (ParagraphInfo p in context.Page.Paragraphs)
            {
                top = p is ImageParagraphInfo
                    ? BuildImageContext(context, p, top)
                    : BuildContext(context, p, context.PageSize.Width, (int)AppSettings.Default.FontSettings.FontSize, paragraphID++, top);

                top += p.MarginBottom;
            }
        }

        private double BuildImageContext(PageRenderData context, ParagraphInfo p, double top)
        {
            var item = (ImageElement)p.Inlines.First();
            BookImage bookImage = _images.First(t => t.ID == item.ImageID);
            double offsetX = (context.PageSize.Width - p.MarginLeft - p.MarginRight - item.Width) / 2.0;
            var imageContext = CreateImage(context, offsetX, top, item, bookImage);
            context.Images.Add(imageContext);
            return top + item.Height;
        }

        private double BuildContext(PageRenderData context, ParagraphInfo p, double width, 
                                    double defaultFontSize, int paragraphID, double top)
        {
            TextAlignment align = GetTextAlignment(p);
            var list = new List<TextElement>();
            bool firstLine = true;
            foreach (TextElementBase baseInlineItem in p.Inlines)
            {
                if (baseInlineItem is EOLElement)
                {
                    top = AppendLine(context, list, align, p, width, top, defaultFontSize, paragraphID,
                                    firstLine);
                    firstLine = false;
                    list.Clear();
                }
                else
                {
                    var inlineItem = (TextElement) baseInlineItem;
                    list.Add(inlineItem);
                }
            }

            return AppendLine(context, list, align == TextAlignment.Justify ? TextAlignment.Left : align, p, width,
                              top, defaultFontSize, paragraphID, firstLine);
        }

        private static double AppendLine(PageRenderData context, ICollection<TextElement> inlines, TextAlignment align, 
                                         ParagraphInfo p, double width, double top, double defaultFontSize, 
                                         int paragraphID, bool firstLine)
        {
            if (inlines.Count == 0)
                return top;

            double height = inlines.Max((t => t.Height));
            double inlinesWidth = inlines.Sum((t => t.Width));
            double leftMargin = 0.0;
            double wordSpacing = 0.0;
            double textIndent = !firstLine || align != TextAlignment.Justify && align != TextAlignment.Left
                              ? 0.0
                              : p.TextIndent;
            width -= p.MarginRight + p.MarginLeft + textIndent;

            switch (align)
            {
                case TextAlignment.Center:
                    leftMargin = (width - inlinesWidth)/2.0;
                    break;
                case TextAlignment.Right:
                    leftMargin = width - inlinesWidth;
                    break;
                case TextAlignment.Justify:
                    wordSpacing = (width - inlinesWidth)/(inlines.Count - 1);
                    break;
            }

            double tempLeftMargin = leftMargin + (p.MarginLeft + textIndent);

            inlines.Aggregate(tempLeftMargin, (current, inlineItem) => 
                BuildInlineItem(context, top, defaultFontSize, paragraphID, inlineItem, height, current, wordSpacing));

            return top + height * (double) AppSettings.Default.FontSettings.FontInterval;
        }

        private static double BuildInlineItem(PageRenderData context, double top, double defaultFontSize, int paragraphID,
                                              TextElement inlineItem, double height, double tempLeftMargin, double wordSpacing)
        {
            double fontSize = inlineItem.Size;
            if (fontSize < 0.01 || !AppSettings.Default.UseCSSFontSize)
                fontSize = defaultFontSize;
            double topMargin = top;
            if (inlineItem.SubOption || inlineItem.SupOption)
                fontSize /= 2.0;
            if (inlineItem.SubOption)
                topMargin += height/2.0;
            if (!string.IsNullOrEmpty(inlineItem.Text) && !string.IsNullOrWhiteSpace(inlineItem.Text))
            {
                var textContext = CreateTextContext(context, paragraphID, inlineItem, height, tempLeftMargin, topMargin);
                context.Texts.Add(textContext);
            }
            var color = new Color?();

            if (!string.IsNullOrEmpty(inlineItem.LinkID))
            {
                var linkContext = CreateLinkContext(inlineItem, height, tempLeftMargin, topMargin);
                context.Links.Add(linkContext);
                color = context.LinkBrush;
            }

            var wordContext = CreateText(context, inlineItem, inlineItem.Bold, inlineItem.Italic, fontSize,
                                         tempLeftMargin, topMargin, color);
            context.Words.Add(wordContext);
            return tempLeftMargin + inlineItem.Width + wordSpacing;
        }

        private static TextRenderData CreateTextContext(PageRenderData context, int paragraphID, TextElement inlineItem,
                                                        double height, double tempLeftMargin, double topMargin)
        {
            return new TextRenderData
                       {
                           Text = inlineItem.Text,
                           Rect = new Rect(tempLeftMargin + context.OffsetX, topMargin + context.OffsetY,
                                           inlineItem.Width, height),
                           ParagraphID = paragraphID,
                           TokenID = inlineItem.TokenID
                       };
        }

        private static LinkRenderData CreateLinkContext(TextElement inlineItem, double height, double tempLeftMargin,
                                                        double topMargin)
        {
            string linkID = inlineItem.LinkID;
            if (linkID.StartsWith("#"))
                linkID = linkID.Remove(0, 1);

            var rect = new Rect(tempLeftMargin, topMargin, inlineItem.Width + 24.0, height + 24.0);

            return new LinkRenderData
                       {
                           LinkID = linkID,
                           Rect = rect
                       };
        }

        private static WordRenderData CreateText(PageRenderData context, TextElement element, bool bold, bool italic,
                                                 double fontSize, double left, double top, Color? color)
        {
            return new WordRenderData
                       {
                           Text = element.Text,
                           Bold = bold,
                           Italic = italic,
                           FontSize = fontSize,
                           Left = left + context.OffsetX,
                           Top = top + context.OffsetY,
                           Color = color
                       };
        }

        private static ImageRenderData CreateImage(PageRenderData context, double offsetX, double top, ImageElement item,
                                                   BookImage bookImage)
        {
            var imageContext = new ImageRenderData
                                   {
                                       Margin = new Thickness(offsetX + context.OffsetX, top + context.OffsetY, 0.0, 0.0),
                                       Width = item.Width,
                                       Height = item.Height,
                                       ImageStream = bookImage.CreateStream()
                                   };
            return imageContext;
        }

        private static TextAlignment GetTextAlignment(ParagraphInfo p)
        {
            if (!string.IsNullOrEmpty(p.TextAlign))
            {
                switch (p.TextAlign)
                {
                    case "center":
                        return TextAlignment.Center;
                    case "right":
                        return TextAlignment.Right;
                    case "left":
                        return TextAlignment.Left;
                }
            }
            return TextAlignment.Justify;
        }
    }
}