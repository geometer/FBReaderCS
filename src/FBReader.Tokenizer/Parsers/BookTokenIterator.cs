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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using FBReader.Common.Exceptions;
using FBReader.Tokenizer.Styling;
using FBReader.Tokenizer.Tokens;

namespace FBReader.Tokenizer.Parsers
{
    public class BookTokenIterator : IEnumerator<TokenBase>
    {
        private BinaryReader _reader;
        private readonly List<int> _refs;
        private IsolatedStorageFile _storage;
        private Stream _tokens;
        private readonly string _tokensPath;
        private int _position;
        private bool _isTokenStreamDisposed;
        private bool _isReaderDisposed;

        public BookTokenIterator(string tokensPath, List<int> refs)
        {
            _refs = refs;
            _position = 0;
            _tokensPath = tokensPath;
            Init();

            Current = null;
        }

        public int Count
        {
            get
            {
                return _refs.Count;
            }
        }

        public TokenBase Current
        {
            get;
            private set;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            if (!_isTokenStreamDisposed)
            {
                _tokens.Dispose();
                _isTokenStreamDisposed = true;
            }

            if (!_isReaderDisposed)
            {
                _reader.Dispose();
                _isReaderDisposed = true;
            }
        }

        public bool MoveNext()
        {
            if (_position >= _refs.Count)
            {
                return false;
            }

            try
            {
                Current = TokenSerializer.Load(_reader, _position++);
            }
            //handle exceptions in case FAS
            catch (ObjectDisposedException dipsposedExp)
            {
                //Reset();
                throw new TokenIteratorUnableMoveNextException("Unable move token iterator", dipsposedExp);
            }
            catch (NullReferenceException nullRedExp)
            {
                //Reset();
                throw new TokenIteratorUnableMoveNextException("Unable move token iterator", nullRedExp);
            }

            return true;
        }

        public void Reset()
        {
            _position = 0;
            Current = null;
            _tokens.Seek(0L, SeekOrigin.Begin);
        }

        public void MoveTo(int id)
        {
            if (id < 0 || id >= _refs.Count)
            {
                throw new ArgumentOutOfRangeException("id", string.Concat("MoveTo(", id, "); Max ID = ", _refs.Count - 1));
            }

            _tokens.Seek(_refs[_position = id], SeekOrigin.Begin);
        }

        public Stack<TagOpenToken> BuildTree(int tokenID)
        {
            int id = tokenID--;
            var list = new List<TagOpenToken>();

            while (tokenID >= 0)
            {
                MoveTo(tokenID);
                if (MoveNext())
                {
                    var closeTagToken = Current as TagCloseToken;
                    if (closeTagToken != null)
                    {
                        tokenID = closeTagToken.ParentID;
                    }
                    else
                    {
                        var openTagToken = Current as TagOpenToken;
                        if (openTagToken != null)
                        {
                            list.Insert(0, openTagToken);
                            tokenID = openTagToken.ParentID;
                        }
                        else
                            --tokenID;
                    }
                }
                else
                    --tokenID;
            }

            var stack = new Stack<TagOpenToken>();
            stack.Push(new TagOpenToken(-1, new XElement("root"), new TextVisualProperties(), -1));
            
            foreach (TagOpenToken openTagToken in list)
            {
                stack.Push(openTagToken);
            }

            if (id < 0)
            {
                Reset();
            }
            else
            {
                MoveTo(id);
            }

            return stack;
        }

        private void Init()
        {
            _storage = IsolatedStorageFile.GetUserStoreForApplication();
            _tokens = new IsolatedStorageFileStream(_tokensPath, FileMode.Open, FileAccess.Read, FileShare.Read, _storage);
            _reader = new BinaryReader(_tokens);
        }
    }
}