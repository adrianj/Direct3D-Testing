using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;

namespace Direct3DExtensions
{
	public class Sprite3DEngine : Basic3DEngine
	{
		protected Sprite spriteGroup;
		protected List<Texturing.SpriteTexture> allSprites = new List<Texturing.SpriteTexture>();


		public void AddSprite(Texturing.SpriteTexture sprite)
		{
			allSprites.Add(sprite);
		}

		public void RemoveSprite(Texturing.SpriteTexture sprite)
		{
			if (!allSprites.Contains(sprite)) return;
			allSprites.Remove(sprite);
		}

		protected override void InitDevice()
		{
			base.InitDevice();
			spriteGroup = new SlimDX.Direct3D10.Sprite(D3DDevice.Device, 1);
		}


		protected override void DrawGeometry()
		{
			base.DrawGeometry();

			D3DDevice.SetRasterizer(FillMode.Solid);
			spriteGroup.Begin(SpriteFlags.SortBackToFront);
			spriteGroup.DrawImmediate(GetSpriteInstances());
			spriteGroup.End();
		}

		protected SpriteInstance[] GetSpriteInstances()
		{
			SpriteInstance[] sprites = new SpriteInstance[allSprites.Count];
			for (int i = 0; i < sprites.Length; i++)
				sprites[i] = allSprites[i].Instance;
			return sprites;
		}


		#region Dispose
		void DisposeManaged()
		{
			if (spriteGroup != null) spriteGroup.Dispose(); spriteGroup = null;
		}
		void DisposeUnmanaged() { }

		bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}

				DisposeUnmanaged();
				disposed = true;
			}
			base.Dispose(disposing);
		}
		#endregion
    
	}
}
