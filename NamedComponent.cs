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
	public class NamedComponent : Component
	{
		#region Default Component Constructors
		private bool designTime = false;
		public bool DesignTime { get { return designTime || this.DesignMode; } }
		public NamedComponent()
			: base()
		{
			this.InitializeComponent();
		}
		public NamedComponent(IContainer container)
			: base()
		{
			container.Add(this);
			this.InitializeComponent();
		}

		private void InitializeComponent()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				designTime = true;
			
		}
		#endregion

		/*
		#region Design Time Properties available at Run Time
		
		private string name;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(false)]
		public string CodeName
		{
			get
			{
				if (ToString().IndexOf(' ') != -1)
					name = GetName();
				return name;
			}
			set
			{
				if (ToString().IndexOf(' ') != -1)
					name = GetName();
				else
					name = value;
			}
		}
		public string GetName()
		{
			int split = ToString().IndexOf(' ');
			return ToString().Substring(0, split);
		}
		private IComponent parent;
		[Browsable(false)]
		public IComponent Parent
		{
			get {
				if (parent == null)
				{
					parent = GetRootComponent();
				}
				return parent;
			}
			set
			{
				if (!designTime)
				{
					if (parent != null && parent != value)
						throw new InvalidOperationException("Can't set ParentForm at Run Time.");
				}
				else
					parent = value;
			}
		}
		private IComponent GetRootComponent()
		{
			IDesignerHost designer = GetHost();
			if (designer == null || designer.RootComponent == null) return null;
			return designer.RootComponent;
		}
		private IDesignerHost GetHost()
		{
			if (!designTime) return null;
			IDesignerHost designer = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
			return designer;
		}
		#endregion

		 */

	}
}
