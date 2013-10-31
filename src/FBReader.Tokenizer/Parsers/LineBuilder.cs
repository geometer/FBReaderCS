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
using FBReader.Hyphen;
using FBReader.Tokenizer.Fonts;
using FBReader.Tokenizer.Styling;
using FBReader.Tokenizer.TextStructure;
using FBReader.Tokenizer.Tokens;

namespace FBReader.Tokenizer.Parsers
{
    public class LineBuilder
    {
        private readonly Queue<TokenBlockBase> _output;
        private readonly IList<double> _headerSizes;
        private readonly HyphenBuilder _hypher;
        private readonly IFontHelper _helper;
        private readonly double _lineWidth;
        private readonly double _textSize;
        private readonly bool _hyphenation;
        private readonly bool _useCssFontSize;

        private Stack<TagOpenToken> _tree;
        private TagOpenToken _lastOpenTag;
        private TextTokenBlock _block;
        private double _textWidth;
        private double _marginLeft;
        private double _marginRight;
        private double _textIndent;
        private double _fontSize;
        private int _firstTokenID;
        private bool _separator;

        public LineBuilder(
            double lineWidth, 
            IList<double> headerSizes, 
            double textSize, 
            IFontHelper helper, 
            bool hyphenation,
            bool useCssFontSize)
        {
            _headerSizes = headerSizes;
            _textSize = textSize;
            _helper = helper;
            _hyphenation = hyphenation;
            _useCssFontSize = useCssFontSize;
            _lineWidth = lineWidth;

            _output = new Queue<TokenBlockBase>();
            _hypher = new HyphenBuilder();
        }


        public IEnumerable<TokenBlockBase> GetLines(BookTokenIterator bookTokens, string lastText, int firstTokenID,
                                                    int stopTokenID = -1, string stopText = null)
        {
            _firstTokenID = firstTokenID;

            _tree = bookTokens.BuildTree(_firstTokenID);
            _lastOpenTag = _tree.Peek();
            _fontSize = GetCurrentFontSize();
            _separator = false;
            bool firstText = true;
            _marginLeft = _marginRight = 0.0;

            foreach (TagOpenToken openTagToken in _tree.Reverse())
                EnterMargin(openTagToken.TextProperties);

            if (string.IsNullOrEmpty(stopText) && stopTokenID > 0)
                --stopTokenID;

            while (bookTokens.MoveNext())
            {
                foreach (TokenBlockBase baseTokenLine in OutputLines(false))
                    yield return baseTokenLine;

                if (!Append(bookTokens, lastText, stopTokenID, stopText, ref firstText)) 
                    break;
            }
            foreach (TokenBlockBase baseTokenLine in OutputLines(true))
                yield return baseTokenLine;
        }

        private bool Append(BookTokenIterator bookTokens, string lastText, int stopTokenID, string stopText, ref bool firstText)
        {
            TokenBase token = bookTokens.Current;

            var pageBreakToken = token as NewPageToken;
            if (pageBreakToken != null)
                AppendToLine(pageBreakToken);

            var imageToken = token as PictureToken;
            if (imageToken != null)
                AppendToLine(imageToken);

            var openTagToken = token as TagOpenToken;
            if (openTagToken != null)
                AppendToLine(openTagToken);

            var closeTagToken = token as TagCloseToken;
            if (closeTagToken != null)
                AppendToLine(closeTagToken);

            var textSeparatorToken = token as WhitespaceToken;
            if (textSeparatorToken != null)
                AppendSeparator();

            var textToken = token as TextToken;
            if (textToken != null && AppendTextToken(textToken, lastText, stopTokenID, stopText, ref firstText) ||
                stopTokenID >= 0 && token.ID >= stopTokenID)
                return false;

            return true;
        }

        private IEnumerable<TokenBlockBase> OutputLines(bool outputCurrentLine)
        {
            while (_output.Count > 0)
                yield return _output.Dequeue();

            if (outputCurrentLine && _block != null)
            {
                yield return _block;
                _block = null;
            }
        }

        private void AppendToLine(NewPageToken token)
        {
            if (_block != null)
            {
                _block.EndLine();
                _block.LastTokenID = token.ID - 1;
                _output.Enqueue(_block);
            }

            var pageBreakLine = new PageBreakBlock();
            pageBreakLine.FirstTokenID = pageBreakLine.LastTokenID = token.ID;
            _output.Enqueue(pageBreakLine);
            _block = null;
        }

