using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions.RoamTerrain
{
	public struct TriTreeNode_cpp
	{
		public int Self;
		public int LeftChild;
		public int RightChild;
		public int BaseNeighbour;
		public int LeftNeighbour;
		public int RightNeighbour;
		public Landscape_cpp Land;
		public TriTreeNode_cpp(Landscape_cpp land, int selfIndex)
		{
			Self = selfIndex;
			Land = land;
			LeftChild = 0;
			RightChild = 0;
			LeftNeighbour = 0;
			RightNeighbour = 0;
			BaseNeighbour = 0;
		}
	}

	public class Patch_cpp
	{
		public const int VARIANCE_DEPTH = 9;

		byte[] m_HeightMap;
		int m_WorldX, m_WorldY;

		byte[] m_VarianceLeft = new byte[1 << VARIANCE_DEPTH];
		byte[] m_VarianceRight = new byte[1 << VARIANCE_DEPTH];

		byte[] m_CurrentVariance;
		bool m_VarianceDirty;
		bool m_isVisible;

		TriTreeNode_cpp m_BaseLeft;
		TriTreeNode_cpp m_BaseRight;

		Landscape_cpp Land;

		public TriTreeNode_cpp GetBaseLeft() { return m_BaseLeft; }
		public TriTreeNode_cpp GetBaseRight() { return m_BaseRight; }
		public bool isDirty() { return m_VarianceDirty; }
		public bool isVisibile() { return m_isVisible; }
		public void SetVisibility(int eyeX, int eyeY, int leftX, int leftY, int rightX, int rightY)
		{
			m_isVisible = true;
		}

		// The static half of the Patch Class
		public virtual void Init(int heightX, int heightY, int worldX, int worldY, byte[] hMap)
		{
			m_BaseLeft = Land.AllocateTri();
			m_BaseRight = Land.AllocateTri();

			m_BaseLeft.BaseNeighbour = m_BaseRight;
			m_BaseRight.BaseNeighbour = m_BaseLeft;

			m_WorldX = worldX;
			m_WorldY = worldY;

			m_HeightMap = hMap;

			m_VarianceDirty = true;
			m_isVisible = false;

		}
		public virtual void Reset()
		{
			m_isVisible = false;

			m_BaseLeft.LeftChild = m_BaseLeft.RightChild = m_BaseRight.LeftChild = m_BaseRight.LeftChild = null;// Attach the two m_Base triangles together
			m_BaseLeft.BaseNeighbour = m_BaseRight;
			m_BaseRight.BaseNeighbour = m_BaseLeft;

			m_BaseLeft.RightNeighbour = m_BaseLeft.LeftNeighbour = m_BaseRight.RightNeighbour = m_BaseRight.LeftNeighbour = null;
		}


		public virtual void Tessellate()
		{
			// Split each of the base triangles
			m_CurrentVariance = m_VarianceLeft;
			RecursTessellate(m_BaseLeft,
								m_WorldX, m_WorldY + Landscape_cpp.PATCH_SIZE,
								m_WorldX + Landscape_cpp.PATCH_SIZE, m_WorldY,
								m_WorldX, m_WorldY,
								1);

			m_CurrentVariance = m_VarianceRight;
			RecursTessellate(m_BaseRight,
								m_WorldX + Landscape_cpp.PATCH_SIZE, m_WorldY,
								m_WorldX, m_WorldY + Landscape_cpp.PATCH_SIZE,
								m_WorldX + Landscape_cpp.PATCH_SIZE, m_WorldY + Landscape_cpp.PATCH_SIZE,
								1);
		}
		public virtual void Render()
		{
			throw new NotImplementedException();

		}



		public virtual void ComputeVariance()
		{
			m_CurrentVariance = m_VarianceLeft;
			RecursComputeVariance(0, Landscape_cpp.PATCH_SIZE, m_HeightMap[Landscape_cpp.PATCH_SIZE * Landscape_cpp.MAP_SIZE],
								Landscape_cpp.PATCH_SIZE, 0, m_HeightMap[Landscape_cpp.PATCH_SIZE],
								0, 0, m_HeightMap[0],
								1);
			m_CurrentVariance = m_VarianceRight;
			RecursComputeVariance(Landscape_cpp.PATCH_SIZE, 0, m_HeightMap[Landscape_cpp.PATCH_SIZE],
									0, Landscape_cpp.PATCH_SIZE, m_HeightMap[Landscape_cpp.PATCH_SIZE * Landscape_cpp.MAP_SIZE],
									Landscape_cpp.PATCH_SIZE, Landscape_cpp.PATCH_SIZE, m_HeightMap[(Landscape_cpp.PATCH_SIZE * Landscape_cpp.MAP_SIZE) + Landscape_cpp.PATCH_SIZE],
									1);

			// Clear the dirty flag for this patch
			m_VarianceDirty = false;
		}
		// The recursive half of the Patch Class
		public virtual void Split(TriTreeNode_cpp tri)
		{
			if (tri.LeftChild != null)
				return;

			if (tri.BaseNeighbour != null && (tri.BaseNeighbour.BaseNeighbour != tri))
				Split(tri.BaseNeighbour);

			//tri.LeftChild = Landscape.AllocateTri();
			//tri.RightChild = Landscape.AllocateTri();
			tri.LeftChild = new TriTreeNode_cpp();
			tri.RightChild = new TriTreeNode_cpp();

			if (tri.RightChild == null)
				return;

			tri.LeftChild.BaseNeighbour = tri.LeftNeighbour;
			tri.LeftNeighbour.LeftNeighbour = tri.RightChild;

			tri.RightChild.BaseNeighbour = tri.RightNeighbour;
			tri.RightChild.RightNeighbour = tri.LeftChild;

			if (tri.LeftNeighbour != null)
			{
				if (tri.LeftNeighbour.BaseNeighbour == tri)
					tri.LeftNeighbour.BaseNeighbour = tri.LeftChild;
				else if (tri.LeftNeighbour.LeftNeighbour == tri)
					tri.LeftNeighbour.LeftNeighbour = tri.LeftChild;
				else if (tri.LeftNeighbour.RightNeighbour == tri)
					tri.LeftNeighbour.RightNeighbour = tri.LeftChild;
				else
					throw new Exception("Illegal Left Neighbour");
			}
			if (tri.RightNeighbour != null)
			{
				if (tri.RightNeighbour.BaseNeighbour == tri)
					tri.RightNeighbour.BaseNeighbour = tri.RightChild;
				else if (tri.RightNeighbour.RightNeighbour == tri)
					tri.RightNeighbour.RightNeighbour = tri.RightChild;
				else if (tri.RightNeighbour.LeftNeighbour == tri)
					tri.RightNeighbour.LeftNeighbour = tri.RightChild;
				else
					throw new Exception("Illegal Right Neighbour");
			}

			if (tri.BaseNeighbour != null)
			{
				if (tri.BaseNeighbour.LeftChild != null)
				{
					tri.BaseNeighbour.LeftChild.RightNeighbour = tri.RightChild;
					tri.BaseNeighbour.RightChild.LeftNeighbour = tri.LeftChild;
					tri.LeftChild.RightNeighbour = tri.BaseNeighbour.RightChild;
					tri.RightChild.LeftNeighbour = tri.BaseNeighbour.LeftChild;
				}
				else
					Split(tri.BaseNeighbour); // Base neighbour, in a diamond with us. Split it.
			}
			else
			{
				// Edge.
				tri.LeftChild.RightNeighbour = null;
				tri.RightChild.LeftNeighbour = null;
			}

		}

		public virtual void RecursTessellate(TriTreeNode_cpp tri, int leftX, int leftY, int rightX, int rightY, int apexX, int apexY, int node)
		{
			float TriVariance = 0;
			int centreX = (leftX + rightX) >> 1;
			int centreY = (leftY + rightY) >> 1;

			if (node < (1 << VARIANCE_DEPTH))
			{
				float distance = (new Vector3((float)centreX, 0, (float)centreY) - Landscape_cpp.gViewPosition).Length();
				TriVariance = ((float)m_CurrentVariance[node] * Landscape_cpp.MAP_SIZE * 2) / distance;
			}

			if((node >= (1<<VARIANCE_DEPTH)) ||
			(TriVariance > Landscape_cpp.gFrameVariance))
			{
				Split(tri);

				if (tri.LeftChild != null &&
					((Math.Abs(leftX - rightX) >= 3) || (Math.Abs(leftY - rightY) >= 3)))
				{
					RecursTessellate(tri.LeftChild, apexX, apexY, leftX, leftY, centreX, centreY, node << 1);
					RecursTessellate(tri.RightChild, rightX, rightX, apexX, apexY, centreX, centreY, 1 + (node << 1));
				}
			}

		}
		public virtual void RecursRender(TriTreeNode_cpp tri, int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			throw new NotImplementedException();
		}

		public virtual byte RecursComputeVariance(int leftX, int leftY, byte leftZ, int rightX, int rightY, byte rightZ,
														int apexX, int apexY, byte apexZ, int node)
		{
			int centreX = (leftX + rightX) >> 1;
			int centreY = (leftY + rightY) >> 1;
			byte myVariance;

			byte centreZ = m_HeightMap[(centreY * Landscape_cpp.MAP_SIZE) + centreX];
			myVariance = (byte)Math.Abs((int)centreZ - (((int)leftZ + (int)rightZ) >> 1));

			if ((Math.Abs(leftX - rightX) >= 8) ||
				(Math.Abs(leftY - rightY) >= 8))
			{
				myVariance = Math.Max(myVariance, RecursComputeVariance(apexX, apexY, apexZ, leftX, leftY, leftZ, centreX, centreY, centreZ, node << 1));
				myVariance = Math.Max(myVariance, RecursComputeVariance(rightX,rightY,rightZ,apexX,apexY,apexZ,centreX,centreY,centreZ,1+(node<<1)));
			}

			if(node < (1<<VARIANCE_DEPTH))
				m_CurrentVariance[node] = (byte)(1 + myVariance);

			return myVariance;
		}


		public System.Collections.Generic.IEnumerator<TriTreeNode_cpp> GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
