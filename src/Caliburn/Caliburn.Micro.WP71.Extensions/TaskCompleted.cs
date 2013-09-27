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
    /// A message which is published when a task completes.
    /// </summary>
    /// <typeparam name="TTaskEventArgs">The type of the task event args.</typeparam>
    public class TaskCompleted<TTaskEventArgs> {
        /// <summary>
        /// Optional state provided by the original sender.
        /// </summary>
        public object State;

        /// <summary>
        /// The results of the task.
        /// </summary>
        public TTaskEventArgs Result;
    }
}