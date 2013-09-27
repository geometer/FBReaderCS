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
using FBReader.Localization;
using FBReader.Settings;

namespace FBReader.AppServices.ViewModels.Pages.Settings
{
    public class FlippingSettingPageViewModel : Screen
    {
        private readonly SettingsController _settingsController;
        private FlippingItemDataModel<FlippingMode> _selectedFlippingMode;
        private FlippingItemDataModel<FlippingStyle> _selectedFlippingStyle;
        private readonly List<FlippingItemDataModel<FlippingStyle>> _flippingStyles = new List<FlippingItemDataModel<FlippingStyle>>
                                                                                 {
                                                                                     new FlippingItemDataModel<FlippingStyle>(FlippingStyle.None, UISettings.SettingsPage_FlippingStyle_None),
                                                                                     new FlippingItemDataModel<FlippingStyle>(FlippingStyle.Shift, UISettings.SettingsPage_FlippingStyle_Shift),
                                                                                     new FlippingItemDataModel<FlippingStyle>(FlippingStyle.Overlap, UISettings.SettingsPage_FlippingStyle_Overlap),
                                                                                 };

        private readonly List<FlippingItemDataModel<FlippingMode>> _flippingModes = new List<FlippingItemDataModel<FlippingMode>>
                                                                               {
                                                                                   new FlippingItemDataModel<FlippingMode>(FlippingMode.Touch, UISettings.SettingsPage_FlippingMode_Touch),
                                                                                   new FlippingItemDataModel<FlippingMode>(FlippingMode.Slide, UISettings.SettingsPage_FlippingMode_Slide),
                                                                                   new FlippingItemDataModel<FlippingMode>(FlippingMode.TouchOrSlide, UISettings.SettingsPage_FlippingMode_TouchOrSlide),
                                                                               };


        public FlippingSettingPageViewModel(SettingsController settingsController)
        {
            _settingsController = settingsController;
        }


        public List<FlippingItemDataModel<FlippingStyle>> FlippingStyles
        {
            get
            {
                return _flippingStyles;
            }
        }

        public List<FlippingItemDataModel<FlippingMode>> FlippingModes
        {
            get
            {
                return _flippingModes;
            }
        }

        public FlippingItemDataModel<FlippingMode> SelectedFlippingMode
        {
            get
            {
                return _selectedFlippingMode;
            }
            set
            {
                _selectedFlippingMode = value;
                AppSettings.Default.FlippingMode = _selectedFlippingMode.Data;
            }
        }

        public FlippingItemDataModel<FlippingStyle> SelectedFlippingStyle
        {
            get
            {
                return _selectedFlippingStyle;
            }
            set
            {
                _selectedFlippingStyle = value;
                AppSettings.Default.FlippingStyle = _selectedFlippingStyle.Data;
            }
        }

        protected override void OnInitialize()
        {
            SelectedFlippingMode = new FlippingItemDataModel<FlippingMode>(AppSettings.Default.FlippingMode, _settingsController.GetFlippingModeName(AppSettings.Default.FlippingMode));
            SelectedFlippingStyle = new FlippingItemDataModel<FlippingStyle>(AppSettings.Default.FlippingStyle, _settingsController.GetFlippingStyleName(AppSettings.Default.FlippingStyle));
        }


    }
}
