using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.CodeDom.Compiler;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoDevelop.HaxeBinding.Tools;


namespace MonoDevelop.HaxeBinding.Projects
{

	public class HaxeProjectConfiguration : ProjectConfiguration
	{
		
		[ItemProperty("AdditionalArguments", DefaultValue="")]
		string mAdditionalArguments = string.Empty;

		public string AdditionalArguments {
			get { return mAdditionalArguments;  }
			set { mAdditionalArguments = value; }
		}

		[ItemProperty("HaxeExecuteTarget", DefaultValue="")]
		HaxeProjectTarget mHaxeProjectTarget;

		public HaxeProjectTarget HaxeProjectTarget {
			get { return mHaxeProjectTarget;  }
			set { mHaxeProjectTarget = value; }
		}

		[ItemProperty("OpenFLTarget", DefaultValue="")]
		OpenFLTarget? mOpenFLTarget;

		public OpenFLTarget? OpenFLTarget {
			get { return mOpenFLTarget;  }
			set { mOpenFLTarget = value; }
		}

		[ItemProperty("HaxeTarget", DefaultValue="")]
		HaxeTarget? mHaxeTarget;

		public HaxeTarget? HaxeTarget {
			get { return mHaxeTarget;  }
			set { mHaxeTarget = value; }
		}

		public override void CopyFrom (ItemConfiguration configuration)
		{
			base.CopyFrom (configuration);

			HaxeProjectConfiguration other = (HaxeProjectConfiguration)configuration;
			mAdditionalArguments = other.mAdditionalArguments;
			mOpenFLTarget = other.mOpenFLTarget;
		}
		
	}
	
}