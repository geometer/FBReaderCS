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
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace FBReader.App.Interaction
{
    public class ManipulationListener : DependencyObject
    {
        private bool _completed;
        private bool _deltaDetected;
        private UIElement _host;
        private ManipulationStartedEventArgs _startArgs;
        private DateTime _startTime;

        public ManipulationListener()
        {
            ExclusiveAccess = false;
        }


        public bool ExclusiveAccess { get; set; }

        public bool IsAttached
        {
            get { return _host != null; }
        }

        public event EventHandler<PointManipulationEventArgs> Tap;

        public event EventHandler<PointManipulationEventArgs> Hold;

        public event EventHandler<ManipulationStartedEventArgs> Started;

        public event EventHandler<ManipulationDeltaEventArgs> Delta;

        public event EventHandler<ManipulationCompletedEventArgs> Completed;

        public void Attach(UIElement e)
        {
            if (_host != null)
                return;
            _host = e;
            e.Tap += OnTap;
            e.Hold += OnHold;
            e.ManipulationCompleted += OnManipulationCompleted;
            e.ManipulationDelta += OnManipulationDelta;
            e.ManipulationStarted += OnManipulationStarted;
        }


        public void Detach(UIElement e)
        {
            e.Tap -= OnTap;
            e.Hold -= OnHold;
            e.ManipulationCompleted -= OnManipulationCompleted;
            e.ManipulationDelta -= OnManipulationDelta;
            e.ManipulationStarted -= OnManipulationStarted;
            _host = null;
        }

        private void OnHold(object sender, GestureEventArgs gestureEventArgs)
        {
            Debug.WriteLine("OnHold");
            _completed = true;
            InvokePointEvent(Hold);
        }

        private void OnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            Debug.WriteLine("OnTap");

            _completed = true;
            InvokeTapEvent();
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            Debug.WriteLine("OnManipulationStarted");
            _completed = false;
            _deltaDetected = false;
            _startArgs = e;
            _startTime = DateTime.Now;
            if (ExclusiveAccess)
                e.Handled = true;
            EventHandler<ManipulationStartedEventArgs> eventHandler = Started;
            if (eventHandler == null)
                return;
            eventHandler(sender, e);
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Debug.WriteLine("OnManipulationDelta");

            if (_completed)
                return;
            _deltaDetected = true;
            EventHandler<ManipulationDeltaEventArgs> eventHandler = Delta;
            if (eventHandler == null)
                return;
            eventHandler(sender, e);
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            Debug.WriteLine("OnManipulationCompleted");
            if (_completed)
                return;

            try
            {
                if (!_deltaDetected && (DateTime.Now - _startTime).TotalSeconds < 0.5)
                {
//                    if (!_tapHandled)
//                        this.InvokeTapEvent();
                }
                else
                {
                    EventHandler<ManipulationCompletedEventArgs> eventHandler = Completed;
                    if (eventHandler == null)
                        return;
                    eventHandler(sender, e);
                }
            }
            finally
            {
                _completed = true;
            }
        }


        private void InvokeTapEvent()
        {
            InvokePointEvent(Tap);
        }

        private void InvokePointEvent(EventHandler<PointManipulationEventArgs> handler)
        {
            if (handler == null || _startArgs == null)
                return;
            UIElement manipulationContainer = _startArgs.ManipulationContainer;
            PointManipulationEventArgs eventArgs = null;
            if (manipulationContainer != _host)
            {
                try
                {
                    Point origin =
                        manipulationContainer.TransformToVisual(_host).Transform(_startArgs.ManipulationOrigin);
                    eventArgs = new PointManipulationEventArgs(origin, _host);
                    handler(_host, eventArgs);
                }
                catch
                {
                }
            }
            else
            {
                eventArgs = new PointManipulationEventArgs(_startArgs.ManipulationOrigin, _host);
                handler(_host, eventArgs);
            }

            if (eventArgs != null)
                _completed = eventArgs.Handled;
        }
    }
}