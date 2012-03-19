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

	// This class pre computes page indices for quick lookup
	// It flattens the texture pyramid so we can store 
	// properties in a flat array based on the page
	public class PageIndexer
	{
		readonly VirtualTextureInfo info;
		readonly int mipcount;
		readonly int[] offsets; // This stores the offsets to the first page of the start of a mipmap level 
		readonly int[] sizes;	// This stores the sizes of various mip levels
		readonly Page[] reverse;

		public string	LastError	{ get; private set; }
		public int		Count		{ get; private set; }	// Number of pages for all levels

		public PageIndexer( VirtualTextureInfo info )
		{
			this.info = info;

			mipcount = MathExtensions.Log2(info.PageTableSize) + 1;

			sizes   = new int[mipcount];
			for( int i = 0; i < mipcount; ++i )
				sizes[i] = ( info.VirtualTextureSize / info.TileSize ) >> i;

			offsets = new int[mipcount];
			Count = 0;
			for( int i = 0; i < mipcount; ++i )
			{
				offsets[i] = Count;
				Count += sizes[i] * sizes[i];
			}

			// Calculate reverse mapping
			reverse = new Page[Count];
			for( int i = 0; i < mipcount; ++i )
			{
				int size = sizes[i];
				for( int y = 0; y < size; ++y )
				for( int x = 0; x < size; ++x )
				{
					Page page = new Page( x, y, i );
					reverse[this[page]] = page;
				}
			}
		}

		public int this[Page page]
		{
			get
			{
				if( page.Mip < 0 || page.Mip >= mipcount )
					throw new Exception( "Page is not valid" );

				int offset = offsets[page.Mip];
				int stride = sizes[page.Mip];

				return offset + page.Y * stride + page.X;
			}
		}

		public Page GetPageFromIndex( int index )
		{
			return reverse[index];
		}

		public bool IsValid( Page page )
		{
			if( page.Mip < 0 )
			{
				LastError = string.Format( "Mip level smaller than zero ({0}).", page );
				return false;
			}

			else if( page.Mip >= mipcount )
			{
				LastError = string.Format( "Mip level larger than max ({1}), ({0}).", page, mipcount );
				return false;
			}

			if( page.X < 0 )
			{
				LastError = string.Format( "X smaller than zero ({0}).", page );
				return false;
			}

			else if( page.X >= sizes[page.Mip] )
			{
				LastError = string.Format( "X larger than max ({1}), ({0}).", page, sizes[page.Mip] );
				return false;
			}

			if( page.Y < 0 )
			{
				LastError = string.Format( "Y smaller than zero ({0}).", page );
				return false;
			}

			else if( page.Y >= sizes[page.Mip] )
			{
				LastError = string.Format( "Y larger than max ({1}), ({0}).", page, sizes[page.Mip] );
				return false;
			}

			return true;
		}
	}
}