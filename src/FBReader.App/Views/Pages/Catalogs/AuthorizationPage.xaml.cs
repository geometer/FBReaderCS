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
using System.Windows.Input;
using FBReader.AppServices.ViewModels.Pages.Catalogs;
using FBReader.Localization;
using Microsoft.Phone.Shell;

namespace FBReader.App.Views.Pages.Catalogs
{
    public partial class AuthorizationPage
    {
        public AuthorizationPage()
        {
            InitializeComponent();
            ((ApplicationBarIconButton) ApplicationBar.Buttons[0]).Text = UIStrings.AppBar_LoginBtn_Text;
        }

        public AuthorizationPageViewModel ViewModel
        {
            get { return (AuthorizationPageViewModel)DataContext; }
        }

        private void UserNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Focus();
                ViewModel.Login();
            }
        }

        private void Login(object sender, EventArgs e)
        {
            ViewModel.Login();
        }
    }
}