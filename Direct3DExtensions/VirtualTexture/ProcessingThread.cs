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
	//using System.Threading.Collections;

	public class ProcessingThread<T>: IDisposable
	{
		Action<T>		action;
		Action<T>		complete;

		Thread			thread;
		Semaphore		semaphore;

		ConcurrentQueue<T>	actionqueue;
		ConcurrentQueue<T>	completequeue;

		public volatile bool IsRunning;

		public ProcessingThread( Action<T> action, Action<T> complete )
		{
			this.action = action;
			this.complete = complete;

			actionqueue = new ConcurrentQueue<T>();
			completequeue = new ConcurrentQueue<T>();

			thread = new Thread( Execute );
			semaphore = new Semaphore( 0, int.MaxValue );

			IsRunning = true;
			thread.Start();
		}

		public void Dispose()
		{
			IsRunning = false;
			semaphore.Release();
			thread.Join();
		}

		public void Enqueue( T element )
		{
			actionqueue.Enqueue( element );
			semaphore.Release(1);
		}

		public void Execute()
		{
			while( IsRunning )
			{
				semaphore.WaitOne();
				if( !IsRunning ) break;

				T element = default(T);
				if( !actionqueue.TryDequeue( out element ) )
				{
					Console.WriteLine("Error dequeuing");
					semaphore.Release(1);
					continue;
				}

				action( element );

				completequeue.Enqueue( element );
			}
		}

		public void Update( int count )
		{
			for( int i = 0; i < count && !completequeue.IsEmpty; ++i )
			{
				T element = default(T);
				if( !completequeue.TryDequeue( out element ) )
				{
					Console.WriteLine("Error dequeuing");
					break;
				}

				complete( element );
			}
		}
	}
}