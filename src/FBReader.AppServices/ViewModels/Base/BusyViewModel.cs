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

using Caliburn.Micro;

namespace FBReader.AppServices.ViewModels.Base
{
    public class BusyViewModel : Screen
    {
        private bool _isBusy;
        private int _busyCounter;

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
                NotifyOfPropertyChange(() => IsNotBusy);
            }
        }

        public bool IsNotBusy
        {
            get
            {
                return !IsBusy;
            }
        }

        protected virtual void StartBusiness()
        {
            IsBusy = true;
            ++_busyCounter;
        }

        protected virtual void StopBusiness()
        {
            if (_busyCounter <= 0)
            {
                return;
            }
            
            --_busyCounter;
            if (_busyCounter == 0)
            {
                IsBusy = false;
            }
        }
    }
}