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
using System.Security.Cryptography;
using System.Text;

namespace FBReader.AppServices.Services
{
    public static class EncryptService
    {
        public static string Encrypt(string str)
        {
            var encryptedString = ProtectedData.Protect(Encoding.UTF8.GetBytes(str), null);
            return Convert.ToBase64String(encryptedString);
        }

        public static string Decrypt(string str)
        {
            var decryptedBytes = ProtectedData.Unprotect(Convert.FromBase64String(str), null);
            return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
        }
    }
}