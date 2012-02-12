using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ImageTiler
{
	public interface ImageFilenameFactory
	{
		string ImageFolder { get; set; }
		string CreateFilename(int zoomLevel, int tileX, int tileY);
	}

	public class SimpleFilenameFactory : ImageFilenameFactory
	{

		public string ImageFolder { get; set; }

		public SimpleFilenameFactory()
		{
			ImageFolder = "";
		}

		public virtual string CreateFilename(int zoomLevel, int tileX, int tileY)
		{
			return this.ImageFolder+ zoomLevel + "_" + tileX + "_" + tileY+".png";
		}
	}
}
