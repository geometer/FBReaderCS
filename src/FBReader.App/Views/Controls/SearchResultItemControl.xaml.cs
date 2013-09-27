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
using FBReader.AppServices.DataModels;
using Microsoft.Phone.Controls;

namespace FBReader.App.Views.Controls
{
    public partial class SearchResultItemControl
    {
        public static readonly DependencyProperty SearchResultProperty =
            DependencyProperty.Register("SearchResult", typeof (SearchInBookResultItemDataModel), typeof (SearchResultItemControl), new PropertyMetadata(default(SearchInBookResultItemDataModel), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (SearchResultItemControl) dependencyObject;
            @this.UpdateText((SearchInBookResultItemDataModel) dependencyPropertyChangedEventArgs.NewValue);
        }

        public SearchInBookResultItemDataModel SearchResult
        {
            get
            {
                return (SearchInBookResultItemDataModel) GetValue(SearchResultProperty);
            }
            set
            {
                SetValue(SearchResultProperty, value);
            }
        }

        public SearchResultItemControl()
        {
            InitializeComponent();
        }

        private void UpdateText(SearchInBookResultItemDataModel newValue)
        {
            TextBefore.Text = newValue.TextBefore;
            TextAfter.Text = newValue.TextAfter;
            SearchedText.Text = newValue.SearchedText;
        }
    }
}