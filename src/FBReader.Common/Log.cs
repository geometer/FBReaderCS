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

namespace FBReader.Common
{
    public static class Log
    {
        private static ILogger _logger;

        public static void Init(ILogger logger)
        {
            _logger = logger;
        }

        public static void Write(string s)
        {
            string timeStamp = DateTime.Now.ToString("mm:ss.fff");

            string formattedString = string.Format("[{0}] - {1}", timeStamp, s);

            _logger.Write(formattedString);
        }

        public static void Write(string formatString, params object[] parameters)
        {
            var s = string.Format(formatString, parameters);
            
            Write(s);
        }
    }

    public interface ILogger
    {
        void Write(string s);
        void Write(string formatString, params object[] parameters);
    }

    public class DebugLogger : ILogger
    {
        public void Write(string s)
        {
            Debug.WriteLine(s); 
        }

        public void Write(string formatString, params object[] parameters)
        {
            Debug.WriteLine(formatString, parameters); 
        }
    }
}
