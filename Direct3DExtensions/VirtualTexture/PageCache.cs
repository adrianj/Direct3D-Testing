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
	using System.Drawing;
	using System.Collections.Generic;

	// This class manages the texture atlas, tracks page loading and schedules loading of new pages.
	// This class also manages the LRU collection that removes unused pages
	public class PageCache
	{
		readonly VirtualTextureInfo	info;

		readonly PageIndexer		indexer;

		readonly TextureAtlas		atlas;
		readonly PageLoader			loader;

		readonly int count;

		int current; // This is used for generating the texture atlas indices before the lru is full

		readonly LruCollection<Page,Point>	lru;
		readonly HashSet<Page>				loading;

		// These events are used to notify the other systems
		public event Action<Page,Point>	Removed;
		public event Action<Page,Point>	Added;

		public PageCache( VirtualTextureInfo info, TextureAtlas atlas, PageLoader loader, PageIndexer indexer, int count )
		{
			this.info  = info;
			this.atlas = atlas;
			this.loader = loader;
			this.indexer = indexer;
			this.count = count;

			lru = new LruCollection<Page,Point>( count * count );
			lru.Removed += ( page, point ) => Removed( page, point );

			loader.LoadComplete += LoadComplete;

			loading = new HashSet<Page>();
		}

		// Update the pages's position in the lru
		public bool Touch( Page page )
		{
			if( !loading.Contains( page ) )
			{
				Point pt = Point.Empty;
				return lru.TryGetValue( page, true, out pt );
			}

			return false;
		}

		// Schedule a load if not already loaded or loading
		public bool Request( Page request )
		{
			if( !loading.Contains( request ) )
			{
				Point pt = Point.Empty;
				if( !lru.TryGetValue( request, false, out pt ) )
				{
					loading.Add( request );
					loader.Submit( request );
					return true;
				}
			}

			return false;
		}

		public void Clear()
		{
			lru.Clear();
			current = 0;
		}

		void LoadComplete( Page page, byte[] data )
		{
			loading.Remove( page );

			// Find a place in the atlas for the data
			Point pt = Point.Empty;

			if( current == count*count )
				pt = lru.RemoveLast();
			else
			{
				pt = new Point( current % count, current / count );
				++current;

				if( current == count * count )
					Console.WriteLine("Atlas Full, using LRU");
			}

			atlas.UploadPage( pt, data );
			lru.Add( page, pt );

			// Signal that we added a page
			Added( page, pt );
		}
	}
}