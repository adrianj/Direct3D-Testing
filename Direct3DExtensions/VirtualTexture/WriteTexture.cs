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

	using D3D10 = SlimDX.Direct3D10;
	using DXGI  = SlimDX.DXGI;

	public class WriteTexture: IDisposable
	{
		public readonly D3D10.Texture2D	Resource;

		public WriteTexture( D3D10.Device device, int width, int height, DXGI.Format format )
		{
			D3D10.Texture2DDescription desc = new D3D10.Texture2DDescription();

			desc.Width  = width;
			desc.Height = height;

			desc.ArraySize = 1;
			desc.BindFlags = D3D10.BindFlags.None;
			desc.CpuAccessFlags = D3D10.CpuAccessFlags.Write;
			desc.Format = format;
			desc.MipLevels = 1;
			desc.OptionFlags = D3D10.ResourceOptionFlags.None;
			desc.SampleDescription = new SlimDX.DXGI.SampleDescription( 1, 0 );
			desc.Usage = D3D10.ResourceUsage.Staging;

			Resource = new D3D10.Texture2D( device, desc );
		}

		public void Dispose()
		{
			Resource.Dispose();
		}
	}
}