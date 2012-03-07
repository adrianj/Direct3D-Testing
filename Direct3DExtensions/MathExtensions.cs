using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;

namespace Direct3DExtensions
{
	public static class MathExtensions
	{
		public enum OnScreen { Yes, No, Partial, BackFacing };

		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
		{
			if (val.CompareTo(min) < 0) return min;
			else if (val.CompareTo(max) > 0) return max;
			else return val;
		}

		public static bool Orientation(Vector3 v0Wvp, Vector3 v1Wvp, Vector3 v2Wvp)
		{
			Plane p = new Plane(v0Wvp, v1Wvp, v2Wvp);
			Vector3 norm = p.Normal;
			bool b = norm.Z > 0;
			return b;
		}

		public static double PowerOfTwo(double f, double minValue)
		{
			double r = minValue;
			if (f <= minValue)
				return r;
			while (r < f)
				r = r * 2;
			return r;
		}

		public static int PowerOfTwo(int i)
		{
			if (i <= 0)
				return 1;
			int r = 1;
			while (r < i)
				r = r << 1;
			return r;
		}

		public static int Log2(int i)
		{
			if (i <= 1)
				return 0;
			int ret = 0;
			for (int x = 1; x < i; x *= 2)
				ret++;
			return ret;
		}

		public static float TriangleArea(Vector3 v0, Vector3 v1, Vector3 v2)
		{
			Vector3 ab = (v0 - v1);
			Vector3 ac = (v0 - v2);
			Vector3 cross = Vector3.Cross(ab, ac);
			float area = cross.Length() * 0.5f;
			return area;
		}


		static Matrix world = Matrix.Identity;
		static Matrix view = Matrix.Identity;
		static Matrix proj = Matrix.Identity;
		static Matrix wvp = Matrix.Identity;
		public static bool VertexOnScreen(Vector3 pos, Mesh mesh, Camera camera, out Vector3 posWvp)
		{
			CheckForRepeatedTransformation(mesh, camera);
			return VertexOnScreen(pos, wvp, out posWvp);
		}

		private static void CheckForRepeatedTransformation(Mesh mesh, Camera camera)
		{
			if (mesh.World != world || camera.View != view || camera.Projection != proj)
			{
				world = mesh.World;
				view = camera.View;
				proj = camera.Projection;
				wvp = Matrix.Multiply(world, view);
				wvp = Matrix.Multiply(wvp, proj);
			}
		}

		public static bool VertexOnScreen(Vector3 pos, Matrix wvp, out Vector3 posWvp)
		{
			posWvp = Vector3.TransformCoordinate(pos, wvp);
			return VertexOnScreen(posWvp);
		}

		public static bool VertexOnScreen(Vector3 posWvp)
		{
			return posWvp.X <= 1 && posWvp.X >= -1
				   && posWvp.Y <= 1 && posWvp.Y >= -1
				   && posWvp.Z <= 1 && posWvp.Z >= 0;
		}

		public static OnScreen TriangleOnScreen(Vector3 v0, Vector3 v1, Vector3 v2, Mesh mesh, Camera camera, out Vector3 v0Wvp, out Vector3 v1Wvp, out Vector3 v2Wvp)
		{
			CheckForRepeatedTransformation(mesh, camera);
			v0Wvp = Vector3.TransformCoordinate(v0,wvp);
			v1Wvp = Vector3.TransformCoordinate(v1, wvp);
			v2Wvp = Vector3.TransformCoordinate(v2, wvp);
			return TriangleOnScreen(v0Wvp, v1Wvp, v2Wvp);
		}

		public static OnScreen TriangleOnScreen(Vector3 v0, Vector3 v1, Vector3 v2, Mesh mesh, Camera camera)
		{
			Vector3 v0wvp;
			Vector3 v1wvp;
			Vector3 v2wvp;
			return TriangleOnScreen(v0, v1, v2, mesh, camera, out v0wvp, out v1wvp, out v2wvp);
		}

