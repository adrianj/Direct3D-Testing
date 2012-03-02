using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using DXGI = SlimDX.DXGI;
using D3D9 = SlimDX.Direct3D9;
using D3D10 = SlimDX.Direct3D10;

namespace Direct3DExtensions
{
	public class MeshConverter : DisposablePattern, IDisposable
	{
		System.Windows.Forms.Form form;
		D3D9.Device device9;

		public D3D9.Device Device9 { get { return device9; } }

		public MeshConverter()
		{
			CreateNullDevice();
		}

		public D3D10.Mesh CreateMesh(D3D10.Device device, D3D9.Mesh mesh9, out D3D10.InputElement[] outDecls)
		{
			var inDecls = mesh9.GetDeclaration();
			outDecls = new D3D10.InputElement[inDecls.Length - 1];
			ConvertDecleration(inDecls, outDecls);

			var flags = D3D10.MeshFlags.None;
			if ((mesh9.CreationOptions & D3D9.MeshFlags.Use32Bit) != 0)
				flags = D3D10.MeshFlags.Has32BitIndices;

			var mesh = new D3D10.Mesh(device, outDecls, D3D9.DeclarationUsage.Position.ToString().ToUpper(), mesh9.VertexCount, mesh9.FaceCount, flags);

			ConvertIndexBuffer(mesh9, mesh);
			ConvertVertexBuffer(mesh9, mesh);
			ConfigureAttributeTable(mesh9, mesh);

			mesh.GenerateAdjacencyAndPointRepresentation(0);
			mesh.Optimize(D3D10.MeshOptimizeFlags.Compact | D3D10.MeshOptimizeFlags.AttributeSort | D3D10.MeshOptimizeFlags.VertexCache);

			mesh.Commit();
			return mesh;
		}

		public Mesh CreateMesh<VType>(D3D9.Mesh mesh9)
		{
			Mesh ret = new BasicMesh();
			Vertex[] buffer = ExtractVertices(mesh9);
			ret.Vertices = buffer;

			int[] indices = ExtractIndices(mesh9);
			ret.Indices = indices;

			return ret;
		}

		private static Vertex[] ExtractVertices(D3D9.Mesh mesh9)
		{
			VertexTypes.PositionNormal[] buffer;
			using (SlimDX.DataStream stream = mesh9.LockVertexBuffer(D3D9.LockFlags.None))
			{
				buffer = stream.ReadRange<VertexTypes.PositionNormal>(mesh9.VertexCount);
				stream.Close();
				stream.Dispose();
			}
			mesh9.UnlockVertexBuffer();
			return buffer.Cast<Vertex>().ToArray();
		}

		private static int[] ExtractIndices(D3D9.Mesh mesh9)
		{
			int[] buffer;
			using (SlimDX.DataStream stream = mesh9.LockIndexBuffer(D3D9.LockFlags.None))
			{
				ushort [] bffer = stream.ReadRange<ushort>(mesh9.FaceCount*3);
				buffer = bffer.Select<ushort, int>((i, o) => { return (int)i; }).ToArray();
				stream.Close();
				stream.Dispose();
			}
			mesh9.UnlockIndexBuffer();
			return buffer;
		}

		private static void ConfigureAttributeTable(D3D9.BaseMesh inMesh, D3D10.Mesh outMesh)
		{
			var inAttribTable = inMesh.GetAttributeTable();

			if (inAttribTable == null || inAttribTable.Length == 0)
			{
				outMesh.SetAttributeTable(new[] {new D3D10.MeshAttributeRange {
                    FaceCount = outMesh.FaceCount,
                    FaceStart = 0,
                    Id = 0,
                    VertexCount = outMesh.VertexCount,
                    VertexStart = 0
                }});
			}
			else
			{
				var outAttribTable = new D3D10.MeshAttributeRange[inAttribTable.Length];
				for (var i = 0; i < inAttribTable.Length; ++i)
				{
					outAttribTable[i].Id = inAttribTable[i].AttribId;
					outAttribTable[i].FaceCount = inAttribTable[i].FaceCount;
					outAttribTable[i].FaceStart = inAttribTable[i].FaceStart;
					outAttribTable[i].VertexCount = inAttribTable[i].VertexCount;
					outAttribTable[i].VertexStart = inAttribTable[i].VertexStart;
				}
				outMesh.SetAttributeTable(outAttribTable);
			}
			
			outMesh.GenerateAttributeBufferFromTable();
		}

		private static void ConvertIndexBuffer(D3D9.BaseMesh inMesh, D3D10.Mesh outMesh)
		{
			using (SlimDX.DataStream inStream = inMesh.LockIndexBuffer(D3D9.LockFlags.None))
			using (D3D10.MeshBuffer outBuffer = outMesh.GetIndexBuffer())
			{
				using (SlimDX.DataStream outStream = outBuffer.Map())
				{
					if ((outMesh.Flags & D3D10.MeshFlags.Has32BitIndices) != 0)
						outStream.WriteRange<int>(inStream.ReadRange<int>(inMesh.FaceCount * 3));
					else
						outStream.WriteRange<short>(inStream.ReadRange<short>(inMesh.FaceCount * 3));
				}
				outBuffer.Unmap();
			}
			inMesh.UnlockIndexBuffer();
		}

