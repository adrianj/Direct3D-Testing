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
	using System.Runtime.InteropServices;

	// Basic information about the virtual texture, used by all virtual texture classes
	[StructLayout(LayoutKind.Sequential,Pack=1)]
	public struct VirtualTextureInfo
	{
		public int VirtualTextureSize;
		public int TileSize;
		public int BorderSize;

		public int PageSize		 { get { return TileSize + 2*BorderSize; } }
		public int PageTableSize { get { return VirtualTextureSize / TileSize; } }

		public void PrintInfo()
		{
			Console.WriteLine( "VirtualTextureSize: {0}", VirtualTextureSize );
			Console.WriteLine( "PageSize:           {0}", PageSize );
			Console.WriteLine( "TileSize:           {0}", TileSize );
			Console.WriteLine( "BorderSize:         {0}", BorderSize );

			Console.WriteLine( "PageTableSize:      {0}", PageTableSize );
		}
	}
}