using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Reflection;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Resources;
using System.IO;
using System.Collections.Generic;

namespace Direct3DLib
{
	[ToolboxItem(true)]
	public class ComplexShape : Shape
	{
		private string sourceFile = "";
		public string SourceFile
		{
			get { return sourceFile; }
			set
			{
				SetShape(value);
			}
		}

		private void SetShape(string value)
		{
			bool success = SetShapeFromResource(value);
			if (!success)
				success = SetShapeFromFile(value);
			if (!success)
				return;
			this.Update();
			sourceFile = value;
		}
		
	
		private bool SetShapeFromResource(string resourcePath)
		{
			try
			{
				using (Stream stream = FindResourceStream(resourcePath))
				{
					using (Shape s = ComplexShapeFactory.CreateFromStream(stream, resourcePath))
						this.Vertices = s.Vertices;
				}
				return true;
			}
			catch (MissingManifestResourceException) { }
			catch (Exception ex) { MessageBox.Show("" + ex);  }
			return false;
		}

		private Stream FindResourceStream(string resourcePath)
		{
			Assembly asm = Assembly.GetEntryAssembly();
			string resource = FindMatchingResourcePath(resourcePath, asm);
			if (String.IsNullOrEmpty(resource))
			{
				asm = Assembly.GetExecutingAssembly();
				resource = FindMatchingResourcePath(resourcePath, asm);
			}
			if (String.IsNullOrEmpty(resource))
			{
				asm = Assembly.GetCallingAssembly();
				resource = FindMatchingResourcePath(resourcePath, asm);
			}
			if (String.IsNullOrEmpty(resource))
				throw new MissingManifestResourceException("Resource not found in manifest '" + resourcePath + "'");
			return asm.GetManifestResourceStream(resource);
		}

		private string FindMatchingResourcePath(string resourcePath, Assembly asm)
		{
			if (asm == null) return "";
			List<string> res = new List<string>(asm.GetManifestResourceNames());
			if(res.Contains(resourcePath)) return resourcePath;
			resourcePath = resourcePath.Replace(Path.DirectorySeparatorChar, '.');
			resourcePath = resourcePath.Replace(Path.AltDirectorySeparatorChar, '.');
			if (res.Contains(resourcePath)) return resourcePath;
			resourcePath = asm.GetName().Name + "." + resourcePath;
			if (res.Contains(resourcePath)) return resourcePath;
			return "";
		}

		private bool SetShapeFromFile(string filePath)
		{
			try
			{
				Shape s = ComplexShapeFactory.CreateFromFile(filePath);
				if (s != null)
				{
					this.Vertices = s.Vertices;
					return true;
				}
			}
			catch (FileNotFoundException ex) { HandleException(ex); }
			catch (DirectoryNotFoundException ex) { HandleException(ex); }
			catch (ArgumentException ex) { HandleException(ex); }
			if (this.DesignTime) return true; 
			return false;
		}

		private void HandleException(Exception ex)
		{
			if (!this.DesignTime)
				MessageBox.Show("" + ex);
		}

		public ComplexShape()
			: base()
		{		}
	}

}
