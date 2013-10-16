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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using FBReader.App.Controls.Manipulations;
using FBReader.AppServices.Controller;
using FBReader.Common;
using FBReader.DataModel.Model;
using FBReader.Render.RenderData;
using FBReader.Settings;

namespace FBReader.App.Controls
{
    public partial class ThreePagePanel : IBookView
    {
        private readonly List<List<TextRenderData>> _texts = new List<List<TextRenderData>>();
        private readonly List<List<LinkRenderData>> _links = new List<List<LinkRenderData>>();
        private readonly DispatcherTimer _actionButtonsTimer = new DispatcherTimer();
        private double _workHeight;
        private double _workWidth;

        public ThreePagePanel()
        {
            InitializeComponent();

            _texts = Enumerable.Range(0, 3).Select(n => new List<TextRenderData>()).ToList();
            _links = Enumerable.Range(0, 3).Select(n => new List<LinkRenderData>()).ToList();
            Bookmarks = new List<BookmarkModel>();

            _actionButtonsTimer.Interval = TimeSpan.FromMilliseconds(400);
            _actionButtonsTimer.Tick += ShowActionButtonsOnTick;

            SelectionActions.Translate += () => Translate(GetSelectionText());
            SelectionActions.Bookmark += () => AddBookmark(GetSelectionText(), FirstSelectionText, LastSelectionText);
            SelectionActions.Copy += () => Copy(GetSelectionText());
            SelectionActions.Share += () => Share(GetSelectionText());
        }

        public Action<string> Copy = delegate { };
        public Action<string> Share = delegate { };
        public Action<string> Translate = delegate { };
        public Action<string, TextRenderData, TextRenderData> AddBookmark = delegate { };


        public IList<TextRenderData> NextTexts
        {
            get
            {
                return _texts[0];
            }
        }

        public IList<TextRenderData> CurrentTexts
        {
            get
            {
                return _texts[1];
            }
        }

        public IList<TextRenderData> PreviousTexts
        {
            get
            {
                return _texts[2];
            }
        }

        public IList<LinkRenderData> NextLinks
        {
            get
            {
                return _links[0];
            }
        }

        public IList<LinkRenderData> CurrentLinks
        {
            get
            {
                return _links[1];
            }
        }

        public IList<LinkRenderData> PreviousLinks
        {
            get
            {
                return _links[2];
            }
        }

        public IList<BookmarkModel> Bookmarks { get; set; }
        public PageManipulatorBase Manipulator { get; set; }

        private static double OffsetX
        {
            get
            {
                return AppSettings.Default.Margin.Left;
            }
        }

        public TextRenderData FirstSelectionText { get; private set; }

        public TextRenderData LastSelectionText { get; private set; }

        public Panel GetChildPanel(int index)
        {
            return (Panel)RootGrid.Children[index];
        }

        public void SetSize(double width, double height, double workWidth, double workHeight)
        {
            _workWidth = workWidth;
            _workHeight = workHeight;

            Width = width;
            Height = height;

            foreach (Panel p in RootGrid.Children)
            {
                p.Width = width;
                p.Height = height;

                p.Clip = new RectangleGeometry {Rect = new Rect(0, 0, width, height)};
            }
        }

        public Panel GetCurrentPagePanel()
        {
            return Manipulator.GetCurrentPagePanel();
        }

        public Panel GetNextPagePanel()
        {
            return Manipulator.GetNextPagePanel();
        }

        public Panel GetPrevPagePanel()
        {
            return Manipulator.GetPrevPagePanel();
        }

        public void SwapPrevWithCurrent()
        {
            Manipulator.SwapPrevWithCurrent();  
            RotateToEnd(_texts);
            RotateToEnd(_links);
        }

        public void SwapNextWithCurrent()
        {
            Manipulator.SwapNextWithCurrent();  
            RotateToStart(_texts);
            RotateToStart(_links);
        }

        public Size GetSize()
        {
            Debug.WriteLine(Width);
            
            return new Size(Math.Abs(_workWidth), Math.Abs(_workHeight));
        }

        public void Clear()
        {
            foreach (Panel p in RootGrid.Children)
                p.Children.Clear();

            if (Manipulator != null)
                Manipulator.Dispose();

            SelectionActions.Hide();
            SelectionPolygon.Visibility = Visibility.Collapsed;
            LeftSelectionItem.Visibility = Visibility.Collapsed;
            RightSelectionItem.Visibility = Visibility.Collapsed;
        }

        private void RotateToStart<T>(IList<T> list)
        {
            T obj = list[2];
            list.RemoveAt(2);
            list.Insert(0, obj);
        }

        private void RotateToEnd<T>(IList<T> list)
        {
            T obj = list[0];
            list.RemoveAt(0);
            list.Add(obj);
        }

        public void Highlight(Panel panel, Rect a, Rect b, BookmarkModel bookmark, Color color)
        {
            PointCollection selectionPolygon = GetSelectionPolygon(a, b, panel.ActualWidth);
            Brush brush = new SolidColorBrush(color){Opacity = 0.3};
            
            var polygon = new Polygon();
            polygon.Width = panel.ActualWidth;
            polygon.Height = panel.ActualHeight;
            polygon.Fill = brush;
            polygon.Points = selectionPolygon;
            polygon.Visibility = Visibility.Visible;

            panel.Children.Insert(0, polygon);
        }

