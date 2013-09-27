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

namespace FBReader.Common.ExtensionMethods
{
    public static class RectExtensions
    {
        public static double DistanceTo(this Rect rect, Point point)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            if (point.Y < rect.Top)
                num1 = rect.Top - point.Y;
            if (point.Y > rect.Bottom)
                num1 = point.Y - rect.Bottom;
            if (point.X < rect.Left)
                num2 = rect.Left - point.X;
            if (point.X > rect.Right)
                num2 = point.X - rect.Right;
            return Math.Sqrt(num2 * num2 + num1 * num1);
        }
    }
}
