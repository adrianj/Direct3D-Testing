using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Direct3DLib
{
	public class CombinedMapDataFactory : IDisposable
	{
		
		private static CombinedMapDataFactory factorySingleton;
		private ShapeHGTFactory shapeFactory = new ShapeHGTFactory();
		private StaticMapFactory textureFactory = StaticMapFactory.Instance;
		private Dictionary<MapDescriptor,Shape> previouslyCreatedTerrain = new Dictionary<MapDescriptor,Shape>();
		
		public bool UseTerrainData = true;
		public bool AutomaticallyDownloadMaps { get { return textureFactory.AutomaticallyDownloadMaps; } set {
			textureFactory.AutomaticallyDownloadMaps = value; } }

		public double UnitsPerDegreeLatitude { get { return shapeFactory.UnitsPerDegreeLatitude; } set { shapeFactory.UnitsPerDegreeLatitude = value; } }

		private Dictionary<MapDescriptor, Shape> TerrainToProcess = new Dictionary<MapDescriptor, Shape>();
		private Dictionary<MapDescriptor, CombinedMapData> TexturesToProcess = new Dictionary<MapDescriptor, CombinedMapData>();
		private BackgroundWorker terrainWorker = new BackgroundWorker();
		private BackgroundWorker textureWorker = new BackgroundWorker();

		public event ShapeChangeEventHandler MapUpdateCompleted;
		private void FireMapUpdateCompletedEvent(ShapeChangeEventArgs e)
		{ if (MapUpdateCompleted != null) MapUpdateCompleted(this, e); }

		#region Static Folder Initialization and Singleton Constructor

		private CombinedMapDataFactory()
		{
			terrainWorker.WorkerSupportsCancellation = true;
			terrainWorker.DoWork += this.UpdateMapTerrainThread;
			terrainWorker.RunWorkerCompleted += (o, e) => { terrainWorker.Dispose(); };
			terrainWorker.RunWorkerAsync();
			textureWorker.DoWork += this.UpdateMapTextureThread;
			textureWorker.RunWorkerCompleted += (o, e) => { textureWorker.Dispose(); };
			textureWorker.RunWorkerAsync();
		}

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
			this.Dispose();
		}

		public void Dispose()
		{
			foreach (KeyValuePair<MapDescriptor,Shape> pair in previouslyCreatedTerrain)
			{
				if (pair.Value != null)
				{
					pair.Value.Dispose();
				}
			}
			terrainWorker.CancelAsync();
		}
		#endregion
		

		public CombinedMapData CreateEmptyMapAtLocation(LatLong location, double elevation, double delta)
		{
			CombinedMapData newMap = new CombinedMapData();
			newMap.ShapeDelta = delta;
			newMap.BottomLeftLocation = location;
			newMap.ZoomLevel = EarthProjection.GetZoomFromElevation(elevation);
			return newMap;
		}


		public void RetrieveOrUpdateMapTerrain(CombinedMapData newMap)
		{
			MapDescriptor md = new MapDescriptor(
				newMap.BottomLeftLocation.Latitude, 
				newMap.BottomLeftLocation.Longitude, 
				newMap.ZoomLevel, 
				newMap.ShapeDelta);
			if(previouslyCreatedTerrain.ContainsKey(md))
			{
				newMap.CopyShapeFrom(previouslyCreatedTerrain[md]);
				FireMapUpdateCompletedEvent(new ShapeChangeEventArgs(newMap, ShapeChangeEventArgs.ChangeAction.Add));
			}
			else
			{
				UpdateMapTerrain(newMap);
				//previouslyCreatedTerrain[md] = newMap;
				//newMap.CopyShapeFrom(shapeFactory.GenerateNullShape());
			}
		}

		public void UpdateMapTexture(CombinedMapData mapToUpdate)
		{
			if (mapToUpdate.ZoomLevel > EarthTiles.MaxGoogleZoom) mapToUpdate.ZoomLevel = EarthTiles.MaxGoogleZoom;
			MapDescriptor desc = mapToUpdate.GetMapDescriptor();
			if (!TexturesToProcess.ContainsKey(desc))
				TexturesToProcess.Add(desc, mapToUpdate);


		}
		private void UpdateMapTextureThread(object o, DoWorkEventArgs e)
		{
			while (!textureWorker.CancellationPending)
			{
				MapDescriptor desc = GetFirstKeyOrNull(TexturesToProcess.Keys);
				if (desc != null)
				{
					CombinedMapData target = TexturesToProcess[desc];
					int logDelta = (int)Math.Log(target.ShapeDelta, 2.0);
					int actualZoom = target.ZoomLevel;
					Image image = textureFactory.GetTiledImage(target.BottomLeftLocation, target.ZoomLevel, logDelta, out actualZoom);
					target.ZoomLevel = actualZoom;
					target.TextureImage = image;
					TexturesToProcess.Remove(desc);
				}
				else
				{
					System.Threading.Thread.Sleep(10);
				}
			}
		}


		public void UpdateMapTerrain(CombinedMapData mapToUpdate)
		{
			MapDescriptor desc = mapToUpdate.GetMapDescriptor();
			if(!TerrainToProcess.ContainsKey(desc))
				TerrainToProcess.Add(desc, mapToUpdate);
		}

		private void UpdateMapTerrainThread(object o, DoWorkEventArgs e)
		{
			while (!terrainWorker.CancellationPending)
			{
				MapDescriptor desc = GetFirstKeyOrNull(TerrainToProcess.Keys);
				if (desc != null)
				{
					Shape target = TerrainToProcess[desc];
					Shape shape = null;
					shapeFactory.BottomLeftLatitude = desc.Latitude;
					shapeFactory.BottomLeftLongitude = desc.Longitude;
					shapeFactory.LatitudeDelta = desc.Delta;
					shapeFactory.LongitudeDelta = desc.Delta;
					if (!UseTerrainData)
						shape = shapeFactory.GenerateNullShape();
					else
					{
						shape = GenerateShapeFromFile(desc);
					}
					Float3 location = new Float3((float)(desc.Longitude * UnitsPerDegreeLatitude), 0, (float)(desc.Latitude * UnitsPerDegreeLatitude));
					shape.Location = location.AsVector3();
					target.CopyShapeFrom(shape);
					//target.Scale = new SlimDX.Vector3(1, 2, 1);
					FireMapUpdateCompletedEvent(new ShapeChangeEventArgs(target, ShapeChangeEventArgs.ChangeAction.Add));
					TerrainToProcess.Remove(desc);
				}
				else
				{
					System.Threading.Thread.Sleep(10);
				}
			}
		}

		private MapDescriptor GetFirstKeyOrNull(ICollection<MapDescriptor> Keys)
		{
			IEnumerator<MapDescriptor> en = Keys.GetEnumerator();
			en.MoveNext();
			return en.Current;
		}

		private Shape GenerateShapeFromFile(MapDescriptor desc)
		{
			long time = Environment.TickCount;
			int logDelta = (int)Math.Log(desc.Delta, 2.0) - EarthTiles.MinLogDelta;
			Shape shape = null;
			try
			{
				string filename = ShapeHGTFactory.CalculateFilenameFromLatLong(new LatLong(desc.Latitude,desc.Longitude));
				using (System.IO.Stream stream = new System.IO.FileStream(filename, System.IO.FileMode.Open))
				{
					shapeFactory.Stream = stream;
					shape = shapeFactory.ReadAndReduceShapeFromFile(logDelta);
				}
			}
			catch (System.IO.FileNotFoundException)
			{
				shape = shapeFactory.GenerateNullShape();
			}
			return shape;
		}

	}
}
