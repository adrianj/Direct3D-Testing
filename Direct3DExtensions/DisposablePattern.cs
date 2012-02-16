using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Direct3DExtensions
{
	public class DisposablePattern : IDisposable
	{
		private bool disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					/* Free resources
					if(resource != null) resource.Dispose();
					resource = null;
					 */
				}
				this.disposed = true;
			}
			/* inheritors should call this:
			 base.Dispose(disposing);
			 */
		}
	}
}
