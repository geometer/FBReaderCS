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

using System.Globalization;
using System.Threading;
using Caliburn.Micro;

namespace FBReader.Localization
{
    public class LocalizationManager : PropertyChangedBase, ILocalizationManager
    {
        public UIStrings UI { get; private set; }
        public UISettings Settings { get; private set; }
        public UINotifications Notifications { get; private set; }

        public LocalizationManager()
        {
            UI = new UIStrings();
            Settings = new UISettings();
            Notifications = new UINotifications();
        }

        public void Reset(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            UIStrings.Culture = cultureInfo;
            UISettings.Culture = cultureInfo;
            UINotifications.Culture = cultureInfo;

            NotifyOfPropertyChange(() => UI);  
            NotifyOfPropertyChange(() => Settings);
            NotifyOfPropertyChange(() => Notifications);
        }
    }
}