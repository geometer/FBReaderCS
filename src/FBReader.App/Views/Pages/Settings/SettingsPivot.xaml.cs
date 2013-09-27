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
using System.Linq;
using FBReader.AppServices.ViewModels.Pages.Settings;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
using FBReader.App.Interaction;


namespace FBReader.App.Views.Pages.Settings
{
    public partial class SettingsPivot
    {
        public SettingsPivot()
        {
            InitializeComponent();
        }

        private void OnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            var pivotHeaderControl = fe.Ancestors<PivotHeadersControl>().FirstOrDefault();
            if (pivotHeaderControl != null)
            {
                pivotHeaderControl.InvalidateMeasure();
            }
        }

        private void PivotItemsSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            AppBar.IsVisible = ((Pivot)sender).SelectedItem is AboutViewModel;
        }

        private void Rate(object sender, EventArgs e)
        {
            ((SettingsPivotViewModel) DataContext).RateApp();
        }

        private void SendEmail(object sender, EventArgs e)
        {
            ((SettingsPivotViewModel)DataContext).SendEmail();
        }
    }
}