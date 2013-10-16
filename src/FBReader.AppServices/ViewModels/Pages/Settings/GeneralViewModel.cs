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
using System.Globalization;
using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.AppServices.DataModels;
using FBReader.AppServices.Events;
using FBReader.Localization;
using FBReader.Settings;

namespace FBReader.AppServices.ViewModels.Pages.Settings
{
    public class GeneralViewModel : SettingsItemBase
    {
        private readonly INavigationService _navigationService;
        private readonly SettingsController _settingsController;
        private readonly ILocalizationManager _localizationManager;
        private readonly IEventAggregator _eventAggregator;

        public GeneralViewModel(
            INavigationService navigationService, 
            SettingsController settingsController, 
            ILocalizationManager localizationManager,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _navigationService = navigationService;
            _settingsController = settingsController;
            _localizationManager = localizationManager;
            _eventAggregator = eventAggregator;

            DisplayName = UISettings.SettingsPage_General;

            InitializeItems();
        }

        public List<CultureInfo> UILanguages
        {
            get { return AppSettings.Default.UILanguages; }
        }

        public CultureInfo SelectedLanguage
        {
            get
            {
                return AppSettings.Default.CurrentUILanguage;
            }
            set 
            {
                AppSettings.Default.CurrentUILanguage = value;
                ChangeUILanguage(value);
            }
        }


        public bool SelectUILanguage { get; set; }

        public List<CultureInfo> TranslateLanguages
        {
            get { return AppSettings.Default.TranslateLanguages; }
        }

        public CultureInfo SelectedTranslateLanguage
        {
            get
            {
                return AppSettings.Default.CurrentTranslateLanguage;
            }
            set
            {
                AppSettings.Default.CurrentTranslateLanguage = value;
                Update();
            }
        }

        public bool SelectTranslateLanguage { get; set; }

        protected override void InitializeItems()
        {
            Items.Add(new SettingItemDataModel
                          {
                              GetSettingName = () => UISettings.SettingsPage_LockScreen,
                              GetSettingValue = _settingsController.GetLockScreenValue,
                              OnTapAction = () => _navigationService.UriFor<LockScreenSettingPageViewModel>().Navigate()
                          });
            Items.Add(new SettingItemDataModel
                          {
                              GetSettingName = () => UISettings.SettingsPage_ScreenOrientation,
                              GetSettingValue = _settingsController.GetOrientationValue,
                              OnTapAction = () => _navigationService.UriFor<OrientationSettingPageViewModel>().Navigate()

                          });
            Items.Add(new SettingItemDataModel
                      {
                          GetSettingName = () => UISettings.SettingsPage_ScreenBrightness,
                          GetSettingValue = _settingsController.GetBrightnessValue,
                          OnTapAction = () => _navigationService.UriFor<ScreenBrightnessPageViewModel>().Navigate()
                      });

            Items.Add(new SettingItemDataModel
                          {
                              GetSettingName = () => UISettings.SettingsPage_Menu,
                              GetSettingValue = _settingsController.GetMenuValue,
                              OnTapAction = () => _navigationService.UriFor<MenuSettingPageViewModel>().Navigate()
                          });
            Items.Add(new SettingItemDataModel
                          {
                              GetSettingName = () => UISettings.SettingsPage_PageFlipping,
                              GetSettingValue = _settingsController.GetFlippingValue,
                              OnTapAction = () => _navigationService.UriFor<FlippingSettingPageViewModel>().Navigate()
                          });
            Items.Add(new SettingItemDataModel
                          {
                              GetSettingName = () => UISettings.SettingsPage_UILanguage,
                              GetSettingValue = _settingsController.GetUILanguageValue,
                              OnTapAction = () => SelectUILanguage = true
                          });
            Items.Add(new SettingItemDataModel
                          {
                              GetSettingName = () => UISettings.SettingsPage_TranslateLanguage,
                              GetSettingValue = _settingsController.GetTranslateLanguageValue,
                              OnTapAction = () => SelectTranslateLanguage = true
                          });
        }

        private void ChangeUILanguage(CultureInfo cultureInfo)
        {
            _localizationManager.Reset(cultureInfo);
            _eventAggregator.Publish(new UILanguageChanged(cultureInfo));
        }

        protected override void Update()
        {
            base.Update();
            DisplayName = UISettings.SettingsPage_General;
        }
    }
}
