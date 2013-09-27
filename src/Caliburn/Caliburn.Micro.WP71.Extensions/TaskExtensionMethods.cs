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
    /// Extension methods related to phone tasks.
    /// </summary>
    public static class TaskExtensionMethods {
        /// <summary>
        /// Creates a task and publishes it using the <see cref="EventAggregator"/>.
        /// </summary>
        /// <typeparam name="TTask">The task to create.</typeparam>
        /// <param name="events">The event aggregator.</param>
        /// <param name="configure">Optional configuration for the task.</param>
        /// <param name="state">Optional state to be passed along to the task completion message.</param>
        public static void RequestTask<TTask>(this IEventAggregator events, Action<TTask> configure = null, object state = null)
            where TTask : new() {
            var task = new TTask();

            if(configure != null) {
                configure(task);
            }

            events.Publish(new TaskExecutionRequested {
                State = state,
                Task = task
            });
        }
    }
}