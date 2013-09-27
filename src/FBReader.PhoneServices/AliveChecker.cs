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
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FBReader.PhoneServices
{
    public static class AliveChecker
    {
        public struct WeakRefWithType
        {
            public WeakRefWithType(object reference)
            {
                Reference = new WeakReference(reference);
                Type = reference.GetType();
            }

            public WeakReference Reference;
            public Type Type;
        }

        private static readonly List<WeakRefWithType> _references = new List<WeakRefWithType>();
        private static Timer _timer;

        static AliveChecker()
        {
            _timer = new Timer(OnTimer, null, 1000,1000);
        }

        static void OnTimer(object state)
        {
            GC.Collect();            

            lock (_references)
            {
                var removed = _references.Where(r => !r.Reference.IsAlive).ToList();

                foreach (var reference in removed)
                {
                    Log("DIED " + reference.Type.Name);
                    _references.Remove(reference);
                }
            }
        }

        public static void Monitor(object instance)
        {
            lock(_references)
            {                
                _references.Add(new WeakRefWithType(instance));
                
                Log("ADDED " + instance.GetType().Name);
            }
        }

        static void Log(string s)
        {
            Debug.WriteLine("~~~ AC ~~~ [{0}] - {1}", DateTime.Now.ToLongTimeString(), s);
        }
    }
}
