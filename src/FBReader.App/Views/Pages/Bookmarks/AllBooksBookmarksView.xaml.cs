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

using System.Windows.Controls;
using FBReader.AppServices.ViewModels.Pages.Bookmarks;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace FBReader.App.Views.Pages.Bookmarks
{
    public partial class AllBooksBookmarksView : UserControl
    {

        private RadDataBoundListBoxItem _focusedItem;

        private AllBooksBookmarksViewModel ViewModel
        {
            get
            {
                return (AllBooksBookmarksViewModel)DataContext;
            }
        }

        public AllBooksBookmarksView()
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
            }
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.RemoveBookmark(_focusedItem.DataContext);
        }
    }
}
