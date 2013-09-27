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

using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;

namespace FBReader.PhoneServices
{
    public class SettingsStorage
    {
        private readonly IsolatedStorageSettings _settings = IsolatedStorageSettings.ApplicationSettings;

        public T GetValue<T>(string key)
        {
            T value;
            bool exists = _settings.TryGetValue(key, out value);

            if (!exists)
                throw new KeyNotFoundException();

            return value;
        }

        public T GetValueWithDefault<T>(string key, T defaultValue)
        {
            T value;
            bool exists = _settings.TryGetValue(key, out value);

            if (exists)
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public void SetValue<T>(string key, T newValue)
        {
            _settings[key] = newValue;

            // Usually data is saved on app closing. But for debug it is usefull to save it incrementally.
            if (Debugger.IsAttached)
                _settings.Save();
        }
    }
}
