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
using System.Windows.Media;

namespace FBReader.Common
{
    public static class SelectionHelper
    {
        public static PointCollection GetSelectionPolygon(Rect topRect, Rect bottomRect, double width, double offsetX, double lineInterval)
        {
            double lineIntervalCompensation = 0;
            if(lineInterval < 1)
            {
                lineIntervalCompensation = 1 - lineInterval;
            }
            double topRectCompensation = topRect.Height * lineIntervalCompensation;
            double bottmRectCompensation = bottomRect.Height * lineIntervalCompensation;

            var pointCollection = new PointCollection();
            if (topRect.Top < bottomRect.Top)
            {
                pointCollection.Add(new Point(offsetX, topRect.Bottom));
                pointCollection.Add(new Point(topRect.Left, topRect.Bottom));
                pointCollection.Add(new Point(topRect.Left, topRect.Top + topRectCompensation));
                pointCollection.Add(new Point(width - offsetX, topRect.Top + topRectCompensation));
                pointCollection.Add(new Point(width - offsetX, bottomRect.Top + bottmRectCompensation));
                pointCollection.Add(new Point(bottomRect.Right, bottomRect.Top + bottmRectCompensation));
                pointCollection.Add(new Point(bottomRect.Right, bottomRect.Bottom));
                pointCollection.Add(new Point(offsetX, bottomRect.Bottom));
            }
            else
            {
                pointCollection.Add(new Point(topRect.Left, topRect.Bottom));
                pointCollection.Add(new Point(topRect.Left, topRect.Top + topRectCompensation));
                pointCollection.Add(new Point(bottomRect.Right, bottomRect.Top + bottmRectCompensation));
                pointCollection.Add(new Point(bottomRect.Right, bottomRect.Bottom));
            }
            return pointCollection;
        } 
    }
}
