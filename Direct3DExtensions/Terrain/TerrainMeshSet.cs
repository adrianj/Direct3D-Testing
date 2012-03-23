using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions.Terrain
{
	public class TerrainMeshSet : BasicMesh
	{
		List<Mesh> grids = new List<Mesh>();

		public override Vector3 Scale
		{
			get { return base.Scale; }
			set { base.Scale = value; UpdateScales(); }
		}

		public override Vector3 Translation
		{
			get { return base.Translation; }
			set { base.Translation = value; UpdateTranslation(); }
		}

		public override Vector3 Rotation
		{
			get { return base.Rotation; }
			set { base.Rotation = value; UpdateRotation(); }
		}

		public TerrainMeshSet()
		{
			Recreate();
			
		}

		private void Recreate()
		{
			int numGrids = 3;
			int gridColumns = 8;
			int rowFactor = 3;
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
			UpdateScales();
		}

		void UpdateScales()
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

		void UpdateTranslation()
		{
			if (grids.Count < 1) return;
			grids[0].Translation = this.Translation;
			float res = MathExtensions.PowerOfTwo(grids.Count) * 4 * this.Scale.X;
			Vector3 t = this.Translation;
			t.X = res * (float)Math.Round(t.X / res);
			t.Z = res * (float)Math.Round(t.Z / res);
			for (int i = 0; i < grids.Count; i++)
			{
				grids[i].Translation = t;
			}
		}

		void UpdateRotation()
		{
			foreach (Mesh g in grids)
				g.Rotation = this.Rotation;
		}

		public override void BindToPass(D3DDevice device, Effect effect, int passIndex)
		{
			foreach (Mesh g in grids)
				g.BindToPass(device, effect, passIndex);
		}

		public override void Draw()
		{
			foreach (Mesh g in grids)
				g.Draw();
		}
	}
}
