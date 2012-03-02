using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Direct3DExtensions;


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
			TestExpandablePlaneTerrain te = new TestExpandablePlaneTerrain();
			te.SetUp();
			te.TestExpandablePlane();
			//te.TestTriangle();
			//TestLandscapeManager tm = new TestLandscapeManager();
			//tm.SetUp();
			//tm.Test();
			//tm.TearDown();
			//TerrainTest lt = new TerrainTest();
			//lt.SetUp();
			//lt.AutoSplitTest();
			//lt.TearDown();
			//RepeatabiliyTest rt = new RepeatabiliyTest();
			//rt.SetUp();
			//rt.KeepAddingShapes();
			//rt.TearDown();
			//CustomFormTest test = new CustomFormTest();
			//test.SetUp();
			//test.Test();
			//Basic3DControl_Test test = new Basic3DControl_Test();
			//test.SetUp();
			//test.RunTextured();
			//test.RunWithError();
			//test.TestAddCustomGeometry();
			//test.TearDown();

		}
	}
}
