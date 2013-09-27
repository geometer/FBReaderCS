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
using System.Windows.Input;
using System.Windows.Media.Animation;
using FBReader.Common;
using FBReader.Settings;

namespace FBReader.App.Controls
{
    public partial class TranslationControl : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TranslationControl), new PropertyMetadata(default(string)));

        public TranslationControl()
        {
            InitializeComponent();

            // Hide taps on border from root tap handler
            Border.Tap += (sender, args) => args.Handled = true;

            TranslationControlRoot.Width = Screen.Width;
            TranslationControlRoot.Height = Screen.Height;

            Opacity = 0;
            Visibility = Visibility.Collapsed;
            
            Border.Margin = new Thickness(12 + AppSettings.Default.Margin.Left, 0, 12 + AppSettings.Default.Margin.Right, 0);
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public bool IsOpen
        {
            get
            {
                return Visibility == Visibility.Visible;
            }
        }

        public void Hide()
        {
            var sb = new Storyboard();

            var da = new DoubleAnimation()
            {
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(250))
            };
            Storyboard.SetTarget(da, this);
            Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));

            sb.Children.Add(da);
            sb.Begin();
            sb.Completed += delegate { Visibility = Visibility.Collapsed; };
        }

        public void Show(string translatedText)
        {
            Text = translatedText;

            var sb = new Storyboard();

            var da = new DoubleAnimation()
                     {
                         To = 1,
                         Duration = new Duration(TimeSpan.FromMilliseconds(250))
                     };
            Storyboard.SetTarget(da, this);
            Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));

            sb.Children.Add(da);
            sb.Begin();

            Visibility = Visibility.Visible;
        }

        private void TranslationControl_OnTap(object sender, GestureEventArgs e)
        {
            Hide();
        }
    }
}
