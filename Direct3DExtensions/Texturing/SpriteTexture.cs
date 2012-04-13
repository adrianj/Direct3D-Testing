using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Texturing
{
	public class SpriteTexture : StagingTexture
	{
		Vector2 scale = new Vector2(1, 1);
		Vector2 translation = new Vector2(-0.5f, 0.5f);
		//float alpha = 1;

		public D3D.SpriteInstance Instance { get; private set; }
		public Vector2 Scale { get { return scale; } set { scale = value; UpdateTransform(); } }
		public Vector2 Translation { get { return translation; } set { translation = value; UpdateTransform(); } }
		public Color4 ColourBlend { get { return Instance.Color; } set { Instance.Color = value; } }

		public SpriteTexture(D3D.Device device, string filename)
			: base(device)
		{
			D3D.ImageLoadInformation iml = new D3D.ImageLoadInformation()
			{
				BindFlags = D3D.BindFlags.ShaderResource,
				CpuAccessFlags = D3D.CpuAccessFlags.None,
				Depth = 1,
				FilterFlags = D3D.FilterFlags.None,
				FirstMipLevel = 0,
				Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm,
				MipFilterFlags = D3D.FilterFlags.None,
				MipLevels = 1,
				OptionFlags = D3D.ResourceOptionFlags.None,
				Usage = D3D.ResourceUsage.Default
			};
			Resource = D3D.Texture2D.FromFile(device, filename, iml);
			View = new D3D.ShaderResourceView(device, Resource);
			Instance = new D3D.SpriteInstance(this.View, new Vector2(0, 0), new Vector2(1, 1));
			
			UpdateTransform();
		}
		/*
		public SpriteTexture(D3D.Device device, RenderTarget renderTarget)
			: base(device)
		{
			this.View = renderTarget.View;
			this.Resource = renderTarget.Resource;
			Instance = new D3D.SpriteInstance(this.View, new Vector2(0, 0), new Vector2(1, 1));
			UpdateTransform();
		}
		 */

		public SpriteTexture(D3D.Device device, int width, int height) : base(device, width, height) { UpdateTransform(); }


		protected override void RecreateTexture(int width, int height)
		{
			base.RecreateTexture(width, height);
			if (View != null) View.Dispose();
			View = new D3D.ShaderResourceView(device, Resource);
			Instance = new D3D.SpriteInstance(this.View, new Vector2(0, 0), new Vector2(1, 1));
			//Instance.Color = new Color4(AlphaBlend, 1, 1, 1);
			UpdateTransform();
		}

		protected override D3D.Texture2DDescription CreateDescription(int width, int height)
		{
			D3D.Texture2DDescription texDesc = new D3D.Texture2DDescription()
			{
				ArraySize = 1,
				MipLevels = 1,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Format = format,
				CpuAccessFlags = D3D.CpuAccessFlags.Write,
				OptionFlags = D3D.ResourceOptionFlags.None,
				BindFlags = D3D.BindFlags.ShaderResource,
				Usage = D3D.ResourceUsage.Dynamic,
				Height = height,
				Width = width
			};
			return texDesc;
		}

		protected void UpdateTransform()
		{
			if (Instance == null) return;
			Instance.Transform = Matrix.Scaling(new Vector3(Scale,1)) * Matrix.Translation(new Vector3(Translation,0));
		}
	}
}
