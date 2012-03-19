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

	using D3D10 = SlimDX.Direct3D10;

	// The virtual texture class contains everything dealing with a single virtual texture.
	// It handles processing the requests, scheduling the pages, loading, the data, updating 
	// the page table and the texture atlas.
	public class VirtualTexture: IDisposable
	{
		public const int ChannelCount = 4;

		readonly D3D10.Device		device;

		readonly VirtualTextureInfo	info;
		readonly PageIndexer		indexer;
		readonly PageTable			pagetable;
		readonly TextureAtlas		atlas;
		readonly PageLoader			loader;		
		readonly PageCache			cache;

		readonly int				atlascount;
		readonly int				uploadsperframe;

		readonly List<PageCount>	toload;

		D3D10.EffectScalarVariable fxmipbias;
		int  mipbias = 4;	// 3 is sufficient, but at 4 it loads 
							// pages just past the mip level so
							// we don't notice any popping.

		// Properties
		public PageLoader Loader { get { return loader; } }

		public int MipBias
		{
			get { return mipbias; }
			set 
			{
				mipbias = value;
				if( mipbias < 0 )
					mipbias = 0;

				fxmipbias.Set( (float)mipbias );
				Console.WriteLine("MipBias: {0}", mipbias);
			}
		}

		public VirtualTexture( D3D10.Device device, VirtualTextureInfo info, int atlassize, int uploadsperframe, string filename )
		{
			this.device = device;
			this.info   = info;

			this.atlascount = atlassize / info.PageSize;
			this.uploadsperframe = uploadsperframe;

			indexer = new PageIndexer( info );
			toload = new List<PageCount>( indexer.Count );

			Console.Write( "Creating PageAtlas...");
			atlas = new TextureAtlas( device, info, atlascount, uploadsperframe );
			Console.WriteLine("done.");

			Console.Write( "Creating PageLoader..." );
			loader = new PageLoader( filename + ".cache", indexer, info );
			Console.WriteLine("done.");

			cache = new PageCache( info, atlas, loader, indexer, atlascount );

			Console.Write( "Creating PageTable...");
			pagetable = new PageTable( device, cache, info, indexer );
			Console.WriteLine("done.");
		}

		public void BindToEffect( D3D10.Effect effect )
		{
			atlas.BindToEffect( effect );
			pagetable.BindToEffect( effect );

			int pagesize = info.PageSize;

			effect.GetVariableByName( "VirtualTextureSize" ).AsScalar().Set( (float)info.VirtualTextureSize );

			effect.GetVariableByName( "AtlasScale" ).AsScalar().Set( 1.0f/atlascount );

			effect.GetVariableByName( "BorderScale" ).AsScalar().Set( (pagesize-2.0f*info.BorderSize)/pagesize );
			effect.GetVariableByName( "BorderOffset" ).AsScalar().Set( info.BorderSize/(float)pagesize );

			fxmipbias = effect.GetVariableByName( "MipBias" ).AsScalar();
			fxmipbias.Set( (float)mipbias );
		}

		public void Dispose()
		{
			pagetable.Dispose();
			atlas.Dispose();
			loader.Dispose();
		}

		public void Clear()
		{
			cache.Clear();
		}

		public void Update( int[] requests )
		{
			toload.Clear();
			
			// Find out what is already in memory
			// If it is, update it's position in the LRU collection
			// Otherwise add it to the list of pages to load
			int touched = 0;
			for( int i = 0; i < requests.Length; ++i )
			{
				if( requests[i] > 0 )
				{
					PageCount pc = new PageCount 
					{ 
						Page = indexer.GetPageFromIndex( i ),
						Count = requests[i]
					};

					if( !cache.Touch( pc.Page ) )
						toload.Add( pc );
					else 
						++touched;
				}
			}

			// Check to make sure we don't thrash
			if( touched < atlascount * atlascount )
			{
				// sort by low res to high res and number of requests
				toload.Sort();
				
				// if more pages than will fit in memory or more than update per frame drop high res pages with lowest use count
				int loadcount = System.Math.Min( System.Math.Min( toload.Count, uploadsperframe ), atlascount * atlascount );
				for( int i = 0; i < loadcount; ++i )
					cache.Request( toload[i].Page );
			}
			else
			{
				// The problem here is that all pages in cache are requested and the new or high res ones don't get uploaded
				// We can adjust the mip bias to make it all fit. This solves the problem of page cache thrashing
				--MipBias;
			}

			// load any waiting requests & update the texture atlas
			loader.Update( uploadsperframe );

			// Update the page table
			pagetable.Update();
		}
	}
}