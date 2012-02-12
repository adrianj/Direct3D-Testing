using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace Direct3DLib
{
	public class MapFileAccessor
	{
		public Image GetImage(MapDescriptor descriptor)
		{
			string filename = descriptor.CalculateFilename();
			if (File.Exists(filename))
			{
				Image image = null;
				descriptor.MapState = MapDescriptor.MapImageState.Correct;
				try
				{
					System.Threading.Thread.Sleep(10);
					image = Bitmap.FromFile(filename);
					return image;
				}
				catch (OutOfMemoryException)
				{
					if (image != null)
					{
						image.Dispose();
						image = null;
					}
				}
			}
			descriptor.MapState = MapDescriptor.MapImageState.Empty;
			return null;
		}

	}
}