        private void AppendToLine(PictureToken token)
        {
            var imageTokenBlock = new ImageTokenBlock
                                      {
                                          ImageID = token.ImageID,
                                          FirstTokenID = _firstTokenID,
                                          LastTokenID = token.ID
                                      };
            _output.Enqueue(imageTokenBlock);
            _firstTokenID = token.ID + 1;
        }

        private void AppendToLine(TagOpenToken token)
        {
            if (!token.TextProperties.Inline)
            {
                if (_block != null)
                {
                    _block.EndParagraph();
                    _firstTokenID = token.ID;
                    _output.Enqueue(_block);
                }
                EnterMargin(token.TextProperties);
                _block = null;
                _textWidth = _textIndent = token.TextProperties.TextIndent;
                _separator = false;
            }
            PushTag(token);
        }

        private void AppendToLine(TagCloseToken token)
        {
            if (!_lastOpenTag.TextProperties.Inline)
            {
                if (_block != null)
                {
                    _block.EndParagraph();
                    _block.LastTokenID = token.ID;
                    _firstTokenID = token.ID + 1;
                    _output.Enqueue(_block);
                }
                if (_lastOpenTag.TextProperties.MarginBottom > 0.0)
                    _output.Enqueue(new SeparatorTokenBlock(_lastOpenTag.TextProperties.MarginBottom)
                                       {
                                           FirstTokenID = token.ID,
                                           LastTokenID = token.ID
                                       });

                LeaveMargin(_lastOpenTag.TextProperties);
                _block = null;
                _textWidth = 0.0;
                _textIndent = 0.0;
                _separator = false;
            }
            PopTag();
        }

        private void AppendToLine(TextToken token)
        {
            if (_separator && (_block == null || _block.Inlines.All(t => !(t is TextElement))))
                _separator = false;
            double width = GetTextWidth(token.Text, _separator, _lastOpenTag.TextProperties);
            if (CanAddText(width))
            {
                AppendToLine(token, width);
            }
            else
            {
                string[] partsOfWord = _hypher.GetPartsOfWord(token.Text, _hyphenation);
                if (partsOfWord.Length > 1)
                {
                    double textWidth = 0.0;
                    string str = string.Empty;
                    foreach (string part in partsOfWord)
                    {
                        string partWithSeparator = part;
                        if (!partWithSeparator.EndsWith("-"))
                            partWithSeparator = partWithSeparator + "-";

                        if (CanAddText(textWidth + GetTextWidth(partWithSeparator, _separator, _lastOpenTag.TextProperties)))
                        {
                            str = str + part;
                            textWidth += GetTextWidth(part, _separator, _lastOpenTag.TextProperties);
                        }
                        else
                            break;
                    }

                    if (!string.IsNullOrEmpty(str))
                    {
                        string partWithSeparator = str;
                        if (!partWithSeparator.EndsWith("-"))
                            partWithSeparator = partWithSeparator + "-";
                        AppendToLine(new TextToken(token.ID, partWithSeparator)
                                         {
                                             Part = str
                                         }, textWidth);

                        string rightPart = token.Text.Remove(0, str.Length);
                        if (rightPart.StartsWith("-"))
                            rightPart = rightPart.Remove(0, 1);
                        token = new TextToken(token.ID, rightPart);
                        CreateEmptyLine(token);
                        AppendToLine(token);
                        return;
                    }
                }
                CreateLine(token);
            }
            _separator = false;
        }

        private bool AppendTextToken(TextToken textToken, string lastText, int stopTokenID, string stopText, ref bool firstText)
        {
            bool flag = false;
            if (textToken.ID == stopTokenID && !string.IsNullOrEmpty(stopText))
            {
                textToken = new TextToken(stopTokenID, stopText + "-") {Part = stopText};
                flag = true;
            }
            if (firstText && !string.IsNullOrEmpty(lastText) && textToken.Text.StartsWith(lastText))
            {
                string text = textToken.Text.Remove(0, lastText.Length);
                if (text.StartsWith("-"))
                    text = text.Remove(0, 1);
                textToken = new TextToken(textToken.ID, text);
            }
            AppendToLine(textToken);
            firstText = false;
            return flag;
        }

