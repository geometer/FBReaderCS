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
using FBReader.Localization;
using Microsoft.Phone.Tasks;

namespace FBReader.AppServices.ViewModels.Pages.Settings
{
    public class SettingsPivotViewModel : Conductor<Screen>.Collection.OneActive
    {
        public SettingsPivotViewModel(
            GeneralViewModel generalViewModel, 
            FormattingViewModel formattingViewModel, 
            AboutViewModel aboutViewModel)
        {
            Items.Add(generalViewModel);
            Items.Add(formattingViewModel);
            Items.Add(aboutViewModel);
        }

        public int SelectedIndex { get; set; }

        public void SendEmail()
        {
            var emailComposer = new EmailComposeTask();
            emailComposer.To = string.Format(UINotifications.DiagnosticsEmail_To_Format, "geometer@fbreader.org");
            emailComposer.Subject = "";
            try
            {
                emailComposer.Show();
            }
            catch
            {
                // Protection from double tap
            }
        }

        public void RateApp()
        {
            var task = new MarketplaceReviewTask();
            try
            {
                task.Show();
            }
            catch
            {
                // Protection from double tap
            }
        }
    }
}
