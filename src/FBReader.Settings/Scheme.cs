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

using System.Windows.Media;

namespace FBReader.Settings
{
    public class Scheme
    {
        public Scheme(
            ColorSchemes colorScheme,
            Color backgroundBrush, 
            Color titleForegroundBrush,
            Color textForegroundBrush, 
            Color linkForegroundBrush,
            Color selectionBrush, 
            Color applicationBarBackgroundBrush,
            Color progressBarBrush, 
            Color systemTrayForegroundColor)
        {
            ColorScheme = colorScheme;
            BackgroundBrush = new SolidColorBrush(backgroundBrush);
            TitleForegroundBrush = new SolidColorBrush(titleForegroundBrush);
            TextForegroundBrush = new SolidColorBrush(textForegroundBrush);
            LinkForegroundBrush = new SolidColorBrush(linkForegroundBrush);
            SelectionBrush = new SolidColorBrush(selectionBrush);
            ApplicationBarBackgroundBrush = new SolidColorBrush(applicationBarBackgroundBrush);
            ProgressBarBrush = new SolidColorBrush(progressBarBrush);
            SystemTrayForegroundColor = systemTrayForegroundColor;
        }
        public ColorSchemes ColorScheme { get; set; }

        public SolidColorBrush BackgroundBrush { get; set; }

        public SolidColorBrush TitleForegroundBrush { get; set; }

        public SolidColorBrush TextForegroundBrush { get; set; }

        public SolidColorBrush LinkForegroundBrush { get; set; }

        public SolidColorBrush ApplicationBarBackgroundBrush { get; set; }

        public SolidColorBrush ProgressBarBrush { get; set; }

        public SolidColorBrush SelectionBrush { get; set; }

        public Color SystemTrayForegroundColor { get; set; }
    }
}
