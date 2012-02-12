using System;
using System.Collections.Generic;
using System.ComponentModel;
using SlimDX;
using SlimDX.Direct3D10;

namespace Direct3DLib
{
	[TypeConverter(typeof(DTALib.BasicTypeConverter))]
    public class VertexList : List<Vertex>
    {
        private PrimitiveTopology mTopology = PrimitiveTopology.TriangleList;
        public PrimitiveTopology Topology { get { return mTopology; } set { mTopology = value; } }
        public List<int> Indices { get; set; }
        public int NumElements { get { if (Indices == null || Indices.Count < 1) return this.Count; return Indices.Count; } }
        public int NumBytes
        {
            get
            {
                return System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vertex))*this.Count;
            }
        }
		public VertexList(List<Vertex> vertices)
			: this(vertices.Count)
		{
			this.AddRange(vertices);
		}
		public VertexList(Vertex[] vertices)
			: this(vertices.Length)
		{
			this.AddRange(vertices);
		}
        public VertexList() : this(64) { }
        public VertexList(int capacity) : base(capacity) { }

		public Vector3[] GetVertexPositions()
		{
			Vector3[] vects = new Vector3[this.Count];
			for (int i = 0; i < vects.Length; i++)
				vects[i] = this[i].Position;
			return vects;
		}

		public override string ToString()
		{
			return "["+this.Count+"] "+this.GetType().Name;
		}

    }
}
