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
using System.Diagnostics;
using System.Reflection;
using FBReader.Common.Analytics;
using FBReader.Common.ExtensionMethods;
using FlurryWP8SDK;
using Microsoft.Phone.Info;

namespace FBReader.AppServices.Services
{
    public class FlurryAnalytics : IAnalytics
    {
        private const string FlurryAPIKey = "S2WKQ94JBGFQZTJQYH5Z";

        public void StartSession()
        {
            Api.SetVersion(Assembly.GetExecutingAssembly().GetVersion());
            Api.SetUserId(GetUniqueId());
            Api.StartSession(FlurryAPIKey);
        }

        private string GetUniqueId()
        {
            object anid2;
            if (UserExtendedProperties.TryGetValue("ANID2", out anid2))
            {
                return anid2.ToString();
            }

            return "UNKNOWN USER";
        } 

        public void EndSession()
        {
            Api.EndSession();
        }

        public void LogError(Exception exception)
        {
            string logText = exception.Message;
            logText += "\n";
            logText += exception.StackTrace;

            logText = logText.Substring(0, Math.Min(250, logText.Length));

            Api.LogError(logText, exception);
            Debug.WriteLine("Flurry Log: error: {0}", logText);
        }
    }
}
