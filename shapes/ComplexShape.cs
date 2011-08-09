using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace Direct3DLib
{
	public class ComplexShape : Shape
	{
		private bool shapeIsValid = false;
		private string sourceFile = "";
		[EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public string SourceFile
		{
			get { return sourceFile; }
			set
			{
				sourceFile = value;
				try
				{
					Shape s = ComplexShapeFactory.CreateFromFile(sourceFile);
					if (s != null)
					{
						this.Vertices = s.Vertices;
						shapeIsValid = true;
					}
				}
				catch (System.IO.FileNotFoundException ex) { MessageBox.Show("" + ex); }
			}
		}

		public override void Update(SlimDX.Direct3D10.Device device, SlimDX.D3DCompiler.ShaderSignature effectSignature)
		{
			if(shapeIsValid)
				base.Update(device, effectSignature);
		}

		public ComplexShape()
			: base()
		{		}
	}
}
