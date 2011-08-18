using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;

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

		private int accessCount = 0;

		private NullImage nullImage = new NullImage();

		public enum WebError {None, Forbidden, NotFound, Other};
		private WebError mostRecentError = WebError.None;

		private Size imageSize = new Size(512, 512);
		public Size ImageSize { get { return imageSize; } set { if (value.Height <= 640 && value.Width <= 640) imageSize = value; nullImage = null; } }

		public enum MapTypes { roadmap, satellite, terrain, hybrid };
		private MapTypes mapType = MapTypes.hybrid;
		public MapTypes MapType { get { return mapType; } set { mapType = value; } }

		private List<MapDescriptor> downloadInProgress = new List<MapDescriptor>();


		// An example URL
		private const string example = "http://maps.googleapis.com/maps/api/staticmap?center=-36.825000,174.790000&size=512x512&zoom=14&maptype=hybrid&sensor=false";

		public Image GetImage(MapDescriptor descriptor)
		{
			nullImage.Text = "" + descriptor;
			Image image = GetImageFromWeb(descriptor);
			if (image != null) return image;
			return nullImage.ImageClone;
		}

		public void FetchAndSaveImageInNewThread(MapDescriptor descriptor)
		{
			if (downloadInProgress.Contains(descriptor))
			{
				return;
			}
			if (mostRecentError == WebError.Forbidden)
			{
				return;
			}
			downloadInProgress.Add(descriptor);
			accessCount++;
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += (o, e) =>
			{
				MapDescriptor desc = e.Argument as MapDescriptor;
				Image image = GetImageFromWeb(desc);
				if(image != null) image.Dispose();
				downloadInProgress.Remove(desc);
			};
			worker.RunWorkerCompleted += (o, e) => { worker.Dispose(); worker = null; };
			worker.RunWorkerAsync(descriptor);
		}


		private Image GetImageFromWeb(MapDescriptor descriptor)
		{
			try
			{
				Image image = null;
				string url = ConstructURL(descriptor);
				string filename = descriptor.CalculateFilename();
				Console.WriteLine(""+DateTime.Now+": Fetching map from web:\n" + url);
				using (Stream stream = OpenWebStream(url))
				{
					image = Image.FromStream(stream);
				}
				image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
				descriptor.MapState = MapDescriptor.MapImageState.Correct;
				Console.WriteLine("Image saved to "+filename);
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
				nullImage.Text += "\n" + response.StatusCode;
				mostRecentError = WebError.Forbidden;
			}
			else
			{
				nullImage.Text += "\n" + webEx.Status;
				mostRecentError = WebError.Other;
			}

		}

		private void HandleOtherException(Exception ex)
		{
			nullImage.Text += "\n" + ex;
			mostRecentError = WebError.Other;
		}

	}

	public class MapDescriptor
	{
		public enum MapImageState { Empty, Correct, Partial }
		public MapImageState MapState { get; set; }
		private double lat = 0;
		private double lng = 0;
		public double Latitude { get { return lat; } set { lat = Fix(value, -90, 90); } }
		public double Longitude { get { return lng; } set { lng = Wrap(value, -180, 180); } }
		public int ZoomLevel { get; set; }
		private double delta = 1;
		public double Delta { get { return delta; } set { delta = value; } }
		public MapDescriptor(double lat, double lng, int zoom, double delta)
		{
			Latitude = lat;
			Longitude = lng;
			ZoomLevel = zoom;
			Delta = delta;
		}
		public MapDescriptor(double lat, double lng, int zoom)
			: this(lat, lng, zoom, 0.125) { }

		public override string ToString()
		{
			string ret = "googlemap_";
			ret += MapWebAccessor.DELIM + MapWebAccessor.CODE_ZOOM + ZoomLevel;
			ret += MapWebAccessor.CODE_CENTRE + Latitude.ToString("F8") + "," + Longitude.ToString("F8");
			ret += ".png";
			return ret;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MapDescriptor)) return false;
			return Equals((MapDescriptor)obj);
		}

		public bool Equals(MapDescriptor d)
		{
			if (d.ZoomLevel != this.ZoomLevel) return false;
			if (d.Longitude != this.Longitude) return false;
			if (d.Latitude != this.Latitude) return false;
			if (d.Delta != this.Delta) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return lat.GetHashCode() ^ lng.GetHashCode() ^ ZoomLevel.GetHashCode();
		}

		public string CalculateFilename()
		{
			string folder = Properties.Settings.Default.MapTextureFolder + Path.DirectorySeparatorChar
			+ "zoom=" + this.ZoomLevel;
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			string filename = folder + Path.DirectorySeparatorChar + this;
			return filename;
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

		private double Fix(double value, double nRange, double pRange)
		{
			if (value < nRange)
				return nRange;
			if (value > pRange)
				return pRange;
			return value;
		}

	}
}
