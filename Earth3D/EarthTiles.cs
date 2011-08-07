using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace Direct3DLib
{
	public class EarthTiles
	{
		public const int TILE_ROWS = 2;
		public const int TILE_COLUMNS = 2;
		public const double MAX_ELEVATION = 2100000;
		public static int MaxGoogleZoom = 13;
		private CombinedMapDataFactory mapFactory = CombinedMapDataFactory.Instance;
		public CombinedMapData[,] currentTiles = new CombinedMapData[TILE_ROWS,TILE_COLUMNS];
		public bool UseTerrainData
		{
			get { return mapFactory.UseTerrainData; }
			set { mapFactory.UseTerrainData = value; }
		}
		public bool AutomaticallyDownloadMaps { get { return mapFactory.AutomaticallyDownloadMaps; } set { mapFactory.AutomaticallyDownloadMaps = value; } }
		private double delta = 0.125;
		public double Delta { get { return delta; } set { delta = value; } }
		private double previousElevation = -1000;
		private LatLong previousLocation = new LatLong(-1000, -1000);

		private List<Shape> engineShapeList;
		public List<Shape> EngineShapeList { set { engineShapeList = value; } }

		private BackgroundWorker textureWorker = new BackgroundWorker();
		public bool FixTerrain { get; set; }
		public bool FixZoom { get; set; }

		public event EventHandler MapChanged;

		public void FireMapChangedEvent()
		{
			EventArgs e = new EventArgs();
			if (MapChanged != null) MapChanged(this, e);
		}
		
		public EarthTiles()
		{
			mapFactory.UnitsPerDegreeLatitude = 100000;
			textureWorker.WorkerSupportsCancellation = true;
			textureWorker.DoWork += new DoWorkEventHandler(textureWorker_DoWork);
		}

		

		private void InitializeAtGivenLatLongElevation(LatLong pos, double elevation)
		{
			//double logDelta = Math.Log(elevation, 2.0) - 15;
			//Delta = Math.Pow(2.0, Math.Floor(logDelta));
			mapFactory.Delta = delta;
			pos = EarthProjection.CalculateNearestLatLongAtDelta(pos,delta);
			for (int i = 0; i < TILE_ROWS; i++)
			{
				for (int k = 0; k < TILE_COLUMNS; k++)
				{
					LatLong newPos = new  LatLong(pos.Latitude - delta * i, pos.Longitude - delta * k);
					mapFactory.BottomLeftLocation = newPos;
					CombinedMapData expectedMap = mapFactory.CreateEmptyMapAtLocation(newPos,elevation,delta);
					if (TerrainUpdateRequired(currentTiles[i, k], expectedMap))
					{
						textureWorker.CancelAsync();
						previousElevation = -1000;
						mapFactory.RetrieveOrUpdateMapTerrain(expectedMap);
						expectedMap.Scale = new SlimDX.Vector3(1, 3, 1);
						AddTileToArray(expectedMap, i, k);
					}
				}
			}
			UpdateMapTextures(pos, elevation);
		}

		private bool TerrainUpdateRequired(CombinedMapData currentMap, CombinedMapData expectedMap)
		{
			if (currentMap == null) return true;
			if (expectedMap.IsSameShape(currentMap)) return false;
			return true;
		}



		private void AddTileToArray(CombinedMapData tile, int row, int column)
		{
			if (engineShapeList != null)
			{
				tile.TextureIndex = row * TILE_COLUMNS + column + Earth3DControl.TEXTURE_OFFSET;
				CombinedMapData previousTile = currentTiles[row, column];
				if(previousTile == tile)
				{
					return;
				}
				if (previousTile != null)
				{
					engineShapeList.Remove(previousTile);
				}
				currentTiles[row, column] = tile;
				engineShapeList.Add(tile);
				FireMapChangedEvent();
			}
		}

		public void InitializeAtCameraLocation(Float3 cameraLocation)
		{
			LatLong latLong = ConvertCameraLocationToLatLong(cameraLocation);
			InitializeAtGivenLatLongElevation(latLong,cameraLocation.Y);
		}

		public void CameraLocationChanged(Float3 newCameraLocation)
		{
			LatLong location = ConvertCameraLocationToLatLong(newCameraLocation);
			UpdateMapTerrain(location, newCameraLocation.Y);
			UpdateMapTextures(location, newCameraLocation.Y);
		}

		public void UpdateMapTerrain(LatLong location, double elevation)
		{
			if (FixTerrain) return;
			InitializeAtGivenLatLongElevation(location, elevation);
		}

		private void UpdateMapTextures(LatLong location, double elevation)
		{
			if (FixZoom) return;
			if (textureWorker.IsBusy) return;
			if(elevation != previousElevation)
				textureWorker.RunWorkerAsync(elevation);
			previousElevation = elevation;
		}


		void textureWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			double elevation = (double)e.Argument;
			UpdateMapTexturesThread(elevation);
		}

		private void UpdateMapTexturesThread(double elevation)
		{
			foreach (CombinedMapData mapToUpdate in currentTiles)
			{
				if (textureWorker.CancellationPending) return;
				CombinedMapData expectedMap = mapFactory.CreateEmptyMapAtLocation(mapToUpdate.BottomLeftPosition, elevation, delta);
				if (!expectedMap.IsSameTexture(mapToUpdate))
				{
					mapFactory.UpdateMapTexture(mapToUpdate, elevation);
				}
			}
		}


		public LatLong ConvertCameraLocationToLatLong(Float3 cameraLocation)
		{
			float units = (float)mapFactory.UnitsPerDegreeLatitude;
			return new LatLong(cameraLocation.Z / units, cameraLocation.X / units);
		}

		
		public Float3 ConvertLatLongElevationToCameraLocation(LatLong latLong, double elevation)
		{
			float units = (float)mapFactory.UnitsPerDegreeLatitude;
			return new Float3((float)latLong.Longitude * units, (float)elevation, (float)latLong.Latitude * units);
		}
		 
	}
}
