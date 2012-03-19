using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions.Texturing
{
	public class ResourceView : DisposablePattern
	{
		public D3D.Resource Resource { get; protected set; }
		public D3D.ShaderResourceView View { get; protected set; }
		protected D3D.Device device;

		

		public ResourceView(D3D.Device device)
		{
			this.device = device;
		}

		void DisposeManaged() { }
		void DisposeUnmanaged()
		{
			if (Resource != null) Resource.Dispose(); Resource = null;
			if (View != null) View.Dispose(); View = null;
		}

		bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}

				DisposeUnmanaged();
				disposed = true;
			}
			base.Dispose(disposing);
		}
    
	}
}
