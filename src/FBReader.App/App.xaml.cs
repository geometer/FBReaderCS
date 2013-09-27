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
using System.Collections;
using System.Windows;
using System.Windows.Media;
using FBReader.App.Utils;

namespace FBReader.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MergeCustomColors();

#if DEBUG && !READING
            //MetroGridHelper.IsVisible = true;
#endif
        }

        private static void MergeCustomColors()
        {
            var dictionaries = new ResourceDictionary();
            var themeStyles = new ResourceDictionary { Source = new Uri("/FBReader.App;component/Resources/FBReaderThemeResources.xaml", UriKind.Relative) };
            dictionaries.MergedDictionaries.Add(themeStyles);


            ResourceDictionary appResources = Application.Current.Resources;
            foreach (DictionaryEntry entry in dictionaries.MergedDictionaries[0])
            {
                var colorBrush = entry.Value as SolidColorBrush;
                var existingBrush = appResources[entry.Key] as SolidColorBrush;
                if (existingBrush != null && colorBrush != null)
                {
                    existingBrush.Color = colorBrush.Color;
                }
            }
        }
    }
}