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

	public class Texture: IDisposable
	{
		public D3D10.Texture2D			Resource	{ get; private set; }
		public D3D10.ShaderResourceView	View		{ get; private set; }

		public Texture( D3D10.Device device, string filename )
		{
			Resource = D3D10.Texture2D.FromFile( device, filename );
			View = new D3D10.ShaderResourceView( device, Resource );
		}

		public Texture( D3D10.Device device, int width, int height, DXGI.Format format, D3D10.ResourceUsage usage, int miplevels )
		{
			D3D10.Texture2DDescription desc = new D3D10.Texture2DDescription();

			desc.Width  = width;
			desc.Height = height;

			D3D10.BindFlags bindflags = D3D10.BindFlags.ShaderResource;
			D3D10.ResourceOptionFlags optionflags = D3D10.ResourceOptionFlags.None;

			if( miplevels != 1 )
			{
				bindflags |= D3D10.BindFlags.RenderTarget;
				optionflags |= D3D10.ResourceOptionFlags.GenerateMipMaps;
			}

			desc.ArraySize = 1;
			desc.BindFlags = bindflags;
			desc.CpuAccessFlags = D3D10.CpuAccessFlags.None;
			desc.Format = format;
			desc.MipLevels = miplevels;
			desc.OptionFlags = optionflags;
			desc.SampleDescription = new SlimDX.DXGI.SampleDescription( 1, 0 );
			desc.Usage = usage;

			Create( device, desc );
		}

		public void Dispose()
		{
			Resource.Dispose();
			View.Dispose();
		}

		public void BindToEffect( D3D10.Effect effect, string name )
		{
			D3D10.EffectResourceVariable fxvar = effect.GetVariableByName( name ).AsResource();
			fxvar.SetResource( View );
		}

		public static int CalcSubResource( int mipslice, int arrayslice, int miplevels )
		{
			//MipSlice:		[in] A zero-based index into an array of subtextures; 0 indicates the first, most detailed subtexture (or mipmap level).
			//ArraySlice:   [in] The zero-based index of the first texture to use (in an array of textures).
			//MipLevels:	[in] Number of mipmap levels (or subtextures) to use.

			return mipslice + ( arrayslice * miplevels );
		}

		void Create( D3D10.Device device, D3D10.Texture2DDescription desc )
		{
			Resource = new D3D10.Texture2D( device, desc );
			View = new D3D10.ShaderResourceView( device, Resource );
		}
	}
}