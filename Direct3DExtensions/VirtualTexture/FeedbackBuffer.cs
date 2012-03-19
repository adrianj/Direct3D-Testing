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
	using System.Collections.Specialized;

	using SlimDX;
	using DXGI = SlimDX.DXGI;
	using D3D10 = SlimDX.Direct3D10;

	// This class is used for sorting the pages based on the mip level and request count
	public class PageCount: IComparable<PageCount>
	{
		public Page Page;
		public int  Count;

		public int CompareTo( PageCount other )
		{
			if( other.Page.Mip != Page.Mip )
				return other.Page.Mip.CompareTo( Page.Mip );

			return other.Count.CompareTo( Count );
		}
	}

	// The feedback buffer is used to generate the page requests that drive the entire system.
	// The scene is rendered into the feedback buffer, which is then downloaded and examined on 
	// the cpu to find which pages were visible and how many times they were used in the scene.
	public class FeedbackBuffer: IDisposable
	{
		const int StagingTextureCount = 1;

		readonly VirtualTextureInfo info;
		readonly D3D10.Device		device;

		readonly int				size;

		readonly Direct3D.RenderTarget	rendertarget;
		readonly Direct3D.DepthBuffer	depthbuffer;

		readonly Direct3D.StagingTexturePool	staging;

		readonly D3D10.Viewport	viewport;
	
		readonly PageIndexer	indexer;

		// This stores the pages by index.  The int value is number of requests.
		public int[] Requests { get; private set; }

		public FeedbackBuffer( D3D10.Device device, VirtualTextureInfo info, int size )
		{
			this.info  = info;
			this.device = device;
			this.size = size;

			indexer = new PageIndexer( info );
			Requests = new int[indexer.Count];

			rendertarget = new Direct3D.RenderTarget( device, size, size, DXGI.Format.R32G32B32A32_Float );
			depthbuffer  = new Direct3D.DepthBuffer( device, size, size, DXGI.Format.D32_Float );
			staging		 = new Direct3D.StagingTexturePool( device, size, size, DXGI.Format.R32G32B32A32_Float, StagingTextureCount, D3D10.CpuAccessFlags.Read );

			viewport = new D3D10.Viewport();
			viewport.X = 0;
			viewport.Y = 0;
			viewport.Width = size;
			viewport.Height = size;
			viewport.MinZ = 0.0f;
			viewport.MaxZ = 1.0f;
		}

		public void Dispose()
		{
			staging.Dispose();
			depthbuffer.Dispose();
			rendertarget.Dispose();
		}

		public void Clear()
		{
			rendertarget.Clear( new Color4( 0 ) );
			depthbuffer.Clear( 1.0f );
			
			// Clear Table
			for( int i = 0; i < indexer.Count; ++i )
				Requests[i] = 0;
		}

		public void SetAsRenderTarget()
		{
			device.Rasterizer.SetViewports( viewport );
			device.OutputMerger.SetTargets( depthbuffer.View, rendertarget.RenderTargetView );
		}

		public void Copy()
		{
			device.CopyResource( rendertarget.Resource, staging.Resource );
			staging.MoveNext();
		}

		public void Download()
		{
			// Download new data
			DataRectangle datarect = staging.Resource.Map( 0, D3D10.MapMode.Read, D3D10.MapFlags.None );
			Vector4[] data = datarect.Data.ReadRange<Vector4>( size * size * VirtualTexture.ChannelCount );
			staging.Resource.Unmap( 0 );

			for( int i = 0; i < size*size; ++i )
			{
				if( data[i].W >= 0.99f )
				{
					Page request = new Page( (int)data[i].X, (int)data[i].Y, (int)data[i].Z );
					AddRequestAndParents( request );
				}
			}
		}

		// This function validates the pages and adds the page's parents 
		// We do this so that we can fall back to them if we run out of memory
		void AddRequestAndParents( Page request )
		{
			int PageTableSizeLog2 = MathExtensions.Log2( info.PageTableSize );
			int count = PageTableSizeLog2 - request.Mip + 1;

			for( int i = 0; i < count; ++i )
			{
				int xpos = request.X >> i;
				int ypos = request.Y >> i;

				Page page = new Page( xpos, ypos, request.Mip + i );

				if( !indexer.IsValid( page ) )
				{
#if DEBUG
					Console.WriteLine( indexer.LastError );
#endif // DEBUG
					return;
				}

				++Requests[indexer[page]];
			}
		}
	}
}