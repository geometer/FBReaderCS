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
using System.Linq;
using System.Windows.Navigation;
using FBReader.AppServices.ViewModels.Pages;

namespace FBReader.App.Views.Pages
{
    public partial class WebBrowserPage
    {
        private WebBrowserPageViewModel ViewModel
        {
            get
            {
                return (WebBrowserPageViewModel) DataContext;
            }
        }

        public WebBrowserPage()
        {
            InitializeComponent();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (ViewModel.BrowserHistoryUrls.Count <= 1)
            {
                return;
            }

            var uri = ViewModel.BrowserHistoryUrls.Last();
            ViewModel.BrowserHistoryUrls.Remove(uri);

            if (ViewModel.BrowserHistoryUrls.Count < 1)
            {
                return;
            }

            BrowserControl.Navigate(new Uri(ViewModel.BrowserHistoryUrls[ViewModel.BrowserHistoryUrls.Count - 1]));
            e.Cancel = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward)
            {
                ViewModel.BrowserHistoryUrls = null;
                BrowserControl.Source = null;
                BrowserControl = null;
                DataContext = null;
            }
            
            base.OnNavigatedFrom(e);
        }

        private void BrowserControlOnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri != null && ViewModel.BrowserHistoryUrls.All(uri => uri != e.Uri.ToString()))
            {
                ViewModel.BrowserHistoryUrls.Add(e.Uri.ToString());
            }
        }
    }
}