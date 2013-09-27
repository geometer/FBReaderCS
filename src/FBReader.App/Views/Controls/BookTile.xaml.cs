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
using System.Windows.Media;

namespace FBReader.App.Views.Controls
{
    public partial class BookTile : UserControl
    {

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof (ImageSource), typeof (BookTile), new PropertyMetadata(default(ImageSource), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (BookTile) dependencyObject;
            @this.BookCover.Source = dependencyPropertyChangedEventArgs.NewValue as ImageSource;
            CheckForEmptyCover(@this);
        }

        private static void CheckForEmptyCover(BookTile tile)
        {
            if (tile.ImageSource == null)
            {
                tile.BookNameTextBlock.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof (string), typeof (BookTile), new PropertyMetadata(default(string), PropertyChangedCallback2));

        private static void PropertyChangedCallback2(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (BookTile)dependencyObject;
            @this.BookNameTextBlock.Text = dependencyPropertyChangedEventArgs.NewValue as string;
            CheckForEmptyCover(@this);
        }

        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public BookTile()
        {
            InitializeComponent();
        }
    }
}
