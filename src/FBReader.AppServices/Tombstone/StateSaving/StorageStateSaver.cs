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

using Caliburn.Micro;

namespace FBReader.AppServices.Tombstone.StateSaving
{
    public class StorageStateSaver : IStorageStateSaver
    {
        private readonly IPhoneService _phoneService;

        public StorageStateSaver(IPhoneService phoneService)
        {
            _phoneService = phoneService;
        }

        public void Save<T>(T obj, string key) where T: class
        {
            _phoneService.State[key] = obj;
        }

        public T Restore<T>(string key) where T: class
        {
            object obj;
            if (_phoneService.State.TryGetValue(key, out obj))
            {
                _phoneService.State.Remove(key);
                return obj as T;
            }
            
            return null;
        }
    }
}