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
using System.Windows.Media;

namespace Caliburn.Micro
{
    /// <summary>
    /// Extension methods for <see cref="System.Windows.UIElement"/>
    /// </summary>
    public static class UIElementExtensions {
        static readonly ILog Log = LogManager.GetLog(typeof(UIElementExtensions));

        /// <summary>
        /// Calls TransformToVisual on the specified element for the specified visual, suppressing the ArgumentException that can occur in some cases.
        /// </summary>
        /// <param name="element">Element on which to call TransformToVisual.</param>
        /// <param name="visual">Visual to pass to the call to TransformToVisual.</param>
        /// <returns>Resulting GeneralTransform object.</returns>
        public static GeneralTransform SafeTransformToVisual(this UIElement element, UIElement visual)
        {
            GeneralTransform result;
            try {
                result = element.TransformToVisual(visual);
            } catch (ArgumentException ex) {
                Log.Error(ex);

                // Not perfect, but better than throwing an exception
                result = new TranslateTransform();
            }
            return result;
        }
    }
}
