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

		private MapWebAccessor googleAccessor = new MapWebAccessor();
		private StaticMapFactory mapFactory = StaticMapFactory.Instance;
		private int zoomLevel { get { return googleAccessor.ZoomLevel; } set { googleAccessor.ZoomLevel = value; } }
		private List<CombinedMapData> workerQueue = new List<CombinedMapData>();
		private BackgroundWorker worker = new BackgroundWorker();
		public bool FactoryBusy { get { return worker.IsBusy; } }

		public void GetMap(CombinedMapData mapToUpdate, double elevation)
		{
			int tileRes = CalculateTileResolution(mapToUpdate.ShapeDelta, elevation);
			mapToUpdate.TileResolution = tileRes;
			//Image image = GetImage(mapToUpdate);
			int logDelta = (int)Math.Log(mapToUpdate.ShapeDelta, 2.0);
			Image image = mapFactory.GetTiledImage(mapToUpdate.BottomLeftPosition, GetZoomFromElevation(elevation), logDelta);
			mapToUpdate.TextureImage = image;
		}


		private Image GetImage(CombinedMapData mapToUpdate)
		{
			return mapFactory.GetTiledImage(mapToUpdate.BottomLeftPosition, 12, -3);
			/*
			string filename = CalculateFilename(mapToUpdate);
			Image ret = null;
			if (ret != null)
				return ret;
			ret = GetImageFromGoogle(mapToUpdate);
			return ret;
			 */
		}

		public int CalculateTileResolution(double delta, double elevation)
		{
			int elevationZoom = GetZoomFromElevation(elevation);
			if (elevationZoom > EarthTiles.MaxGoogleZoom) elevationZoom = EarthTiles.MaxGoogleZoom;
			int shapeZoom = (int)Math.Log(delta, 2.0);
			int tileRes = shapeZoom + elevationZoom - 8;
			if (tileRes > MapWebAccessor.MAX_TILE_RES) tileRes = MapWebAccessor.MAX_TILE_RES;
			return tileRes;
		}

		private string CalculateFilename(CombinedMapData mapToUpdate)
		{
			string folder = Properties.Settings.Default.MapTextureFolder + Path.DirectorySeparatorChar +
				"" + mapToUpdate.ShapeDelta + "_" + zoomLevel;
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			string imageFilename = folder+Path.DirectorySeparatorChar+
				"latlong_" + mapToUpdate.BottomLeftPosition + ".jpg";
			return imageFilename;
		}

		private Image GetImageFromFile(string filename)
		{
			if (File.Exists(filename))
				return Bitmap.FromFile(filename);
			return null;
		}

		private Image GetImageFromGoogle(CombinedMapData mapToUpdate)
		{
			googleAccessor.ZoomLevel = MapWebAccessor.ConvertDeltaToZoomLevel(mapToUpdate.ShapeDelta);
			googleAccessor.TileResolution = mapToUpdate.TileResolution;
			LatLong centre = CalculateCentreLatLong(mapToUpdate.BottomLeftPosition, mapToUpdate.ShapeDelta);
			googleAccessor.CentreLatitude = centre.Latitude;
			googleAccessor.CentreLongitude = centre.Longitude;
			Image ret = googleAccessor.DownloadImageSet();
			return ret;
		}

		public static int GetZoomFromElevation(double elevation)
		{
			if (elevation <= 0)
				return EarthTiles.MaxGoogleZoom;
			double zoom = 25.0 - Math.Log(elevation, 2.0);
			int z = (int)zoom;
			return z;
		}

		private LatLong CalculateCentreLatLong(LatLong bottomLeft, double delta)
		{
			LatLong ret = new LatLong();
			ret.Latitude = bottomLeft.Latitude + delta / 2;
			ret.Longitude = bottomLeft.Longitude + delta / 2;
			return ret;
		}



	}
}
