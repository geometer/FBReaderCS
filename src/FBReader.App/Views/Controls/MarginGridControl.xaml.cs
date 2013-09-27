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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FBReader.App.Views.Controls
{
    public partial class MarginGridControl : UserControl
    {
        public static readonly DependencyProperty LineBrushProperty =
            DependencyProperty.Register("LineBrush", typeof (Brush), typeof (MarginGridControl), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty LineMarginsProperty =
            DependencyProperty.Register("LineMargins", typeof(Thickness), typeof(MarginGridControl), new PropertyMetadata(default(Thickness), PropertyChangedCallback));



        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(MarginGridControl), new PropertyMetadata(default(double)));

        public double StrokeThickness
        {
            get
            {
                return (double)GetValue(StrokeThicknessProperty);
            }
            set
            {
                SetValue(StrokeThicknessProperty, value);
            }
        }

        public Thickness LineMargins
        {
            get
            {
                return (Thickness) GetValue(LineMarginsProperty);
            }
            set
            {
                SetValue(LineMarginsProperty, value);
            }
        }

        public Brush LineBrush
        {
            get
            {
                return (Brush) GetValue(LineBrushProperty);
            }
            set
            {
                SetValue(LineBrushProperty, value);
            }
        }

        public MarginGridControl()
        {
            InitializeComponent();
        }

        private void SetMargins(Thickness margin)
        {
            LeftLine.Margin = new Thickness(margin.Left, 0, 0, 0);
            RightLine.Margin = new Thickness(0, 0, margin.Right, 0);
            TopLine.Margin = new Thickness(0, margin.Top, 0, 0);
            BottomLine.Margin = new Thickness(0, 0, 0, margin.Bottom);

        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (MarginGridControl) dependencyObject;
            @this.SetMargins((Thickness)dependencyPropertyChangedEventArgs.NewValue);
        }
    }
}
