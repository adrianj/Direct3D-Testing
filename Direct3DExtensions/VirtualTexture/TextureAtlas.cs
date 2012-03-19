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
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;
	using System.Collections.Generic;

	using D3D10 = SlimDX.Direct3D10;
	using DXGI  = SlimDX.DXGI;

	// The texture atlas holds the pages in GPU memory.
	public class TextureAtlas: IDisposable
	{
		readonly VirtualTextureInfo	info;
		readonly D3D10.Device		device;
		
		readonly Direct3D.Texture				resource;
		readonly Direct3D.StagingTexturePool	staging;

		public TextureAtlas( D3D10.Device device, VirtualTextureInfo info, int count, int uploadsperframe )
		{
			this.device = device;
			this.info   = info;
			
			int pagesize = info.PageSize;
			resource = new Direct3D.Texture( device, count * pagesize, count * pagesize, DXGI.Format.R8G8B8A8_UNorm, D3D10.ResourceUsage.Default, 1 );			
			staging = new Direct3D.StagingTexturePool( device, pagesize, pagesize, DXGI.Format.R8G8B8A8_UNorm, uploadsperframe, D3D10.CpuAccessFlags.Write );
		}

		public void Dispose()
		{
			resource.Dispose();
			staging.Dispose();
		}

		public void BindToEffect( D3D10.Effect effect )
		{
			D3D10.EffectResourceVariable fxpagetabletex2 = effect.GetVariableByName( "TextureAtlas" ).AsResource();
			fxpagetabletex2.SetResource( resource.View );
		}

		public void UploadPage( Point pt, byte[] data )
		{
			D3D10.Texture2D writer = staging.Resource;
			staging.MoveNext();

			SlimDX.DataRectangle rect = writer.Map( 0, D3D10.MapMode.Write, D3D10.MapFlags.None );

			int pagesize = info.PageSize;
			
			for( int y = 0; y < pagesize; ++y )
			{
				rect.Data.WriteRange( data, y*pagesize * VirtualTexture.ChannelCount, pagesize * VirtualTexture.ChannelCount );
				rect.Data.Position += rect.Pitch - pagesize * VirtualTexture.ChannelCount;
			}

			rect.Data.Position = 0;

			writer.Unmap( 0 );

			D3D10.ResourceRegion region = new D3D10.ResourceRegion();
			region.Left  = 0;	region.Right  = pagesize;
			region.Top   = 0;	region.Bottom = pagesize;
			region.Front = 0;	region.Back   = 1;

			int xpos = pt.X * info.PageSize;
			int ypos = pt.Y * info.PageSize;
			device.CopySubresourceRegion( writer, 0, region, resource.Resource, 0, xpos, ypos, 0 );
		}
	}
}