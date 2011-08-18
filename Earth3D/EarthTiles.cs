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
		public enum MapPosition { None=0x00, N=0x01, NE=0x03, E=0x02, SE=0x06, S=0x04, SW=0x0C, W=0x08, NW=0x09, Up=0x10, Down = 0x20 };
		public const int TILE_COUNT = 3;
		public const double MAX_ELEVATION = 65000;
		public static int MaxGoogleZoom = 14;
		private int MaxDeltaFactor = 15;
		public static int MinLogDelta = -4;
		private double unitsPerDegreeLatitude = 100000;
		private CombinedMapDataFactory mapFactory = CombinedMapDataFactory.Instance;
		public CombinedMapData[,] currentTiles = new CombinedMapData[TILE_COUNT, TILE_COUNT];
		public bool UseTerrainData
		{
			get { return mapFactory.UseTerrainData; }
			set { mapFactory.UseTerrainData = value; }
		}
		public bool AutomaticallyDownloadMaps { get { return mapFactory.AutomaticallyDownloadMaps; } set { mapFactory.AutomaticallyDownloadMaps = value; } }

		private int currentZoomLevel = 10;
		private double currentDelta = 1;
		private double currentElevation = 0;
		private double previousElevation = 0;
		private LatLong currentLocation = LatLong.MinValue;
		private LatLong previousLocation = LatLong.MinValue;

		private bool pauseUpdates = false;

		public bool FixTerrain { get; set; }
		public bool FixZoom { get; set; }

		public event ShapeChangeEventHandler MapChanged;

		private LatLong minDisplayedLatLong
		{
			get
			{
				LatLong min = LatLong.MaxValue;
				foreach (CombinedMapData cmd in currentTiles)
					min = LatLong.Min(min, cmd.BottomLeftLocation);
				return min;
			}
		}

		public void FireMapChangedEvent(ShapeChangeEventArgs e)
		{
			if (MapChanged != null) MapChanged(this, e);
		}
		
		public EarthTiles()
		{
			mapFactory.UnitsPerDegreeLatitude = unitsPerDegreeLatitude;
			mapFactory.MapUpdateCompleted += new ShapeChangeEventHandler(mapFactory_MapUpdateCompleted);
		}


		

		private void InitializeAtGivenLatLongElevation(LatLong pos, double elevation)
		{
			currentDelta = GetDeltaFromElevation(elevation);
			pos = EarthProjection.CalculateNearestLatLongAtDelta(pos, currentDelta, false);
			for (int row = 0; row < TILE_COUNT; row++)
			{
				for (int col = 0; col < TILE_COUNT; col++)
				{
					InitializeTile(pos, elevation, row, col);
				}
			}
			currentElevation = elevation;
			currentLocation = pos;
		}

		private void InitializeTile(LatLong pos, double elevation, int row, int col)
		{
			LatLong newPos = new LatLong(pos.Latitude - currentDelta * (row - 1), pos.Longitude - currentDelta * (col - 1));
			CombinedMapData expectedMap = mapFactory.CreateEmptyMapAtLocation(newPos, elevation, currentDelta);
			currentZoomLevel = expectedMap.ZoomLevel;
			if (TerrainUpdateRequired(currentTiles[row, col], expectedMap))
			{
				if (currentTiles[row, col] != null)
					waitingList[expectedMap] = currentTiles[row, col];
				mapFactory.RetrieveOrUpdateMapTerrain(expectedMap);
				AddTileToArray(expectedMap, row, col);
				currentTiles[row, col] = expectedMap;
			}
			UpdateMapTexture(currentTiles[row, col], elevation);
		}


		private double GetDeltaFromElevation(double elevation)
		{
			double logDelta = Math.Floor(Math.Log(elevation, 2.0) - MaxDeltaFactor);
			if (elevation <= 0) logDelta = MinLogDelta;
			if (logDelta < MinLogDelta) logDelta = MinLogDelta;
			return Math.Pow(2.0, logDelta);
			
		}

		private bool TerrainUpdateRequired(CombinedMapData currentMap, CombinedMapData expectedMap)
		{
			if (currentMap == null) return true;
			if (expectedMap.IsSameShape(currentMap)) return false;
			return true;
		}

		private void AddTileToArray(CombinedMapData tile, int row, int column)
		{
			tile.TextureIndex = row * TILE_COUNT + column + Earth3DControl.TEXTURE_OFFSET;
			CombinedMapData previousTile = currentTiles[row, column];
			if (previousTile == tile)
			{
				return;
			}
			currentTiles[row, column] = tile;
			AddTileToEngine(tile);
			//RemoveTileFromEngine(previousTile);
		}

		Dictionary<CombinedMapData, CombinedMapData> waitingList = new Dictionary<CombinedMapData, CombinedMapData>();
		void mapFactory_MapUpdateCompleted(object sender, ShapeChangeEventArgs e)
		{
			CombinedMapData tile = e.ChangedShape as CombinedMapData;
			if (waitingList.ContainsKey(tile))
				waitingList[tile].Dispose();
		}

		Stack<CombinedMapData> removalList = new Stack<CombinedMapData>();
		private void RemoveOldTiles()
		{
			while (removalList.Count > 0)
			{
				CombinedMapData map = removalList.Pop();
				RemoveTileFromEngine(map);
			}
		}

		private void RemoveTileFromEngine(CombinedMapData tile)
		{
			ShapeChangeEventArgs se = new ShapeChangeEventArgs(tile, ShapeChangeEventArgs.ChangeAction.Remove);
			FireMapChangedEvent(se);
		}

		private void AddTileToEngine(CombinedMapData tile)
		{
			ShapeChangeEventArgs e = new ShapeChangeEventArgs(tile, ShapeChangeEventArgs.ChangeAction.Add);
			FireMapChangedEvent(e);
		}


		public void InitializeAtCameraLocation(Float3 cameraLocation)
		{
			LatLong latLong = ConvertCameraLocationToLatLong(cameraLocation);
			InitializeAtGivenLatLongElevation(latLong,cameraLocation.Y);
		}

		public void CameraLocationChanged(Float3 newCameraLocation)
		{
			currentLocation = ConvertCameraLocationToLatLong(newCameraLocation);
			currentElevation = newCameraLocation.Y;
			MapPosition direction = CalculateTravelDirection(currentLocation, previousLocation,currentElevation,previousElevation);
			if (direction != MapPosition.None && !FixTerrain)
			{
				MoveTerrainInDirection(direction, currentLocation, currentElevation);
			}
			if(HasElevationChanged(currentElevation,previousElevation) != MapPosition.None)
			{
			}
			UpdateAllTextures(currentElevation);
			previousLocation = currentLocation;
			previousElevation = currentElevation;
		}

		private void MoveTerrainInDirection(MapPosition direction, LatLong location, double elevation)
		{
			if ((direction & MapPosition.Up) == MapPosition.Up)
			{
				UpdateAllTerrainAtElevation(location,elevation);
			}
			else if ((direction & MapPosition.Down) == MapPosition.Down)
			{
				UpdateAllTerrainAtElevation(location,elevation);
			}
			else if(!pauseUpdates)
			{
				if ((direction & MapPosition.N) == MapPosition.N)
				{
					for (int col = 0; col < TILE_COUNT; col++)
					{
						LatLong locationOffset = new LatLong(TILE_COUNT * currentDelta, 0);
						LatLong newLocation = LatLong.Add(currentTiles[TILE_COUNT - 1, col].BottomLeftLocation, locationOffset);
						CombinedMapData newTile = SwapTileToNewLocation(currentTiles[TILE_COUNT - 1, col], newLocation);
						for (int row = TILE_COUNT - 1; row > 0; row--)
						{
							currentTiles[row, col] = currentTiles[row - 1, col];
						}
						currentTiles[0, col] = newTile;
					}
				}
				if ((direction & MapPosition.S) == MapPosition.S)
				{
					for (int col = 0; col < TILE_COUNT; col++)
					{
						LatLong locationOffset = new LatLong(-TILE_COUNT * currentDelta, 0);
						LatLong newLocation = LatLong.Add(currentTiles[0, col].BottomLeftLocation, locationOffset);
						CombinedMapData newTile = SwapTileToNewLocation(currentTiles[0, col], newLocation);
						for (int row = 0; row < TILE_COUNT - 1; row++)
						{
							currentTiles[row, col] = currentTiles[row + 1, col];
						}
						currentTiles[TILE_COUNT - 1, col] = newTile;
					}
				}
				if ((direction & MapPosition.W) == MapPosition.W)
				{
					for (int row = 0; row < TILE_COUNT; row++)
					{
						LatLong locationOffset = new LatLong(0, -TILE_COUNT * currentDelta);
						LatLong newLocation = LatLong.Add(currentTiles[row, 0].BottomLeftLocation, locationOffset);
						CombinedMapData newTile = SwapTileToNewLocation(currentTiles[row, 0], newLocation);
						for (int col = 0; col < TILE_COUNT - 1; col++)
						{
							currentTiles[row, col] = currentTiles[row, col + 1];
						}
						currentTiles[row, TILE_COUNT - 1] = newTile;
					}
				}
				if ((direction & MapPosition.E) == MapPosition.E)
				{
					for (int row = 0; row < TILE_COUNT; row++)
					{
						LatLong locationOffset = new LatLong(0, TILE_COUNT * currentDelta);
						LatLong newLocation = LatLong.Add(currentTiles[row, TILE_COUNT - 1].BottomLeftLocation, locationOffset);
						CombinedMapData newTile = SwapTileToNewLocation(currentTiles[row, TILE_COUNT - 1], newLocation);
						for (int col = TILE_COUNT - 1; col > 0; col--)
						{
							currentTiles[row, col] = currentTiles[row, col - 1];
						}
						currentTiles[row, 0] = newTile;
					}
				}
			}
		}

		private void UpdateAllTerrainAtElevation(LatLong location, double elevation)
		{
			pauseUpdates = true;
			InitializeAtGivenLatLongElevation(location, elevation);
			pauseUpdates = false;
		}


		private CombinedMapData SwapTileToNewLocation(CombinedMapData prevTile, LatLong newLocation)
		{
			CombinedMapData newTile = new CombinedMapData(prevTile);
			newTile.BottomLeftLocation = newLocation;
			mapFactory.RetrieveOrUpdateMapTerrain(newTile);
			AddTileToEngine(newTile);
			RemoveTileFromEngine(prevTile);
			UpdateMapTexture(newTile, currentElevation);
			return newTile;
		}


		private MapPosition CalculateTravelDirection(LatLong newLocation, LatLong oldLocation, double newElevation, double prevElevation)
		{
			MapPosition direction = MapPosition.None;
			direction |= HasElevationChanged(newElevation, prevElevation);
			if (newLocation.Equals(oldLocation)) return direction;
			LatLong diff = LatLong.Subtract(newLocation,oldLocation);
			double delta = GetDeltaFromElevation(prevElevation);
			double normLat = NormalizeDistance(newLocation.Latitude - minDisplayedLatLong.Latitude,currentDelta);
			double normLong = NormalizeDistance(newLocation.Longitude - minDisplayedLatLong.Longitude, currentDelta);
			
			if (normLat > 2.1 && diff.Latitude > 0)
				direction |= MapPosition.N;
			if (normLat < 0.9 && diff.Latitude < 0)
				direction |= MapPosition.S;
			if (normLong > 2.1 && diff.Longitude > 0)
				direction |= MapPosition.E;
			if (normLong < 0.9 && diff.Longitude < 0)
				direction |= MapPosition.W;

			if (normLat > 3 || normLat < 0 || normLong > 3 || normLong < 0)
				Console.WriteLine("Extreme travel!");

			return direction;
		}

		private MapPosition HasElevationChanged(double newElevation, double oldElevation)
		{
			if (newElevation == oldElevation) return MapPosition.None;
			double logNew = Math.Log(newElevation, 2.0);
			double logOld = Math.Log(oldElevation, 2.0);
			if (newElevation > oldElevation) // Camera Rising
			{
				logNew = Math.Floor(logNew);
				logOld = Math.Floor(logOld);
				double midPoint = Math.Pow(2.0,Math.Floor(logNew)-0.2);
				//if(oldElevation > midPoint)
				if(logOld != logNew)
					return MapPosition.Up;
			}
			else // Camera Falling
			{
				logNew = Math.Floor(logNew+0.5);
				logOld = Math.Floor(logOld+0.5);
				double midPoint = Math.Pow(2.0, Math.Floor(logOld)+0.2);
				//if (oldElevation > midPoint)
				if (logOld != logNew)
					return MapPosition.Down;
			}
			return MapPosition.None;
		}

		private double NormalizeDistance(double distance, double delta)
		{
			double d = (distance  / delta) % TILE_COUNT;
			return d;
		}


		private void UpdateMapTexture(CombinedMapData mapToUpdate, double elevation)
		{
			CombinedMapData expectedMap = mapFactory.CreateEmptyMapAtLocation(mapToUpdate.BottomLeftLocation, elevation, mapToUpdate.ShapeDelta);
			expectedMap.ZoomLevel = currentZoomLevel;
			if (!expectedMap.IsSameTexture(mapToUpdate))
			{
				mapToUpdate.ZoomLevel = expectedMap.ZoomLevel;
				mapFactory.UpdateMapTexture(mapToUpdate);
			}
		}

		private void UpdateAllTextures(double elevation)
		{
			//double delta = GetDeltaFromElevation(elevation);
			foreach (CombinedMapData mapToUpdate in currentTiles)
			{
				UpdateMapTexture(mapToUpdate, elevation);
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

	public delegate void ShapeChangeEventHandler(object sender, ShapeChangeEventArgs e);
	public class ShapeChangeEventArgs : EventArgs
	{
		public enum ChangeAction { None, Add, Remove, Swap };
		private ChangeAction action = ChangeAction.None;
		public ChangeAction Action { get { return action; } }
		private Shape changedShape;
		public Shape ChangedShape { get { return changedShape; } }
		public ShapeChangeEventArgs(Shape changedShape, ChangeAction action)
		{
			this.changedShape = changedShape;
			this.action = action;
		}
	}

}
