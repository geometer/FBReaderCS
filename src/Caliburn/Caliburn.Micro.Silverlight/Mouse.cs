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

namespace Caliburn.Micro {
    using System.Windows;

    /// <summary>
    /// A mouse helper utility.
    /// </summary>
    public static class Mouse {
        /// <summary>
        /// The current position of the mouse.
        /// </summary>
        public static Point Position { get; set; }

        /// <summary>
        /// Initializes the mouse helper with the UIElement to use in mouse tracking.
        /// </summary>
        /// <param name="element">The UIElement to use for mouse tracking.</param>
        public static void Initialize(UIElement element) {
            element.MouseMove += (s, e) => { Position = e.GetPosition(null); };
        }
    }
}