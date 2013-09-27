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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FBReader.AppServices.Services;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.Localization;
using FBReader.Settings;
using Microsoft.Phone.Tasks;

namespace FBReader.AppServices.DataModels
{
    public class SharingDataModel : NotifyPropertyChangedBase
    {
        private readonly AppSettings _appSettings;
        private readonly INotificationsService _notificationsService;
        private readonly ISkyDriveService _skyDriveService;
        private string _publicUri;
        private CancellationTokenSource _cancellationTokenSource;
        private BookModel _book;
        private string _bookTitle;
        private string _text;

        public SharingDataModel(
            AppSettings appSettings,
            INotificationsService notificationsService,
            ISkyDriveService skyDriveService)
        {
            _appSettings = appSettings;
            _notificationsService = notificationsService;
            _skyDriveService = skyDriveService;
            ShareMethods = new List<ShareMethodDataModel>
                               {
                                   new ShareMethodDataModel {Method = Method.Email, Name = UIStrings.Sharing_Email},
                                   new ShareMethodDataModel {Method = Method.SocialNetworks, Name = UIStrings.Sharing_Social}
                               };
        }

        public List<ShareMethodDataModel> ShareMethods { get; set; }
        public bool ShowShareMethodSelector { get; set; }

        public async Task<bool> ShowMessage()
        {
            if (!_appSettings.DontShowUploadToSkyDriveMessage)
            {
                var result = await _notificationsService.ShowMessage(UINotifications.General_AttentionCaption,
                                                                     UINotifications.ShareBook_ShareMessage,
                                                                     UINotifications.ShareBook_DontShowAgain);
                
                _appSettings.DontShowUploadToSkyDriveMessage = result.IsCheckBoxChecked && result.Result;
                return result.Result;
            }
            return true;
        }

        public async Task UploadBook(BookModel book)
        {
            _book = book;
            
            try
            {
                if(_cancellationTokenSource != null)
                    _cancellationTokenSource.Cancel();

                _cancellationTokenSource = new CancellationTokenSource();
                _publicUri = await _skyDriveService.UploadAsync(_book, _cancellationTokenSource.Token);
                if (string.IsNullOrEmpty(_publicUri))
                    return;

                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                EventWaitHandle waitHandle = new AutoResetEvent(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                              {
                                                                  ShowShareMethodSelector = 
                                                                      true && 
                                                                      !_cancellationTokenSource.IsCancellationRequested;
                                                                  waitHandle.Set();
                                                              });
                await Task.Factory.StartNew(() => waitHandle.WaitOne());
            }
            catch (FileUploadException)
            {
                _notificationsService.ShowAlert(UINotifications.General_ErrorCaption,
                                                UINotifications.ShareBook_UploadError);
            }
            catch (OperationCanceledException)
            {

            }
        }

        public void ShareText(string bookTitle, string text)
        {
            _bookTitle = string.Format(UIStrings.Sharing_QuoteFormat, bookTitle);
            _text = text ?? string.Empty;
            ShowShareMethodSelector = true;
        }

        public void Share(ShareMethodDataModel dataModel)
        {
            string uri = _publicUri;
            _publicUri = null;

            switch (dataModel.Method)
            {
                case Method.Email:
                    if (_book != null)
                        SendEmail(_book, uri);
                    else
                        SendEmail(_bookTitle, _text);

                    break;
                case Method.SocialNetworks:
                    if(_book != null)
                        ShareInSocialNetworks(_book, uri);
                    else
                        ShareInSocialNetworks(_bookTitle, _text);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void SendEmail(string bookTitle, string text)
        {
            try
            {
                var email = new EmailComposeTask();
                email.Subject = bookTitle;
                email.Body = text;

                email.Show();
            }
            catch (Exception)
            {
            }
        }

        private void ShareInSocialNetworks(string bookTitle, string text)
        {
            try
            {
                var shareLink = new ShareLinkTask();

                shareLink.Title = bookTitle;
                shareLink.Message = text;
                shareLink.LinkUri = new Uri("http://fbreader.org/");
                shareLink.Show();
            }
            catch (Exception)
            {
            }
        }

        private void SendEmail(BookModel book, string url)
        {

            try
            {
                var email = new EmailComposeTask();
                email.Subject = book.Author + " " + book.Title;
                email.Body = url;

                email.Show();
            }
            catch (Exception)
            {
            }
        }

        private void ShareInSocialNetworks(BookModel book, string url)
        {
            try
            {
                var shareLink = new ShareLinkTask();
                shareLink.LinkUri = new Uri(url);
                shareLink.Title = book.Author + " " + book.Title;
                shareLink.Show();
            }
            catch (Exception)
            {
            }
        }

        public void Cancel()
        {
            if(_cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();
        }
    }
}
