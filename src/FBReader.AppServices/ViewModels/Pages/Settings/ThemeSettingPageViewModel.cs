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

using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.AppServices.DataModels;
using FBReader.Settings;

namespace FBReader.AppServices.ViewModels.Pages.Settings
{
    public class ThemeSettingPageViewModel : Screen
    {
        private readonly SettingsController _settingsController;
        private readonly INavigationService _navigationService;

        public ThemeSettingPageViewModel(SettingsController settingsController, INavigationService navigationService)
        {
            _settingsController = settingsController;
            _navigationService = navigationService;
        }

        public ColorSchemes SelectedScheme
        {
            get
            {
                return AppSettings.Default.ColorSchemeKey;
            }
            set
            {
                AppSettings.Default.ColorSchemeKey = value;
            }
        }

        public List<ColorSchemeDataModel> Schemes
        {
            get
            {
                return AppSettings.Default.Schemes.Select(s => new ColorSchemeDataModel(s, _settingsController)).ToList();
            }
        }
    }
}
