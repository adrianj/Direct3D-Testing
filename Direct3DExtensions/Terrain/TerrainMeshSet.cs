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
		int numGrids = 3;
		GeometryOutputStream<VertexTypes.Pos3Norm3Tex3> gsOutput;
		int renderPassIndex;
		bool doSetup = true;

		D3D.InputLayout renderPassLayout;

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
			gsOutput = new GeometryOutputStream<VertexTypes.Pos3Norm3Tex3>(device.Device, effect[passIndex], this.Indices.Length);
			base.BindToPass(device, effect, passIndex);
		}

		public void BindToRenderPass(D3DDevice device, Effect effect, int passIndex)
		{
			renderPassIndex = passIndex;
			renderPassLayout = VertexTypes.GetInputLayout(this.Device.Device, effect[passIndex], typeof(VertexTypes.Pos3Norm3Tex3));
		}

		int i = 0;

		public override void Draw()
		{
			if (doSetup)
			{
				base.DoDrawConstants();
				base.DoDrawVertices();
				gsOutput.BindTarget();
				base.DoDrawFinal();
				gsOutput.UnbindTarget();
				doSetup = false;
			}
			RenderFromStreamOut();
			i++;
		}

		private void RenderFromStreamOut()
		{
			Effect[renderPassIndex].Apply();
			
			Device.Device.InputAssembler.SetInputLayout(renderPassLayout);
			Device.Device.InputAssembler.SetPrimitiveTopology(Topology);
			int vsize = VertexTypes.SizeOf(typeof(VertexTypes.Pos3Norm3Tex3));
			Device.Device.InputAssembler.SetVertexBuffers(0, new D3D.VertexBufferBinding(gsOutput.GSOutputBuffer, vsize, 0));
			//Device.Device.InputAssembler.SetIndexBuffer(indexbuffer, SlimDX.DXGI.Format.R32_UInt, 0);
			 
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
