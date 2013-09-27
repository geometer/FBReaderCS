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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace FBReader.App.Controls.ApplicationBar
{
    public partial class FBReaderApplicationBar : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SliderForegroundBrushProperty = DependencyProperty.Register("SliderForegroundBrush", typeof (Brush), typeof (FBReaderApplicationBar), new PropertyMetadata(default(Brush)));
        public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register("MenuItems", typeof(FBReaderApplicationBarItemCollection<FBReaderApplicationBarMenuItem>), typeof(FBReaderApplicationBar), new PropertyMetadata(null));
        public static readonly DependencyProperty IconButtonsProperty = DependencyProperty.Register("IconButtons", typeof(FBReaderApplicationBarItemCollection<FBReaderApplicationBarIconButton>), typeof(FBReaderApplicationBar), new PropertyMetadata(null));       
        public static readonly DependencyProperty PageNumberProperty = DependencyProperty.Register("PageNumber", typeof (int), typeof (FBReaderApplicationBar), new PropertyMetadata(1));        
        public static readonly DependencyProperty TotalPagesProperty = DependencyProperty.Register("TotalPages", typeof (int), typeof (FBReaderApplicationBar), new PropertyMetadata(1));
        public static readonly DependencyProperty PageOrientationProperty = DependencyProperty.Register("PageOrientation", typeof (PageOrientation), typeof (FBReaderApplicationBar), new PropertyMetadata(default(PageOrientation), PropertyOrientationChanged));
        public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.Register("CurrentState", typeof (ApplicationBarState), typeof (FBReaderApplicationBar), new PropertyMetadata(ApplicationBarState.Minimized, PropertyStateChanged));
        public static readonly DependencyProperty MinimizedStateViewProperty = DependencyProperty.Register("ApplicationBarMinimizedStateView", typeof(ApplicationBarMinimizedState), typeof(FBReaderApplicationBar), new PropertyMetadata(ApplicationBarMinimizedState.Minimized, PropertyMinimizedStateViewChanged));

        private readonly Dictionary<ApplicationBarState, int> _heights = new Dictionary<ApplicationBarState, int>
                                                                    {
                                                                        {ApplicationBarState.Full, 430},
                                                                        {ApplicationBarState.Minimized, 24},
                                                                        {ApplicationBarState.Normal, 150}
                                                                    };

        public event Action Opened = delegate { };
        public event Action Closed = delegate { };
        public event Action<int> PageSelected = delegate { }; 
        public event Action PageSelectionCancelled = delegate { }; 
        public event Action PageSelectionApplied = delegate { }; 

    
        public FBReaderApplicationBar()
        {
            InitializeComponent();
            Tap += OnTap;
            Slider.PageSelected += SliderOnPageSelected;

            MenuItems = new FBReaderApplicationBarItemCollection<FBReaderApplicationBarMenuItem>(this);
            IconButtons = new FBReaderApplicationBarItemCollection<FBReaderApplicationBarIconButton>(this);
        }

        private void SliderOnPageSelected(int page)
        {
            PageSelected(page);

            // Change appbar buttons
            AnimateIconButtons(true);
        }

        private void AnimateIconButtons(bool flag)
        {
            var storyboard = new Storyboard();
            var translateAnimation = new DoubleAnimation()
                       {
                           To = flag ? -800 : 0,
                           Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                           EasingFunction = new SineEase(){ EasingMode = flag ? EasingMode.EaseIn : EasingMode.EaseOut}
                       };

            Storyboard.SetTarget(translateAnimation, IconButtonsGrid);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            storyboard.Children.Add(translateAnimation);
            storyboard.Begin();
        }

        public Brush SliderForegroundBrush
        {
            get { return (Brush)GetValue(SliderForegroundBrushProperty); }
            set { SetValue(SliderForegroundBrushProperty, value); }
        }

        public FBReaderApplicationBarItemCollection<FBReaderApplicationBarIconButton> IconButtons
        {
            get { return (FBReaderApplicationBarItemCollection<FBReaderApplicationBarIconButton>)GetValue(IconButtonsProperty); }
            set { SetValue(IconButtonsProperty, value); }
        }

        public FBReaderApplicationBarItemCollection<FBReaderApplicationBarMenuItem> MenuItems
        {
            get { return (FBReaderApplicationBarItemCollection<FBReaderApplicationBarMenuItem>)GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }
       
        public int PageNumber
        {
            get { return (int)GetValue(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public int TotalPages
        {
            get { return (int)GetValue(TotalPagesProperty); }
            set { SetValue(TotalPagesProperty, value); }
        }

        public ApplicationBarState CurrentState
        {
            get { return (ApplicationBarState)GetValue(CurrentStateProperty); }
            set { SetValue(CurrentStateProperty, value); }
        }

        public ApplicationBarMinimizedState ApplicationBarMinimizedStateView
        {
            get { return (ApplicationBarMinimizedState)GetValue(MinimizedStateViewProperty); }
            set { SetValue(MinimizedStateViewProperty, value); }
        }

        public PageOrientation PageOrientation
        {
            get { return (PageOrientation)GetValue(PageOrientationProperty); }
            set { SetValue(PageOrientationProperty, value); }
        }

        private TranslateTransform TranslateTransform
        {
            get { return (TranslateTransform)RootBorder.RenderTransform; }
        }

        public double HeightInMinimizedState
        {
            get
            {
                return ApplicationBarMinimizedStateView == ApplicationBarMinimizedState.Hidden
                           ? 0
                           : _heights[ApplicationBarState.Minimized];
            }
        }

        public bool IsOpen
        {
            get { return CurrentState != ApplicationBarState.Minimized; }
        }

        public void Open()
        {
            if (CurrentState == ApplicationBarState.Minimized)
            {
                CurrentState = ApplicationBarState.Normal;
            }
        }

        public void Close()
        {
            CurrentState = ApplicationBarState.Minimized;
        }

        private static void PropertyOrientationChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (FBReaderApplicationBar)dependencyObject;
            @this.ApplyOrientation();
        }

        private static void PropertyStateChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (FBReaderApplicationBar)dependencyObject;
            @this.UpdateVisualState();
            @this.OnPropertyChanged("IsOpen");
            
            if (((ApplicationBarState) dependencyPropertyChangedEventArgs.OldValue) == ApplicationBarState.Minimized)
                @this.Opened();
            else if (((ApplicationBarState)dependencyPropertyChangedEventArgs.NewValue) == ApplicationBarState.Minimized)
                @this.Closed();
        }

        private static void PropertyMinimizedStateViewChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var @this = (FBReaderApplicationBar)dependencyObject;
            @this.UpdateVisualState();
        }
        
        private void OnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            gestureEventArgs.Handled = true;
        }

        private void ApplyOrientation()
        {
            // Minimize on orientation change - this is done intentionally.
            CurrentState = ApplicationBarState.Minimized;

            switch (PageOrientation)
            {
                case PageOrientation.PortraitUp:
                case PageOrientation.Portrait:
                    _heights[ApplicationBarState.Full] = 430;
                    this.Width = Application.Current.Host.Content.ActualWidth;
                    this.Height = _heights[ApplicationBarState.Full];

                    Canvas.SetTop(this, Application.Current.Host.Content.ActualHeight);

                    break;
                default:
                    _heights[ApplicationBarState.Full] = 330;
                    this.Width = Application.Current.Host.Content.ActualHeight;
                    this.Height = _heights[ApplicationBarState.Full];

                    Canvas.SetTop(this, Application.Current.Host.Content.ActualWidth);

                    break;
            }
        }

        private void UpdateVisualState(bool fromZero = false)
        {
            double yTranslation = _heights[CurrentState];
            if (CurrentState == ApplicationBarState.Minimized && ApplicationBarMinimizedStateView == ApplicationBarMinimizedState.Hidden)
                yTranslation = 0;

            var sb = new Storyboard();
            var da = new DoubleAnimation()
                         {
                             To = -yTranslation,
                             Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                             EasingFunction = new SineEase()
                         };
            if (fromZero)
                da.From = 0;

            MenuScroll.ScrollToVerticalOffset(0);

            Storyboard.SetTarget(da, TranslateTransform);
            Storyboard.SetTargetProperty(da, new PropertyPath("Y"));
            sb.Children.Add(da);
            sb.Begin();

            Slider.IsMinimized = CurrentState == ApplicationBarState.Minimized;
            Slider.IsEnabled = CurrentState != ApplicationBarState.Minimized;
        }

        private void OnManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            var control = (FrameworkElement) sender;
            control.CaptureMouse();

            e.Handled = true;
        }

        private void OnManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            var maxSize = _heights[ApplicationBarState.Full];
            var minSize = (ApplicationBarMinimizedStateView == ApplicationBarMinimizedState.Minimized
                              ? _heights[ApplicationBarState.Minimized]
                              : 0);

            var desiredTransform = TranslateTransform.Y + e.DeltaManipulation.Translation.Y;
            var normalizedTransform = -Math.Max(minSize, Math.Min(maxSize, -desiredTransform));
            TranslateTransform.Y = normalizedTransform;

            e.Handled = true;
        }

        private void OnManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            var control = (FrameworkElement)sender;
            control.ReleaseMouseCapture();

            if (e.IsTapGesture())
            {
                switch (CurrentState)
                {
                    case ApplicationBarState.Minimized:
                        CurrentState = ApplicationBarState.Normal;
                        break;
                    case ApplicationBarState.Normal:
                        CurrentState = ApplicationBarState.Full;
                        break;
                    case ApplicationBarState.Full:
                        CurrentState = ApplicationBarState.Minimized;
                        break;
                    default:
                        throw new NotSupportedException();

                }
            }
            else
            {
                GoToNearestState(TranslateTransform.Y + e.FinalVelocities.LinearVelocity.Y / 20);
            }

            e.Handled = true;
        }

        private void GoToNearestState(double velocityAppliedTransform)
        {
            var nearestState = _heights.Select(kv => new { Diff = Math.Abs(velocityAppliedTransform + kv.Value), State = kv.Key })
                                       .OrderBy(a => a.Diff)
                                       .ToArray()
                                       .First().State;

            CurrentState = nearestState;
            UpdateVisualState();
        }

        private void AppBarItemClick(object sender, EventArgs e)
        {
            Close();

            var src = (FrameworkElement) sender;
            var data = (FBReaderApplicationBarItemBase) src.DataContext;

            data.DoClick();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ApplyPageSelectionClick(object sender, EventArgs e)
        {
            PageSelectionApplied();

            AnimateIconButtons(false);
        }

        private void CancelPageSelectionClick(object sender, EventArgs e)
        {            
            PageSelectionCancelled();

            AnimateIconButtons(false);            
        }

        public void CancelPageSelectionMode()
        {
            AnimateIconButtons(false);
        }
    }
}
