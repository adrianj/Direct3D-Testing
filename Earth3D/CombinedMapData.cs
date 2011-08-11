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
		private LatLong bottomLeft = new LatLong();
		public LatLong BottomLeftPosition { get { return bottomLeft; } set { bottomLeft = value; } }
	
		private double shapeDelta = 0.125;
		public double ShapeDelta { get { return shapeDelta; } set { shapeDelta = value; } }
		private int tileRes = -2;
		public int ZoomLevel { get { return tileRes; } set { tileRes = value; } }
		private bool updateRequired = true;
		private Image image;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Image TextureImage { get { return image; } set { if (image != value && value != null) updateRequired = true; image = value; } }

		public CombinedMapData()
		{
			image = null;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CombinedMapData)) return false;
			CombinedMapData other = (CombinedMapData)obj;
			return Equals(other);
		}

		public bool Equals(CombinedMapData other)
		{
			if (!other.ShapeDelta.Equals(this.ShapeDelta)) return false;
			if (!other.BottomLeftPosition.Equals(this.bottomLeft)) return false;
			if (other.ZoomLevel != this.ZoomLevel) return false;
			if (other.image != this.image) return false;
			return base.Equals(other);
		}

		public bool IsSameShape(CombinedMapData other)
		{
			if (!other.ShapeDelta.Equals(this.ShapeDelta)) return false;
			if (!other.BottomLeftPosition.Equals(this.bottomLeft)) return false;
			return true;
		}

		public bool IsSameTexture(CombinedMapData other)
		{
			if (other.shapeDelta != this.shapeDelta) return false;
			if (!other.BottomLeftPosition.Equals(this.bottomLeft)) return false;
			if (other.ZoomLevel != this.ZoomLevel) return false;
			if (other.TextureImage == null) return false;
			return true;
		}

		public override int GetHashCode()
		{
			return bottomLeft.GetHashCode() ^ ShapeDelta.GetHashCode();
		}

		public override string ToString()
		{
			return "CombinedMap:" + bottomLeft + "," + shapeDelta +","+tileRes;
		}

		private void SetTextureInSeperateThread(SlimDX.Direct3D10.Device device, ShaderHelper helper)
		{
			updateRequired = false;
			BackgroundWorker worker = new BackgroundWorker();
			SlimDX.Direct3D10.Texture2D tex = null;
			worker.DoWork += (o, e) =>
			{
				Image img = (Image)e.Argument;
				try
				{
					if (TextureIndex >= 0 && img != null)
					{
						byte[] bytes = ImageConverter.ConvertImageToBytes(img);
						tex = ImageConverter.ConvertBytesToTexture2D(device, bytes);
						helper.TextureSet[TextureIndex].TextureImage = tex;
					}
				}
				catch (SlimDX.Direct3D10.Direct3D10Exception) { }
			};
			worker.RunWorkerAsync(image);
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
