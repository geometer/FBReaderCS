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
using System.Collections.Generic;

namespace FBReader.AppServices
{
    public static class TransientStorage
    {
        private static readonly IDictionary<string, object> Storage = new Dictionary<string, object>();

        public static string Put<T>(T value)
        {
            var key = Guid.NewGuid().ToString();
            Storage[key] = value;
            return key;
        }

        public static T Get<T>(string key)
        {
            if (!Storage.ContainsKey(key))
            {
                throw new ArgumentException(string.Format("Unable to find object by key {0} in transient storage", key));
            }

            var result = Storage[key];
            Storage.Remove(key);
            return (T)result;
        }

        public static bool Contains(string key)
        {
            if (key == null)
                return false;

            return Storage.ContainsKey(key);
        }
    }

}
