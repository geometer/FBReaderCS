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
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace FBReader.App.Views.Controls
{
    public partial class HighlightedTextBlockControl
    {

        private string _oldText;
        private string _oldQuery;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (HighlightedTextBlockControl), new PropertyMetadata(default(string), PropertyChangedCallback));



        public static readonly DependencyProperty QueryProperty =
            DependencyProperty.Register("Query", typeof(string), typeof(HighlightedTextBlockControl), new PropertyMetadata(default(string), PropertyChangedCallback));

        public string Query
        {
            get { return (string) GetValue(QueryProperty); }
            set { SetValue(QueryProperty, value); }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public HighlightedTextBlockControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateValues();
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (HighlightedTextBlockControl) dependencyObject;
            
            @this.UpdateValues();
        }

        private void UpdateValues()
        {
            if(TextBlock == null)
                return;
            if(Text == null || Query == null)
                return;

            if (Text == _oldText && Query == _oldQuery)
                return;

            _oldText = Text;
            _oldQuery = Query;

            var parts = new List<string>();
            var searchedParts = new List<string>();
            int startIndex = 0;

            do
            {
                int index = Text.IndexOf(Query, startIndex, StringComparison.InvariantCultureIgnoreCase);
                if (index == -1)
                {
                    parts.Add(Text.Substring(startIndex, Text.Length - startIndex));
                    break;
                }
                var part = Text.Substring(startIndex, index - startIndex);
                var searchedPart = Text.Substring(index, Query.Length);
                searchedParts.Add(searchedPart);
                parts.Add(part);
                startIndex = index + Query.Length;
            } while (startIndex < Text.Length);

            for (int i = 0; i < parts.Count; i++)
            {
                var run = new Run();
                run.Text = parts[i];
                TextBlock.Inlines.Add(run);
                if (i >= searchedParts.Count)
                    continue;

                var searchedRun = new Run();
                searchedRun.Text = searchedParts[i];
                searchedRun.Foreground = (SolidColorBrush)Resources["PhoneAccentBrush"];
                TextBlock.Inlines.Add(searchedRun);
            }
        }
    }
}
