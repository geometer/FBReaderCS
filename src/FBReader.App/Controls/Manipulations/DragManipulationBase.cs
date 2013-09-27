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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FBReader.Common;
using FBReader.Settings;
using Telerik.Windows.Controls;

namespace FBReader.App.Controls.Manipulations
{
    public abstract class DragManipulationBase : PageManipulatorBase
    {
        public const int DRAG_THRESHOLD = 150;

        private bool _skipNextEvent;
        private bool _isPaused;

        protected DragManipulationBase(ThreePagePanel bookView, FlippingMode mode) 
            :  base(bookView, mode)
        {
            RegisterManipulations();
            ResetTranslations();
        }


        protected abstract void DoFlip(FlipDirection direction);

        public override void Pause()
        {
            //UnregisterManipulations();
            _isPaused = true;
        }

        public override void Resume()
        {
            _isPaused = false;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {

            if (_isPaused)
                return;

            e.Handled = true;

            if (_skipNextEvent)
            {
                _skipNextEvent = false;
                return;
            }

            // Tap
            if (e.IsTapGesture())
            {
                if ((_mode & FlippingMode.Touch) == FlippingMode.Touch)
                {
                    var transform = (e.OriginalSource as FrameworkElement).TransformToVisual(_bookView);
                    var localControlPoint = e.ManipulationOrigin;
                    var point = transform.Transform(localControlPoint);
                    if (point.X > Screen.Width/2)
                    {
                        if (!IsLastPage)
                        {
                            UpdateLayers(FlipDirection.Forward);
                            DoFlip(FlipDirection.Forward);
                        }
                    }
                    else
                    {
                        if (!IsFirstPage)
                        {
                            UpdateLayers(FlipDirection.Backward);
                            DoFlip(FlipDirection.Backward);
                        }
                    }
                }
            }
            else
            {
                if ((_mode & FlippingMode.Slide) == FlippingMode.Slide)
                {
                    ManipulationCompleted(sender, e);
                }
            }
        }

        public override void CancelNextEvent()
        {
            _skipNextEvent = true;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_isPaused)
                return;

            e.Handled = true;

            if (e.CumulativeManipulation.Translation.X > 0 && IsFirstPage ||
                e.CumulativeManipulation.Translation.X < 0 && IsLastPage)
            {
               CancelNextEvent();
                e.Complete();
            }
            else
            {
                if ((_mode & FlippingMode.Slide) == FlippingMode.Slide)
                    ManipulationDelta(sender, e);
            }
        }

        public override void UpdatePanelsVisibility()
        {            
            RegisterManipulations();
        }

        protected abstract void ManipulationCompleted(object sender, ManipulationCompletedEventArgs e);
        protected abstract void ManipulationDelta(object sender, ManipulationDeltaEventArgs e);

        protected virtual void UpdateLayers(FlipDirection direction)
        {
            
        }

        protected abstract void ResetTranslations();

        protected void RegisterManipulations()
        {
            //RegisterHorizontalDrag(GetCurrentPagePanel());
            ApplyToEachPanel(RegisterHorizontalDrag);
        }

        protected void UnregisterManipulations()
        {
            ApplyToEachPanel(UnregisterHorizontalDrag);
        }

        private void UnregisterHorizontalDrag(Panel panel)
        {
            panel.ManipulationDelta -= OnManipulationDelta;
            panel.ManipulationCompleted -= OnManipulationCompleted;
        }

        private void RegisterHorizontalDrag(Panel panel)
        {
            // Hacky but very protective
            panel.ManipulationDelta -= OnManipulationDelta;
            panel.ManipulationCompleted -= OnManipulationCompleted;
            panel.ManipulationStarted -= OnManipulationStarted;

            panel.ManipulationStarted += OnManipulationStarted;
            panel.ManipulationDelta += OnManipulationDelta;
            panel.ManipulationCompleted += OnManipulationCompleted;
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (_isPaused)
                return;
        }

        public override void Dispose()
        {
            ApplyToEachPanel(UnregisterHorizontalDrag);
        }

    }
}
