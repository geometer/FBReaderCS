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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Coding4Fun.Toolkit.Controls;
using FBReader.AppServices.Services;
using Telerik.Windows.Controls;

namespace FBReader.App
{
    public class NotificationsService : INotificationsService
    {
        private readonly INavigationService _navigationService;

        public NotificationsService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void ShowAlert(string caption, string text)
        {
            Execute.OnUIThread(() => MessageBox.Show(text, caption, MessageBoxButton.OK));
        }

        public bool ShowMessage(string caption, string text)
        {
            return MessageBox.Show(text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        public void ShowToast(string title, string message, Uri navigationUri, Uri icon = null)
        {
            Execute.OnUIThread(() =>
                                   {

                                       var toast = new ToastPrompt
                                                       {
                                                           MinWidth = Common.Screen.Width,
                                                           Title = title,
                                                           Message = message,
                                                           ImageSource = icon != null ? new BitmapImage(icon) : null
                                                       };
                                       toast.Tap += delegate { _navigationService.Navigate(navigationUri); };
                                       toast.Show();
                                   });
        }

        public Task<MessageResult> ShowMessage(string caption, string text, string checkButtonText)
        {

            var taskCompletionSource = new TaskCompletionSource<MessageResult>();

            Action<MessageBoxClosedEventArgs> closeHandler = args =>
                                                                 {
                                                                     var result = new MessageResult();
                                                                     result.IsCheckBoxChecked = args.IsCheckBoxChecked;
                                                                     result.Result = args.Result == DialogResult.OK;
                                                                     taskCompletionSource.SetResult(result);
                                                                 };

            RadMessageBox.Show(
                caption, 
                MessageBoxButtons.OKCancel, 
                text, 
                checkButtonText, 
                vibrate: false,
                closedHandler: closeHandler);

            return taskCompletionSource.Task;
        }
    }
}