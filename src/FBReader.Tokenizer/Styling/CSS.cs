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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ExCSS;
using FBReader.Tokenizer.Fonts;

namespace FBReader.Tokenizer.Styling
{
    public class CSS
    {
        public CSS()
        {
            Rules = new Collection<CssRule>();
        }

        public Collection<CssRule> Rules { get; private set; }

        public void Analyze(string css)
        {
            var parsedCss = new Parser().Parse(css);
            foreach (var rule in parsedCss.Ruleset.OfType<StyleRule>())
            {
                var selectors = new List<string>();
                
                var selectorList = rule.Selector as MultipleSelectorList;
                if (selectorList != null)
                {
                    selectors.AddRange(selectorList.Select(selector => selector.ToString()));
                }
                else
                {
                    selectors.Add(rule.Selector.ToString());
                }

                var rules = new Dictionary<string, string>();
                foreach (var declaration in rule.Declarations)
                {
                    rules[declaration.Name] = declaration.Term.ToString();
                }

                Rules.Add(new CssRule(selectors, rules));
            }
        }

        public void ApplyProperties(string[] selectors, TextVisualProperties properties)
        {
            foreach (CssRule rule in Rules.Where(r => r.Selectors.Any(selectors.Contains)))
            {
                string val;
                if (rule.TryGetValueForRule("font-weight", out val))
                {
                    properties.Bold = val == "bold";
                }
                if (rule.TryGetValueForRule("font-style", out val))
                {
                    properties.Italic = val == "italic";
                }
                if (rule.TryGetValueForRule("display", out val))
                {
                    properties.Inline = val == "inline";
                }
                if (rule.TryGetValueForRule("vertical-align", out val))
                {
                    properties.SupOption = val == "super";
                }
                if (rule.TryGetValueForRule("vertical-align", out val))
                {
                    properties.SubOption = val == "sub";
                }
                if (rule.TryGetValueForRule("font-size", out val))
                {
                    double size;
                    FontSizeType type;
                    if (TryParseFontSize(val, out size, out type))
                    {
                        properties.FontSize = size;
                        properties.FontSizeType = type;
                    }
                }
                if (rule.TryGetValueForRule("text-align", out val) && (val != "inherit"))
                {
                    properties.TextAlign = val;
                }
                if (rule.TryGetValueForRule("margin", out val))
                {
                    double left;
                    double right;
                    double bottom;
                    ParseMargin(val, out left, out right, out bottom);
                    properties.MarginLeft = left;
                    properties.MarginRight = right;
                    properties.MarginBottom = bottom;
                }
                if (rule.TryGetValueForRule("margin-left", out val))
                {
                    properties.MarginLeft = ParseMarginValue(val);
                }
                if (rule.TryGetValueForRule("margin-right", out val))
                {
                    properties.MarginRight = ParseMarginValue(val);
                }
                if (rule.TryGetValueForRule("margin-bottom", out val))
                {
                    properties.MarginBottom = ParseMarginValue(val);
                }
                if (rule.TryGetValueForRule("text-indent", out val))
                {
                    properties.TextIndent = ParseMarginValue(val);
                }
            }
        }

        private double ParseMarginValue(string margin, double normal = 16.0)
        {
            if (margin.EndsWith("px"))
            {
                double pixels;
                margin = margin.Remove(margin.Length - 2, 2);
                if (!double.TryParse(margin, NumberStyles.Float, CultureInfo.InvariantCulture, out pixels))
                {
                    return 0.0;
                }
                return pixels;
            }

            if (margin.EndsWith("em"))
            {
                double emFactor;
                margin = margin.Remove(margin.Length - 2, 2);
                if (!double.TryParse(margin, NumberStyles.Float, CultureInfo.InvariantCulture, out emFactor))
                {
                    emFactor = 1.0;
                }
                return (normal * emFactor);
            }

            return 0.0;
        }

        private void ParseMargin(string margin, out double left, out double right, out double bottom)
        {
            left = right = bottom = 0;
            string[] strArray = margin.Split(new[] { ' ' });
            if (strArray.Length == 1)
            {
                double marginLeft = ParseMarginValue(strArray[0]);
                left = marginLeft;
                right = marginLeft;
                bottom = marginLeft;
            }
            else if (strArray.Length == 2)
            {
                double marginBottom = ParseMarginValue(strArray[0]);
                double marginHor = ParseMarginValue(strArray[1]);
                left = marginHor;
                right = marginHor;
                bottom = marginBottom;
            }
            else if (strArray.Length == 3)
            {
                double marginVert = ParseMarginValue(strArray[2]);
                double marginHor = ParseMarginValue(strArray[1]);
                left = marginHor;
                right = marginHor;
                bottom = marginVert;
            }
            else if (strArray.Length > 3)
            {
                double marginRight = ParseMarginValue(strArray[1]);
                double marginBottom = ParseMarginValue(strArray[2]);
                double marginLeft = ParseMarginValue(strArray[3]);
                left = marginLeft;
                right = marginRight;
                bottom = marginBottom;
            }
        }

        private bool TryParseFontSize(string value, out double size, out FontSizeType type)
        {
            if (value.EndsWith("px"))
            {
                int num;
                value = value.Remove(value.Length - 2, 2);
                if (int.TryParse(value, out num))
                {
                    type = FontSizeType.Px;
                    size = num;
                    return true;
                }
            }
            else if (value.EndsWith("em"))
            {
                double fontSize;
                value = value.Remove(value.Length - 2, 2);
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fontSize))
                {
                    size = fontSize;
                    type = FontSizeType.Em;
                    return true;
                }
            }
            type = FontSizeType.Unknown;
            size = 0;
            return false;
        }
    }
}