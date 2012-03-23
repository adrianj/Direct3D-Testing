using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Texturing
{
	public class TiledTexture : ShaderTexture
	{
		protected StagingTexture staging;

		public int WidthInTiles { get { return this.Description.Width / staging.Description.Width; } }
		public int HeightInTiles { get { return this.Description.Height / staging.Description.Height; } }
		public int TileWidth { get { return staging.Description.Width; } }
		public int TileHeight { get { return staging.Description.Height; } }


		public TiledTexture(D3D.Device device, int tileWidth, int tileHeight, int widthInTiles, int heightInTiles, SlimDX.DXGI.Format format) 
			: base(device,tileWidth*widthInTiles, tileHeight*heightInTiles,format)
		{
			staging = new StagingTexture(device, tileWidth, tileHeight, format);
		}


		public virtual void WriteTexture<T>(T[,] data, int tileXIndex, int tileYIndex) where T : IConvertible
		{
			staging.WriteTexture(data);
			this.WriteTexture(staging, tileXIndex * staging.Description.Width, tileYIndex * staging.Description.Height);
		}


		void DisposeManaged() { if (staging != null) staging.Dispose(); staging = null; }
		void DisposeUnmanaged() { }

		bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}

				DisposeUnmanaged();
				disposed = true;
			}
			base.Dispose(disposing);
		}
    
	}
}
