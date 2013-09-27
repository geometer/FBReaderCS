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
    /// <summary>
    /// Handles the storage of an object instance.
    /// </summary>
    public interface IStorageHandler {
        /// <summary>
        /// Gets or sets the coordinator.
        /// </summary>
        /// <value>
        /// The coordinator.
        /// </value>
        StorageCoordinator Coordinator { get; set; }

        /// <summary>
        /// Overrided by inheritors to configure the handler for use.
        /// </summary>
        void Configure();

        /// <summary>
        /// Indicates whether the specified instance can be stored by this handler.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        bool Handles(object instance);

        /// <summary>
        /// Saves the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="mode">The mode.</param>
        void Save(object instance, StorageMode mode);

        /// <summary>
        /// Restores the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="mode">The mode.</param>
        void Restore(object instance, StorageMode mode);
    }
}