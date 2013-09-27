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
    /// Defines an interface through which the storage system can communicate with an IoC container.
    /// </summary>
    public interface IPhoneContainer {
        /// <summary>
        /// Occurs when a new instance is created.
        /// </summary>
        event Action<object> Activated;

        /// <summary>
        /// Registers the service as a singleton stored in the phone state.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="phoneStateKey">The phone state key.</param>
        /// <param name="implementation">The implementation.</param>
        void RegisterWithPhoneService(Type service, string phoneStateKey, Type implementation);

        /// <summary>
        /// Registers the service as a singleton stored in the app settings.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="appSettingsKey">The app settings key.</param>
        /// <param name="implementation">The implementation.</param>
        void RegisterWithAppSettings(Type service, string appSettingsKey, Type implementation);
    }
}