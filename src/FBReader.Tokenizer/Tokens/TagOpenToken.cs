/*
 * Author: Vitaly Leschenko, CactusSoft (http://cactussoft.biz/), 2013
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

using System.IO;
using System.Xml.Linq;
using FBReader.Tokenizer.Styling;
using HtmlAgilityPack;

namespace FBReader.Tokenizer.Tokens
{
    public class TagOpenToken : TokenBase
    {
        public TagOpenToken(int id, XElement element, TextVisualProperties properties, int parentID)
            : base(id)
        {
            Name = element.Name.LocalName;
            TextProperties = properties;
            ParentID = parentID;
        }

        public TagOpenToken(int id, HtmlNode node, TextVisualProperties properties, int parentID)
            : base(id)
        {
            Name = node.Name;
            TextProperties = properties;
            ParentID = parentID;
        }

        private TagOpenToken(int id, string name, TextVisualProperties properties, int parentId)
            : base(id)
        {
            Name = name;
            TextProperties = properties;
            ParentID = parentId;
        }

        public string Name { get; private set; }
        public int ParentID { get; private set; }
        public TextVisualProperties TextProperties { get; private set; }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(ParentID);
            TextProperties.Save(writer);
        }

        public static TokenBase Load(BinaryReader reader, int id)
        {
            var name = reader.ReadString();
            int parentID = reader.ReadInt32();
            TextVisualProperties properties = TextVisualProperties.Load(reader);

            return new TagOpenToken(id, name, properties, parentID);
        }
    }
}