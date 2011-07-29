using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.ComponentModel;

namespace Direct3DLib
{
	public class MapTextureFactory
	{
		#region Static Methods and Constructor
		private static MapTextureFactory singleton;

		public static MapTextureFactory Instance
		{
			get
			{
				if (singleton == null)
					singleton = new MapTextureFactory();
				return singleton;
			}
		}

		private MapTextureFactory() { }
		#endregion

		private StaticMapAccessor googleAccessor = new StaticMapAccessor();
		private string imageFilename = "";
		public CombinedMapData MapToAccess {get;set;}
		private double elevation = 2000;
		private int zoomLevel { get { return googleAccessor.ZoomLevel; } set { googleAccessor.ZoomLevel = value; } }
		public double Elevation { get { return elevation; } set { elevation = value; zoomLevel = GetZoomFromElevation(value); } }
		private List<CombinedMapData> workerQueue = new List<CombinedMapData>();
		private BackgroundWorker worker = new BackgroundWorker();
		public bool FactoryBusy { get { return worker.IsBusy; } }

		public void GetMap(CombinedMapData mapToUpdate)
		{
			//MapToAccess = mapToUpdate;
			GetMapInSeperateThread(mapToUpdate);
		}

		private void GetMapInSeperateThread(CombinedMapData mapToUpdate)
		{
			if (AlreadyProcessingThisMap(mapToUpdate)) return;
			MapToAccess = mapToUpdate;
			worker.DoWork += (o, e) =>
			{
				Console.WriteLine("queue count: " + workerQueue.Count);
				int tileRes = CalculateTileResolution();
				MapToAccess.TileResolution = tileRes;
				Image image = GetImage();
				MapToAccess.TextureImage = image;
				Console.WriteLine("new queue count: " + workerQueue.Count);
			};
			if (!worker.IsBusy)
				worker.RunWorkerAsync();
		}

		private bool AlreadyProcessingThisMap(CombinedMapData mapToUpdate)
		{
			if (MapToAccess == null) return false;
			Console.Write(MapToAccess.IsSameTexture(mapToUpdate)+", ");
			Console.WriteLine(mapToUpdate.IsSameTexture(MapToAccess));
			if (!worker.IsBusy) return false;
			if (mapToUpdate.IsSameTexture(MapToAccess)) return true;
			return false;
		}


		private Image GetImage()
		{
			CalculateFilename();
			Image ret = null;
			//ret = GetImageFromFile();
			if (ret != null)
				return ret;
			ret = GetImageFromGoogle();
			return ret;
		}

		public int CalculateTileResolution(double delta, double elevation)
		{
			int tileRes = googleAccessor.TileResolution;
			double logDelta = Math.Log(delta, 2.0);
			int elevationZoom = GetZoomFromElevation(elevation) - 8;
			if (elevationZoom > EarthTiles.MAX_ELEVATION_ZOOM) elevationZoom = EarthTiles.MAX_ELEVATION_ZOOM;
			int shapeZoom = (int)Math.Log(delta, 2.0);
			int final = shapeZoom + elevationZoom;
			if (final > StaticMapAccessor.MAX_TILE_RES) final = StaticMapAccessor.MAX_TILE_RES;
			return final;
		}
		private int CalculateTileResolution()
		{
			return CalculateTileResolution(MapToAccess.ShapeDelta, Elevation);
		}

		private void CalculateFilename()
		{
			string folder = Properties.Settings.Default.MapTextureFolder + Path.DirectorySeparatorChar +
				"" + MapToAccess.ShapeDelta + "_" + zoomLevel;
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			imageFilename = folder+Path.DirectorySeparatorChar+
				"latlong_"+MapToAccess.BottomLeftPosition+".jpg";
		}

		private Image GetImageFromFile()
		{
			if (File.Exists(imageFilename))
				return Bitmap.FromFile(imageFilename);
			return null;
		}

		private Image GetImageFromGoogle()
		{
			googleAccessor.ZoomLevel = StaticMapAccessor.ConvertDeltaToZoomLevel(MapToAccess.ShapeDelta);
			googleAccessor.TileResolution = MapToAccess.TileResolution;
			LatLong centre = CalculateCentreLatLong(MapToAccess.BottomLeftPosition, MapToAccess.ShapeDelta);
			Console.WriteLine("Image Centre: " + centre+", Zoom: "+googleAccessor.ZoomLevel);
			googleAccessor.CentreLatitude = centre.latitude;
			googleAccessor.CentreLongitude = centre.longitude;
			Image ret = googleAccessor.DownloadImageSet();
			//ret.Save(imageFilename);
			return ret;
		}

		public static int GetZoomFromElevation(double elevation)
		{
			double zoom = 24.0 - Math.Log(elevation, 2.0);
			int z = (int)zoom;
			return z;
		}

		private LatLong CalculateCentreLatLong(LatLong bottomLeft, double delta)
		{
			LatLong ret = new LatLong();
			ret.latitude = bottomLeft.latitude + delta / 2;
			ret.longitude = bottomLeft.longitude + delta / 2;
			return ret;
		}



	}
}