		private static void ConvertVertexBuffer(D3D9.BaseMesh inMesh, D3D10.Mesh outMesh)
		{
			using (SlimDX.DataStream inStream = inMesh.LockVertexBuffer(D3D9.LockFlags.None))
			using (D3D10.MeshBuffer outBuffer = outMesh.GetVertexBuffer(0))
			{
				using (SlimDX.DataStream outStream = outBuffer.Map())
				{
					outStream.WriteRange<byte>(inStream.ReadRange<byte>(inMesh.VertexCount * inMesh.BytesPerVertex));
				}
				outBuffer.Unmap();
			}
			inMesh.UnlockIndexBuffer();
		}


        private static void ConvertDecleration(D3D9.VertexElement[] inDecls, D3D10.InputElement[] outDecls) {
            for (var i = 0; i < inDecls.Length - 1; ++i) {
                outDecls[i].SemanticName = ConvertSemanticName(inDecls[i].Usage);
                outDecls[i].SemanticIndex = inDecls[i].UsageIndex;
                outDecls[i].AlignedByteOffset = inDecls[i].Offset;
                outDecls[i].Slot = inDecls[i].Stream;
				outDecls[i].Classification = D3D10.InputClassification.PerVertexData;
                outDecls[i].InstanceDataStepRate = 0;
                outDecls[i].Format = ConvertFormat(inDecls[i].Type);
            }
        }


		private static string ConvertSemanticName(D3D9.DeclarationUsage usage)
		{
			switch (usage)
			{
				case D3D9.DeclarationUsage.TextureCoordinate:
					return "TEXCOORD";
				case D3D9.DeclarationUsage.PositionTransformed:
					return "POSITIONT";
				case D3D9.DeclarationUsage.TessellateFactor:
					return "TESSFACTOR";
				case D3D9.DeclarationUsage.PointSize:
					return "PSIZE";
				default:
					return usage.ToString().ToUpper();
			}
		}

		private static DXGI.Format ConvertFormat(D3D9.DeclarationType type)
		{
			switch (type)
			{
				case D3D9.DeclarationType.Float1: return DXGI.Format.R32_Float;
				case D3D9.DeclarationType.Float2: return DXGI.Format.R32G32_Float;
				case D3D9.DeclarationType.Float3: return DXGI.Format.R32G32B32_Float;
				case D3D9.DeclarationType.Float4: return DXGI.Format.R32G32B32A32_Float;
				case D3D9.DeclarationType.Color: return DXGI.Format.R8G8B8A8_UNorm;
				case D3D9.DeclarationType.Ubyte4: return DXGI.Format.R8G8B8A8_UInt;
				case D3D9.DeclarationType.Short2: return DXGI.Format.R16G16_SInt;
				case D3D9.DeclarationType.Short4: return DXGI.Format.R16G16B16A16_SInt;
				case D3D9.DeclarationType.UByte4N: return DXGI.Format.R8G8B8A8_UNorm;
				case D3D9.DeclarationType.Short2N: return DXGI.Format.R16G16_SNorm;
				case D3D9.DeclarationType.Short4N: return DXGI.Format.R16G16B16A16_SNorm;
				case D3D9.DeclarationType.UShort2N: return DXGI.Format.R16G16_UNorm;
				case D3D9.DeclarationType.UShort4N: return DXGI.Format.R16G16B16A16_UNorm;
				case D3D9.DeclarationType.UDec3: return DXGI.Format.R10G10B10A2_UInt;
				case D3D9.DeclarationType.Dec3N: return DXGI.Format.R10G10B10A2_UNorm;
				case D3D9.DeclarationType.HalfTwo: return DXGI.Format.R16G16_Float;
				case D3D9.DeclarationType.HalfFour: return DXGI.Format.R16G16B16A16_Float;
				default: return DXGI.Format.Unknown;
			}
		}

		private void CreateNullDevice()
		{
			form = new System.Windows.Forms.Form();
			using (var direct3D = new D3D9.Direct3D())
				device9 = new D3D9.Device(direct3D, 0, D3D9.DeviceType.NullReference, form.Handle, D3D9.CreateFlags.HardwareVertexProcessing, new D3D9.PresentParameters
				{
					BackBufferCount = 1,
					BackBufferFormat = D3D9.Format.A8R8G8B8,
					BackBufferHeight = 1,
					BackBufferWidth = 1,
					SwapEffect = D3D9.SwapEffect.Copy,
					Windowed = true
				});
		}

		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (form != null) form.Dispose();
					form = null;
					if (device9 != null) device9.Dispose();
					device9 = null;
				}
				this.disposed = true;
			}
			base.Dispose(disposing);
		}
	}
}
