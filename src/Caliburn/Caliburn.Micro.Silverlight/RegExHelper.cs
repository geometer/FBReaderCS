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

namespace Caliburn.Micro {
    using System;

    /// <summary>
    ///  Helper class for encoding strings to regular expression patterns
    /// </summary>
    public static class RegExHelper {
        /// <summary>
        /// Regular expression pattern for valid name
        /// </summary>
        public const string NameRegEx = @"[A-Za-z_]\w*";

        /// <summary>
        /// Regular expression pattern for subnamespace (including dot)
        /// </summary>
        public const string SubNamespaceRegEx = NameRegEx + @"\.";

        /// <summary>
        /// Regular expression pattern for namespace or namespace fragment
        /// </summary>
        public const string NamespaceRegEx = "(" + SubNamespaceRegEx + ")*";

        /// <summary>
        /// Creates a named capture group with the specified regular expression 
        /// </summary>
        /// <param name="groupName">Name of capture group to create</param>
        /// <param name="regEx">Regular expression pattern to capture</param>
        /// <returns>Regular expression capture group with the specified group name</returns>
        public static string GetCaptureGroup(string groupName, string regEx) {
            return String.Concat(@"(?<", groupName, ">", regEx, ")");
        }

        /// <summary>
        /// Converts a namespace (including wildcards) to a regular expression string
        /// </summary>
        /// <param name="srcNamespace">Source namespace to convert to regular expression</param>
        /// <returns>Namespace converted to a regular expression</returns>
        public static string NamespaceToRegEx(string srcNamespace) {
            //Need to escape the "." as it's a special character in regular expression syntax
            var nsencoded = srcNamespace.Replace(".", @"\.");

            //Replace "*" wildcard with regular expression syntax
            nsencoded = nsencoded.Replace(@"*\.", NamespaceRegEx);
            return nsencoded;
        }

        /// <summary>
        /// Creates a capture group for a valid name regular expression pattern
        /// </summary>
        /// <param name="groupName">Name of capture group to create</param>
        /// <returns>Regular expression capture group with the specified group name</returns>
        public static string GetNameCaptureGroup(string groupName) {
            return GetCaptureGroup(groupName, NameRegEx);
        }

        /// <summary>
        /// Creates a capture group for a namespace regular expression pattern
        /// </summary>
        /// <param name="groupName">Name of capture group to create</param>
        /// <returns>Regular expression capture group with the specified group name</returns>
        public static string GetNamespaceCaptureGroup(string groupName) {
            return GetCaptureGroup(groupName, NamespaceRegEx);
        }
    }
}