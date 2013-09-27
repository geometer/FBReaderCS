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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FBReader.Common;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace FBReader.App.Views.Controls
{
    public partial class ColorSelectorControl : UserControl
    {
        private readonly List<SolidColorBrush> _bookmarkColors = new[]
                                                       {
                                                            Color.FromArgb(0xFF, 0xA4, 0xC4, 0x00),
                                                            Color.FromArgb(0xFF, 0x60, 0xA9, 0x17),
                                                            Color.FromArgb(0xFF, 0x00, 0x8A, 0x00),
                                                            Color.FromArgb(0xFF, 0x00, 0xAB, 0xA9),
                                                            Color.FromArgb(0xFF, 0x1B, 0xA1, 0xE2),
                                                            Color.FromArgb(0xFF, 0x00, 0x50, 0xEF),
                                                            Color.FromArgb(0xFF, 0x6A, 0x00, 0xFF),
                                                            Color.FromArgb(0xFF, 0xAA, 0x00, 0xFF),
                                                            Color.FromArgb(0xFF, 0xF4, 0x72, 0xD0),
                                                            Color.FromArgb(0xFF, 0xD8, 0x00, 0x73),
                                                            Color.FromArgb(0xFF, 0xA2, 0x00, 0x25),
                                                            Color.FromArgb(0xFF, 0xE5, 0x14, 0x00),
                                                            Color.FromArgb(0xFF, 0xFA, 0x68, 0x00),
                                                            Color.FromArgb(0xFF, 0xF0, 0xA3, 0x0A),
                                                            Color.FromArgb(0xFF, 0xE3, 0xC8, 0x00),
                                                            Color.FromArgb(0xFF, 0x82, 0x5A, 0x2C),
                                                            Color.FromArgb(0xFF, 0x6D, 0x87, 0x64),
                                                            Color.FromArgb(0xFF, 0x64, 0x76, 0x87),
                                                            Color.FromArgb(0xFF, 0x76, 0x60, 0x8A),
                                                            Color.FromArgb(0xFF, 0x87, 0x79, 0x4E)
                                                       }.Select(c => new SolidColorBrush(c)).ToList();

        public event Action<Color> ColorSelected = delegate { };
        public event Action Closed = delegate { };  

        public ColorSelectorControl()
        {
            InitializeComponent();
            ColorsPanel.ItemsSource = _bookmarkColors;
            Root.Width = Screen.Width;
            Root.Height = Screen.Height;
            Screen.Frame.OrientationChanged += FrameOnOrientationChanged;

            PopupWindow.WindowClosed += PopupWindowOnWindowClosed;
            
        }

        private void FrameOnOrientationChanged(object sender, OrientationChangedEventArgs orientationChangedEventArgs)
        {
            Root.Width = Screen.Width;
            Root.Height = Screen.Height;
        }

        public void Show()
        {
            PopupWindow.IsOpen = true;
        }

        public void Hide()
        {
            PopupWindow.IsOpen = false;
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            var colorRect = (FrameworkElement) sender;
            var brush = (SolidColorBrush) colorRect.DataContext;
            var color = brush.Color;
            ColorSelected(color);
        }

        private void PopupWindowOnWindowClosed(object sender, WindowClosedEventArgs windowClosedEventArgs)
        {
            Closed();
        }
    }
}
