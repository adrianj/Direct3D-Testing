using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Direct3DLib
{
	public class CombinedMapDataFactory
	{

		private static CombinedMapDataFactory factorySingleton;
		private ShapeHGTFactory shapeFactory = new ShapeHGTFactory();
		private StaticMapFactory textureFactory = StaticMapFactory.Instance;
		private List<CombinedMapData> previouslyCreatedTerrain = new List<CombinedMapData>();
		
		public bool UseTerrainData = true;
		public bool AutomaticallyDownloadMaps { get { return textureFactory.AutomaticallyDownloadMaps; } set {
			textureFactory.AutomaticallyDownloadMaps = value; } }

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
				}
				return factorySingleton;
			}
		}
		~CombinedMapDataFactory()
		{
			foreach (CombinedMapData cmd in previouslyCreatedTerrain)
			{
				if (cmd != null)
				{
					cmd.Dispose();
				}
			}
		}
		#endregion
		

		public CombinedMapData CreateEmptyMapAtLocation(LatLong location, double elevation, double delta)
		{
			CombinedMapData newMap = new CombinedMapData();
			newMap.ShapeDelta = delta;
			newMap.BottomLeftPosition = location;
			newMap.ZoomLevel = EarthProjection.GetZoomFromElevation(elevation);
			return newMap;
		}


		public void RetrieveOrUpdateMapTerrain(CombinedMapData newMap)
		{
			CombinedMapData foundInList = previouslyCreatedTerrain.Find(newMap.IsSameShape);
			if (foundInList != null)
			{
				foundInList.CopyShapeTo(newMap);
			}
			else
			{
				UpdateMapTerrain(newMap);
				previouslyCreatedTerrain.Add(newMap);
			}
		}

		public void UpdateMapTexture(CombinedMapData mapToUpdate, double elevation)
		{
			int zoomLevel = EarthProjection.GetZoomFromElevation(elevation);
			if (zoomLevel > EarthTiles.MaxGoogleZoom) zoomLevel = EarthTiles.MaxGoogleZoom;
			mapToUpdate.ZoomLevel = zoomLevel;
			int logDelta = (int)Math.Log(mapToUpdate.ShapeDelta, 2.0);
			mapToUpdate.TextureImage = textureFactory.GetTiledImage(mapToUpdate.BottomLeftPosition, mapToUpdate.ZoomLevel, logDelta);
			//if(!textureFactory.FactoryBusy)
			//	textureFactory.GetMap(mapToUpdate, elevation);
		}


		public void UpdateMapTerrain(CombinedMapData mapToUpdate)
		{
			Shape shape = null;
			if (!UseTerrainData)
				shape = shapeFactory.GenerateNullShape();
			else
			{
				Console.WriteLine("Getting terrain");
				string filename = ShapeHGTFactory.CalculateFilenameFromLatLong(BottomLeftLocation);
				Console.WriteLine("file: " + filename);
				shapeFactory.Filename = filename;
				try
				{
					shape = shapeFactory.ReadShapeFromFile();
					Console.WriteLine("success");
				}
				catch (System.IO.FileNotFoundException)
				{
					shape = shapeFactory.GenerateNullShape();
					Console.WriteLine("fail");
				}
			}
			Float3 location = new Float3((float)(BottomLeftLocation.Longitude * UnitsPerDegreeLatitude), 0, (float)(BottomLeftLocation.Latitude * UnitsPerDegreeLatitude));
			shape.Location = location.AsVector3();
			mapToUpdate.CopyShapeFrom(shape);
			//mapToUpdate.TerrainShape = shape;
		}
	}
}
