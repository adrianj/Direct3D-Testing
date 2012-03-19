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

	// This class loads the pages from disk in a separate thread and then signals when complete.
	public class PageLoader: IDisposable
	{
		class ReadState
		{
			public Page		Page;
			public byte[]	Data;
		}
	
		readonly string filename;

		readonly VirtualTextureInfo info;
		readonly PageIndexer		indexer;

		readonly TileDataFile					file;
		readonly ProcessingThread<ReadState>	readthread;

		// Properties
		public bool ColorMipLevels { get; set; }
		public bool ShowBorders { get; set; }

		// Events
		public event System.Action<Page,byte[]>	LoadComplete;

		public PageLoader( string filename, PageIndexer indexer, VirtualTextureInfo info )
		{
			this.filename = filename;
			this.info = info;
			this.indexer = indexer;

			file = new TileDataFile( filename, info, FileAccess.Read );

			readthread = new ProcessingThread<ReadState>( LoadPage, PageLoadComplete );
		}

		public void Dispose()
		{
			readthread.Dispose();
			file.Dispose();
		}

		public void Submit( Page request )
		{
			ReadState state = new ReadState();
			state.Page = request;

			readthread.Enqueue( state );
#if DEBUG
			//Console.WriteLine("Requested: {0}", state.Page );
#endif
		}

		public void Update( int uploadsperframe )
		{
			readthread.Update( uploadsperframe );
		}

		void LoadPage( ReadState state )
		{
			int size = info.PageSize * info.PageSize * VirtualTexture.ChannelCount;

			state.Data = new byte[size];
		
			if( ColorMipLevels )
				CopyColor( state.Data, state.Page );

			else
				file.ReadPage( indexer[state.Page], state.Data );

			if( ShowBorders )
				CopyBorder( state.Data );

			//System.Threading.Thread.Sleep( 50 );
		}

		void PageLoadComplete( ReadState state )
		{
#if DEBUG
			//Console.WriteLine("Loaded: {0}", state.Page);
#endif
			LoadComplete( state.Page, state.Data );
		}

		void CopyBorder( byte[] image )
		{
			int pagesize = info.PageSize;
			int bordersize = info.BorderSize;

			for( int i = 0; i < pagesize; ++i )
			{
				int xindex = bordersize * pagesize + i;
				image[xindex*VirtualTexture.ChannelCount+0] = 0;
				image[xindex*VirtualTexture.ChannelCount+1] = 255;
				image[xindex*VirtualTexture.ChannelCount+2] = 0;
				image[xindex*VirtualTexture.ChannelCount+3] = 255;
				
				int yindex = i * pagesize + bordersize;
				image[yindex*VirtualTexture.ChannelCount+0] = 0;
				image[yindex*VirtualTexture.ChannelCount+1] = 255;
				image[yindex*VirtualTexture.ChannelCount+2] = 0;
				image[yindex*VirtualTexture.ChannelCount+3] = 255;
			}
		}

		void CopyColor( byte[] image, Page request )
		{
			byte[][] colors = 
			{
				new byte[] {   0,   0, 255, 255 },
				new byte[] {   0, 255, 255, 255 },
				new byte[] { 255,   0,   0, 255 },
				new byte[] { 255,   0, 255, 255 },
				new byte[] { 255, 255,   0, 255 },
				new byte[] {  64,  64, 192, 255 },

				new byte[] {  64, 192,  64, 255 },
				new byte[] {  64, 192, 192, 255 },
				new byte[] { 192,  64,  64, 255 },
				new byte[] { 192,  64, 192, 255 },
				new byte[] { 192, 192,  64, 255 },
				new byte[] {   0, 255,   0, 255 }
			};

			int pagesize = info.PageSize;

			for( int y = 0; y < pagesize; ++y )
			for( int x = 0; x < pagesize; ++x )
			{
				image[(y*pagesize+x)*VirtualTexture.ChannelCount+0] = colors[request.Mip][0];
				image[(y*pagesize+x)*VirtualTexture.ChannelCount+1] = colors[request.Mip][1];
				image[(y*pagesize+x)*VirtualTexture.ChannelCount+2] = colors[request.Mip][2];
				image[(y*pagesize+x)*VirtualTexture.ChannelCount+3] = colors[request.Mip][3];
			}
		}
	}
}