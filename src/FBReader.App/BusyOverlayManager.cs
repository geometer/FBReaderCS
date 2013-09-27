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
using System.Threading.Tasks;
using FBReader.App.Controls;
using FBReader.Common;

namespace FBReader.App
{
    public class BusyOverlayManager : IBusyOverlayManager
    {
        private BusyOverlay _busyOverlay;
        private bool _hideAppBar;
        private int _counter;

        public event Action Closed;
        public event Action Closing;

        public bool Closable { get; set; }
        public string Content { get; set; }
        
        
        public async Task<IBusyOverlayManager> Start(bool hideAppBar = true)
        {
            if (_counter > 0)
                return this;
            _counter++;

            _hideAppBar = hideAppBar;

            _busyOverlay = (BusyOverlay)await BusyOverlay.Create(Content, Closable);
            _busyOverlay.Closed += OnClosed;
            _busyOverlay.Closing += OnClosing;
            return this;
        }

        public void Stop()
        {
            if (_counter == 0)
                return;

            _counter--;
            _counter = _counter >= 0 ? _counter : 0;
            
            if (_busyOverlay == null)
                return;
            
            _busyOverlay.Closed -= OnClosed;
            _busyOverlay.Closing -= OnClosing;

            _busyOverlay.Dispose();
        }

        public void Dispose()
        {
            Stop();
        }

        protected virtual void OnClosed()
        {
            Action handler = Closed;
            if (handler != null) handler();
        }

        protected virtual void OnClosing()
        {
            Action handler = Closing;
            if (handler != null) handler();
        }

    }
}