		public static OnScreen TriangleOnScreen(Vector3 v0Wvp, Vector3 v1Wvp, Vector3 v2Wvp)
		{
			int numOnScreen = 0;
			if (VertexOnScreen(v0Wvp)) numOnScreen++;
			if (VertexOnScreen(v1Wvp)) numOnScreen++;
			if (VertexOnScreen(v2Wvp)) numOnScreen++;

			if (!Orientation(v0Wvp,v1Wvp,v2Wvp))
				return OnScreen.BackFacing;
			if (numOnScreen == 0)
				return OnScreen.No;
			if (numOnScreen == 3)
				return OnScreen.Yes;
			return OnScreen.Partial;
		}


		public static bool TriangleFullyOnScreen(Vector3 v0, Vector3 v1, Vector3 v2, Mesh mesh, Camera camera, out Vector3 v0Wvp, out Vector3 v1Wvp, out Vector3 v2Wvp)
		{
			CheckForRepeatedTransformation(mesh, camera);
			v0Wvp = Vector3.TransformCoordinate(v0, wvp);
			v1Wvp = Vector3.TransformCoordinate(v1, wvp);
			v2Wvp = Vector3.TransformCoordinate(v2, wvp);
			return TriangleFullyOnScreen(v0Wvp, v1Wvp, v2Wvp);
		}

		public static bool TriangleFullyOnScreen(Vector3 v0, Vector3 v1, Vector3 v2, Mesh mesh, Camera camera)
		{
			Vector3 v0wvp;
			Vector3 v1wvp;
			Vector3 v2wvp;
			return TriangleFullyOnScreen(v0, v1, v2, mesh, camera, out v0wvp, out v1wvp, out v2wvp);
		}

		public static bool TriangleFullyOnScreen(Vector3 v0Wvp, Vector3 v1Wvp, Vector3 v2Wvp)
		{
			return VertexOnScreen(v0Wvp) || VertexOnScreen(v1Wvp) || VertexOnScreen(v2Wvp);
		}



		public static void AppendVertices(List<Vector3> destVertexBuffer, List<int> destIndexBuffer, List<Vector3> srcVertexBuffer, List<int> srcIndexBuffer)
		{
			int iOffset = destVertexBuffer.Count;
			destVertexBuffer.AddRange(srcVertexBuffer);
			for (int i = 0; i < srcIndexBuffer.Count; i++)
			{
				destIndexBuffer.Add(iOffset + srcIndexBuffer[i]);
			}
		}

		public static void ReflectVerticesThroughXAxis(List<Vector3> srcVertexBuffer, List<int> srcIndexBuffer, out List<Vector3> destVertexBuffer, out List<int> destIndexBuffer)
		{
			destIndexBuffer = new List<int>(srcIndexBuffer);
			destVertexBuffer = new List<Vector3>();
			for (int i = 0; i < srcVertexBuffer.Count; i++)
			{
				Vector3 src = srcVertexBuffer[i];
				destVertexBuffer.Add(new Vector3(-src.X, src.Y, src.Z));
			}
			destIndexBuffer.Reverse();
		}

		public static void TransformVertices(List<Vector3> srcVertexBuffer, out List<Vector3> destVertexBuffer, Matrix transMatrix)
		{
			destVertexBuffer = new List<Vector3>(srcVertexBuffer.Count);
			foreach (Vector3 v in srcVertexBuffer)
			{
				destVertexBuffer.Add(Vector3.TransformCoordinate(v, transMatrix));
			}
		}

		public static float Max<T>(T[,] array) where T : IConvertible
		{
			float max = float.MinValue;
			foreach (IConvertible c in (array as Array))
			{
				float f = Convert.ToSingle(c);
				if (f > max)
					max = f;
			}
			return max;
		}

		public static float Min<T>(T[,] array) where T : IConvertible
		{
			float min = float.MaxValue;
			foreach (IConvertible c in (array as Array))
			{
				float f = Convert.ToSingle(c);
				if (f < min)
					min = f;
			}
			return min;
		}
	}
}
