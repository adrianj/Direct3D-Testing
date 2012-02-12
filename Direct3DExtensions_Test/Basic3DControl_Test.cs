using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

using Direct3DExtensions;

using D3DControl = Direct3DExtensions.Basic3DControl;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class Basic3DControl_Test
	{
		bool windowSet = false;

		[SetUp]
		public void SetUp()
		{
			Application.EnableVisualStyles();
			if (!windowSet)
			{
				Application.SetCompatibleTextRenderingDefault(false);
				windowSet = true;
			}
		}

		[Test]
		public void RunBasic()
		{
			try
			{
				HostForm form = new HostForm();
				D3DControl con = new D3DControl();
				form.SetControl(con);
				form.Text = "Run Basic";
				con.BackColor = System.Drawing.Color.LightSteelBlue;
				Assert.That(con.IsDesignMode, Is.Not.True);
				Application.Run(form);
				Assert.That(true);
			}
			catch (Exception)
			{
				Assert.That(false);
			}
		}

		[Test]
		public void RunTextured()
		{
			try
			{
				HostForm form = new HostForm();
				D3DControl con = new Textured3DControl();
				form.SetControl(con);
				form.Text = "Run Textured";
				con.BackColor = System.Drawing.Color.LightSteelBlue;
				Assert.That(con.IsDesignMode, Is.Not.True);
				Application.Run(form);
				Assert.That(true);
			}
			catch (Exception)
			{
				Assert.That(false);
			}
		}

	}
}
