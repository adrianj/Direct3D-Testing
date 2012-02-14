using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Direct3DExtensions
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface Geometry : IDisposable
	{
		void Init(D3DDevice device, Effect effect, int passNumber);
		void Draw();
		List<Mesh> Meshes {get;}
	}

	public class BasicGeometry : Geometry
	{
		public List<Mesh> Meshes { get; private set; }

		public BasicGeometry()
		{
			Meshes = new List<Mesh>();
		}

		public void Init(D3DDevice device, Effect effect, int passNumber)
		{
			foreach (Mesh mesh in Meshes)
				mesh.BindToPass(device,effect[passNumber]);
		}


		public void Draw()
		{
			foreach (Mesh mesh in Meshes)
				mesh.Draw();
		}

		public void Dispose()
		{
			foreach (Mesh mesh in Meshes)
				mesh.Dispose();
			Meshes.Clear();
		}

	}
}
