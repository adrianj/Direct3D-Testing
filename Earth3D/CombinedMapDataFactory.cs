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
		private MapTextureFactory textureFactory = MapTextureFactory.Instance;
		private List<CombinedMapData> previouslyCreatedMaps = new List<CombinedMapData>();

		public double Delta
		{
			get { return shapeFactory.LongitudeDelta; }
			set { shapeFactory.LongitudeDelta = value; shapeFactory.LatitudeDelta = value; }
		}
		public LatLong BottomLeftLocation
		{
			get { return new LatLong(shapeFactory.BottomLeftLatitude, shapeFactory.BottomLeftLongitude); }
			set { shapeFactory.BottomLeftLatitude = value.latitude; shapeFactory.BottomLeftLongitude = value.longitude; }
		}
		public double UnitsPerDegreeLatitude { get { return shapeFactory.UnitsPerDegreeLatitude; } set { shapeFactory.UnitsPerDegreeLatitude = value; } }

		public double Elevation { get { return textureFactory.Elevation; } set { textureFactory.Elevation = value; } }

		private CombinedMapData newMap;

		#region Static Folder Initialization and Singleton Constructor
		public static void SetSourceFolders()
		{
			SetMapTerrainFolder(AskForFolder("Please provide folder with terrain data (.hgt files)",Properties.Settings.Default.MapTerrainFolder));
			SetMapTextureFolder(AskForFolder("Please provide folder with terrain texture images (.jpg files)", Properties.Settings.Default.MapTextureFolder));
		}

		public static void SetMapTerrainFolder(string folderPath)
		{
			Properties.Settings.Default.MapTerrainFolder = folderPath;
			Properties.Settings.Default.Save();
		}

		public static void SetMapTextureFolder(string folderPath)
		{
			Properties.Settings.Default.MapTextureFolder = folderPath;
			Properties.Settings.Default.Save();
		}

		private static string AskForFolder(string requestMessage, string rootPath)
		{
			System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
			fbd.Description = requestMessage;
			fbd.SelectedPath = rootPath;
			fbd.ShowDialog();
			return fbd.SelectedPath;
		}

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
		#endregion
		
		public CombinedMapData CreateCombinedMapData()
		{
			newMap = CreateEmptyMapAtLocation(BottomLeftLocation, Elevation, Delta);
			return RetrieveOrCreateMap(newMap);
		}

		public CombinedMapData CreateEmptyMapAtLocation(LatLong location, double elevation, double delta)
		{
			CombinedMapData newMap = new CombinedMapData();
			newMap.ShapeDelta = delta;
			newMap.BottomLeftPosition = location;
			newMap.TileResolution = textureFactory.CalculateTileResolution(delta, elevation);
			return newMap;
		}


		private CombinedMapData RetrieveOrCreateMap(CombinedMapData newMap)
		{
			CombinedMapData foundInList = previouslyCreatedMaps.Find(newMap.IsSameShape);
			if (foundInList != null)
				return foundInList;
			else
			{
				UpdateMapTerrain(newMap);
				UpdateMapTexture(newMap);
				previouslyCreatedMaps.Add(newMap);
				return newMap;
			}
		}

		public void UpdateMapTexture(CombinedMapData mapToUpdate)
		{
			if(!textureFactory.FactoryBusy)
				textureFactory.GetMap(mapToUpdate);
		}


		private void UpdateMapTerrain(CombinedMapData mapToUpdate)
		{
			string filename = ShapeHGTFactory.CalculateFilenameFromLatLong(BottomLeftLocation);
			shapeFactory.Filename = filename;
			Shape shape = null;
			try
			{
				shape = shapeFactory.ReadShapeFromFile();
			}
			catch (System.IO.FileNotFoundException)
			{
				shape = shapeFactory.GenerateNullShape();
			}
			Float3 location = new Float3((float)(BottomLeftLocation.longitude * UnitsPerDegreeLatitude), 0, (float)(BottomLeftLocation.latitude * UnitsPerDegreeLatitude));
			shape.Location = location.AsVector3();
			mapToUpdate.TerrainShape = shape;
		}
	}
}