        private void AppendToLine(TextToken token, double textWidth)
        {
            string text = token.Text;
            if (string.IsNullOrEmpty(text))
                return;

            _block = _block ?? new TextTokenBlock
                                   {
                                       TextAlign = _lastOpenTag.TextProperties.TextAlign,
                                       MarginLeft = _marginLeft,
                                       MarginRight = _marginRight,
                                       FirstTokenID = _firstTokenID,
                                       TextIndent = _textIndent
                                   };
            
            _block.LastTokenID = token.ID;
            _block.UpdateHeight(GetTextHeight(text));
            if (_separator)
            {
                TextVisualProperties properties = _lastOpenTag.TextProperties.Clone();
                var inlineItem = _block.Inlines.OfType<TextElement>().LastOrDefault();
                if (inlineItem != null && string.IsNullOrEmpty(inlineItem.LinkID))
                    properties.LinkID = string.Empty;
                _block.AddText(" ", properties, _fontSize, GetTextSize(" ", properties));
            }
            _block.AddText(text, _lastOpenTag.TextProperties, _fontSize, GetTextSize(text, _lastOpenTag.TextProperties), token.Part, token.ID);
            _textWidth += textWidth;
        }

        private void AppendSeparator()
        {
            _separator = true;
        }

        private bool CanAddText(double textWidth)
        {
            return _marginLeft + _textWidth + textWidth + _marginRight <= _lineWidth;
        }

        private void CreateEmptyLine(TokenBase token)
        {
            if (_block != null)
            {
                _block.EndLine();
                _block.LastTokenID = token.ID - 1;
                _output.Enqueue(_block);
            }
            _block = new TextTokenBlock
                         {
                             TextAlign = _lastOpenTag.TextProperties.TextAlign,
                             MarginLeft = _marginLeft,
                             MarginRight = _marginRight,
                             FirstTokenID = token.ID,
                             LastTokenID = token.ID,
                             TextIndent = 0.0
                         };
            _firstTokenID = token.ID;
            _textWidth = 0.0;
            _separator = false;
        }

        private void CreateLine(TextToken token)
        {
            CreateEmptyLine(token);

            string text = token.Text;
            _block.UpdateHeight(GetTextHeight(text));
            _textWidth = GetTextWidth(text, false, _lastOpenTag.TextProperties);
            _block.AddText(
                text, 
                _lastOpenTag.TextProperties, 
                _fontSize, 
                GetTextSize(text, _lastOpenTag.TextProperties), 
                token.Part, 
                token.ID);
        }

        private Size GetTextSize(string text, TextVisualProperties properties)
        {
            return new Size(
                GetTextWidth(text, false, properties), 
                GetTextHeight(text, properties.Bold, properties.Italic));
        }

        private double GetTextHeight(string text, bool bold = false, bool italic = false)
        {
            return _helper.GetSize(string.IsNullOrEmpty(text) ? ' ' : text.First(), _fontSize, bold, italic).Height;
        }

        private double GetTextWidth(string text, bool separator, TextVisualProperties properties)
        {
            if (separator)
                text = " " + text;
            double size = _fontSize*(properties.SubOption || properties.SupOption ? 0.5 : 1.0);
            var width = GetTextWidth(text, properties, size);
            return width;
        }

        private double GetTextWidth(string text, TextVisualProperties properties, double size)
        {
            return text.Aggregate(0.0, (s, c) => s + _helper.GetSize(c, size, properties.Bold, properties.Italic).Width);
        }

        private double GetCurrentFontSize()
        {
            if (!_useCssFontSize)
                return _textSize;

            if (_lastOpenTag.TextProperties.FontSizeType == FontSizeType.Px)
                return _lastOpenTag.TextProperties.FontSize;
            double fontSizeFactor = 1.0;
            if (_lastOpenTag.TextProperties.FontSizeType == FontSizeType.Em)
                fontSizeFactor = _lastOpenTag.TextProperties.FontSize;
            int index = 0;
            bool flag = false;
            foreach (TagOpenToken openTagToken in _tree)
            {
                if (openTagToken.Name == "section")
                    ++index;
                if (openTagToken.Name == "title")
                    flag = true;
            }
            if (flag && index < _headerSizes.Count)
                return fontSizeFactor*_headerSizes[index];
            return fontSizeFactor*_textSize;
        }

        private void EnterMargin(TextVisualProperties properties)
        {
            _marginLeft += properties.MarginLeft;
            _marginRight += properties.MarginRight;
        }

        private void LeaveMargin(TextVisualProperties properties)
        {
            _marginLeft -= properties.MarginLeft;
            _marginRight -= properties.MarginRight;
        }

        private void PushTag(TagOpenToken tag)
        {
            _tree.Push(tag);
            _lastOpenTag = tag;
            _fontSize = GetCurrentFontSize();
        }

        private void PopTag()
        {
            _tree.Pop();
            _lastOpenTag = _tree.Peek();
            _fontSize = GetCurrentFontSize();
        }
    }
}