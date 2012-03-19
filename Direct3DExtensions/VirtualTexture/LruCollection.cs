/* 
MIT License

Copyright (c) 2009 Brad Blanchard (www.linedef.com)

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

namespace Direct3DExtensions.VirtualTexture
{	
	using System;
	using System.Collections.Generic;

	public class LruCollection<Key, Value>
	{
		int	capacity;
		LinkedList<KeyValuePair<Key,Value>>						list;
		Dictionary<Key,LinkedListNode<KeyValuePair<Key,Value>>>	entries;

		// Events
		public event Action<Key,Value>	Removed;

		// Properties
		public int Capacity
		{
			get { return capacity; }
			set 
			{ 
				capacity = value; 
				ShrinkToCapacity(); 
			}
		}

		protected virtual bool NeedsEviction
		{
			get { return Count > Capacity; }
		}

		public int Count 
		{ 
			get { return entries.Count; }
		}

		// Constructor
		public LruCollection( int capacity )
		{
			list = new LinkedList<KeyValuePair<Key,Value>>();
			entries = new Dictionary<Key,LinkedListNode<KeyValuePair<Key,Value>>>();

			Capacity = capacity;
			Clear();
		}

		public void Add( Key key, Value value )
		{
			if( ContainsKey(key) ) 
				throw new ArgumentException("Key already in dictionary");

			list.AddFirst( new KeyValuePair<Key,Value>( key, value ) );
			entries.Add( key, list.First );

			ShrinkToCapacity();
		}

		public bool ContainsKey( Key key ) 
		{ 
			return entries.ContainsKey(key); 
		}

		public void Remove( Key key )
		{
			LinkedListNode<KeyValuePair<Key,Value>> entry = null;
			if( entries.TryGetValue( key, out entry ) )
				Remove( entry );
		}

		public void Clear()
		{
			if( Removed != null )
				foreach( LinkedListNode<KeyValuePair<Key,Value>> entry in entries.Values )
					Removed( entry.Value.Key, entry.Value.Value );

			list.Clear();
			entries.Clear();
		}

		public bool TryGetValue( Key key, bool update, out Value value )
		{
			LinkedListNode<KeyValuePair<Key,Value>> node;
			if( entries.TryGetValue( key, out node ) )
			{
				if( update ) MoveToTop( node );
				value = node.Value.Value;
				return true;
			}

			value = default(Value);
			return false;
		}
		
		public Value RemoveLast()
		{
			Value key = list.Last.Value.Value;
			Remove( list.Last );
			return key;
		}

		// Private Functions
		void Remove( LinkedListNode<KeyValuePair<Key,Value>> entry )
		{
			list.Remove( entry );
			entries.Remove( entry.Value.Key );

			if( Removed != null )
				Removed( entry.Value.Key, entry.Value.Value );
		}

		void ShrinkToCapacity()
		{
			while( NeedsEviction ) 
				Remove( list.Last );
		}

		void MoveToTop( LinkedListNode<KeyValuePair<Key,Value>> entry )
		{
			list.Remove( entry );
			list.AddFirst( entry );
		}
	}
}