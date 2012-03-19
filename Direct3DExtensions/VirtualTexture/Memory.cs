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
	using System.Runtime.InteropServices;
	using System.Runtime.Serialization.Formatters.Binary;

	public class Memory
	{
		public static object RawDeserialize( byte[] rawData, int position, Type anyType )
		{
			int rawsize = Marshal.SizeOf( anyType );

			if( rawsize > rawData.Length )
				return null;

			IntPtr buffer = Marshal.AllocHGlobal( rawsize );
			Marshal.Copy( rawData, position, buffer, rawsize );
			object retobj = Marshal.PtrToStructure( buffer, anyType );
			Marshal.FreeHGlobal( buffer );

			return retobj;
		}

		public static byte[] RawSerialize( object anything )
		{
			int rawsize = Marshal.SizeOf( anything );

			IntPtr buffer = Marshal.AllocHGlobal( rawsize );

			Marshal.StructureToPtr( anything, buffer, false );
	
			byte[] rawdatas = new byte[ rawsize ];
			Marshal.Copy( buffer, rawdatas, 0, rawsize );
			
			Marshal.FreeHGlobal( buffer );

			return rawdatas;
		}

		public static void Write<Type>( Stream stream, Type obj )
		{
			byte[] data = RawSerialize( obj );
			stream.Write( data, 0, data.Length );
		}

		public static Type Read<Type>( Stream stream )
		{
			int bytesize  = Marshal.SizeOf(typeof(Type));
			byte[] data   = new byte[bytesize];
			int remaining = bytesize;
			int read      = 0;

			while( remaining > 0 )
			{
				int count = stream.Read( data, read, remaining );
				if( count == 0 )
					break;

				read += count;
				remaining -= count;
			}

			return (Type)Memory.RawDeserialize( data, 0, typeof(Type) );
		}
	}
}