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
using System.IO;

namespace FBReader.Tokenizer.Tokens
{
    public static class TokenSerializer
    {
        private static readonly Dictionary<Type, TokenType> Types = new Dictionary<Type, TokenType>
            {
                {typeof (TagOpenToken), TokenType.TagOpenToken},
                {typeof (TagCloseToken), TokenType.TagCloseToken},
                {typeof (TextToken), TokenType.TextToken},
                {typeof (WhitespaceToken), TokenType.WhitespaceToken},
                {typeof (NewPageToken), TokenType.NewPageToken},
                {typeof (PictureToken), TokenType.PictureToken}
            };

        public static TokenBase Load(BinaryReader reader, int id)
        {
            var type = (TokenType) reader.ReadByte();
            switch (type)
            {
                case TokenType.TagOpenToken:
                    return TagOpenToken.Load(reader, id);
                case TokenType.TagCloseToken:
                    return TagCloseToken.Load(reader, id);
                case TokenType.TextToken:
                    return TextToken.Load(reader, id);
                case TokenType.WhitespaceToken:
                    return new WhitespaceToken(id);
                case TokenType.NewPageToken:
                    return new NewPageToken(id);
                case TokenType.PictureToken:
                    return PictureToken.Load(reader, id);
                default:
                    return null;
            }
        }

        public static void Save(BinaryWriter writer, TokenBase token)
        {
            TokenType type;
            if(!Types.TryGetValue(token.GetType(), out type))
                throw new NotSupportedException();

            writer.Write((byte)type);
            token.Save(writer);
        }
    }
}