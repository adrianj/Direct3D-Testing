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
	using System.IO;
	using System.Diagnostics;
	using System.Runtime.InteropServices;

	// This class read and writes pages to disk
	public class TileDataFile: System.IDisposable
	{
		const int DataOffset = 16;

		readonly VirtualTextureInfo	info;
		readonly int			size;
		readonly FileStream		file;

		public TileDataFile( string filename, VirtualTextureInfo info, FileAccess access )
		{
			Debug.Assert( DataOffset >= Marshal.SizeOf(typeof(VirtualTextureInfo)) );

			this.info = info;
			this.size = info.PageSize * info.PageSize * 4;

			FileMode mode = FileMode.Open;
			if( access != FileAccess.Read )
				mode = FileMode.OpenOrCreate;

			file = new FileStream( filename, mode, access, FileShare.None, 4096, FileOptions.RandomAccess );
		}

		public void Dispose()
		{
			file.Close();
		}

		public VirtualTextureInfo ReadInfo()
		{
			file.Position = 0;
			return Memory.Read<VirtualTextureInfo>( file );
		}

		public void WriteInfo()
		{
			file.Position = 0;
			Memory.Write<VirtualTextureInfo>( file, info );
		}

		public void WritePage( long index, byte[] data )
		{
			file.Position = size * index + DataOffset;
			file.Write( data, 0, data.Length );
		}

		public void ReadPage( long index, byte[] data )
		{
			file.Position = size * index + DataOffset;
			file.Read( data, 0, data.Length );
		}
	}
}
