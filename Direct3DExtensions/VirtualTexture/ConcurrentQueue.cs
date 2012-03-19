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
	using System.Threading;

	// This concurrent queue is based on this paper: http://www.cs.rochester.edu/research/synchronization/pseudocode/queues.html#nbq
	public class ConcurrentQueue<T>
	{
		class Node
		{
			public T	value;
			public Node next;
		}

		Node head;
		Node tail;

		public bool IsEmpty
		{
			get { return head == tail; }
		}

		public ConcurrentQueue()
		{
			head = tail = new Node();			
		}

		public void Enqueue( T value )
		{
			Node node = new Node();
			node.value = value;

			Node oldtail;
			Node oldnext;

			while( true )
			{
				oldtail = tail;
				oldnext = tail.next;

				if( oldtail == tail )
				{
					if( oldnext == null )
					{
						if( Interlocked.CompareExchange<Node>( ref tail.next, node, null ) == null )
							break;
					}

					else
					{
						Interlocked.CompareExchange<Node>( ref tail, oldnext, oldtail );
					}
				}
			}

			Interlocked.CompareExchange<Node>( ref tail, node, oldtail );
		}

		public bool TryDequeue( out T value )
		{
			Node oldhead;
			Node oldnext;
			Node oldtail;

			while( true )
			{
				oldhead = head;
				oldtail = tail;
				oldnext = head.next;

				if( oldhead == head )
				{
					if( oldhead == oldtail )
					{
						//if( oldnext == null )
						{
							value = default(T);
							return false;
						}

						//Interlocked.CompareExchange<Node>( ref tail, oldnext, oldtail );
					}

					else
					{
						value = oldnext.value;
						if( Interlocked.CompareExchange<Node>( ref head, oldnext, oldhead ) == oldhead )
							break;

					}
				}
			}

			return true;
		}
	}
}