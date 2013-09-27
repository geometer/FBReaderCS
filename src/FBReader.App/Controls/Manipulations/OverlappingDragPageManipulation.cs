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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FBReader.Common;
using FBReader.Settings;

namespace FBReader.App.Controls.Manipulations
{
    public class OverlappingDragPageManipulation : DragManipulationBase
    {
        public OverlappingDragPageManipulation(ThreePagePanel bookView, FlippingMode mode)
            : base(bookView, mode)
        {
        }

        protected override void ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            double totalManipulation = (e.TotalManipulation.Translation.X + e.FinalVelocities.LinearVelocity.X/20);
            
            FlipDirection flipDirection = totalManipulation > DRAG_THRESHOLD
                ? FlipDirection.Backward
                : totalManipulation < -DRAG_THRESHOLD ? FlipDirection.Forward : FlipDirection.Revert;

            DoFlip(flipDirection);
        }

        protected override void DoFlip(FlipDirection flipDirection)
        {            
            var sb = new Storyboard();
            var da = new DoubleAnimation
                     {
                         Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                         EasingFunction = new SineEase()
                     };

            switch (flipDirection)
            {
                case FlipDirection.Forward:
                    da.To = -Screen.Width;
                    break;
                case FlipDirection.Backward:
                    da.To = Screen.Width;
                    break;
                case FlipDirection.Revert:
                    da.To = 0;
                    break;

                default:
                    throw new NotSupportedException();
            }

            var transform = (TranslateTransform) GetCurrentPagePanel().RenderTransform;

            Storyboard.SetTarget(da, transform);
            Storyboard.SetTargetProperty(da, new PropertyPath("X"));

            sb.Children.Add(da);
            if (flipDirection != FlipDirection.Revert)
            {
                UnregisterManipulations();

                sb.Completed += delegate
                                {
                                    TurnPage(flipDirection == FlipDirection.Forward);
                                };
            }

            sb.Begin();
        }

        protected override void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            FlipDirection direction = e.CumulativeManipulation.Translation.X > 0 ? FlipDirection.Backward : FlipDirection.Forward;
            UpdateLayers(direction);

            var src = (Canvas) sender;
            var transform = (TranslateTransform) src.RenderTransform;

            transform.X += e.DeltaManipulation.Translation.X;
        }

        protected override void UpdateLayers(FlipDirection direction)
        {
            if (direction == FlipDirection.Backward)
            {
                Canvas.SetZIndex(GetPrevPagePanel(), 5);
                Canvas.SetZIndex(GetNextPagePanel(), 0);
            }
            else if (direction == FlipDirection.Forward)
            {
                Canvas.SetZIndex(GetNextPagePanel(), 5);
                Canvas.SetZIndex(GetPrevPagePanel(), 0);
            }
        }

        public override void UpdatePanelsVisibility()
        {
            Canvas.SetZIndex(GetCurrentPagePanel(), 10);
            Canvas.SetZIndex(GetPrevPagePanel(), 0);
            Canvas.SetZIndex(GetNextPagePanel(), 5);

            ResetTranslations();

            base.UpdatePanelsVisibility();
        }

        protected override void ResetTranslations()
        {
            ApplyToEachPanel(p => ((TranslateTransform) p.RenderTransform).X = 0);
        }
    }
}