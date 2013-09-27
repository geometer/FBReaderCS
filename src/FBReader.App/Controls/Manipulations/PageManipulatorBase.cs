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
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using FBReader.Settings;

namespace FBReader.App.Controls.Manipulations
{
    public abstract class PageManipulatorBase : IDisposable
    {
        protected readonly ThreePagePanel _bookView;
        protected readonly FlippingMode _mode;
        private int _offset;

        protected PageManipulatorBase(ThreePagePanel bookView, FlippingMode mode)
        {
            _bookView = bookView;
            _mode = mode;
        }

        public bool IsLastPage { get; set; }
        public bool IsFirstPage { get; set; }

        public IEnumerable<Panel> Panels
        {
            get
            {
                yield return GetPrevPagePanel();
                yield return GetCurrentPagePanel();
                yield return GetNextPagePanel();
            }
        }

        public virtual void Dispose()
        {
            TurnForward = null;
            TurnBackward = null;
        }

        public event Action TurnForward;
        public event Action TurnBackward;

        public abstract void Pause();
        public abstract void Resume();


        public Panel GetCurrentPagePanel()
        {
            int offset = (1 + _offset)%3;

            return _bookView.GetChildPanel(offset);
        }

        public Panel GetNextPagePanel()
        {
            int offset = (2 + _offset)%3;

            return _bookView.GetChildPanel(offset);
        }

        public Panel GetPrevPagePanel()
        {
            int offset = (3 + _offset)%3;

            return _bookView.GetChildPanel(offset);
        }

        public void SwapPrevWithCurrent()
        {
            _offset = ((_offset - 1) + 3)%3;
        }


        public void SwapNextWithCurrent()
        {
            _offset = (_offset + 1)%3;
        }

        protected virtual void TurnPage(bool isForward)
        {
            if (isForward)
                OnTurnForward();
            else
                OnTurnBackward();
        }

        public abstract void UpdatePanelsVisibility();

        protected void ApplyToEachPanel(Action<Panel> action)
        {
            foreach (Panel p in Panels)
                action(p);
        }

        protected virtual void OnTurnBackward()
        {
            Action handler = TurnBackward;
            if (handler != null)
            {
                handler();
            }
        }

        protected virtual void OnTurnForward()
        {
            Action handler = TurnForward;
            if (handler != null)
            {
                handler();
            }
        }

        public abstract void CancelNextEvent();
    }
}