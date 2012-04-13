using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Terrain
{
	public class TerrainMeshSet : BasicMesh
	{
		D3D.InputLayout renderPassLayout;
		int numGrids = 3;
		GeometryOutputStream<VertexTypes.Pos3Norm3Tex3> gsOutput;
		int renderPassIndex;
		public Texturing.SpriteTexture MinimapSprite { get; private set; }
		public bool doSetup { get; private set; }
		public Texturing.RenderTarget MinimapTarget { get; private set; }


		public override Vector3 Translation
		{
			get { return base.Translation; }
			set { UpdateTranslation(value); }
		}

		public override Vector3 Scale
		{
			get { return base.Scale; }
			set { UpdateScale(value); }
		}

		public TerrainMeshSet()
		{
			Recreate();
			doSetup = true;
		}

		private void Recreate()
		{
			int numGrids = 3;
			int gridColumns = 16;
			int rowFactor = 1;
			List<Mesh> grids = new List<Mesh>();
			using (MeshFactory fact = new MeshFactory())
				grids.Add(fact.CreateDiamondGrid(gridColumns, gridColumns));
			for (int i = 0; i < numGrids-1; i++)
			{
				grids.Add(new ExpandableSquareGrid(gridColumns, gridColumns * rowFactor));
				gridColumns = (gridColumns + gridColumns * rowFactor) / 2;
				rowFactor = MathExtensions.Clamp(rowFactor / 2, 1, 10);
			}
			for (int i = 0; i < numGrids; i++)
			{
				grids.Add(new ExpandableSquareGrid(gridColumns, gridColumns));
			}
			SetScales(grids);
			MeshOptimiser.CombineIntoSingleMesh(this, grids);
			this.numGrids = grids.Count;
		}

		public void AddMinimapToSpriteEngine(Sprite3DEngine engine)
		{
			engine.AddSprite(MinimapSprite);
		}

		
		void SetScales(List<Mesh> grids)
		{
			if (grids.Count < 1) return;
			grids[0].Scale = this.Scale;
			for (int i = 1; i < grids.Count; i++)
			{
				Vector3 s = this.Scale * (1 << (i-1));
				s.Y = this.Scale.Y;
				grids[i].Scale = s;
			}
		}

		void UpdateScale(Vector3 value)
		{
			if (!base.Scale.Equals(value))
			{
				base.Scale = value;
				doSetup = true;
			}
		}
		
		void UpdateTranslation(Vector3 value)
		{
			float res = MathExtensions.PowerOfTwo(numGrids) * 2 * this.Scale.X;
			Vector3 t = value;
			t.X = res * (float)Math.Round(t.X / res);
			t.Z = res * (float)Math.Round(t.Z / res);
			if (!base.Translation.Equals(t))
			{
				base.Translation = t;
				doSetup = true;
			}
		}

		public override void BindToPass(D3DDevice device, Effect effect, int passIndex)
		{
			BindToSetupPass(device, effect, passIndex);
		}

		private void BindToSetupPass(D3DDevice device, Effect effect, int passIndex)
		{
			gsOutput = new GeometryOutputStream<VertexTypes.Pos3Norm3Tex3>(device.Device, effect[passIndex], this.Indices.Length);
			base.BindToPass(device, effect, passIndex);
			CreateMinimapTarget();
		}

		public void BindToRenderPass(D3DDevice device, Effect effect, int passIndex)
		{
			renderPassIndex = passIndex;
			renderPassLayout = VertexTypes.GetInputLayout(this.Device.Device, effect[passIndex], typeof(VertexTypes.Pos3Norm3Tex3));
		}

		private void CreateMinimapTarget()
		{
			System.Drawing.Size size = Device.GetSize();
			if (MinimapTarget != null)
			{
				if (MinimapTarget.GetSize().Equals(Device.GetSize()))
					return;
				MinimapTarget.Dispose();
			}
			MinimapSprite = new Texturing.SpriteTexture(Device.Device, size.Width, size.Height);
			MinimapTarget = new Texturing.RenderTarget(Device.Device, size.Width, size.Height);
			//MinimapSprite.Scale = new Vector2(0.5f, 0.5f);
			//MinimapSprite.Translation = new Vector2(0.75f, 0.75f);
			MinimapSprite.Scale = new Vector2(2, 2);
			MinimapSprite.Translation = new Vector2(0, 0);
			MinimapSprite.ColourBlend = new Color4(1, 1, 1, 1);
		}

		public override void Draw()
		{
			
			if (doSetup)
			{
				D3D.RenderTargetView[] previousRenderTargets = ChangeRenderTargetToMinimap();
				base.DoDrawConstants();
				base.DoDrawVertices();
				base.DoDrawFinal();
				RestoreRenderTargets(previousRenderTargets);
				UpdateMinimapSprite();
				doSetup = false;
			}
			RenderFromStreamOut();
			
		}

		private void RestoreRenderTargets(D3D.RenderTargetView[] previousRenderTargets)
		{
			gsOutput.UnbindTarget();
			Device.SetRenderTargets(previousRenderTargets);
			Device.SetBlendState(true);
		}

		private D3D.RenderTargetView[] ChangeRenderTargetToMinimap()
		{
			D3D.RenderTargetView[] previousRenderTargets = Device.GetRenderTargets();
			Device.SetBlendState(false);
			CreateMinimapTarget();
			MinimapTarget.Clear(new Color4(0, 0, 0, 0));
			Device.SetRenderTargets(new D3D.RenderTargetView[] { MinimapTarget.RenderView });
			gsOutput.BindTarget();
			return previousRenderTargets;
		}

		private void UpdateMinimapSprite()
		{
			MinimapSprite.WriteTexture(MinimapTarget.Texture);
		}

		private void RenderFromStreamOut()
		{
			Effect.SetCamera();
			Effect[renderPassIndex].Apply();
			
			Device.Device.InputAssembler.SetInputLayout(renderPassLayout);
			Device.Device.InputAssembler.SetPrimitiveTopology(Topology);
			int vsize = VertexTypes.SizeOf(typeof(VertexTypes.Pos3Norm3Tex3));
			Device.Device.InputAssembler.SetVertexBuffers(0, new D3D.VertexBufferBinding(gsOutput.GSOutputBuffer, vsize, 0));
			Device.Device.DrawAuto();
		}

		public IEnumerable<VertexTypes.Pos3Norm3Tex3> ReadGSVertices()
		{
			return gsOutput;
		}


		#region Dispose
		void DisposeManaged() { if (gsOutput != null) gsOutput.Dispose(); gsOutput = null;
		if (renderPassLayout != null) renderPassLayout.Dispose(); renderPassLayout = null;
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
