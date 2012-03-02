using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions.Terrain
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Patch
	{
		public float distanceToCamera { get; set; }
		
		public int worldX, worldY;

		public Landscape Land;
		public TriTreeNode BaseLeft { get; set; }
		public TriTreeNode BaseRight { get; set; }

		public bool IsVisible { get; set; }

		public Patch(Landscape land)
		{
			Land = land;
		}

		public void Init(int heightX, int heightY, int worldX, int worldY)
		{
			// Store Patch offsets for the world and heightmap.
			this.worldX = worldX;
			this.worldY = worldY;
			// Store pointer to first byte of the height data for this patch.
			//heightMap = hMap;

			BaseLeft = Land[Land.GetNextNode()];
			BaseRight = Land[Land.GetNextNode()];
			Reset();

		}

		public void Reset()
		{
			// Assume patch is not visible.
			IsVisible = false;

			// Reset the important relationships
			BaseLeft.LeftChild = BaseLeft.RightChild = BaseRight.LeftChild = BaseRight.RightChild = 0;
			// Clear the other relationships.
			BaseLeft.LeftNeighbor = BaseLeft.RightNeighbor = BaseRight.LeftNeighbor = BaseRight.RightNeighbor = 0;

			// Attach the two m_Base triangles together
			BaseLeft.BaseNeighbor = BaseRight.Self;
			BaseRight.BaseNeighbor = BaseLeft.Self;

		}

		public void Tessellate()
		{
			// Split each of the base triangles
			RecursTessellate(BaseLeft,
								worldX, worldY + Land.PATCH_SIZE,
								worldX + Land.PATCH_SIZE, worldY,
								worldX, worldY);

			RecursTessellate(BaseRight,
								worldX + Land.PATCH_SIZE, worldY,
								worldX, worldY + Land.PATCH_SIZE,
								worldX + Land.PATCH_SIZE, worldY + Land.PATCH_SIZE);
		}

		public void Render(List<Vertex> vBuf)
		{
			RecursRender(vBuf, BaseLeft, worldX, Land.PATCH_SIZE+worldY, Land.PATCH_SIZE+worldX, worldY, worldX, worldY);
			RecursRender(vBuf, BaseRight, Land.PATCH_SIZE+worldX, worldY, worldX, Land.PATCH_SIZE+worldY, Land.PATCH_SIZE+worldX, Land.PATCH_SIZE+worldY);
		}


		public void Split(TriTreeNode tri)
		{

			CheckTriChildren(tri);

			if (!IsLeafNode(tri))
				return;
			
			

			if (tri.BaseNeighbor != 0 && (Land[tri.BaseNeighbor].BaseNeighbor != tri.Self))
				Split(Land[tri.BaseNeighbor]);

			tri.LeftChild = Land.GetNextNode();
			tri.RightChild = Land.GetNextNode();
			
			TriTreeNode lc = Land[tri.LeftChild];
			TriTreeNode rc = Land[tri.RightChild];

			// If creation failed, just exit.
			if (rc.Self == 0 || lc.Self == 0)
			{
				tri.LeftChild = 0;
				tri.RightChild = 0;
				return;
			}
			CheckTriChildren(tri);

			// Fill in the information we can get from the parent (neighbor pointers)
			lc.BaseNeighbor = tri.LeftNeighbor;
			lc.LeftNeighbor = tri.RightChild;

			rc.BaseNeighbor = tri.RightNeighbor;
			rc.RightNeighbor = tri.LeftChild;

			// Link our Left Neighbor to the new children
			if (tri.LeftNeighbor != 0)
			{
				TriTreeNode ln = Land[tri.LeftNeighbor];
				if (ln.BaseNeighbor == tri.Self)
					ln.BaseNeighbor = tri.LeftChild;
				else if (ln.LeftNeighbor == tri.Self)
					ln.LeftNeighbor = tri.LeftChild;
				else if (ln.RightNeighbor == tri.Self)
					ln.RightNeighbor = tri.LeftChild;
				else
				{ }// throw new Exception("Illegal Left Neighbour");// Illegal Left Neighbor!
				// Not throwing exception, since this is actually legal if a patch is invisible.
			}


			// Link our Right Neighbor to the new children
			if (tri.RightNeighbor != 0)
			{
				TriTreeNode rn = Land[tri.RightNeighbor];
				if (rn.BaseNeighbor == tri.Self)
					rn.BaseNeighbor = tri.RightChild;
				else if (rn.RightNeighbor == tri.Self)
					rn.RightNeighbor = tri.RightChild;
				else if (rn.LeftNeighbor == tri.Self)
					rn.LeftNeighbor = tri.RightChild;
				else
				{ }// throw new Exception("Illegal Right Neighbour");// Illegal Right Neighbor!
				
			}

			// Link our Base Neighbor to the new children
			if (tri.BaseNeighbor != 0)
			{
				TriTreeNode bn = Land[tri.BaseNeighbor];
				if (bn.LeftChild != 0)
				{
					Land[bn.LeftChild].RightNeighbor = tri.RightChild;
					Land[bn.RightChild].LeftNeighbor = tri.LeftChild;
					lc.RightNeighbor = bn.RightChild;
					rc.LeftNeighbor = bn.LeftChild;
				}
				else
					Split(bn);  // Base Neighbor (in a diamond with us) was not split yet, so do that now.
			}
			else
			{
				// An edge triangle, trivial case.
				lc.RightNeighbor = 0;
				rc.LeftNeighbor = 0;
			}
			IsLeafNode(tri);
			CheckTriNeighbours(tri);
		}

		private static bool IsLeafNode(TriTreeNode tri)
		{
			if (tri.Self == 0)
				throw new Exception("Used invalid TriTreeNode. " + tri);
			if (tri.LeftChild == 0 || tri.RightChild == 0)
			{
				if (tri.RightChild != tri.LeftChild)
					throw new Exception("TriTreeNode has only one valid child. " + tri);
				return true;
			}
			return false;
		}

		void CheckTriChildren(TriTreeNode tri)
		{
			if (tri.LeftChild == 0)
				return;

			if (tri.LeftChild <= tri.Self)
				throw new Exception("TriTreeNode is younger than LeftChild. " + tri);

			if (tri.RightChild <= tri.Self)
				throw new Exception("TriTreeNode is younger than RightChild. " + tri);

		}

		void CheckTriNeighbours(TriTreeNode tri)
		{
			if (tri.BaseNeighbor == tri.Self)
				throw new Exception("TriTreeNode is its own BaseNeighbour. " + tri);
			if (tri.LeftNeighbor == tri.Self)
				throw new Exception("TriTreeNode is its own LeftNeighbor. " + tri);
			if (tri.RightNeighbor == tri.Self)
				throw new Exception("TriTreeNode is its own RightNeighbor. " + tri);
		}


		public void RecursTessellate(TriTreeNode tri, int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			int centerX = (leftX + rightX) >> 1; // Compute X coordinate of center of Hypotenuse
			int centerY = (leftY + rightY) >> 1; // Compute Y coord...


			bool doSplit = Land.SplitFunction(leftX, leftY, rightX, rightY, apexX, apexY);

			if(	 doSplit)
			{
				//Console.WriteLine("split: " + tri);
				Split(tri);														// Split this triangle.

				if (tri.LeftChild != 0 &&											// If this triangle was split, try to split it's children as well.
					((Math.Abs(leftX - rightX) >= 2) || (Math.Abs(leftY - rightY) >= 2)))	// Tessellate all the way down to one vertex per height field entry
				{
					RecursTessellate(Land[tri.LeftChild], apexX, apexY, leftX, leftY, centerX, centerY);
					RecursTessellate(Land[tri.RightChild], rightX, rightY, apexX, apexY, centerX, centerY);
				}
			}
		}

		public void RecursRender(List<Vertex> vBuf, TriTreeNode tri, int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{

			if (tri.LeftChild != 0)
			{

				int centerX = (leftX + rightX) >> 1;	// Compute X coordinate of center of Hypotenuse
				int centerY = (leftY + rightY) >> 1;	// Compute Y coord...

				RecursRender(vBuf, Land[tri.LeftChild], apexX, apexY, leftX, leftY, centerX, centerY);
				RecursRender(vBuf, Land[tri.RightChild], rightX, rightY, apexX, apexY, centerX, centerY);
				return;
			}

			float leftH = Land.FetchFunction(leftX, leftY);
			float rightH = Land.FetchFunction(rightX, rightY);
			float apexH = Land.FetchFunction(apexX, apexY);

			vBuf.Add(CreateVertex(leftX, leftY, leftH));
			vBuf.Add(CreateVertex(rightX, rightY, rightH));
			vBuf.Add(CreateVertex(apexX, apexY, apexH));

		}

		static Random rand = new Random();

		private static VertexTypes.PositionTexture CreateVertex(int leftX, int leftY, float leftH)
		{
			Vector3 Pos = new Vector3(leftX, leftH, leftY);
			Vector2 Tex = new Vector2((float)rand.NextDouble(), (float)rand.NextDouble());
			VertexTypes.PositionTexture v = new VertexTypes.PositionTexture() { Pos = Pos, TexCoord = Tex };
			return v;
		}


		public override string ToString()
		{
			return "Patch (" + worldX + "," + worldY + ")";
		}

		 
	}
}
