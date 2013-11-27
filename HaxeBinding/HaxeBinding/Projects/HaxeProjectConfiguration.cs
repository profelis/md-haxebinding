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

		// HaxeProjectConfigurationPanel
		[ItemProperty("AdditionalArguments", DefaultValue="")]
		string mAdditionalArguments = string.Empty;

		public string AdditionalArguments {
			get { return mAdditionalArguments;  }
			set { mAdditionalArguments = value; }
		}

		[ItemProperty("HaxeExecuteTarget", DefaultValue="")]
		int mHaxeProjectTarget;

		public HaxeProjectTarget HaxeProjectTarget {
			get { return (HaxeProjectTarget)mHaxeProjectTarget;  }
			set { mHaxeProjectTarget = (int)value; }
		}

		[ItemProperty("OpenFLTarget", DefaultValue="")]
		int? mOpenFLTarget = null;

		public OpenFLTarget? OpenFLTarget {
			get { return (OpenFLTarget)mOpenFLTarget;  }
			set { mOpenFLTarget = (int)value; }
		}

		[ItemProperty("HaxeTarget", DefaultValue="")]
		int? mHaxeTarget = null;

		public HaxeTarget? HaxeTarget {
			get { return (HaxeTarget)mHaxeTarget;  }
			set { mHaxeTarget = (int)value; }
		}

		public override void CopyFrom (ItemConfiguration configuration)
		{
			base.CopyFrom (configuration);

			HaxeProjectConfiguration other = (HaxeProjectConfiguration)configuration;
			mAdditionalArguments = other.mAdditionalArguments;
			mHaxeProjectTarget = other.mHaxeProjectTarget;
			mOpenFLTarget = other.mOpenFLTarget;
			mHaxeTarget = other.mHaxeTarget;
		}
		
	}
	
}