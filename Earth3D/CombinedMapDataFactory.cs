﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Direct3DLib
{
	public class CombinedMapDataFactory
	{
		public bool UseTerrainData = true;

		private static CombinedMapDataFactory factorySingleton;
		private ShapeHGTFactory shapeFactory = new ShapeHGTFactory();
		private MapTextureFactory textureFactory = MapTextureFactory.Instance;
		private List<CombinedMapData> previouslyCreatedTerrain = new List<CombinedMapData>();

		public double Delta
		{
			get { return shapeFactory.LongitudeDelta; }
			set { shapeFactory.LongitudeDelta = value; shapeFactory.LatitudeDelta = value; }
		}
		public LatLong BottomLeftLocation
		{
			get { return new LatLong(shapeFactory.BottomLeftLatitude, shapeFactory.BottomLeftLongitude); }
			set { shapeFactory.BottomLeftLatitude = value.Latitude; shapeFactory.BottomLeftLongitude = value.Longitude; }
		}
		public double UnitsPerDegreeLatitude { get { return shapeFactory.UnitsPerDegreeLatitude; } set { shapeFactory.UnitsPerDegreeLatitude = value; } }

		#region Static Folder Initialization and Singleton Constructor

		public static CombinedMapDataFactory Instance
		{
			get
			{
				if (factorySingleton == null)
				{
					factorySingleton = new CombinedMapDataFactory();
					//SetSourceFolders();
				}
				return factorySingleton;
			}
		}
		~CombinedMapDataFactory()
		{
			foreach (CombinedMapData cmd in previouslyCreatedTerrain)
			{
				if (cmd != null && cmd.TerrainShape != null)
				{
					cmd.TerrainShape.Dispose();
				}
			}
		}
		#endregion
		

		public CombinedMapData CreateEmptyMapAtLocation(LatLong location, double elevation, double delta)
		{
			CombinedMapData newMap = new CombinedMapData();
			newMap.ShapeDelta = delta;
			newMap.BottomLeftPosition = location;
			newMap.TileResolution = textureFactory.CalculateTileResolution(delta, elevation);
			return newMap;
		}


		public void RetrieveOrUpdateMapTerrain(CombinedMapData newMap)
		{
			CombinedMapData foundInList = previouslyCreatedTerrain.Find(newMap.IsSameShape);
			if (foundInList != null)
			{
				newMap.TerrainShape = foundInList.TerrainShape;
			}
			else
			{
				UpdateMapTerrain(newMap);
				previouslyCreatedTerrain.Add(newMap);
			}
		}

		public void UpdateMapTexture(CombinedMapData mapToUpdate, double elevation)
		{
			if(!textureFactory.FactoryBusy)
				textureFactory.GetMap(mapToUpdate, elevation);
		}


		public void UpdateMapTerrain(CombinedMapData mapToUpdate)
		{
			Shape shape = null;
			if (!UseTerrainData)
				shape = shapeFactory.GenerateNullShape();
			else
			{
				string filename = ShapeHGTFactory.CalculateFilenameFromLatLong(BottomLeftLocation);
				shapeFactory.Filename = filename;
				try
				{
					shape = shapeFactory.ReadShapeFromFile();
				}
				catch (System.IO.FileNotFoundException)
				{
					shape = shapeFactory.GenerateNullShape();
				}
			}
			Float3 location = new Float3((float)(BottomLeftLocation.Longitude * UnitsPerDegreeLatitude), 0, (float)(BottomLeftLocation.Latitude * UnitsPerDegreeLatitude));
			shape.Location = location.AsVector3();
			mapToUpdate.TerrainShape = shape;
		}
	}
}