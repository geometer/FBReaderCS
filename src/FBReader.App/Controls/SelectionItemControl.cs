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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FBReader.App.Controls
{
    public class SelectionItemControl : StackPanel
    {
        private readonly Line _line;
        private double _selectionHeight;

        public double SelectionHeight
        {
            get
            {
                return _selectionHeight;
            }
            set
            {
                _selectionHeight = value;
                Update();
            }
        }

        private void Update()
        {
            _line.Y2 = _selectionHeight;
            UpdateLayout();
        }

        public SelectionItemControl() : this(0)
        {
        }

        public SelectionItemControl(double selectionHeight)
        {
            float strokeThickness = 2;
            string imagePath = "/Resources/Images/WVGA/TextSelect.png";
            float imageSide = 22;
            if (Application.Current.Host.Content.ScaleFactor != 100) // is HD
            {
                float sf = (Application.Current.Host.Content.ScaleFactor / 100f);
                imagePath = "/Resources/Images/WXGA/TextSelect.png";
                strokeThickness = 4 / sf;
                imageSide = 36 / sf;
            }

            _line = new Line();
            _line.StrokeThickness = strokeThickness;
            _line.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x48, 0x89));
            _line.X1 = 0;
            _line.Y1 = 0;
            _line.X2 = 0;
            _line.Y2 = selectionHeight;
            _line.StrokeEndLineCap = PenLineCap.Square;
            _line.HorizontalAlignment = HorizontalAlignment.Left;
            _line.Margin = new Thickness(imageSide / 2, 0, 0, 0);
            _line.VerticalAlignment = VerticalAlignment.Bottom;

            Children.Add(_line);

            var image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),                
                Width = imageSide,
                Height = imageSide,
            };

            Children.Add(image);
        }
    }
}
