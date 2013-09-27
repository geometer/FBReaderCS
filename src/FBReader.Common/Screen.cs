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
using Microsoft.Phone.Controls;

namespace FBReader.Common
{
    public static class Screen
    {
        private static PhoneApplicationFrame _frame;

        private const int WVGA_SCALE_FACTOR = 100;
        private const int WXGA_SCALE_FACTOR = 160;
        private const int _720P_SCALE_FACTOR = 150;

        private static readonly double ScaleFactor = Application.Current.Host.Content.ScaleFactor / 100d;

        //480x800
        public static bool IsWVGA
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == WVGA_SCALE_FACTOR;
            }
        }

        //768x1280
        public static bool IsWXGA
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == WXGA_SCALE_FACTOR;
            }
        }

        //720x1280 pixels
        public static bool Is720p
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == _720P_SCALE_FACTOR;
            }
        }

        public static double RoundScalePixel(double value)
        {
            return (int)(value * ScaleFactor) / ScaleFactor;
        }


        public static void Init(PhoneApplicationFrame frame)
        {
            _frame = frame;
        }

        public static PhoneApplicationFrame Frame
        {
            get
            {
                return _frame;
            }
        }

        public static double Width
        {
            get
            {
                return Frame.Orientation == PageOrientation.PortraitUp
                           ? Application.Current.Host.Content.ActualWidth
                           : Application.Current.Host.Content.ActualHeight;
            }
        }

        public static double Height
        {
            get
            {
                return Frame.Orientation != PageOrientation.PortraitUp
                         ? Application.Current.Host.Content.ActualWidth
                         : Application.Current.Host.Content.ActualHeight;
            }
        }
    }
}
