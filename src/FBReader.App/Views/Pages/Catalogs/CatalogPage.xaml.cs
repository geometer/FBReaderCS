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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using FBReader.App.Interaction;
using FBReader.AppServices.ViewModels.Pages.Catalogs;
using FBReader.DataModel.Model;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;

namespace FBReader.App.Views.Pages.Catalogs
{
    public partial class CatalogPage
    {
        private const string NAVIGATION_OFFSETS_KEY = "NavigationOffsets";
        private const string NAVIGATION_CONTEXS_KEY = "NavigationContexs";
        private readonly Stack<CatalogItemModel> _navigationDataContexts = new Stack<CatalogItemModel>();
        private readonly Stack<double> _navigationVerticalOffsets = new Stack<double>();
        private ScrollViewer _scrollViewer;

        public CatalogPageViewModel ViewModel
        {
            get { return (CatalogPageViewModel) DataContext; }
        }

        public CatalogPage()
        {
            InitializeComponent();

            Loaded += (sender, e) =>
                {
                    ViewModel.CatalogNavigated += ViewModelOnCatalogNavigated;
                    ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
                    
                    var isSearchEnabled = ViewModel.IsSearchEnabled;
                    AppBar.Buttons[0].IsVisible = isSearchEnabled;
                    AppBar.MenuItems[0].IsVisible = ViewModel.CanRefresh;
                    AppBar.Mode = isSearchEnabled ? ApplicationBarMode.Default : ApplicationBarMode.Minimized;
                    ItemsControl.Margin = isSearchEnabled ? new Thickness(24, 48, 24, 72) : new Thickness(24, 48, 24, 24);

                   

                    _scrollViewer = ItemsControl.Descendants<ScrollViewer>().SingleOrDefault();

                };

            Unloaded += (sender, e) =>
                {
                    ViewModel.CatalogNavigated -= ViewModelOnCatalogNavigated;
                    ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;

                };
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if(propertyChangedEventArgs.PropertyName != "IsAuthorized")
                return;

            switch (propertyChangedEventArgs.PropertyName)
            {
                case "IsAuthorized":
                    AppBar.MenuItems[1].IsVisible = ViewModel.IsAuthorized;
                    break;
                case "IsBusy":
                    AppBar.MenuItems[0].IsEnabled = !ViewModel.IsBusy;
                    break;
            }
        }

        private void ViewModelOnCatalogNavigated(object sender, bool forwardNavigation)
        {
            if (forwardNavigation)
                return;
            
            ItemsControl.Visibility = Visibility.Collapsed;
            _scrollViewer.ScrollToVerticalOffset(0);
            if (ItemsControl.ItemsSource == null || _navigationDataContexts.Count == 0)
            {
                ItemsControl.Visibility = Visibility.Visible;
                return;
            }

            var dataContext = _navigationDataContexts.Pop();
            // a little bit magic
            Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        ItemsControl.BringIntoView(dataContext);
                        _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset - _navigationVerticalOffsets.Pop());
                    }
                    catch (Exception)
                    {
                    }
                    ItemsControl.Visibility = Visibility.Visible;
                });
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            var viewModel = DataContext as CatalogPageViewModel;
            if (viewModel == null)
            {
                base.OnBackKeyPress(e);
                return;
            }
            
            if (viewModel.CanGoToPreviousLevel)
            {
                e.Cancel = true;
                viewModel.GoBack();
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!State.ContainsKey(NAVIGATION_OFFSETS_KEY) || !State.ContainsKey(NAVIGATION_CONTEXS_KEY))
            {
                return;
            }

            var navigationOffsets = State[NAVIGATION_OFFSETS_KEY] as List<double> ?? new List<double>();
            var navigationContexs = State[NAVIGATION_CONTEXS_KEY] as List<CatalogItemModel> ?? new List<CatalogItemModel>();

            if (navigationOffsets.Count != navigationContexs.Count)
            {
                return;
            }
            int stackCount = navigationContexs.Count;

            _navigationDataContexts.Clear();
            _navigationVerticalOffsets.Clear();
            for (var i = stackCount - 1; i >= 0; --i)
            {
                _navigationDataContexts.Push(navigationContexs[i]);
                _navigationVerticalOffsets.Push(navigationOffsets[i]);
            }
            State.Remove(NAVIGATION_OFFSETS_KEY);
            State.Remove(NAVIGATION_CONTEXS_KEY);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                State[NAVIGATION_OFFSETS_KEY] = _navigationVerticalOffsets.ToList();
                State[NAVIGATION_CONTEXS_KEY] = _navigationDataContexts.ToList();
            }
        }

        private void NavigateToSearch(object sender, EventArgs e)
        {
            ViewModel.NavigateToSearch();
        }

        private void ItemsControl_OnItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            if (!(e.Item.DataContext is CatalogBookItemModel))
            {
                var firstViewportElement = ItemsControl.ViewportItems.First();
                double itemVerticalOffset;
                CatalogItemModel item;
                if (_scrollViewer.VerticalOffset < ItemsControl.ActualHeight * 2)
                {
                    itemVerticalOffset = -_scrollViewer.VerticalOffset;
                    item = ViewModel.FolderItems.First();
                }
                else
                {
                    var transform = firstViewportElement.TransformToVisual(ItemsControl);
                    Point absolutePosition = transform.Transform(new Point(0, 0));
                    itemVerticalOffset = absolutePosition.Y;
                    item = (CatalogItemModel) firstViewportElement.DataContext;
                }

                _navigationDataContexts.Push(item);
                _navigationVerticalOffsets.Push(itemVerticalOffset);
            }

            ViewModel.NavigateToItem((CatalogItemModel)e.Item.DataContext);
        }

        private void ItemsControl_OnSelectionChanging(object sender, SelectionChangingEventArgs e)
        {
            e.Cancel = true;
        }

        private void AppBarLogoutMenuItemClick(object sender, EventArgs e)
        {
            State.Remove(NAVIGATION_OFFSETS_KEY);
            State.Remove(NAVIGATION_CONTEXS_KEY);
            ViewModel.Logout();
        }

        private void AppBarRefreshMenuItemClick(object sender, EventArgs e)
        {
            ViewModel.Update();
        }
    }
}