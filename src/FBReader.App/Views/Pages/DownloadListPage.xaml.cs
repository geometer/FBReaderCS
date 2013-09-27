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
using FBReader.AppServices.ViewModels.Pages;
using FBReader.Render.Downloading.Model;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace FBReader.App.Views.Pages
{
    public partial class DownloadListPage
    {
        private RadDataBoundListBoxItem _focusedItem;

        private DownloadListPageViewModel ViewModel
        {
            get
            {
                return (DownloadListPageViewModel) DataContext;
            }
        }

        public DownloadListPage()
        {
            InitializeComponent();
        }

        private void RadDataBoundListBox_OnSelectionChanging(object sender, SelectionChangingEventArgs e)
        {
            e.Cancel = true;
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

            var item = (DownloadItemDataModel)_focusedItem.DataContext;
            RestartMenuItem.Visibility = item.Status == DownloadStatus.Error ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnDeleteTap(object sender, GestureEventArgs e)
        {
            var item = (DownloadItemDataModel) _focusedItem.DataContext;
            item.Cancel();
            ViewModel.Remove(item);
            ViewModel.DownloadsContainer.Remove(item);
        }

        private void OnRestartTap(object sender, GestureEventArgs e)
        {
            var item = (DownloadItemDataModel)_focusedItem.DataContext;
            ViewModel.Restart(item);
        }
    }
}