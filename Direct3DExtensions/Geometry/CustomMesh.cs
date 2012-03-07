using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions
{
	public class CustomMesh : BasicMesh
	{
		public void SetVertexData<T>(IEnumerable<Vector3> vBuf, IEnumerable<int> iBuf) where T : Vertex
		{
			base.SetVertexPositionData<T>(vBuf.ToArray(), iBuf.ToArray());
		}

	}
}
