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
		void Draw();
		void Clear();
		void Add(Mesh mesh);
		void Remove(Mesh mesh);
	}

	public class BasicGeometry : DisposablePattern, Geometry
	{
		[TypeConverter(typeof(CollectionConverter))]
		public List<Mesh> Meshes { get; private set; }

		public BasicGeometry()
		{
			Meshes = new List<Mesh>();
		}

		public void Add(Mesh mesh)
		{
			Meshes.Add(mesh);
		}

		public void Remove(Mesh mesh)
		{
			if (!Meshes.Contains(mesh)) return;
			Meshes.Remove(mesh);
		}

		public void Clear()
		{
			foreach (Mesh mesh in Meshes)
				mesh.Dispose();
			Meshes.Clear();
		}

		public void Draw()
		{
			foreach (Mesh mesh in Meshes)
				mesh.Draw();
		}

		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Clear();
				}
				this.disposed = true;
			}
			base.Dispose(disposing);
		}


	}
}
