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

namespace FBReader.App.Controls.ApplicationBar
{
    public class FBReaderApplicationBarItemBase : DependencyObject
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FBReaderApplicationBarItemBase), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof (bool), typeof (FBReaderApplicationBarItemBase), new PropertyMetadata(true));

        public bool IsEnabled
        {
            get { return (bool) GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        internal FBReaderApplicationBar AppBar
        {
            get;
            set;
        }

        public string Message { get; set; }

        internal void DoClick()
        {
            if (string.IsNullOrEmpty(Message))
                return;

            var dataContext = AppBar.DataContext;

            var method = dataContext.GetType().GetMethod(Message);
            if (method == null)
            {
                throw new InvalidOperationException(string.Format("Invalid 'Message' parameter. ViewModel {0} doesn't contain method '{1}()'", dataContext.GetType().Name, Message));
            }

            method.Invoke(dataContext, new object[0]);
        }
    }
}