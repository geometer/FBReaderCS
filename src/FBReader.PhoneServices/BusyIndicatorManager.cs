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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace FBReader.PhoneServices
{
    public class BusyIndicatorManager : DependencyObject, IBusyIndicatorManager
    {
        public static readonly DependencyProperty BusyIndicatorProperty =
            DependencyProperty.RegisterAttached("BusyIndicator", typeof (IBusyIndicatorManager),
                                                typeof (BusyIndicatorManager), new PropertyMetadata(null));


        private static ProgressIndicator _progressIndicator;
        private readonly PhoneApplicationPage _page;
        private int _isBusyCounter;

        public static IBusyIndicatorManager Create(PhoneApplicationPage page)
        {
            var busyIndicator = page.GetValue(BusyIndicatorProperty) as IBusyIndicatorManager;
            if (busyIndicator == null)
            {
                busyIndicator = new BusyIndicatorManager(page);
                page.SetValue(BusyIndicatorProperty, busyIndicator);
            }
            
            return busyIndicator;
        }

        private BusyIndicatorManager(PhoneApplicationPage page)
        {
            _page = page;

            if (_progressIndicator == null)
            {
                _progressIndicator = new ProgressIndicator();
            }

            _page.SetValue(SystemTray.ProgressIndicatorProperty, _progressIndicator);
        }

        public bool IsBusy
        {
            get
            {
                return _isBusyCounter > 0;
            }
        }

        public void Start()
        {
            _isBusyCounter++;
            UpdateIndicatorVisibility();
        }

        public void Stop()
        {
            if (_isBusyCounter > 0)
            {
                _isBusyCounter--;
            }

            UpdateIndicatorVisibility();
        }

        private void UpdateIndicatorVisibility()
        {
            var isRunning = _isBusyCounter > 0;

            Dispatcher.BeginInvoke(
                () =>
                    {
                        _progressIndicator.IsVisible = _progressIndicator.IsIndeterminate = isRunning;
                    });
        }
    }
}
