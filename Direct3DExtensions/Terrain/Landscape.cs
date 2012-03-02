using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Direct3DExtensions;
using SlimDX;

namespace Direct3DExtensions.Terrain
{
	public delegate bool IndexedTriFunction(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY);
	public delegate short FetchMapDataFunction(int worldX, int worldY);

	public class Landscape : BasicMesh
	{
		public int MaxTris = 25000;
		public int MAP_SIZE = 128;
		public int NUM_PATCHES_PER_SIDE = 32;
		public int PATCH_SIZE { get { return MAP_SIZE / NUM_PATCHES_PER_SIDE; } }
		public float FrameVariance = 1.0f;
		public int desiredTris = 15000;

		//int[,] heightMap;

		Patch[,] Patches;
		public Vector3 CameraPos;

		int allocatedTris = 0;
		public int AllocatedTris { get { return allocatedTris; } }
		TriTreeNode[] nodeList;

		public IndexedTriFunction SplitFunction = Landscape.AlwaysFalse;
		public IndexedTriFunction VisibleFunction = Landscape.AlwaysTrue;
		public FetchMapDataFunction FetchFunction = Landscape.FetchZero;

		public TriTreeNode this[int index] { get { return nodeList[index]; } }


		public Patch[] AllPatches
		{
			get
			{
				Patch[] ret = new Patch[NUM_PATCHES_PER_SIDE * NUM_PATCHES_PER_SIDE];
				for (int y = 0; y < NUM_PATCHES_PER_SIDE; y++)
					for (int x = 0; x < NUM_PATCHES_PER_SIDE; x++)
						ret[y * NUM_PATCHES_PER_SIDE + x] = Patches[y, x];
				return ret;
			}
		}

		public void Tessellate()
		{
			foreach (Patch p in GetVisibleSortedPatches())
				p.Tessellate();
			this.RecreateVertices();
		}

		List<Patch> GetVisibleSortedPatches()
		{
			List<Patch> ret = new List<Patch>();
			for(int y = 0; y < NUM_PATCHES_PER_SIDE; y++)
				for (int x = 0; x < NUM_PATCHES_PER_SIDE; x++)
				{
					Patch p = Patches[y, x];
					if (p.IsVisible)
					{
						p.distanceToCamera = CalculatePatchDistance(p);
						ret.Add(p);
					}
				}
			ret.Sort((p1, p2) => { return p1.distanceToCamera.CompareTo(p2.distanceToCamera); });
			return ret;
		}

		float CalculatePatchDistance(Patch p)
		{
			int centerX = (p.worldX + p.worldX+PATCH_SIZE) >> 1;
			int centerY = (p.worldY + p.worldX + PATCH_SIZE) >> 1;
			int centerZ = FetchFunction(centerX, centerY);
			Vector3 center = new Vector3(centerX, centerZ, centerY);
			float distance = (center - CameraPos).Length();
			return distance;
		}


		public void Init()
		{
			MaxTris = NUM_PATCHES_PER_SIDE * NUM_PATCHES_PER_SIDE * 4;
			nodeList = new TriTreeNode[MaxTris];

			for (int i = 0; i < nodeList.Length; i++)
			{
				nodeList[i] = new TriTreeNode();
				nodeList[i].Self = i;
			}

			Patches = new Patch[NUM_PATCHES_PER_SIDE, NUM_PATCHES_PER_SIDE];

			for(int y = 0; y < NUM_PATCHES_PER_SIDE; y++)
				for (int x = 0; x < NUM_PATCHES_PER_SIDE; x++)
				{
					Patch p = new Patch(this);
					p.Init(x*PATCH_SIZE, y * PATCH_SIZE, x * PATCH_SIZE, y * PATCH_SIZE);
					Patches[y, x] = p;
				}
			this.Reset();
			this.RecreateVertices();
		}


