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

namespace FBReader.App.Interaction
{
    public static class ManipulationService
    {
        public static DependencyProperty ManipulationListenerProperty = DependencyProperty.RegisterAttached("ManipulationListener", typeof(ManipulationListener), typeof(ManipulationService), new PropertyMetadata((object)null, new PropertyChangedCallback(ManipulationService.OnManipulationListenerChanged)));

        static ManipulationService()
        {
        }

        public static ManipulationListener GetManipulationListener(UIElement element)
        {
            return (ManipulationListener)element.GetValue(ManipulationService.ManipulationListenerProperty);
        }

        public static void SetManipulationListener(UIElement element, ManipulationListener listener)
        {
            element.SetValue(ManipulationService.ManipulationListenerProperty, (object)listener);
        }

        private static void OnManipulationListenerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement e1 = d as UIElement;
            if (e1 == null)
                return;
            if (e.OldValue != null)
                ((ManipulationListener)e.OldValue).Detach(e1);
            if (e.NewValue == null)
                return;
            ((ManipulationListener)e.NewValue).Attach(e1);
        }
    }
}