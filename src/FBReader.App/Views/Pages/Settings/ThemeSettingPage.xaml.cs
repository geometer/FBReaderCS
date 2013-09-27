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
using System.Windows.Controls;
using System.Windows.Data;
using FBReader.AppServices.DataModels;
using FBReader.Settings;
using Microsoft.Phone.Controls;
using System.Linq;
using FBReader.App.Interaction;

namespace FBReader.App.Views.Pages.Settings
{
    public partial class ThemeSettingPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty ColorSchemeProperty =
            DependencyProperty.Register("ColorScheme", typeof (ColorSchemes), typeof (ThemeSettingPage), new PropertyMetadata(default(ColorSchemes)));

        public ColorSchemes ColorScheme
        {
            get
            {
                return (ColorSchemes) GetValue(ColorSchemeProperty);
            }
            set
            {
                SetValue(ColorSchemeProperty, value);
            }
        }

        public ThemeSettingPage()
        {
            InitializeComponent();

            SetBinding(ColorSchemeProperty, new Binding("SelectedScheme")
                                                {
                                                    Mode = BindingMode.TwoWay, 
                                                });

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var radioButtons = ThemesPanel.Descendants<RadioButton>()
                .Single(rb => ((ColorSchemeDataModel) rb.DataContext).ColorScheme == ColorScheme);
            radioButtons.IsChecked = true;

        }

        private void RadioButtonOnChecked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton) sender;
            var content = (ColorSchemeDataModel)radioButton.DataContext;
            ColorScheme = content.ColorScheme;
        }

        private void RadioButtonClick(object sender, RoutedEventArgs e)
        {
            IsHitTestVisible = false;
            NavigationService.GoBack();
        }
    }
}