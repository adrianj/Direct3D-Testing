using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;

namespace Direct3DLib
{
    [TypeConverter(typeof(VertexListTypeConverter))]
    public class VertexList : List<Vertex>
    {
        private PrimitiveTopology mTopology = PrimitiveTopology.TriangleList;
        public PrimitiveTopology Topology { get { return mTopology; } set { mTopology = value; } }
        public List<int> Indices { get; set; }
        public IEnumerator<Vertex> GetTopologyEnumerator()
        {
            return null;
        }
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
        public VertexList(Vertex[] vertices) : this(vertices.Length)
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

		/// <summary>
		/// Returns a list of vertices in a LIST order. Ie, for TriangleList then it is simply an iteration through
		/// the Indices, and the Vertices are provided in trios. The same applies for LineList and providing vertices
		/// as pairs.  TriangleStrip and LineStrip are not yet implemented.
		/// </summary>
		/// <returns></returns>
		public void UpdateNormals()
        {
            if (Topology == PrimitiveTopology.TriangleList && Indices != null)
            {
                for (int i = 0; i < Indices.Count; i += 3)
                {
                    Vertex v1 = this[Indices[i]];
                    Vertex v2 = this[Indices[i+1]];
                    Vertex v3 = this[Indices[i+2]];
                    Plane pl = new Plane(v1.Position, v2.Position, v3.Position);
                    Vector3 norm = pl.Normal;
                    v1.Normal = norm;
                    v2.Normal = norm;
                    v3.Normal = norm;

                    this[Indices[i]] = v1;
                    this[Indices[i + 1]] = v2;
                    this[Indices[i + 2]] = v3;
                }
            }
        }

    }

    public class VertexListTypeConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(VertexList));
        }
    }
}
