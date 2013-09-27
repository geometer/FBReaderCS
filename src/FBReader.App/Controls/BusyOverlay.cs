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
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;

namespace FBReader.App.Controls
{
    public class BusyOverlay : IDisposable
    {
        private readonly bool _closable;
        private readonly Color _overlayBackgroundColor = Color.FromArgb(128, 0, 0, 0);

        private readonly Border _border;
        private readonly PhoneApplicationPage _page;
        private readonly RadWindow _popup;

        private bool _canClose;
        private static BusyOverlay _overlay;

        private static int _counter;
        private bool _appBarVisibility;
        private bool _hideAppBar;
        private IApplicationBar _appBar;

        public event Action Closed;
        public event Action Closing;

        public BusyOverlay(bool closable, string content = null, bool hideAppBar = true)
        {
            _closable = closable;
            _page = (PhoneApplicationPage)((PhoneApplicationFrame)Application.Current.RootVisual).Content;
            _hideAppBar = hideAppBar;

            _popup = new RadWindow()
            {
                IsAnimationEnabled = false,
                IsClosedOnOutsideTap = false,
                Content = _border = new Border()
                {
                    Opacity = 0.5,
                    Width = _page.ActualWidth,
                    Height = _page.ActualHeight,
                    Background = new SolidColorBrush(_overlayBackgroundColor),
                    Child = new RadBusyIndicator()
                    {
                        IsRunning = true,
                        AnimationStyle = AnimationStyle.AnimationStyle9,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Content = content
                    }
                }
            };

        }

        public static async Task<IDisposable> Create(string content = null, bool closable = false, bool hideAppBar = true)
        {
            if (_counter == 0)
            {
                _overlay = new BusyOverlay(closable, content, hideAppBar);

                _overlay.Show();
            }

            _overlay.UpdateSize();

            _counter++;

            await Task.Delay(10);

            return _overlay;
        }

        public void Dispose()
        {
            _counter--;
            _counter = _counter >= 0 ? _counter : 0;
            if (_counter == 0)
            {
                Hide();

                _page.OrientationChanged -= PageOrientationChanged;
                _popup.WindowClosing -= PopupClosing;
            }
        }

        public void Show()
        {
            _page.OrientationChanged += PageOrientationChanged;
            _popup.WindowClosing += PopupClosing;

            if (_page.ApplicationBar != null && _hideAppBar)
            {
                _appBarVisibility = _page.ApplicationBar.IsVisible;
                _page.ApplicationBar.IsVisible = false;
                _appBar = _page.ApplicationBar;
            }

            _page.Content.IsHitTestVisible = false;

            _canClose = false;
            _popup.IsOpen = true;

            var storyboard = new Storyboard();
            var opacityAnimation = new DoubleAnimation()
            {
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };

            Storyboard.SetTarget(opacityAnimation, (Border)_popup.Content);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin();

            _popup.UpdateLayout();
        }

        private void Hide()
        {
            _canClose = true;

            var storyboard = new Storyboard();
            var opacityAnimation = new DoubleAnimation()
            {
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(150))
            };

            Storyboard.SetTarget(opacityAnimation, (Border)_popup.Content);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            storyboard.Children.Add(opacityAnimation);

            storyboard.Completed += delegate
            {
                _popup.IsOpen = false;
                _popup.WindowClosing -= PopupClosing;
                
                OnClosed();
            };
            storyboard.Begin();

            if (_appBar != null && _hideAppBar)
                _appBar.IsVisible = _appBarVisibility;
            _page.Content.IsHitTestVisible = true;

        }

        private void PageOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            UpdateSize();
        }

        private void UpdateSize()
        {
            _border.Width = _page.ActualWidth;
            _border.Height = _page.ActualHeight;
        }

        private void PopupClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !_canClose;
            if (_closable && e.Cancel)
            {
                OnClosing();
            }
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