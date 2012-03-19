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

	// This structure is the hierarchical representation of the page table.
	[DebuggerDisplay("( {Level}, {Rectangle.X}, {Rectangle.Y} )")]
	public class Quadtree
	{
		public int			Level;
		public Rectangle	Rectangle;
		public Point		Mapping;

		[DebuggerDisplay("( {Children[0]}, {Children[1]}, {Children[2]}, {Children[3]} )")]
		public Quadtree[] Children;

		public Quadtree( Rectangle rect, int level )
		{
			Level = level;
			Rectangle = rect;
			Children = new Quadtree[4];
		}

		public void Add( Page request, Point mapping )
		{
			int scale = 1 << request.Mip; // Same as pow( 2, mip )
			int x = request.X * scale;
			int y = request.Y * scale;

			Quadtree node = this;

			while( request.Mip < node.Level )
			{
				for( int i = 0; i < 4; ++i )
				{
					Rectangle rect = GetRectangle( node, i );
					if( rect.Contains( x, y ) )
					{
						// Create a new one if needed
						if( node.Children[i] == null )
						{
							node.Children[i] = new Quadtree( rect, node.Level-1 );
							node = node.Children[i];
							break;
						}

						// Otherwise traverse the tree
						else
						{
							node = node.Children[i];
							break;
						}
					}
				}
			}
			
			// We have created the correct node, now set the mapping
			node.Mapping = mapping;
		}

		public void Remove( Page request )
		{
			int index;
			Quadtree node = FindPage( this, request, out index );

			if( node != null )
				node.Children[index] = null;
		}

		public void Write( SimpleImage image, int miplevel )
		{
			Quadtree.Write( this, image, miplevel );
		}

		// Static Functions
		static Rectangle GetRectangle( Quadtree node, int index )
		{
			int x = node.Rectangle.X;
			int y = node.Rectangle.Y;
			int w = node.Rectangle.Width/2;
			int h = node.Rectangle.Height/2;

			switch( index )
			{
				case 0: return new Rectangle( x,     y,     w, h );
				case 1: return new Rectangle( x + w, y,     w, h );
				case 2: return new Rectangle( x + w, y + h, w, h );
				case 3: return new Rectangle( x,     y + h, w, h );
			}

			throw new ArgumentOutOfRangeException( "index" );
		}

		static void Write( Quadtree node, SimpleImage image, int miplevel )
		{
			if( node.Level >= miplevel )
			{
				int rx = node.Rectangle.X >> miplevel;
				int ry = node.Rectangle.Y >> miplevel;
				int rw = node.Rectangle.Width >> miplevel;
				int rh = node.Rectangle.Height >> miplevel;

				SimpleImage.Fill( image, new Rectangle( rx, ry, rw, rh ), (byte)node.Mapping.X, (byte)node.Mapping.Y, (byte)node.Level, 255 );

				foreach( Quadtree child in node.Children )
					if( child != null )
						Quadtree.Write( child, image, miplevel );
			}
		}

		static Quadtree FindPage( Quadtree node, Page request, out int index )
		{
			int scale = 1 << request.Mip; // Same as pow( 2, mip )
			int x = request.X * scale;
			int y = request.Y * scale;

			// Find the parent of the child we want to remove
			bool exitloop = false;
			while( !exitloop )
			{
				exitloop = true;

				for( int i = 0; i < 4; ++i )
				{
					if( node.Children[i] != null && node.Children[i].Rectangle.Contains( x, y ) )
					{
						// We found it
						if( request.Mip == node.Level-1 )
						{
							index = i;
							return node;
						}

						// Check the children
						else
						{
							node = node.Children[i];
							exitloop = false;
						}
					}
				}
			}

			// We couldn't find it so it must not exist anymore
			index = -1;
			return null;
		}
	}
}
