using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace Direct3DLib
{
	[TypeConverter(typeof(BasicTypeConverter))]
	public class CombinedMapData : Shape
	{
		private MapDescriptor desc = new MapDescriptor(0, 0, 10,0.125);

		//private LatLong bottomLeft = new LatLong();
		public LatLong BottomLeftLocation { get { return desc.LatLong; } set { desc.LatLong = value; } }
	
		//private double shapeDelta = 0.125;
		public double ShapeDelta { get { return desc.Delta; } set { desc.Delta = value; } }
		//private int tileRes = -2;
		public int ZoomLevel { get { return desc.ZoomLevel; } set { desc.ZoomLevel = value; } }
		private bool updateRequired = true;
		private Image image;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Image TextureImage { get { return image; } set { if (image != value && value != null) updateRequired = true; image = value; } }

		public CombinedMapData() : base()
		{
			image = null;
		}
		public CombinedMapData(CombinedMapData copy) : this()
		{
			MapDescriptor md = new MapDescriptor(copy.BottomLeftLocation.Latitude, copy.BottomLeftLocation.Longitude, copy.ZoomLevel, copy.ShapeDelta);
			this.TextureIndex = copy.TextureIndex;
			/*
			this.ShapeDelta = copy.ShapeDelta;
			this.ZoomLevel = copy.ZoomLevel;
			this.BottomLeftLocation = copy.BottomLeftLocation;
			 */
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CombinedMapData)) return false;
			CombinedMapData other = (CombinedMapData)obj;
			return Equals(other);
		}

		public bool Equals(CombinedMapData other)
		{
			if (!other.GetMapDescriptor().Equals(desc)) return false;
			//if (!other.ShapeDelta.Equals(this.ShapeDelta)) return false;
			//if (!other.BottomLeftLocation.Equals(this.BottomLeftLocation)) return false;
			//if (other.ZoomLevel != this.ZoomLevel) return false;
			if (other.image != this.image) return false;
			return base.Equals(other);
		}

		public bool IsSameShape(CombinedMapData other)
		{
			if (!other.ShapeDelta.Equals(this.ShapeDelta)) return false;
			if (!other.BottomLeftLocation.Equals(this.BottomLeftLocation)) return false;
			return true;
		}

		public bool IsSameTexture(CombinedMapData other)
		{
			if (other.ShapeDelta != this.ShapeDelta) return false;
			if (!other.BottomLeftLocation.Equals(this.BottomLeftLocation)) return false;
			if (other.ZoomLevel != this.ZoomLevel) return false;
			if (other.TextureImage == null) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return desc.GetHashCode();
		}

		public override string ToString()
		{
			return "CombinedMap:" + BottomLeftLocation + "," + ShapeDelta + "," + ZoomLevel;
		}

		private void SetTextureInSeperateThread(SlimDX.Direct3D10.Device device, ShaderHelper helper)
		{
			updateRequired = false;
			if (this.image == null)
			{
				return;
			}
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += (o, e) =>
			{
				Image img = (Image)e.Argument;
				try
				{
					if (TextureIndex >= 0 && img != null)
					{
						byte[] bytes = ImageConverter.ConvertImageToBytes(img);
						SlimDX.Direct3D10.Texture2D tex = ImageConverter.ConvertBytesToTexture2D(device, bytes);
						helper.TextureSet[TextureIndex].TextureImage = tex;
						img.Dispose();
						img = null;
					}
				}
				catch (SlimDX.Direct3D10.Direct3D10Exception) { }
			};
			worker.RunWorkerCompleted += (o, e) => { worker.Dispose(); };
			worker.RunWorkerAsync(this.image.Clone());
		}

		public MapDescriptor GetMapDescriptor()
		{
			return desc;
		}


		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed)
				{
					if (disposing)
						DisposeManaged();
					this.disposed = true;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override void Render(SlimDX.Direct3D10.Device device, ShaderHelper shaderHelper)
		{
			if (updateRequired)
			{
				SetTextureInSeperateThread(device, shaderHelper);
			}
			base.Render(device, shaderHelper);
		}

		private void DisposeManaged()
		{
			if (image != null) image.Dispose();
		}
	}

}
