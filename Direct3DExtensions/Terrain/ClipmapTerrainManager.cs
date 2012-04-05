using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using Direct3DExtensions.Texturing;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Terrain
{
	public class ClipmapTerrainManager : DisposablePattern
	{
		MultipleEffect3DEngine engine;
		ExTerrainManager terrainManager;
		Effect effect;
		TiledTexture texture;
		Point columnRow = new Point();
		D3D.EffectVectorVariable locationVar;


		public int WidthInTiles { get; set; }
		public int WidthOfTiles { get; set; }
		public TerrainHeightTextureFetcher TerrainFetcher { get; set; }
		public PointF StartingLongLat { get; set; }
		public string TextureVariableName { get; set; }
		public string SrtmDataFolder { get { return TerrainFetcher.SrtmDataFolder; } set { TerrainFetcher.SrtmDataFolder = value; } }

		public ClipmapTerrainManager(MultipleEffect3DEngine engine, Effect effect)
		{
			this.engine = engine;
			this.effect = effect;

			WidthInTiles = 1;
			WidthOfTiles = 256;
			TerrainFetcher = new Srtm3TextureFetcher();
			StartingLongLat = new PointF(174, -37);
			TextureVariableName = "HiresTexture";

			engine.InitializationComplete += (o, e) => { OnInitComplete(); };
			engine.PreRendering += (o, e) => { OnPreRender(); };
			engine.CameraChanged += (o, e) => { OnCameraChanged(e); };
		}

		void OnInitComplete()
		{
			if(TextureVariableName.EndsWith(ShaderTexture.VariableTextureSuffix))
				TextureVariableName = TextureVariableName.Substring(0, TextureVariableName.Length - ShaderTexture.VariableTextureSuffix.Length);
			texture = new TiledTexture(engine.D3DDevice.Device, WidthOfTiles, WidthOfTiles, WidthInTiles, WidthInTiles, SlimDX.DXGI.Format.R32_Float);
			texture.BindToEffect(effect, TextureVariableName);

			string locationVarName = TextureVariableName + "Location";
			locationVar = effect.GetVariableByName(locationVarName).AsVector();
			locationVar.Set(new Vector2());

			InitTerrainAtStartingLongLat();
		}

		private void InitTerrainAtStartingLongLat()
		{
			for(int y = 0; y < WidthInTiles; y++)
				for (int x = 0; x < WidthInTiles; x++)
				{
					UpdateTile(x, y, x, y);
				}
		}

		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
		long fetchTime = 0;
		long writeTime = 0;

		private void UpdateTile(int tileIndexX, int tileIndexY, int texIndexX, int texIndexY)
		{
			watch.Restart();
			long start = watch.ElapsedTicks;
			long end = watch.ElapsedTicks;
			int offset = WidthOfTiles * (WidthInTiles - 1) / 2;
			Point loc = new Point(tileIndexX * WidthOfTiles - offset, tileIndexY * WidthOfTiles - offset);
			Rectangle region = new Rectangle(loc, new Size(WidthOfTiles, WidthOfTiles));
			short[,] data = TerrainFetcher.FetchTerrain(StartingLongLat, region);
			end = watch.ElapsedTicks;
			fetchTime = (fetchTime + end - start) / 2;
			start = watch.ElapsedTicks;
			texture.WriteTexture(data, texIndexX, texIndexY);
			end = watch.ElapsedTicks;
			writeTime = (writeTime + end - start) / 2;
		}

		void OnPreRender()
		{

		}


		void OnCameraChanged(CameraChangedEventArgs e)
		{
			if (e.PositionChanged)
			{

				Point newColRow = CalculateColumnRowFromCameraPosition(e.Camera.Position);

				if (newColRow != columnRow)
				{

					if (this.TerrainFetcher is Srtm30TextureFetcher) Console.WriteLine("Lores update!");
					if (newColRow.X > columnRow.X)
					{
						columnRow.X = newColRow.X;
						UpdateRightColumn();
					}
					if (newColRow.X < columnRow.X)
					{
						columnRow.X = newColRow.X;
						UpdateLeftColumn();
					}

					if (newColRow.Y > columnRow.Y)
					{
						columnRow.Y = newColRow.Y;
						UpdateTopRow();
					}
					if (newColRow.Y < columnRow.Y)
					{
						columnRow.Y = newColRow.Y;
						UpdateBottomRow();
					}

					Vector2 location = new Vector2((float)columnRow.X / (float)WidthInTiles, (float)columnRow.Y / (float)WidthInTiles);
					locationVar.Set(location);

				}
			}
		}


		Point CalculateColumnRowFromCameraPosition(Vector3 camPos)
		{
			float x = camPos.X * TerrainFetcher.PixelsPerLongitude;
			float y = camPos.Z * TerrainFetcher.PixelsPerLatitude;
			x /= WidthOfTiles;
			y /= WidthOfTiles;
			x /= 1200;
			y /= 1200;
			return new Point((int)x, (int)y);
		}

		void UpdateTopRow()
		{
			UpdateRow(columnRow.X, columnRow.Y + WidthInTiles-1, MathExtensions.PositiveMod(columnRow.Y - 1, WidthInTiles));
		}

		void UpdateBottomRow()
		{
			UpdateRow(columnRow.X, columnRow.Y, MathExtensions.PositiveMod(columnRow.Y, WidthInTiles));
		}

		void UpdateRow(int tileIndexX, int tileIndexY, int texIndexY)
		{
			for (int x = 0; x < WidthInTiles; x++)
				UpdateTile(tileIndexX + x, tileIndexY, MathExtensions.PositiveMod(tileIndexX + x, WidthInTiles), texIndexY);
		}

		void UpdateLeftColumn()
		{
			UpdateColumn(columnRow.X, columnRow.Y, MathExtensions.PositiveMod(columnRow.X, WidthInTiles));
		}

		void UpdateRightColumn()
		{
			UpdateColumn(columnRow.X + WidthInTiles - 1, columnRow.Y, MathExtensions.PositiveMod(columnRow.X - 1, WidthInTiles));
		}

		void UpdateColumn(int tileIndexX, int tileIndexY, int texIndexX)
		{
			for (int y = 0; y < WidthInTiles; y++)
				UpdateTile(tileIndexX, tileIndexY + y, texIndexX, MathExtensions.PositiveMod(tileIndexY+y,WidthInTiles));
		}



		public float[,] GetTerrain()
		{
			throw new NotImplementedException();
		}


		void DisposeManaged() { if (terrainManager != null) terrainManager.Dispose(); terrainManager = null; }
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
