using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Direct3DExtensions;

using D3DControl = Direct3DExtensions.Basic3DControl;

namespace Direct3DExtensions_Test
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			HostForm form = new HostForm();
			form.SetControl(new Textured3DControl());
				Application.Run(form);
		}
	}
}
