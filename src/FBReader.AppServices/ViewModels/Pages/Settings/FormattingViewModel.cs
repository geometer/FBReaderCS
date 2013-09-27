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

using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.AppServices.DataModels;
using FBReader.Localization;

namespace FBReader.AppServices.ViewModels.Pages.Settings
{
    public class FormattingViewModel : SettingsItemBase
    {
        private readonly INavigationService _navigationService;
        private readonly SettingsController _settingsController;

        public FormattingViewModel(
            INavigationService navigationService, 
            SettingsController settingsController,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _navigationService = navigationService;
            _settingsController = settingsController;
            DisplayName = UISettings.SettingPage_Pivot_Formatting;
            
            InitializeItems();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            DisplayName = UISettings.SettingPage_Pivot_Formatting;
        }

        protected override void InitializeItems()
        {
            Items.Add(new SettingItemDataModel
            {
                GetSettingName = () => UISettings.SettingsPage_CSS,
                GetSettingValue = _settingsController.GetCSSValue,
                OnTapAction = () => _navigationService.UriFor<CSSSettingPageViewModel>().Navigate()
            });
            Items.Add(new SettingItemDataModel
            {
                GetSettingName = () => UISettings.SettingsPage_Font,
                GetSettingValue = _settingsController.GetFontValue,
                OnTapAction = () => _navigationService.UriFor<FontSettingPageViewModel>().Navigate()
            });
            Items.Add(new SettingItemDataModel
            {
                GetSettingName = () => UISettings.SettingsPage_Hyphenation,
                GetSettingValue = _settingsController.GetHyphenationValue,
                OnTapAction = () => _navigationService.UriFor<HyphenationSettingPageViewModel>().Navigate()
            });
            Items.Add(new SettingItemDataModel
            {
                GetSettingName = () => UISettings.SettingsPage_ColorScheme,
                GetSettingValue = _settingsController.GetColorSchemeValue,
                OnTapAction = () => _navigationService.UriFor<ThemeSettingPageViewModel>().Navigate()
            });
            Items.Add(new SettingItemDataModel
            {
                GetSettingName = () => UISettings.SettingsPage_Margins,
                GetSettingValue = _settingsController.GetMarginValue,
                OnTapAction = () => _navigationService.UriFor<MarginsSettingPageViewModel>().Navigate()
            });
        }

        protected override void Update()
        {
            base.Update();
            DisplayName = UISettings.SettingPage_Pivot_Formatting;
        }
    }
}
