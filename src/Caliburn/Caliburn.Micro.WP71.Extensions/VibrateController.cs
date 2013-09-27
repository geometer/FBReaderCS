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
    using Microsoft.Devices;

    /// <summary>
    ///   Allows applications to start and stop vibration on the device.
    /// </summary>
    public interface IVibrateController {
        /// <summary>
        ///   Starts vibration on the device.
        /// </summary>
        /// <param name="duration"> A TimeSpan object specifying the amount of time for which the phone vibrates. </param>
        void Start(TimeSpan duration);

        /// <summary>
        ///   Stops vibration on the device.
        /// </summary>
        void Stop();
    }

    /// <summary>
    ///   The default implementation of <see cref="IVibrateController" /> , using the system controller.
    /// </summary>
    public class SystemVibrateController : IVibrateController {
        /// <summary>
        ///   Starts vibration on the device.
        /// </summary>
        /// <param name="duration"> A TimeSpan object specifying the amount of time for which the phone vibrates. </param>
        public void Start(TimeSpan duration) {
            VibrateController.Default.Start(duration);
        }

        /// <summary>
        ///   Stops vibration on the device.
        /// </summary>
        public void Stop() {
            VibrateController.Default.Stop();
        }
    }
}