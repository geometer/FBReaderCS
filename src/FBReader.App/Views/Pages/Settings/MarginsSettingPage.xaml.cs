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
using System.Windows.Data;
using Microsoft.Phone.Controls;

namespace FBReader.App.Views.Pages.Settings
{
    public partial class MarginsSettingPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty ExampleMarginProperty =
            DependencyProperty.Register("ExampleMargin", typeof(Thickness), typeof(MarginsSettingPage), new PropertyMetadata(default(Thickness), PropertyChangedCallback));

        public Thickness ExampleMargin
        {
            get { return (Thickness)GetValue(ExampleMarginProperty); }
            set { SetValue(ExampleMarginProperty, value); }
        }

        public MarginsSettingPage()
        {
            InitializeComponent();

            SetBinding(ExampleMarginProperty, new Binding("Margin"));
        }

        private void ChangeMargins(Thickness margin)
        {
            var horisontalCoef = Display.Width / 480;
            var verticalCoef = Display.Height / 800;
            var resizedMargin = new Thickness(
                margin.Left * horisontalCoef,
                margin.Top * verticalCoef,
                margin.Right * horisontalCoef,
                margin.Bottom * verticalCoef);
            LineGrid.LineMargins = resizedMargin;
            DummyText.Margin = resizedMargin;

            var newDummyTextHeight = Display.Height - resizedMargin.Top - resizedMargin.Bottom;

            var lines = Math.Floor(newDummyTextHeight / DummyText.LineHeight);
            newDummyTextHeight = lines * DummyText.LineHeight;
            DummyText.Height = newDummyTextHeight;
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (MarginsSettingPage) dependencyObject;
            @this.ChangeMargins((Thickness)dependencyPropertyChangedEventArgs.NewValue);
        }
    }
}