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

    /// <summary>
    ///   An implementation of <see cref = "IResult" /> that enables sequential execution of multiple results.
    /// </summary>
    public class SequentialResult : IResult {
        readonly IEnumerator<IResult> enumerator;
        ActionExecutionContext context;

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SequentialResult" /> class.
        /// </summary>
        /// <param name = "enumerator">The enumerator.</param>
        public SequentialResult(IEnumerator<IResult> enumerator) {
            this.enumerator = enumerator;
        }

        /// <summary>
        ///   Occurs when execution has completed.
        /// </summary>
        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        /// <summary>
        ///   Executes the result using the specified context.
        /// </summary>
        /// <param name = "context">The context.</param>
        public void Execute(ActionExecutionContext context) {
            this.context = context;
            ChildCompleted(null, new ResultCompletionEventArgs());
        }

        void ChildCompleted(object sender, ResultCompletionEventArgs args) {
            var previous = sender as IResult;
            if (previous != null) {
                previous.Completed -= ChildCompleted;
            }

            if(args.Error != null || args.WasCancelled) {
                OnComplete(args.Error, args.WasCancelled);
                return;
            }

            var moveNextSucceeded = false;
            try {
                moveNextSucceeded = enumerator.MoveNext();
            }
            catch(Exception ex) {
                OnComplete(ex, false);
                return;
            }

            if(moveNextSucceeded) {
                try {
                    var next = enumerator.Current;
                    IoC.BuildUp(next);
                    next.Completed += ChildCompleted;
                    next.Execute(context);
                }
                catch(Exception ex) {
                    OnComplete(ex, false);
                    return;
                }
            }
            else {
                OnComplete(null, false);
            }
        }

        void OnComplete(Exception error, bool wasCancelled) {
            enumerator.Dispose();
            Completed(this, new ResultCompletionEventArgs { Error = error, WasCancelled = wasCancelled });
        }
    }
}