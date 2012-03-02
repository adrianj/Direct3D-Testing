using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions.RoamTerrain
{
	public class Landscape_cpp : BasicMesh
	{
		public const int MAP_SIZE = 16;
		public const int NUM_PATCHES_PER_SIDE = 2;
		public const int PATCH_SIZE = MAP_SIZE / NUM_PATCHES_PER_SIDE;
		public const int POOL_SIZE = 25000;

		public static Vector3 gViewPosition;
		public static float gFrameVariance;


		byte[] m_HeightMap;										// HeightMap of the Landscape
		Patch_cpp[,] m_Patches = new Patch_cpp[NUM_PATCHES_PER_SIDE, NUM_PATCHES_PER_SIDE];	// Array of patches

		static int m_NextTriNode;										// Index to next free TriTreeNode
		static TriTreeNode_cpp[] m_TriPool = new TriTreeNode_cpp[POOL_SIZE];						// Pool of TriTree nodes for splitting

		static int GetNextTriNode() { return m_NextTriNode; }
		static void SetNextTriNode(int nNextNode) { m_NextTriNode = nNextNode; }

		public TriTreeNode_cpp AllocateTri()
		{
			if (m_NextTriNode >= POOL_SIZE)
				return m_TriPool[0];
			TriTreeNode_cpp tri = m_TriPool[m_NextTriNode];
			return tri;
		}

		public virtual void Init(byte[] hMap)
		{
			Patch_cpp patch;
			m_HeightMap = hMap;
			for (int y = 0; y < NUM_PATCHES_PER_SIDE; y++)
				for (int x = 0; x < NUM_PATCHES_PER_SIDE; x++)
				{
					patch = new Patch_cpp();
					m_Patches[y, x] = patch;
					patch.Init(x * PATCH_SIZE, y * PATCH_SIZE, x * PATCH_SIZE, y * PATCH_SIZE, hMap);
					patch.ComputeVariance();
				}
			for (int i = 0; i < m_TriPool.Length; i++)
				m_TriPool[i] = new TriTreeNode_cpp();
		}

		public virtual void Reset()
		{
			Patch_cpp patch;
			SetNextTriNode(0);


			for (int y = 0; y < NUM_PATCHES_PER_SIDE; y++)
				for (int x = 0; x < NUM_PATCHES_PER_SIDE; x++)
				{
					patch = m_Patches[y, x];
					patch.Reset();
					patch.SetVisibility(0, 0, 0, 0, 0, 0);

					if (patch.isDirty())
						patch.ComputeVariance();

					if (patch.isVisibile())
					{
						if (x > 0)
							patch.GetBaseLeft().LeftNeighbour = m_Patches[y, x].GetBaseRight();
						else
							patch.GetBaseLeft().LeftNeighbour = null;

						if (x < (NUM_PATCHES_PER_SIDE - 1))
							patch.GetBaseRight().LeftNeighbour = m_Patches[y, x + 1].GetBaseLeft();
						else
							patch.GetBaseRight().LeftNeighbour = null;

						if (y > 0)
							patch.GetBaseLeft().RightNeighbour = m_Patches[y - 1, x].GetBaseRight();
						else
							patch.GetBaseLeft().RightNeighbour = null;

						if (y < (NUM_PATCHES_PER_SIDE - 1))
							patch.GetBaseRight().RightNeighbour = m_Patches[y + 1, x].GetBaseLeft();
						else
							patch.GetBaseRight().RightNeighbour = null;
					}

				}

		}


		public virtual void Tessellate()
		{
			Patch_cpp patch;
			for (int y = 0; y < NUM_PATCHES_PER_SIDE; y++)
				for (int x = 0; x < NUM_PATCHES_PER_SIDE; x++)
				{
					patch = m_Patches[y, x];
					if (patch.isVisibile())
						patch.Tessellate();
				}

		}


		public virtual void Render()
		{
			throw new NotImplementedException();
		}
	}
}
