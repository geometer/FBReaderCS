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

namespace Caliburn.Micro
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
#if WinRT
    using Windows.UI.Xaml;
#else
    using System.Windows;
#endif

    /// <summary>
    /// The context used during the execution of an Action or its guard.
    /// </summary>
    public class ActionExecutionContext : IDisposable
    {
        WeakReference message;
        WeakReference source;
        WeakReference target;
        WeakReference view;
        Dictionary<string, object> values;

        /// <summary>
        /// Determines whether the action can execute.
        /// </summary>
        /// <remarks>Returns true if the action can execute, false otherwise.</remarks>
        public Func<bool> CanExecute;

        /// <summary>
        /// Any event arguments associated with the action's invocation.
        /// </summary>
        public object EventArgs;

        /// <summary>
        /// The actual method info to be invoked.
        /// </summary>
        public MethodInfo Method;

        /// <summary>
        /// The message being executed.
        /// </summary>
        public ActionMessage Message
        {
            get { return message == null ? null : message.Target as ActionMessage; }
            set { message = new WeakReference(value); }
        }

        /// <summary>
        /// The source from which the message originates.
        /// </summary>
        public FrameworkElement Source
        {
            get { return source == null ? null : source.Target as FrameworkElement; }
            set { source = new WeakReference(value); }
        }

        /// <summary>
        /// The instance on which the action is invoked.
        /// </summary>
        public object Target
        {
            get { return target == null ? null : target.Target; }
            set { target = new WeakReference(value); }
        }

        /// <summary>
        /// The view associated with the target.
        /// </summary>
        public DependencyObject View
        {
            get { return view == null ? null : view.Target as DependencyObject; }
            set { view = new WeakReference(value); }
        }

        /// <summary>
        /// Gets or sets additional data needed to invoke the action.
        /// </summary>
        /// <param name="key">The data key.</param>
        /// <returns>Custom data associated with the context.</returns>
        public object this[string key]
        {
            get
            {
                if (values == null)
                    values = new Dictionary<string, object>();

                object result;
                values.TryGetValue(key, out result);

                return result;
            }
            set
            {
                if (values == null)
                    values = new Dictionary<string, object>();

                values[key] = value;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Disposing(this, System.EventArgs.Empty);
        }

        /// <summary>
        /// Called when the execution context is disposed
        /// </summary>
        public event EventHandler Disposing = delegate { };
    }
}