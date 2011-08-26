using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;

namespace Direct3DLib
{
	/// <summary>
	/// This class adds some support for uniquely named components.
	/// </summary>
	[ToolboxItem(false)]
	//[TypeConverter(typeof(BasicTypeConverter))]
	public class NamedComponent : System.ComponentModel.Component
	{
		#region Default Component Constructors
		private bool designTime = false;
		[Category("Design")]
		public bool DesignTime { get { return designTime || this.DesignMode; } }
		public NamedComponent()
			: base()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				designTime = true;
			this.InitializeComponent();
		}

		public NamedComponent(IContainer container)
			: base()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				designTime = true;
			container.Add(this);
			this.InitializeComponent();
		}


		private void InitializeComponent()
		{
		}
		#endregion

		private string name;
		public string Name
		{
			get
			{
				if (this.Site != null)
					name = this.Site.Name;
				return name;
			}
			set
			{
				if (this.Site != null) name = this.Site.Name;
				else name = value; 
			}
		}



	}
}
