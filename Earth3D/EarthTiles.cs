using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Direct3DLib
{
	public class EarthTiles
	{
		public const int TILE_ROWS = 2;
		public const int TILE_COLUMNS = 2;
		public const int MAX_ELEVATION_ZOOM = 4;
		private CombinedMapDataFactory mapFactory = CombinedMapDataFactory.Instance;
		private CombinedMapData[,] currentTiles = new CombinedMapData[TILE_ROWS,TILE_COLUMNS];
		private double delta = 0.125;
		public double Delta { get { return delta; } set { delta = value; } }
		private Float3 previousCameraLocation = new Float3();

		private List<IRenderable> engineShapeList;
		public List<IRenderable> EngineShapeList { set { engineShapeList = value; } }

		private double elevation { get { return mapFactory.Elevation; } set { mapFactory.Elevation = value; } }

		public event EventHandler MapChanged;

		public void FireMapChangedEvent()
		{
			EventArgs e = new EventArgs();
			if (MapChanged != null) MapChanged(this, e);
		}
		
		public EarthTiles()
		{
			mapFactory.UnitsPerDegreeLatitude = 100000;
		}
		

		public void InitializeAtGivenLatLongElevation(LatLong pos, double elevation)
		{
			mapFactory.Delta = delta;
			mapFactory.Elevation = elevation;
			for (int i = 0; i < TILE_ROWS; i++)
			{
				for (int k = 0; k < TILE_COLUMNS; k++)
				{
					LatLong newPos = new  LatLong(pos.latitude + delta * i, pos.longitude + delta * k);
					newPos = CalculateNearestLatLongAtCurrentDelta(newPos);
					mapFactory.BottomLeftLocation = newPos;
					CombinedMapData tile = mapFactory.CreateCombinedMapData();
					tile.TerrainShape.Scale = new SlimDX.Vector3(1, 3, 1);
					AddTileToArray(tile, i, k);
				}
			}
		}

		public LatLong CalculateNearestLatLongAtCurrentDelta(LatLong latLong)
		{
			long lat = Convert.ToInt64(Math.Floor(latLong.latitude / delta));
			long lng = Convert.ToInt64(Math.Floor(latLong.longitude / delta));
			LatLong ret = new LatLong((double)lat * delta, (double)lng * delta);
			return ret;
		}

		private int CalculateApproxZoomLevel(double delta)
		{
			return (int)(1.0 / delta) + 4;
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
			UpdateMapTextures(newCameraLocation.Y);
		}


		private void UpdateMapTextures(double elevation)
		{
			mapFactory.Elevation = elevation;
			foreach (CombinedMapData mapToUpdate in currentTiles)
			{
				CombinedMapData expectedMap = mapFactory.CreateEmptyMapAtLocation(mapToUpdate.BottomLeftPosition, elevation, delta);
				if(!expectedMap.IsSameTexture(mapToUpdate))
					mapFactory.UpdateMapTexture(mapToUpdate);
				
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
			return new Float3((float)latLong.longitude * units, (float)elevation, (float)latLong.latitude * units);
		}
		 
	}
}
