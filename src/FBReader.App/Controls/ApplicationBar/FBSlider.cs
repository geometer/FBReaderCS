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
using System.Windows;
using System.Windows.Controls;

namespace FBReader.App.Controls.ApplicationBar
{    
    public class FBSlider : Slider
    {
        public static readonly DependencyProperty IsMinimizedProperty =
            DependencyProperty.Register("IsMinimized", typeof (bool), typeof (FBSlider), new PropertyMetadata(default(bool), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (FBSlider) dependencyObject;
            var newState = (bool) dependencyPropertyChangedEventArgs.NewValue;
            var newStateName = newState ? "Minimized" : "FullSize";

            VisualStateManager.GoToState(@this, newStateName, true);
        }

        public bool IsMinimized
        {
            get { return (bool) GetValue(IsMinimizedProperty); }
            set { SetValue(IsMinimizedProperty, value); }
        }

        public event Action<int> PageSelected = delegate { }; 

        public FBSlider()
        {
            DefaultStyleKey = typeof (FBSlider);

            ManipulationCompleted += FBSlider_ManipulationCompleted;
        }

        public void FBSlider_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            PageSelected((int)Value);
        }

    }
}