        public void SetSelection(TextRenderData wa, TextRenderData wb)
        {
            SelectionActions.Hide();

            Rect rect = wa.Rect;
            Rect lastSelectionRect = wb.Rect;
            Polygon currentPolygon = SelectionPolygon;
            if (currentPolygon == null)
                return;
            if (rect.Top > lastSelectionRect.Top || Math.Abs(rect.Top - lastSelectionRect.Top) < 0.0001 && rect.Left > lastSelectionRect.Left)
            {
                TextRenderData textContext = wa;
                wa = wb;
                wb = textContext;
            }
            if (FirstSelectionText == wa && LastSelectionText == wb)
                return;
            FirstSelectionText = wa;
            LastSelectionText = wb;
            Rect firstSelectionRect = wa.Rect;
            lastSelectionRect = wb.Rect;
            PointCollection selectionPolygon = GetSelectionPolygon(firstSelectionRect, lastSelectionRect, GetCurrentPagePanel().ActualWidth);
            currentPolygon.Points = selectionPolygon;
            currentPolygon.Fill = AppSettings.Default.ColorScheme.SelectionBrush;
            currentPolygon.Visibility = Visibility.Visible;



            BuildSelectionItem(LeftSelectionItem, firstSelectionRect.Left - 11, firstSelectionRect.Top, firstSelectionRect.Height);
            BuildSelectionItem(RightSelectionItem, lastSelectionRect.Right - 11, lastSelectionRect.Top, lastSelectionRect.Height);

            var tmp = (firstSelectionRect.Left + SelectionActions.Width + OffsetX) - GetCurrentPagePanel().ActualWidth;
            tmp = tmp < 0 ? 0 : tmp;
            var leftMargin = firstSelectionRect.Left - tmp;

            var topMargin = firstSelectionRect.Top - SelectionActions.Height - 12;
            topMargin = topMargin < 0 ? 0 : topMargin;

            SelectionActions.Margin = new Thickness(leftMargin, topMargin, 0, 0);

        }

        private void BuildSelectionItem(SelectionItemControl control, double leftOffset, double topOffset, double height)
        {
            double lineIntervalCoef = Convert.ToDouble(AppSettings.Default.FontSettings.FontInterval);
            lineIntervalCoef = lineIntervalCoef >= 1 ? 1 : lineIntervalCoef;

            leftOffset = Screen.RoundScalePixel(leftOffset);
            topOffset = Screen.RoundScalePixel(topOffset);
            height = Screen.RoundScalePixel(height * lineIntervalCoef);

            control.Margin = new Thickness(leftOffset, topOffset + height * (1 - lineIntervalCoef), 0, 0);
            
            control.SelectionHeight = height;
            control.Visibility = Visibility.Visible;
        }

        private static PointCollection GetSelectionPolygon(Rect a, Rect b, double width)
        {
            return SelectionHelper.GetSelectionPolygon(a, b, width, OffsetX,
                                                       Convert.ToDouble(AppSettings.Default.FontSettings.FontInterval));
        }

        public bool CheckSelection(Point point)
        {
            Rect firstSelectionRect = FirstSelectionText.Rect;
            Rect lastSelectionRect = LastSelectionText.Rect;
            
            if (point.Y < firstSelectionRect.Top || point.Y > lastSelectionRect.Bottom)
                return false;
            
            if (firstSelectionRect.Top < lastSelectionRect.Top)
                return (point.Y >= firstSelectionRect.Bottom || point.X >= firstSelectionRect.Left) && (point.Y <= lastSelectionRect.Top || point.X <= lastSelectionRect.Right);
            
            if (point.X >= firstSelectionRect.Left)
                return point.X <= lastSelectionRect.Right;
            
            return false;
        }

        public void ClearSelection()
        {
            _actionButtonsTimer.Stop();
            SelectionActions.Hide();

            FirstSelectionText = LastSelectionText = null;
            SelectionPolygon.Visibility = Visibility.Collapsed;

            RightSelectionItem.Visibility = LeftSelectionItem.Visibility = Visibility.Collapsed;
        }

        public string GetSelectionText()
        {
            if (SelectionPolygon.Visibility != Visibility.Visible)
                return string.Empty;
            var stringBuilder = new StringBuilder();
            bool flag1 = true;
            bool flag2 = false;
            int num = 0;

            var lastWord = PreviousTexts.LastOrDefault() ?? new TextRenderData();
            if (lastWord.TokenID == FirstSelectionText.TokenID)
                stringBuilder.Append(lastWord.Text);

            RemoveBreakSymbol(stringBuilder);

            foreach (TextRenderData textContext in CurrentTexts)
            {
                if (textContext.Rect == FirstSelectionText.Rect)
                {
                    flag2 = true;
                    num = textContext.ParagraphID;
                }
                if (flag2)
                {
                    if (num != textContext.ParagraphID)
                    {
                        stringBuilder.AppendLine();
                        num = textContext.ParagraphID;
                        flag1 = true;
                    }
                    if (!flag1)
                    {
                        if (!RemoveBreakSymbol(stringBuilder))
                        {
                            stringBuilder.Append(" ");
                        }
                    }
                    flag1 = false;
                    stringBuilder.Append(textContext.Text);
                }
                if (textContext.Rect != LastSelectionText.Rect) 
                    continue;
                
                var nextWord = NextTexts.FirstOrDefault() ?? new TextRenderData();
                if(textContext.TokenID == nextWord.TokenID)
                {
                    RemoveBreakSymbol(stringBuilder);

                    stringBuilder.Append(nextWord.Text);
                }
                break;
            }
            return stringBuilder.ToString();
        }

        private static bool RemoveBreakSymbol(StringBuilder stringBuilder)
        {
            if (Regex.IsMatch(stringBuilder.ToString(), "\\w\\-$"))
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                return true;
            }
            return false;
        }

        public void ShowActionButtons()
        {
            SelectionActions.Hide();
            _actionButtonsTimer.Stop();
            _actionButtonsTimer.Start();
        }

        private void ShowActionButtonsOnTick(object sender, EventArgs eventArgs)
        {
            _actionButtonsTimer.Stop();
            SelectionActions.Show();
        }
    }
}
