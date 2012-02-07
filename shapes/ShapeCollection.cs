using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Reflection;

namespace Direct3DLib
{
	public class ShapeCollection : Shape
	{
		private List<IndexedShape> allShapes = new List<IndexedShape>();

		public ShapeCollection()
			: base()
		{

		}

		public List<Shape> ShapeList
		{
			get
			{
				List<Shape> ret = new List<Shape>();
				foreach (IndexedShape ishape in allShapes)
					ret.Add(ishape.shape);
				return ret;
			}
		}

		public void AddShape(Shape s)
		{
			IndexedShape ishape = new IndexedShape(s, this.Vertices.Count);
			if (!allShapes.Contains(ishape))
			{
				AddVertices(s);
				allShapes.Add(ishape);
				s.ShapeUpdated += new ShapeChangeEventHandler(s_ShapeUpdated);
				this.Update();
			}
		}

		void s_ShapeUpdated(object sender, ShapeChangeEventArgs e)
		{
			RemoveShape(e.ChangedShape);
			AddShape(e.ChangedShape);
		}

		private void AddVertices(Shape s)
		{
			for(int i = 0; i < s.Vertices.Count; i+=3)
			{
				Vertex[] triangle = Vertex.TransformTriangle(new Vertex[] { s.Vertices[i], s.Vertices[i+1], s.Vertices[i+2] }, s.World);
				this.Vertices.AddRange(triangle);
			}
		}

		public bool RemoveShape(Shape s)
		{
			IndexedShape ishape = new IndexedShape(s, 0);
			bool ret = allShapes.Remove(ishape);
			if (ret)
			{
				s.ShapeUpdated -= s_ShapeUpdated;
				RebuildList();
				this.Update();
			}
			return ret;
		}

		private void RebuildList()
		{
			this.Vertices.Clear();
			foreach (IndexedShape s in allShapes)
			{
				s.offset = this.Vertices.Count;
				AddVertices(s.shape);
			}
		}

		public void ClearAndDispose()
		{
			foreach (IndexedShape s in allShapes)
			{
				Shape shape = s.shape;
				shape.Dispose();
			}
			allShapes.Clear();
			this.Vertices.Clear();
			this.Update();
		}

		public Shape SelectedShape
		{
			get
			{
				if (allShapes.Count < 1) return null;

				foreach (IndexedShape ishape in allShapes)
				{
					//Console.WriteLine("" + ishape.offset + ", " + SelectedVertexIndex + ", " + ishape.shape.Vertices.Count);
					if (SelectedVertexIndex < ishape.offset + ishape.shape.Vertices.Count)
						return ishape.shape;
				}
				return allShapes[0].shape;
			}
		}

		public override bool RayIntersects(SlimDX.Ray ray, out float distance)
		{
			bool ret = base.RayIntersects(ray, out distance);

			return ret;
		}

		protected override void Dispose(bool disposing)
		{
			foreach (IndexedShape ishape in allShapes)
				ishape.shape.Dispose();
			base.Dispose(disposing);
		}
	}

	internal class IndexedShape
	{
		public Shape shape { get; set; }
		public int offset { get; set; }
		public IndexedShape(Shape s, int index)
		{
			this.shape = s;
			this.offset = index;
		}
		public override bool Equals(object obj)
		{
			if (obj is IndexedShape)
				return shape.Equals((obj as IndexedShape).shape);
			return false;
		}
		public override int GetHashCode()
		{
			return shape.GetHashCode();
		}
	}

	public class ShapeCollectionEditor : DTALib.ObjectCollectionEditor
	{
		public ShapeCollectionEditor(Type type) : base(type) { }
		protected override IEnumerable<Type> GetTypes()
		{
			List<Type> types = new List<Type>();
			Assembly asm = Assembly.GetExecutingAssembly();
			if (asm != null)
				types.AddRange(GetShapesInAssembly(asm));
			asm = Assembly.GetEntryAssembly();
			if (asm != null)
				types.AddRange(GetShapesInAssembly(asm));
			return types.ToArray();
		}

		private List<Type> GetShapesInAssembly(Assembly asm)
		{
			List<Type> ret = new List<Type>();
			foreach (Type t in asm.GetTypes())
			{
				if (t.IsSubclassOf(typeof(Shape)))
					if (IsToolBoxItem(t))
						ret.Add(t);
			}
			return ret;
		}

		bool IsToolBoxItem(Type type)
		{
			return true;
		}
	}

}


