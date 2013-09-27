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
using System.Windows;
using FBReader.AppServices.ViewModels.Pages;
using FBReader.DataModel.Model;
using FBReader.Localization;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace FBReader.App.Views.Pages
{
    public partial class UserLibraryPage
    {
        private RadDataBoundListBoxItem _focusedItem;

        private UserLibraryPageViewModel ViewModel
        {
            get { return (UserLibraryPageViewModel) DataContext; }
        }

        public UserLibraryPage()
        {
            InitializeComponent();

            ((ApplicationBarIconButton) ApplicationBar.Buttons[0]).Text = UIStrings.AppBar_SearchBtn_Text;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[1]).Text = UIStrings.AppBar_SortBtn_Text;

            Loaded += (sender, e) =>
                {
                    var viewModel = (UserLibraryPageViewModel) DataContext;
                    ApplicationBar.IsVisible = !viewModel.ShowOnlyFavourites;
                    viewModel.PropertyChanged += (s, args) =>
                        {
                            if (args.PropertyName.Equals("IsOpenSortModes"))
                            {
                                ApplicationBar.IsVisible = !((UserLibraryPageViewModel) DataContext).IsOpenSortModes;
                            }
                        };
                };
        }

        private void SortButtonClick(object sender, EventArgs e)
        {
            ApplicationBar.IsVisible = false;
            ((UserLibraryPageViewModel) DataContext).IsOpenSortModes = true;
        }

        private void RadContextMenu_OnOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            _focusedItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (_focusedItem == null)
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
                return;
            }
            bool onlyFavourited = ViewModel.ShowOnlyFavourites;
            RemoveFromFavouritesMenuItem.Visibility = onlyFavourited ? Visibility.Visible : Visibility.Collapsed;
            DeleteMenuItem.Visibility = onlyFavourited ? Visibility.Collapsed : Visibility.Visible;
            PinMenuItem.Visibility = onlyFavourited ? Visibility.Collapsed : Visibility.Visible;

            bool canPinToStart = ViewModel.CanPinToStart((BookModel) _focusedItem.DataContext);
            PinMenuItem.IsEnabled = canPinToStart;
        }

        private void DeleteOnTap(object sender, GestureEventArgs e)
        {
            ViewModel.RemoveBook((BookModel)_focusedItem.DataContext);
        }

        private void RemoveFromBookmarksOnTap(object sender, GestureEventArgs e)
        {
            ViewModel.RemoveFromFavourites((BookModel) _focusedItem.DataContext);
        }

        private void PinOnTap(object sender, GestureEventArgs e)
        {
            ViewModel.PinToStart((BookModel) _focusedItem.DataContext);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (SortModePopup.IsOpen)
            {
                ViewModel.IsOpenSortModes = false;
                ApplicationBar.IsVisible = true;
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        private void SearchButtonClick(object sender, EventArgs e)
        {
            ViewModel.NavigateToSearch();
        }

        
    }
}