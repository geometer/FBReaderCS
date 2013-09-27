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
    using System.Windows.Interactivity;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;

    /// <summary>
    /// An <see cref="ApplicationBarIconButton"/> capable of triggering action messages.
    /// </summary>
    public class AppBarButton : ApplicationBarIconButton {
        /// <summary>
        /// The action message.
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// An <see cref="ApplicationBarMenuItem"/> capable of triggering action messages.
    /// </summary>
    public class AppBarMenuItem : ApplicationBarMenuItem {
        /// <summary>
        /// The action message.
        /// </summary>
        public string Message { get; set; }
    }

    class AppBarButtonTrigger : TriggerBase<PhoneApplicationPage> {
        public AppBarButtonTrigger(IApplicationBarMenuItem button) {
            button.Click += ButtonClicked;
        }

        void ButtonClicked(object sender, EventArgs e) {
            InvokeActions(e);
        }
    }

    class AppBarMenuItemTrigger : TriggerBase<PhoneApplicationPage> {
        public AppBarMenuItemTrigger(IApplicationBarMenuItem menuItem) {
            menuItem.Click += ButtonClicked;
        }

        void ButtonClicked(object sender, EventArgs e) {
            InvokeActions(e);
        }
    }
}