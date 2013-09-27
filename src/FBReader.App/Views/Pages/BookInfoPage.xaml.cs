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
using System.Windows.Data;
using FBReader.AppServices.ViewModels.Pages;
using FBReader.Localization;
using Microsoft.Phone.Shell;

namespace FBReader.App.Views.Pages
{
    public partial class BookInfoPage
    {
        private BookInfoPageViewModel ViewModel 
        {
            get
            {
                return (BookInfoPageViewModel) DataContext;
            }
        }

        public BookInfoPage()
        {
            InitializeComponent();

            Loaded += (sender, e) =>
                {
                    ViewModel.PropertyChanged += ViewModelPropertyChanged;
                    if (!ViewModel.IsBusy)
                    {
                        ValidateAppBarIndexPropertyForAppBar();
                    }
                };
            Unloaded += (sender, e) => ViewModel.PropertyChanged -= ViewModelPropertyChanged;
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsBusy"))
            {
                ValidateIsBusyPropertyForAppBar();
            }
            else if (e.PropertyName.Equals("ApplicationBarIndex") && !ViewModel.IsBusy)
            {
                ValidateAppBarIndexPropertyForAppBar();
            }
            else if (e.PropertyName.Equals("IsFavouriteBook") && !ViewModel.IsBusy)
            {
                ValidateIsFavouritePropertyForAppBar();
            }
            else if (e.PropertyName == "DataContext")
            {
                BookCoverGrid.SetBinding(DataContextProperty,
                                           new Binding
                                               {
                                                   Source = ViewModel,
                                                   Converter = (IValueConverter)Application.Current.Resources["BookInfoPageImageToSourceConverter"]
                                               });
            }
        }

        private void ValidateIsBusyPropertyForAppBar()
        {
            if (ViewModel.IsBusy)
            {
                SetButtonsEnabled(false);
            }
            else
            {
                ValidateAppBarIndexPropertyForAppBar();
            }
        }

        private void SetButtonsEnabled(bool isEnabled)
        {
            foreach (IApplicationBarIconButton button in ApplicationBar.Buttons)
            {
                button.IsEnabled = isEnabled;
            }
        }

        private void ValidateAppBarIndexPropertyForAppBar()
        {
            switch (ViewModel.ApplicationBarIndex)
            {
                case 0:
                    ApplicationBar = Resources["FirstAppBar"] as IApplicationBar;
                    SetButtonText(0, UIStrings.AppBar_ReadBtn_Text);
                    SetButtonText(1, UIStrings.EditBookPage_AppBar_MakeNewTitle);
                    SetButtonText(2, ViewModel.IsFavouriteBook ? UIStrings.ReadPage_AppBar_FromFavorites : UIStrings.ReadPage_AppBar_ToFavorites);
                    SetButtonIcon(2, ViewModel.IsFavouriteBook ? "/Resources/Icons/appbar_favorites_del.png" : "/Resources/Icons/appbar_favorites_add.png");
                    SetButtonText(3, UIStrings.AppBar_ShareBtn_Text);
                    break;

                case 1:
                    ApplicationBar = (Resources["SecondAppBar"] as IApplicationBar);
                    if (ViewModel.IsBookFree)
                    {
                        SetButtonText(0, UIStrings.AppBar_DownloadBtn_Text);
                        SetButtonIcon(0, "/Resources/Icons/download.png");
                        ((IApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= Download;
                        ((IApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= Buy;

                        ((IApplicationBarIconButton) ApplicationBar.Buttons[0]).Click += Download;
                    }
                    else
                    {
                        SetButtonText(0, UIStrings.AppBar_BuyBtn_Text);
                        SetButtonIcon(0, "/Resources/Icons/buy.png");
                        ((IApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= Download;
                        ((IApplicationBarIconButton)ApplicationBar.Buttons[0]).Click -= Buy;

                        ((IApplicationBarIconButton)ApplicationBar.Buttons[0]).Click += Buy;
                    }
                    break;

                default:
                    ApplicationBar = Resources["ThirdAppBar"] as IApplicationBar;
                    SetButtonText(0, UIStrings.AppBar_BuyBtn_Text);
                    SetButtonText(1, UIStrings.AppBar_ReadTrialBtn_Text);
                    SetButtonText(2, UIStrings.AppBar_DeleteBtn_Text);
                    break;
            }

            SetButtonsEnabled(true);
        }

        private void ValidateIsFavouritePropertyForAppBar()
        {
            if (ViewModel.ApplicationBarIndex != 0)
            {
                return;
            }

            SetButtonIcon(2, ViewModel.IsFavouriteBook ? "/Resources/Icons/appbar_favorites_del.png" : "/Resources/Icons/appbar_favorites_add.png");
            SetButtonText(2, ViewModel.IsFavouriteBook ? UIStrings.ReadPage_AppBar_FromFavorites : UIStrings.ReadPage_AppBar_ToFavorites);
        }

        private void SetButtonText(int i, string text)
        {
            ((IApplicationBarIconButton) ApplicationBar.Buttons[i]).Text = text;
        }

        private void SetButtonIcon(int i, string uri)
        {
            ((IApplicationBarIconButton)ApplicationBar.Buttons[i]).IconUri = new Uri(uri, UriKind.RelativeOrAbsolute);
        }

        private void Download(object sender, EventArgs e)
        {
            ViewModel.DownloadBook(true);
        }

        private void Edit(object sender, EventArgs e)
        {
            ViewModel.Edit();
        }

        private void AddRemoveToFavourites(object sender, EventArgs e)
        {
            ViewModel.AddRemoveToFavourites();
        }

        private void Buy(object sender, EventArgs e)
        {
            ViewModel.Buy();
        }

        private void Share(object sender, EventArgs e)
        {
            ViewModel.ShareAsync();
        }

        private void Read(object sender, EventArgs e)
        {
            ViewModel.Read();
        }

        private void DownloadTrial(object sender, EventArgs e)
        {
            ViewModel.DownloadBook(false);
        }

        private void DeleteTrial(object sender, EventArgs e)
        {
            ViewModel.DeleteTrial();
        }
    }
}