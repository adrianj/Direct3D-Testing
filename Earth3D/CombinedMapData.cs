using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace Direct3DLib
{
	[TypeConverter(typeof(GenericTypeConverter<CombinedMapData>))]
	public class CombinedMapData : IRenderable
	{
		#region IRenderable Interface Implementation
		public bool RayIntersects(SlimDX.Ray ray, out float distance)
		{
			distance = 0;
			if (terrainShape == null)
				return false;
			return terrainShape.RayIntersects(ray, out distance);
		}
		public bool CanPick { get { if (terrainShape == null) return false; return terrainShape.CanPick; } }
		public int TextureIndex
		{
			get { if (terrainShape == null) return -1; return terrainShape.TextureIndex; }
			set { if(terrainShape != null) terrainShape.TextureIndex = value; }
		}
		public SlimDX.Matrix World { get { if (terrainShape == null) return SlimDX.Matrix.Identity; return terrainShape.World; } }
		public VertexList Vertices { get { if (terrainShape == null) return new VertexList(); return terrainShape.Vertices; } }
		public void Update(SlimDX.Direct3D10.Device device, SlimDX.Direct3D10.Effect effect)
		{
			if (terrainShape != null)
				terrainShape.Update(device, effect);
		}
		public void Render(SlimDX.Direct3D10.Device device, ShaderHelper helper)
		{
			if (terrainShape != null)
			{
				if (updateRequired)
				{
					if (TextureIndex >= 0 && image != null)
					{
						SetTextureInSeperateThread(device,helper);
						updateRequired = false;
					}
				}
				terrainShape.Render(device, helper);
			}
		}
		public SlimDX.BoundingBox MaxBoundingBox { get { return terrainShape.MaxBoundingBox; } }
		//public bool IsOnScreen(SlimDX.Matrix viewProj) { return terrainShape.IsOnScreen(viewProj); }
		#endregion
		private bool updateRequired = true;
		private LatLong bottomLeft = new LatLong();
		public LatLong BottomLeftPosition { get { return bottomLeft; } set { bottomLeft = value; } }
	
		private double shapeDelta = 0.125;
		public double ShapeDelta { get { return shapeDelta; } set { shapeDelta = value; } }
		private int tileRes = -2;
		public int ZoomLevel { get { return tileRes; } set { tileRes = value; } }
		private Shape terrainShape;
		public Shape TerrainShape
		{
			get { return terrainShape; }
			set
			{
				terrainShape = value;
				if (terrainShape != null)
				{
					terrainShape.Name = "" + this;
					updateRequired = true;
				}
			}
		}
		private Image image;
		public Image TextureImage { get { return image; } set { image = value; updateRequired = true; } }

		public CombinedMapData()
		{
			terrainShape = null;
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
			if (other.terrainShape != this.terrainShape) return false;
			if (other.image != this.image) return false;
			return true;
		}

		public bool IsSameShape(CombinedMapData other)
		{
			if (!other.ShapeDelta.Equals(this.ShapeDelta)) return false;
			if (!other.BottomLeftPosition.Equals(this.bottomLeft)) return false;
			if (other.terrainShape == null) return false;
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
			BackgroundWorker worker = new BackgroundWorker();
			SlimDX.Direct3D10.Texture2D tex = null;
			worker.DoWork += (o, e) =>
			{
				if (TextureIndex >= 0)
				{
					byte[] bytes = ImageConverter.ConvertImageToBytes(image);
					tex = ImageConverter.ConvertBytesToTexture2D(device, bytes);
					helper.TextureSet[TextureIndex].TextureImage = tex;
				}
			};
			worker.RunWorkerAsync();
		}

		public void Dispose()
		{
			if (terrainShape != null) terrainShape.Dispose();
		}
	}

}
