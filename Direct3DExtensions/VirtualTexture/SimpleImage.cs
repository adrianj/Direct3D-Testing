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
	using System.Drawing;

	public class SimpleImage
	{
		public int		Width;
		public int		Height;
		public int		Channels;
		public byte[]	Data;

		public SimpleImage( int width, int height, int channels )
		{
			Width    = width;
			Height   = height;
			Channels = channels;
			Data     = new byte[width*height*channels];
		}

		public SimpleImage( int width, int height, int channels, byte[] data )
		{
			Width    = width;
			Height   = height;
			Channels = channels;
			Data     = data;
		}

		public static void Copy(SimpleImage dest, Point dest_offset, SimpleImage src, Rectangle src_rect)
		{
			int width = System.Math.Min(dest.Width - dest_offset.X, src_rect.Width);
			int height = System.Math.Min(dest.Height - dest_offset.Y, src_rect.Height);
			int channels = System.Math.Min(dest.Channels, src.Channels);

			for (int j = 0; j < height; ++j)
				for (int i = 0; i < width; ++i)
				{
					int i1 = ((j + dest_offset.Y) * dest.Width + (i + dest_offset.X)) * dest.Channels;
					int i2 = ((j + src_rect.Y) * src.Width + (i + src_rect.X)) * src.Channels;

					for (int c = 0; c < channels; ++c)
						dest.Data[i1 + c] = src.Data[i2 + c];
				}
		}

		public static void Fill( SimpleImage image, Rectangle rect, byte r, byte g, byte b, byte a )
		{
			for( int y = rect.Top; y < rect.Bottom; ++y )
			for( int x = rect.Left; x < rect.Right; ++x )
			{
				image.Data[image.Channels*(y*image.Width+x)+0] = r;
				image.Data[image.Channels*(y*image.Width+x)+1] = g;
				image.Data[image.Channels*(y*image.Width+x)+2] = b;
				image.Data[image.Channels*(y*image.Width+x)+3] = a;
			}
		}

		public static void Mipmap( byte[] source, int size, int channels, byte[] dest )
		{
			int mipsize = size / 2;

			for( int y = 0; y < mipsize; ++y )
			for( int x = 0; x < mipsize; ++x )
			{
				for( int c = 0; c < channels; ++c )
				{
					int index = channels*((y*2)*size+(x*2)) + c;
					
					int sum_value = 4 >> 1;
					sum_value += source[index + channels*(0*size+0)];
					sum_value += source[index + channels*(0*size+1)];
					sum_value += source[index + channels*(1*size+0)];
					sum_value += source[index + channels*(1*size+1)];
					dest[channels*(y*mipsize+x)+c] = (byte)(sum_value / 4);
				}
			}
		}
	}
}






