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
using System.Windows.Input;
using System.Windows.Media.Animation;
using FBReader.App.Interaction;

namespace FBReader.App.Controls
{
    public partial class SelectionActionsControl
    {

        private readonly Storyboard _showAnimation = new Storyboard
                                                {
                                                    Children =
                                                        {
                                                            new DoubleAnimation
                                                                {
                                                                    To = 1,
                                                                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                                                                    EasingFunction = new CubicEase()
                                                                }
                                                        }
                                                };

        public event Action Copy = delegate { }; 
        public event Action Share = delegate { }; 
        public event Action Translate = delegate { }; 
        public event Action Bookmark = delegate { }; 

        public SelectionActionsControl()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;
            Opacity = 0;

            Storyboard.SetTarget(_showAnimation, this);
            Storyboard.SetTargetProperty(_showAnimation, new PropertyPath("Opacity"));

        }

        public void Show()
        {
            Visibility = Visibility.Visible;    
            _showAnimation.Begin();
        }

        public void Hide()
        {
            _showAnimation.Stop();
            Opacity = 0;
            Visibility = Visibility.Collapsed;
        }

        private void CopyButtonOnTap(object sender, GestureEventArgs e)
        {
            Copy();
            e.Handled = true;

        }

        private void ShareButtonOnTap(object sender, GestureEventArgs e)
        {
            Share();
            e.Handled = true;

        }

        private void TranslateButtonOnTap(object sender, GestureEventArgs e)
        {
            Translate();
            e.Handled = true;
        }

        private void BookmarkButtonOnTap(object sender, GestureEventArgs e)
        {
            Bookmark();
            e.Handled = true;

        }

        private void ActionManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
        }


        private void ActionButtonOnHold(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }
    }
}
