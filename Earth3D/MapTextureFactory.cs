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
		private int zoomLevel { get { return googleAccessor.ZoomLevel; } set { googleAccessor.ZoomLevel = value; } }
		private List<CombinedMapData> workerQueue = new List<CombinedMapData>();
		private BackgroundWorker worker = new BackgroundWorker();
		public bool FactoryBusy { get { return worker.IsBusy; } }

		public void GetMap(CombinedMapData mapToUpdate, double elevation)
		{
			int tileRes = CalculateTileResolution(mapToUpdate.ShapeDelta, elevation);
			mapToUpdate.TileResolution = tileRes;
			Image image = GetImage(mapToUpdate);
			mapToUpdate.TextureImage = image;
		}


		private Image GetImage(CombinedMapData mapToUpdate)
		{
			string filename = CalculateFilename(mapToUpdate);
			Image ret = null;
			if (ret != null)
				return ret;
			ret = GetImageFromGoogle(mapToUpdate);
			return ret;
		}

		public int CalculateTileResolution(double delta, double elevation)
		{
			int elevationZoom = GetZoomFromElevation(elevation) - 8;
			if (elevationZoom > EarthTiles.MAX_ELEVATION_ZOOM) elevationZoom = EarthTiles.MAX_ELEVATION_ZOOM;
			int shapeZoom = (int)Math.Log(delta, 2.0);
			int final = shapeZoom + elevationZoom;
			if (final > StaticMapAccessor.MAX_TILE_RES) final = StaticMapAccessor.MAX_TILE_RES;
			return final;
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
			googleAccessor.ZoomLevel = StaticMapAccessor.ConvertDeltaToZoomLevel(mapToUpdate.ShapeDelta);
			googleAccessor.TileResolution = mapToUpdate.TileResolution;
			LatLong centre = CalculateCentreLatLong(mapToUpdate.BottomLeftPosition, mapToUpdate.ShapeDelta);
			googleAccessor.CentreLatitude = centre.latitude;
			googleAccessor.CentreLongitude = centre.longitude;
			Image ret = googleAccessor.DownloadImageSet();
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
