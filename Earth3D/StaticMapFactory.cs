using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;


namespace Direct3DLib
{
	public class StaticMapFactory
	{
		#region Singleton Constructor
		private static StaticMapFactory instance;
		private StaticMapFactory() { }
		public static StaticMapFactory Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new StaticMapFactory();
				}
				return instance;
			}
		}
		#endregion

		public const int MIN_TEXTURE_SIZE = 16;
		private MapWebAccessor webAccessor = new MapWebAccessor();
		private MapFileAccessor fileAccessor = new MapFileAccessor();
		private NullImage nullImage = new NullImage();

		private int initialZoomLevel = 0;
		private int StandardZoomLevel = 9;

		private bool automaticallyDownloadMaps = false;
		public bool AutomaticallyDownloadMaps
		{
			get { return automaticallyDownloadMaps; }
			set { automaticallyDownloadMaps = value; }
		}

		public Image GetTiledImage(LatLong bottomLeftLocation, int desiredZoomLevel, int logDelta, out int actualZoomLevel)
		{
			initialZoomLevel = desiredZoomLevel;
			nullImage.Text = "bottomLeft: " + bottomLeftLocation + "\ndesiredZoom: " + desiredZoomLevel;
			return RecursivelyGetTiledImage(bottomLeftLocation, desiredZoomLevel, logDelta, out actualZoomLevel);
		}

		private Image RecursivelyGetTiledImage(LatLong bottomLeftLocation, int desiredZoomLevel, int logDelta, out int actualZoomLevel)
		{
			if (desiredZoomLevel < 0)
			{
				actualZoomLevel = -1;
				return nullImage.ImageClone;
			}
			int minZoomLevel = CalculateZoomFromLogDelta(logDelta);
			if (minZoomLevel == desiredZoomLevel)
			{
				return GetImageFromSource(bottomLeftLocation, desiredZoomLevel, logDelta, out actualZoomLevel);
			}
			else if (minZoomLevel > desiredZoomLevel)
			{
				return CropFromLargerImage(bottomLeftLocation, desiredZoomLevel, logDelta, out actualZoomLevel);
			}
			else
			{
				return StitchFromImageTiles(bottomLeftLocation, desiredZoomLevel, logDelta, out actualZoomLevel);
			}
		}


		private Image CropFromLargerImage(LatLong bottomLeftLocation, int desiredZoomLevel, int logDelta, out int actualZoomLevel)
		{
			double delta = Math.Pow(2.0,logDelta+1);
			LatLong newLocation = EarthProjection.CalculateNearestLatLongAtDelta(bottomLeftLocation, delta,false);
			using (Image image = RecursivelyGetTiledImage(newLocation, desiredZoomLevel, logDelta + 1,out actualZoomLevel))
			{
				float width = image.Width / 2;
				if (width < MIN_TEXTURE_SIZE) width = MIN_TEXTURE_SIZE;
				float height = image.Height / 2;
				if (height < MIN_TEXTURE_SIZE) height = MIN_TEXTURE_SIZE;
				float yOffset = height;
				if (bottomLeftLocation.Latitude > newLocation.Latitude) yOffset = 0;
				float xOffset = 0;
				if (bottomLeftLocation.Longitude > newLocation.Longitude) xOffset = width;
				Image ret = ImageConverter.CropImage(image, new RectangleF(xOffset, yOffset, width, height));
				return ret;
			}
		}

		private Image StitchFromImageTiles(LatLong bottomLeftLocation, int desiredZoomLevel, int logDelta, out int actualZoomLevel)
		{
			int nTiles = 2;
			Image[] imageTiles = new Image[nTiles * nTiles];
			int newLogDelta = logDelta - 1;
			double delta = Math.Pow(2.0, newLogDelta);
			actualZoomLevel = desiredZoomLevel;
			for (int i = 0; i < nTiles; i++)
			{
				for (int k = 0; k < nTiles; k++)
				{
					LatLong tileLocation = new LatLong(bottomLeftLocation.Latitude + delta * i, bottomLeftLocation.Longitude + delta * k);
					imageTiles[(nTiles - i - 1) * nTiles + k] = RecursivelyGetTiledImage(tileLocation,desiredZoomLevel,newLogDelta, out actualZoomLevel);
				}
			}
			Image ret = ImageConverter.StitchImages(imageTiles,nTiles,nTiles);
			foreach (Image image in imageTiles)
				if (image != null) image.Dispose();
			return ret;
		}


		private Image GetImageFromSource(LatLong bottomLeftLocation, int desiredZoomLevel, int logDelta, out int actualZoomLevel)
		{
			actualZoomLevel = desiredZoomLevel;
			int imageDelta = CalculateLogDeltaFromZoom(desiredZoomLevel);
			LatLong centreLocation = CalculateCentreLocation(bottomLeftLocation, imageDelta);
			MapDescriptor d = new MapDescriptor(centreLocation.Latitude, centreLocation.Longitude, desiredZoomLevel);
			Image image = GetImageFromFile(d);
			if (image == null)
			{
				FetchImageFromWeb(d);
				return RecursivelyGetTiledImage(bottomLeftLocation, desiredZoomLevel - 1, logDelta, out actualZoomLevel);
			}
			return image;
		}

		private void FetchImageFromWeb(MapDescriptor descriptor)
		{
			if (descriptor.ZoomLevel == initialZoomLevel)
			{
				if (AutomaticallyDownloadMaps)
					webAccessor.FetchAndSaveImageInNewThread(descriptor);
			}
		}

		private int CalculateZoomFromLogDelta(int logDelta)
		{
			return StandardZoomLevel - logDelta;
		}

		private int CalculateLogDeltaFromZoom(int zoomLevel)
		{
			return StandardZoomLevel - zoomLevel;
		}

		private LatLong CalculateCentreLocation(LatLong bottomLeftLocation, int logDelta)
		{
			double delta = Math.Pow(2.0, logDelta);
			double lat = bottomLeftLocation.Latitude + delta/2.0;
			double lng = bottomLeftLocation.Longitude + delta/2.0;
			return new LatLong(lat, lng);
		}

		public Image GetImageFromFile(MapDescriptor description)
		{
			nullImage.Text += "\n" + description;
			description.MapState = MapDescriptor.MapImageState.Partial;
			using (Image image = fileAccessor.GetImage(description))
			{
				if (image == null) return null;
				RectangleF bounds = EarthProjection.CalculateImageBoundsAtLatitude(image.Width, image.Height, description.Latitude);
				Image ret = ImageConverter.CropImage(image, bounds);
				return ret;
			}
		}

	}

	public class EarthProjection
	{
		private static double XScale = 256.0 / 360.0;

		public static double GetYScaleAtLatitude(double latitude)
		{
			double sec = 1 / Math.Cos(Math.Abs(latitude) * Math.PI / 180.0);
			double ys = XScale * sec;
			if (ys > 1) Console.WriteLine("Extreme Latitude: " + latitude + ", " + ys);
			return ys;
		}

		public static double GetXScaleAtLatitude(double latitude)
		{
			return XScale;
		}

		public static RectangleF CalculateImageBoundsAtLatitude(int imageWidth, int imageHeight, double latitude)
		{
			double w = (double)(imageWidth);
			double h = (double)(imageHeight);
			double xs = GetXScaleAtLatitude(latitude);
			double ys = GetYScaleAtLatitude(latitude);
			double width = xs * w;
			double height = ys * h;
			double left = (w - width) / 2.0;
			double top = Math.Floor((h - height) / 2.0);
			return new RectangleF((float)left, (float)top, (float)width, (float)height);
		}

		public static LatLong CalculateNearestLatLongAtDelta(LatLong latLong, double delta)
		{
			return CalculateNearestLatLongAtDelta(latLong, delta, true);
		}

		public static LatLong CalculateNearestLatLongAtDelta(LatLong latLong, double delta, bool roundToNearest)
		{
			double dLat = latLong.Latitude / delta;
			double dLng = latLong.Longitude / delta;
			if (!roundToNearest)
			{
				dLat = Math.Floor(dLat);
				dLng = Math.Floor(dLng);
			}
			long lat = Convert.ToInt64(dLat);
			long lng = Convert.ToInt64(dLng);
			LatLong ret = new LatLong((double)lat * delta, (double)lng * delta);
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
	}
}
