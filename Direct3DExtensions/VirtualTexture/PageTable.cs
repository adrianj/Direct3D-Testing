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
	using System.Diagnostics;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	using D3D10 = SlimDX.Direct3D10;
	using DXGI = SlimDX.DXGI;

	// This class manages the PageTable (indirection texture).
	public class PageTable: IDisposable
	{
		readonly VirtualTextureInfo		info;
		readonly D3D10.Device			device;

		readonly Direct3D.Texture		texture;
		readonly Direct3D.WriteTexture	staging;

		readonly PageIndexer			indexer;
		
		readonly Quadtree				quadtree;

		SlimDX.DataBox[]				tabledata;
		D3D10.ResourceRegion[]			regions;		// Just so we don't have to create them every time
		SimpleImage[]					data;			// This is the CPU copy of the page table texture with mips

		public PageTable( D3D10.Device device, PageCache cache, VirtualTextureInfo info, PageIndexer indexer )
		{
			this.info = info;
			this.device = device;
			this.indexer = indexer;

			quadtree = new Quadtree(new Rectangle(0, 0, info.PageTableSize, info.PageTableSize), MathExtensions.Log2(info.PageTableSize));

			int size = info.PageTableSize;
			texture = new Direct3D.Texture( device, size, size, DXGI.Format.R8G8B8A8_UNorm, D3D10.ResourceUsage.Default, 0 );
			staging = new Direct3D.WriteTexture( device, size, size, DXGI.Format.R8G8B8A8_UNorm );

			cache.Added   += ( Page page, Point pt ) => quadtree.Add( page, pt );
			cache.Removed += ( Page page, Point pt ) => quadtree.Remove( page );

			SetupDataAndInfo();
		}

		public void Dispose()
		{
			texture.Dispose();
			staging.Dispose();

			for( int i = 0; i < tabledata.Length; ++i )
				tabledata[i].Data.Dispose();
		}

		public void BindToEffect( D3D10.Effect effect )
		{
			texture.BindToEffect( effect, "PageTable" );

			D3D10.EffectScalarVariable fxpagetablesize = effect.GetVariableByName( "PageTableSize" ).AsScalar();
			fxpagetablesize.Set( (float)info.PageTableSize );
		}

		public void Update()
		{
			int PageTableSizeLog2 = MathExtensions.Log2(info.PageTableSize);

			for( int i = 0; i < PageTableSizeLog2+1; ++i )
			{
				quadtree.Write( data[i], i );

				tabledata[i].Data.Position = 0;
				tabledata[i].Data.WriteRange<byte>( data[i].Data );
				tabledata[i].Data.Position = 0;

				device.UpdateSubresource( tabledata[i], texture.Resource, i, regions[i] );
			}
		}

		void SetupDataAndInfo()
		{
			int PageTableSizeLog2 = MathExtensions.Log2(info.PageTableSize);

			tabledata = new SlimDX.DataBox[PageTableSizeLog2+1];
			regions = new D3D10.ResourceRegion[PageTableSizeLog2+1];
			data = new SimpleImage[PageTableSizeLog2+1];

			for( int i = 0; i < PageTableSizeLog2+1; ++i )
			{
				int size = info.PageTableSize >> i;
				SlimDX.DataStream stream = new SlimDX.DataStream( size * size * VirtualTexture.ChannelCount, false, true );
				tabledata[i] = new SlimDX.DataBox( size * VirtualTexture.ChannelCount, size * size * VirtualTexture.ChannelCount, stream );

				D3D10.ResourceRegion region = new D3D10.ResourceRegion();
				region.Left = 0;	region.Right  = size;
				region.Top  = 0;	region.Bottom = size;
				region.Front = 0;	region.Back   = 1;
				regions[i] = region;

				data[i] = new SimpleImage( size, size, VirtualTexture.ChannelCount );
			}
		}
	}
}