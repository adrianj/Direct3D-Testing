using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Direct3DLib
{
	public class MapFileAccessor
	{
		public Image GetImage(MapDescriptor descriptor)
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

		private string CalculateFilename(MapDescriptor descriptor)
		{
			string folder = Properties.Settings.Default.MapTextureFolder + Path.DirectorySeparatorChar
			+ "zoom=" + descriptor.ZoomLevel;
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			string filename = folder + Path.DirectorySeparatorChar + descriptor;
			return filename;
		}
	}
}
