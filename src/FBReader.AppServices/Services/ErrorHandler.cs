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
using System.Net;
using Caliburn.Micro;
using FBReader.Localization;
using FBReader.WebClient.Exceptions;
using Microsoft.Xna.Framework.GamerServices;

namespace FBReader.AppServices.Services
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly INotificationsService _notificationsService;

        public ErrorHandler(INotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }

        public void Handle(Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                return;
            }

            if (exception is OpdsFormatException)
            {
                ShowMessage(UINotifications.General_OpdsFormatErrorCaption, UINotifications.General_OpdsFormatErrorMessage);
            }
            else if (exception is AggregateException)
            {
                Handle(((AggregateException)exception).Flatten().InnerException);
            }
            else if (exception is NetworkNotAvailableException)
            {
                ShowMessage(UINotifications.General_AttentionCaption, UINotifications.General_NoNetworkMessage);
            }
            else if (exception is WebException)
            {
                ShowMessage(UINotifications.General_ErrorCaption, UINotifications.General_ServerCommunicationErrorMessage);
            }
            else
            {
                ShowMessage(UINotifications.General_ErrorCaption, UINotifications.General_CriticalErrorMessage);
            }
        }

        private void ShowMessage(string caption, string text)
        {
            Execute.OnUIThread(() => _notificationsService.ShowAlert(caption, text));
        }
    }
}