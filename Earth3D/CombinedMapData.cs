using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace Direct3DLib
{
	[TypeConverter(typeof(DTALib.BasicTypeConverter))]
	public class CombinedMapData : Shape
	{
		private MapDescriptor desc = new MapDescriptor(0, 0, 10,0.125);

		public LatLong BottomLeftLocation { get { return desc.LatLong; } set { desc.LatLong = value; } }
	
		public double ShapeDelta { get { return desc.Delta; } set { desc.Delta = value; } }
		public int ZoomLevel { get { return desc.ZoomLevel; } set { desc.ZoomLevel = value; } }
		private bool updateRequired = true;
		//private Image image;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Image TextureImage { get { return TextureMipMaps[0]; } set { TextureMipMaps = new Image[] { value }; } }


		Image[] mipmaps;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Image[] TextureMipMaps { get { return mipmaps; } set { if (mipmaps != value && value.Length > 0) updateRequired = true; mipmaps = value; } }

		public CombinedMapData() : base()
		{
			TextureImage = new NullImage().ImageClone;
		}
		public CombinedMapData(CombinedMapData copy) : this()
		{
			desc = new MapDescriptor(copy.BottomLeftLocation.Latitude, copy.BottomLeftLocation.Longitude, copy.ZoomLevel, copy.ShapeDelta);
			this.TextureIndex = copy.TextureIndex;
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
			if (other.TextureMipMaps != this.TextureMipMaps) return false;
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
			if (this.TextureMipMaps == null || this.TextureMipMaps.Length < 1)
			{
				return;
			}
			Image[] clones = new Image[this.mipmaps.Length];
			for (int i = 0; i < mipmaps.Length; i++)
			{
				clones[i] = mipmaps[i].Clone() as Image;
			}
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += (o, e) =>
			{
				Image [] img = e.Argument as Image[];
				try
				{
					if (TextureIndex >= 0 && img != null)
					{
						byte[] bytes = ImageConverter.ConvertImageToBytes(img[0]);
						SlimDX.Direct3D10.Texture2D tex = ImageConverter.ConvertBytesToTexture2D(device, bytes);
						helper.TextureSet[TextureIndex].TextureImage = tex;
						foreach(Image i in img) i.Dispose();
						img = null;
					}
				}
				catch (SlimDX.Direct3D10.Direct3D10Exception) { }
			};
			worker.RunWorkerCompleted += (o, e) => { worker.Dispose(); };
			worker.RunWorkerAsync(clones);
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
			if (mipmaps != null && mipmaps.Length > 0)
				foreach (Image i in mipmaps) i.Dispose();
			mipmaps = null;
		}
	}

}
