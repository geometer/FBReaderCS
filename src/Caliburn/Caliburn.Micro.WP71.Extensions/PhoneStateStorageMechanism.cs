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
    using System.Collections.Generic;

    /// <summary>
    /// Stores data in the phone state.
    /// </summary>
    public class PhoneStateStorageMechanism : IStorageMechanism {
        readonly IPhoneContainer container;
        readonly IPhoneService phoneService;
        List<string> keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneStateStorageMechanism"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="phoneService">The phone service.</param>
        public PhoneStateStorageMechanism(IPhoneContainer container, IPhoneService phoneService) {
            this.container = container;
            this.phoneService = phoneService;
        }

        /// <summary>
        /// Indicates what storage modes this mechanism provides.
        /// </summary>
        /// <param name="mode">The storage mode to check.</param>
        /// <returns>
        /// Whether or not it is supported.
        /// </returns>
        public bool Supports(StorageMode mode) {
            return (mode & StorageMode.Temporary) == StorageMode.Temporary;
        }

        /// <summary>
        /// Begins the storage transaction.
        /// </summary>
        public void BeginStoring() {
            keys = new List<string>();
        }

        /// <summary>
        /// Stores the value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        public void Store(string key, object data) {
            if (!phoneService.State.ContainsKey(key)) {
                keys.Add(key);
            }

            phoneService.State[key] = data;
        }

        /// <summary>
        /// Ends the storage transaction.
        /// </summary>
        public void EndStoring() { }

        /// <summary>
        /// Tries to get the data previously stored with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// true if found; false otherwise
        /// </returns>
        public bool TryGet(string key, out object value) {
            return phoneService.State.TryGetValue(key, out value);
        }

        /// <summary>
        /// Deletes the data with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Delete(string key) {
            phoneService.State.Remove(key);
        }

        /// <summary>
        /// Clears the data stored in the last storage transaction.
        /// </summary>
        public void ClearLastSession() {
            if(keys != null) {
                keys.Apply(x => phoneService.State.Remove(x));
                keys = null;
            }
        }

        /// <summary>
        /// Registers service with the storage mechanism as a singleton.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="key">The key.</param>
        /// <param name="implementation">The implementation.</param>
        public void RegisterSingleton(Type service, string key, Type implementation) {
            container.RegisterWithPhoneService(service, key, implementation);
        }
    }
}