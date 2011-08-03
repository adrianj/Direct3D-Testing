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
	public class MapWebAccessor
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
		public const int MAX_GOOGLE_ZOOM = 21;
        public static double X_SCALE = 256.0/360.0;
		public static double X_OFFSET = 0;
        public static double Y_SCALE = 256.0/360.0;
		public static double Y_OFFSET = 0;

		private Image nullImage;

		public enum WebError {None, Forbidden, NotFound, Other};
		private WebError mostRecentError = WebError.None;

		private MapDescriptor mapDescriptor = new MapDescriptor(-36.825, 174.75, DEFAULT_ZOOM_LEVEL);

		public double CentreLatitude { get { return mapDescriptor.Latitude; } set { if (value <= 90 && value >= -90) mapDescriptor.Latitude = value; } }
		public double CentreLongitude { get { return mapDescriptor.Longitude; } set { if (value <= 180 && value >= -180) mapDescriptor.Longitude = value; } }

		private Size imageSize = new Size(512, 512);
		public Size ImageSize { get { return imageSize; } set { if (value.Height <= 640 && value.Width <= 640) imageSize = value; nullImage = null; } }

		public int ZoomLevel { get { return mapDescriptor.ZoomLevel; } set { if (value >= 0 && value <= MAX_GOOGLE_ZOOM) mapDescriptor.ZoomLevel = value; } }

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
			int nTiles = CalculateNumberOfTiles();
			List<MapDescriptor> allMaps = GetMapDescriptors();
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
			return  ret;
		}

		public List<MapDescriptor> GetMapDescriptors()
		{
			double startLat = CentreLatitude;
			double startLong = CentreLongitude;
			int zoom = ZoomLevel + TileResolution;
			double delta = ConvertZoomLevelToDelta(zoom);
			int nTiles = CalculateNumberOfTiles();
			List<MapDescriptor> descriptors = new List<MapDescriptor>();
			for (int i = 0; i < nTiles; i++)
			{
				double xOffset = (double)i - (double)nTiles / 2.0 + 0.5;
				double latitude = startLat - xOffset * delta;
				for (int k = 0; k < nTiles; k++)
				{
					double yOffset = (double)k - (double)nTiles / 2.0 + 0.5;
					double longitude = startLong + yOffset * delta;
					MapDescriptor descriptor = new MapDescriptor(latitude, longitude, zoom);
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
			double w = (double)(ret.Width);
			double h = (double)(ret.Height);
			double xs = X_SCALE;
			double sec = 1 / Math.Cos(Math.Abs(CentreLatitude) * Math.PI/180.0);
			double ys = X_SCALE * sec;
			double width = xs * w;
			double height = ys * h;
			double left = (w - width) / 2.0 - X_OFFSET;
			double top = Math.Floor((h - height) / 2.0 - Y_OFFSET);
			return new RectangleF((float)left, (float)top, (float)width, (float)height);
		}

		private Image CropImage(Image image, RectangleF rect)
		{
			Bitmap bmp = new Bitmap(image);
			if (rect.Width > image.Width) rect.Width = image.Width;
			if (rect.X < 0) rect.X = 0;
			if (rect.Height > image.Height) rect.Height = image.Height;
			if (rect.Y < 0) rect.Y = 0;
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

		public Image GetImage(MapDescriptor descriptor)
		{
			Image image = GetImageFromFile(descriptor);
			if (image != null) return image;
			image = GetImageFromWeb(descriptor);
			if (image != null) return image;
			return NullImage;
		}

		private Image GetImageFromFile(MapDescriptor descriptor)
		{
			string filename = CalculateFilename(descriptor);
			if (File.Exists(filename))
			{
				descriptor.MapState = MapDescriptor.MapImageState.Correct;
				return Bitmap.FromFile(filename);
			}
			descriptor.MapState = MapDescriptor.MapImageState.Empty;
			return null;
		}

		private Image GetImageFromWeb(MapDescriptor descriptor)
		{
			if (mostRecentError == WebError.Forbidden)
			{
				descriptor.MapState = MapDescriptor.MapImageState.Empty;
				return null;
			}
			try
			{
				Image image = null;
				url = ConstructURL(descriptor);
				string filename = CalculateFilename(descriptor);
				Console.WriteLine(""+DateTime.Now+": Fetching map from web:\n" + url);
				using (Stream stream = OpenWebStream(url))
					image = Image.FromStream(stream);
				image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
				descriptor.MapState = MapDescriptor.MapImageState.Correct;
				Console.WriteLine("done");
				return image;
			}
			catch (WebException webEx) { HandleWebException(webEx); }
			catch (Exception ex) { HandleOtherException(ex); }
			descriptor.MapState = MapDescriptor.MapImageState.Empty;
			return null;
		}


		private string ConstructURL(MapDescriptor descriptor)
		{
			string ret = MapWebAccessor.GOOGLE_MAP_URL;
			ret += MapWebAccessor.CODE_CENTRE + descriptor.Latitude.ToString("F8") + "," + descriptor.Longitude.ToString("F8");
			ret += MapWebAccessor.DELIM + MapWebAccessor.CODE_SIZE + ImageSize.Width + "x" + ImageSize.Height;
			ret += MapWebAccessor.DELIM + MapWebAccessor.CODE_ZOOM + descriptor.ZoomLevel;
			ret += MapWebAccessor.DELIM + MapWebAccessor.CODE_MAPTYPE + MapType;
			ret += MapWebAccessor.DELIM + MapWebAccessor.CODE_SENSOR + "false";
			return ret;
		}

		private string CalculateFilename(MapDescriptor descriptor)
		{
			string folder = Properties.Settings.Default.MapTextureFolder + Path.DirectorySeparatorChar
			+ "zoom=" + descriptor.ZoomLevel;
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

	public class MapDescriptor
	{
		public enum MapImageState { Empty, Correct, Partial }
		public MapImageState MapState { get; set; }
		private double lat = 0;
		private double lng = 0;
		public double Latitude { get { return lat; } set { lat = Wrap(value, -90, 90); } }
		public double Longitude { get { return lng; } set { lng = Wrap(value, -180, 180); } }
		public int ZoomLevel { get; set; }
		public MapDescriptor(double lat, double lng, int zoom)
		{
			Latitude = lat;
			Longitude = lng;
			ZoomLevel = zoom;
		}

		public override string ToString()
		{
			string ret = "googlemap_";
			ret += MapWebAccessor.DELIM + MapWebAccessor.CODE_ZOOM + ZoomLevel;
			ret += MapWebAccessor.CODE_CENTRE + Latitude.ToString("F8") + "," + Longitude.ToString("F8");
			ret += ".png";
			return ret;
		}

		private double Wrap(double value, double nRange, double pRange)
		{
			double ret = value;
			double diff = pRange - nRange;
			while (ret < nRange)
				ret += diff;
			while (ret > pRange)
				ret -= diff;
			return ret;
		}

	}
}
