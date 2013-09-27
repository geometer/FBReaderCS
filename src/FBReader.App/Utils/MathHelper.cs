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

namespace FBReader.App.Utils
{
    public static class MathHelper
    {

        public static bool IsPointInTriangle(Point first, Point second, Point third, Point point)
        {
            Func<double, double, double, double, double, double, double> mult = (x1, y1, x2, y2, x, y) => (x - x2) * (y2 - y1) - (y - y2) * (x2 - x1);

            bool cp1 = mult(first.X, first.Y, second.X, second.Y, point.X, point.Y) < 0.0;
            bool cp2 = mult(second.X, second.Y, third.X, third.Y, point.X, point.Y) < 0.0;
            bool cp3 = mult(third.X, third.Y, first.X, first.Y, point.X, point.Y) < 0.0;
            return cp1 == cp2 && cp2 == cp3;
        }

        public static int RoundToEven(double value)
        {
            return (int)(value - value % 2);
        }
    }
}
