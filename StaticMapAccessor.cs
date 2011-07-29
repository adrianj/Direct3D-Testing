using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using System.IO;

namespace Direct3DLib
{
	public class StaticMapAccessor
	{
		public const string GOOGLE_MAP_URL = "http://maps.googleapis.com/maps/api/staticmap?";
		public const string DELIM = "&";
		public const string CODE_ZOOM = "zoom=";
		public const string CODE_SIZE = "size=";
		public const string CODE_CENTRE = "center=";
		public const string CODE_SENSOR = "sensor=";
		public const string CODE_MAPTYPE = "maptype=";
		public const int MAX_TILE_RES = 4;
		public const int DEFAULT_ZOOM_LEVEL = 10;

		private Image nullImage;

		public enum WebError {None, Forbidden, NotFound, Other};
		private WebError mostRecentError = WebError.None;

		private StaticMapDescriptor mapDescriptor = new StaticMapDescriptor(-36.825, 174.75, DEFAULT_ZOOM_LEVEL);

		public double CentreLatitude { get { return mapDescriptor.Latitude; } set { if (value <= 90 && value >= -90) mapDescriptor.Latitude = value; } }

		public double CentreLongitude { get { return mapDescriptor.Longitude; } set { if (value <= 180 && value >= -180) mapDescriptor.Longitude = value; } }

		private Size imageSize = new Size(512, 512);
		public Size ImageSize { get { return imageSize; } set { if (value.Height <= 640 && value.Width <= 640) imageSize = value; nullImage = null; } }

		public int ZoomLevel { get { return mapDescriptor.ZoomLevel; } set { if (value >= 0 && value <= 21) mapDescriptor.ZoomLevel = value; } }

		private int tileResolution = 0;
		public int TileResolution { get { return tileResolution; } set { if (value > MAX_TILE_RES) tileResolution = MAX_TILE_RES; else tileResolution = value; } }

		public enum MapTypes { roadmap, satellite, terrain, hybrid };
		private MapTypes mapType = MapTypes.hybrid;
		public MapTypes MapType { get { return mapType; } set { mapType = value; } }

		private string url;
		public string URL { get { return url; } }

		public override string ToString()
		{
			return mapDescriptor.ToString();
		}

		// An example URL
		private const string example = "http://maps.googleapis.com/maps/api/staticmap?center=-36.825000,174.790000&size=512x512&zoom=14&maptype=hybrid&sensor=false";


		public Image DownloadImageSet()
		{
			mostRecentError = WebError.None;
			//double startLat = CentreLatitude;
			//double startLong = CentreLongitude;
			//int startZoom = ZoomLevel;
			//ZoomLevel = startZoom + TileResolution;
			//double delta = ConvertZoomLevelToDelta(ZoomLevel);
			int nTiles = CalculateNumberOfTiles();
			List<StaticMapDescriptor> allMaps = GetMapDescriptors();
			Image[] images = new Image[nTiles * nTiles];
			for (int i = 0; i < allMaps.Count; i++)
			{
				Image image = GetImage(allMaps[i]);
				RectangleF rect = CalculateBounds(image);
				image = CropImage(image, rect);
				images[i] = image;
			}
			Image ret;
			if(tileResolution >= 0)
				ret = ImageConverter.StitchImages(images, nTiles, nTiles);
			else
				ret = SelectCentreOfImage(images[0]);
			//ZoomLevel = startZoom;
			//CentreLatitude = startLat;
			//CentreLongitude = startLong;
			return  ret;
		}

		public List<StaticMapDescriptor> GetMapDescriptors()
		{
			double startLat = CentreLatitude;
			double startLong = CentreLongitude;
			int zoom = ZoomLevel + TileResolution;
			double delta = ConvertZoomLevelToDelta(zoom);
			int nTiles = CalculateNumberOfTiles();
			List<StaticMapDescriptor> descriptors = new List<StaticMapDescriptor>();
			for (int i = 0; i < nTiles; i++)
			{
				double xOffset = (double)i - (double)nTiles / 2.0 + 0.5;
				double latitude = startLat - xOffset * delta;
				for (int k = 0; k < nTiles; k++)
				{
					double yOffset = (double)k - (double)nTiles / 2.0 + 0.5;
					double longitude = startLong + yOffset * delta;
					StaticMapDescriptor descriptor = new StaticMapDescriptor(latitude, longitude, zoom);
					descriptors.Add(descriptor);
				}
			}
			return descriptors;
		}

		private Image SelectCentreOfImage(Image image)
		{
			float bounds = (float)Math.Pow(2.0, tileResolution);
			RectangleF rect = new RectangleF(bounds / 2 * image.Width, bounds / 2 * image.Height, bounds * image.Width, bounds * image.Height);
			return CropImage(image, rect);
		}

		private int CalculateNumberOfTiles()
		{
			int n = (int)Math.Pow(2.0,tileResolution);
			if (n < 1) n = 1;
			return n;
		}

