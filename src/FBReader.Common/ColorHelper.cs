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

namespace FBReader.Common
{
    public static class ColorHelper
    {
        public static int ToInt32(this Color color)
        {
            return color.A << 24 | color.R << 16 | color.G << 8 | color.B;
        }

        public static Color ToColor(int color)
        {
            return Color.FromArgb(
                (byte)((color >> 24) & 0xff), 
                (byte)((color >> 16) & 0xff), 
                (byte)((color >> 8) & 0xff), 
                (byte)(color & 0xff));
        }
    }
}
