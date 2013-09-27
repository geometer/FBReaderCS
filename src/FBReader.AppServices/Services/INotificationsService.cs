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

namespace FBReader.AppServices.Services
{
    public interface INotificationsService
    {
        void ShowAlert(string caption, string text);
        bool ShowMessage(string caption, string text);
        Task<MessageResult> ShowMessage(string caption, string text, string checkButtonText);
        void ShowToast(string title, string message, Uri navigationUri, Uri icon = null);
    }

    public struct MessageResult
    {
        public bool Result { get; set; }
        public bool IsCheckBoxChecked { get; set; }
    }
}