		private RectangleF CalculateBounds(Image ret)
		{
			double xScale = 100.0 / 140.63;
			double yScale = 100.0 / 112.5;
			double xOffset = (1.0 - xScale) / 2.0;
			double yOffset = (1.0 - yScale) / 2.0;
			float left = (float)(xOffset * (double)ret.Width);
			float top = (float)(yOffset * (double)ret.Height);
			float width = (float)(xScale * (double)ret.Width);
			float height = (float)(yScale * (double)ret.Height);
			return new RectangleF(left, top, width, height);
		}

		private Image CropImage(Image image, RectangleF rect)
		{
			Bitmap bmp = new Bitmap(image);
			return bmp.Clone(rect, bmp.PixelFormat);
		}
		public static int ConvertDeltaToZoomLevel(double delta)
		{
			int zoom = -(int)Math.Log(delta, 2);
			zoom += 9;
			return zoom;
		}

		public static double ConvertZoomLevelToDelta(int zoom)
		{
			double delta = Math.Pow(2, 9 - zoom);
			return delta;
		}

		public Image GetImage(StaticMapDescriptor descriptor)
		{
			Image image = GetImageFromFile(descriptor);
			if (image != null) return image;
			image = GetImageFromWeb(descriptor);
			if (image != null) return image;
			return NullImage;
		}

		private Image GetImageFromFile(StaticMapDescriptor descriptor)
		{
			string filename = CalculateFilename(descriptor);
			//Console.WriteLine("Fetching map: " + filename);
			if (File.Exists(filename))
				return Bitmap.FromFile(filename);
			return null;
		}

		private Image GetImageFromWeb(StaticMapDescriptor descriptor)
		{
			if (mostRecentError == WebError.Forbidden)
				return null;
			try
			{
				Image image = null;
				url = ConstructURL(descriptor);
				string filename = CalculateFilename(descriptor);
				Console.WriteLine("Fetching map from web: " + filename);
				using (Stream stream = OpenWebStream(url))
					image = Image.FromStream(stream);
				image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
				return image;
			}
			catch (WebException webEx) { HandleWebException(webEx); }
			catch (Exception ex) { HandleOtherException(ex); }
			return null;
		}


		public string ConstructURL(StaticMapDescriptor descriptor)
		{
			string ret = StaticMapAccessor.GOOGLE_MAP_URL;
			ret += StaticMapAccessor.CODE_CENTRE + descriptor.Latitude.ToString("F8") + "," + descriptor.Longitude.ToString("F8");
			ret += StaticMapAccessor.DELIM + StaticMapAccessor.CODE_SIZE + ImageSize.Width + "x" + ImageSize.Height;
			ret += StaticMapAccessor.DELIM + StaticMapAccessor.CODE_ZOOM + descriptor.ZoomLevel;
			ret += StaticMapAccessor.DELIM + StaticMapAccessor.CODE_MAPTYPE + MapType;
			ret += StaticMapAccessor.DELIM + StaticMapAccessor.CODE_SENSOR + "false";
			return ret;
		}

		private string CalculateFilename(StaticMapDescriptor descriptor)
		{
			string folder = "zoom=" + descriptor.ZoomLevel;
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			string filename = folder + Path.DirectorySeparatorChar + descriptor;
			return filename;
		}

		private Image NullImage
		{
			get
			{
				if (nullImage == null)
				{
					nullImage = new Bitmap(imageSize.Width, imageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					ClearNullImage();
				}
				return nullImage;
			}
		}

		private void ClearNullImage()
		{
			using (Graphics g = Graphics.FromImage(NullImage))
				g.FillRectangle(Brushes.White, 0, 0, imageSize.Width, imageSize.Height);
		}

		private void WriteTextOnNullImage(string text)
		{
			ClearNullImage();
			using (Graphics g = Graphics.FromImage(NullImage))
				g.DrawString(text, SystemFonts.DefaultFont, Brushes.DarkBlue, (float)(imageSize.Width) / 2.0f, (float)(imageSize.Height) / 2.0f);
		}

		private Stream OpenWebStream(string url)
		{
			HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
			WebResponse response = httpRequest.GetResponse();
			Stream stream = response.GetResponseStream();
			return stream;
		}


		private void HandleWebException(WebException webEx)
		{
			if (webEx.Status == WebExceptionStatus.ProtocolError)
			{
				HttpWebResponse response = (HttpWebResponse)webEx.Response;
				WriteTextOnNullImage("" + response.StatusCode);
				mostRecentError = WebError.Forbidden;
			}
			else
			{
				WriteTextOnNullImage("" + webEx.Status);
				mostRecentError = WebError.Other;
			}

		}

		private void HandleOtherException(Exception ex)
		{
			WriteTextOnNullImage("" + ex);
			mostRecentError = WebError.Other;
		}

	}

	public class StaticMapDescriptor
	{
		public double Latitude;
		public double Longitude;
		public int ZoomLevel;
		public StaticMapDescriptor(double lat, double lng, int zoom)
		{
			Latitude = lat;
			Longitude = lng;
			ZoomLevel = zoom;
		}

		public override string ToString()
		{
			string ret = "googlemap_";
			ret += StaticMapAccessor.DELIM + StaticMapAccessor.CODE_ZOOM + ZoomLevel;
			ret += StaticMapAccessor.CODE_CENTRE + Latitude.ToString("F8") + "," + Longitude.ToString("F8");
			ret += ".png";
			return ret;
		}

	}
}
