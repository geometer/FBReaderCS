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

using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace FBReader.App.Controls
{
    public class ToggleSwitch : RadToggleSwitch
    {
        public static readonly DependencyProperty CheckedContentProperty =
            DependencyProperty.Register("CheckedContent", typeof (string), typeof (ToggleSwitch), new PropertyMetadata(default(ContentControl)));

        public static readonly DependencyProperty UncheckedContentProperty =
            DependencyProperty.Register("UncheckedContent", typeof(string), typeof(ToggleSwitch), new PropertyMetadata(default(ContentControl)));

        public string UncheckedContent
        {
            get { return (string)GetValue(UncheckedContentProperty); }
            set { SetValue(UncheckedContentProperty, value); }
        }

        public string CheckedContent
        {
            get { return (string)GetValue(CheckedContentProperty); }
            set { SetValue(CheckedContentProperty, value); }
        }

        public ToggleSwitch()
        {
            Checked += OnChecked;
            Unchecked += OnUnchecked;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Content = IsChecked ? CheckedContent : UncheckedContent;
        }

        private void OnUnchecked(object sender, RoutedEventArgs routedEventArgs)
        {
            Content = UncheckedContent;
        }

        private void OnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            Content = CheckedContent;
        }
    }
}