		public void Reset()
		{
			allocatedTris = 0;

			foreach (TriTreeNode node in nodeList)
			{
				node.BaseNeighbor = 0;
				node.LeftNeighbor = 0;
				node.RightNeighbor = 0;
				node.LeftChild = 0;
				node.RightChild = 0;
			}

			for(int y = 0; y < NUM_PATCHES_PER_SIDE; y++)
				for (int x = 0; x < NUM_PATCHES_PER_SIDE; x++)
				{
					Patch p = Patches[y, x];
					p.Reset();
					p.IsVisible = IsPatchVisible(p);

					if (p.IsVisible)
					{
						if (x > 0)
							p.BaseLeft.LeftNeighbor = Patches[y, x - 1].BaseRight.Self;
						else
							p.BaseLeft.LeftNeighbor = 0;

						if (x < (NUM_PATCHES_PER_SIDE - 1))
							p.BaseRight.LeftNeighbor = Patches[y, x + 1].BaseLeft.Self;
						else
							p.BaseRight.LeftNeighbor = 0;

						if (y > 0)
							p.BaseLeft.RightNeighbor = Patches[y - 1, x].BaseRight.Self;
						else
							p.BaseLeft.RightNeighbor = 0;

						if (y < (NUM_PATCHES_PER_SIDE - 1))
							p.BaseRight.RightNeighbor = Patches[y + 1, x].BaseLeft.Self;
						else
							p.BaseRight.RightNeighbor = 0;
					}
					allocatedTris += 2;
				}
		}

		Random rand = new Random(100);

		bool IsPatchVisible(Patch p)
		{
			bool b = VisibleFunction(p.worldX, p.worldY + this.PATCH_SIZE,
								p.worldX + this.PATCH_SIZE, p.worldY,
								p.worldX, p.worldY);
			b |= VisibleFunction(p.worldX + this.PATCH_SIZE, p.worldY,
								p.worldX, p.worldY + this.PATCH_SIZE,
								p.worldX + this.PATCH_SIZE, p.worldY + this.PATCH_SIZE);
			
			return b;
		}


		public int GetNextNode()
		{
			allocatedTris++;
			if (allocatedTris >= nodeList.Length)
				return 0;
			TriTreeNode node = this[allocatedTris];
			node.LeftChild = node.RightChild = 0;
			node.LeftNeighbor = node.RightNeighbor = node.BaseNeighbor = 0;
			node.Variance = 0;
			return allocatedTris;
		}

		public void RecreateVertices()
		{
			List<Vertex> vBuf = this.Render();

			int nVerts = vBuf.Count;
			this.Vertices = vBuf.ToArray();
			Indices = new int[nVerts];
			for (int i = 0; i < nVerts; i++)
				Indices[i] = i;
			this.UploadToGpu();
		}

		public List<Vertex> Render()
		{
			List<Vertex> vBuf = new List<Vertex>();
			for (int y = 0; y < NUM_PATCHES_PER_SIDE; y++)
				for (int x = 0; x < NUM_PATCHES_PER_SIDE; x++)
				{
					Patch p = Patches[y, x];
					if (p.IsVisible)
						p.Render(vBuf);
				}

			if (allocatedTris > desiredTris)
				FrameVariance *= 1.2f;
			else if (allocatedTris < desiredTris)
				FrameVariance /= 1.2f;
			FrameVariance = MathExtensions.Clamp(FrameVariance, 0.001f, 10000);


			return vBuf;
		}

		public int NumVertices()
		{
			int num = 0;
			foreach (TriTreeNode node in this)
				num++;

			return num;
		}

		public System.Collections.Generic.IEnumerator<TriTreeNode> GetEnumerator()
		{
			for (int i = 1; i < nodeList.Length; i++)
			{
				TriTreeNode node = nodeList[i];
				if (node.Self == 0) break;
				if (node.LeftNeighbor == 0 && node.RightNeighbor == 0 && node.BaseNeighbor == 0) break;
				if (node.RightChild == 0 && node.LeftChild == 0)
					yield return node;
			}
		}

		public override void BindToPass(D3DDevice device, Effect effect, int passIndex)
		{
			this.Init();
			this.Reset();
			this.Tessellate();
			base.BindToPass(device, effect, passIndex);
		}


		public static bool AlwaysFalse(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			return false;
		}
		public static bool AlwaysTrue(int leftX, int leftY, int rightX, int rightY, int apexX, int apexY)
		{
			return true;
		}
		public static short FetchZero(int worldX, int worldY)
		{
			return 0;
		}
	}
}
