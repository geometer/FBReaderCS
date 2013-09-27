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
    using System;

    /// <summary>
    /// The mode used to save/restore data.
    /// </summary>
    [Flags]
    public enum StorageMode {
        /// <summary>
        /// Automatic Determine the Mode
        /// </summary>
        Automatic = 0,
        /// <summary>
        /// Use Temporary storage.
        /// </summary>
        Temporary = 2,
        /// <summary>
        /// Use Permenent storage.
        /// </summary>
        Permanent = 4,
        /// <summary>
        /// Use any storage mechanism available.
        /// </summary>
        Any = Temporary | Permanent
    }
}