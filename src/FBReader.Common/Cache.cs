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

namespace FBReader.Common
{
    public class Cache<TKey, TValue> where TValue : class
    {
        private readonly Dictionary<TKey, TValue> cache;
        private readonly Func<TKey, TValue> loader;

        public Cache(Func<TKey, TValue> loader)
        {
            cache = new Dictionary<TKey, TValue>();
            this.loader = loader;
        }

        public TValue this[TKey key]
        {
            get { return cache[key]; }
            set { cache[key] = value; }
        }

        public void Clear()
        {
            cache.Clear();
        }

        public TValue Get(TKey key)
        {
            if (!cache.ContainsKey(key))
            {
                lock (this)
                {
                    if (!cache.ContainsKey(key))
                    {
                        TValue val = loader(key);
                        if (val == null)
                            return default(TValue);
                        
                        cache[key] = val;
                        return val;
                    }
                }
            }
            return cache[key];
        }
    }
}