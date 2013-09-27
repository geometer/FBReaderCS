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
#if !SILVERLIGHT || SL5 || WP8
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Manages coroutine execution.
    /// </summary>
    public static class Coroutine {
        static readonly ILog Log = LogManager.GetLog(typeof(Coroutine));

        /// <summary>
        /// Creates the parent enumerator.
        /// </summary>
        public static Func<IEnumerator<IResult>, IResult> CreateParentEnumerator = inner => new SequentialResult(inner);

        /// <summary>
        /// Executes a coroutine.
        /// </summary>
        /// <param name="coroutine">The coroutine to execute.</param>
        /// <param name="context">The context to execute the coroutine within.</param>
        /// /// <param name="callback">The completion callback for the coroutine.</param>
        public static void BeginExecute(IEnumerator<IResult> coroutine, ActionExecutionContext context = null, EventHandler<ResultCompletionEventArgs> callback = null) {
            Log.Info("Executing coroutine.");

            var enumerator = CreateParentEnumerator(coroutine);
            IoC.BuildUp(enumerator);

            if (callback != null) {
                ExecuteOnCompleted(enumerator, callback);
            }

            ExecuteOnCompleted(enumerator, Completed);
            enumerator.Execute(context ?? new ActionExecutionContext());
        }

#if !SILVERLIGHT || SL5 || WP8
        /// <summary>
        /// Executes a coroutine asynchronous.
        /// </summary>
        /// <param name="coroutine">The coroutine to execute.</param>
        /// <param name="context">The context to execute the coroutine within.</param>
        /// <returns>A task that represents the asynchronous coroutine.</returns>
        public static Task ExecuteAsync(IEnumerator<IResult> coroutine, ActionExecutionContext context = null) {
            var taskSource = new TaskCompletionSource<object>();

            BeginExecute(coroutine, context, (s, e) => {
                if (e.Error != null)
                    taskSource.SetException(e.Error);
                else if (e.WasCancelled)
                    taskSource.SetCanceled();
                else
                    taskSource.SetResult(null);
            });

            return taskSource.Task;
        }
#endif

        static void ExecuteOnCompleted(IResult result, EventHandler<ResultCompletionEventArgs> handler) {
            EventHandler<ResultCompletionEventArgs> onCompledted = null;
            onCompledted = (s, e) => {
                result.Completed -= onCompledted;
                handler(s, e);
            };
            result.Completed += onCompledted;
        }

        /// <summary>
        /// Called upon completion of a coroutine.
        /// </summary>
        public static event EventHandler<ResultCompletionEventArgs> Completed = (s, e) => {
            if(e.Error != null) {
                Log.Error(e.Error);
            }
            else if(e.WasCancelled) {
                Log.Info("Coroutine execution cancelled.");
            }
            else {
                Log.Info("Coroutine execution completed.");
            }
        };
    }

    /// <summary>
    ///  Denotes a class which can handle a particular type of message and uses a Coroutine to do so.
    /// </summary>
    public interface IHandleWithCoroutine<TMessage> : IHandle {  //don't use contravariance here
		/// <summary>
		///  Handle the message with a Coroutine.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>The coroutine to execute.</returns>
		IEnumerable<IResult> Handle(TMessage message);
    }
}
