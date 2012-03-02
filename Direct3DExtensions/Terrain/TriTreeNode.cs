using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions.Terrain
{
	public delegate bool ShouldSplitDelegate(TriTreeNode node);

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class TriTreeNode
	{
		// Each of these integers refers to an index into an array of TriTreeNodes maintained by the Landscape class.
		public int Self;
		public int LeftNeighbor;
		public int RightNeighbor;
		public int BaseNeighbor;

		public int LeftChild;
		public int RightChild;

		public byte Variance;

		public bool IsLeaf()
		{
			if (Self == 0) return false;
			if (LeftChild == 0 && RightChild == 0) return true;
			return false;
		}


		public override string ToString()
		{
			return "" + Self + " N: " + LeftNeighbor + "," + RightNeighbor + "," + BaseNeighbor + " C: " + LeftChild + "," + RightChild;
		}
	}
}
