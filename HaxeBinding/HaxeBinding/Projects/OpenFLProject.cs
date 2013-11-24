using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.CodeDom.Compiler;
using System.Diagnostics;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoDevelop.HaxeBinding.Tools;
using HaxeBinding;

namespace MonoDevelop.HaxeBinding.Projects
{
	public class OpenFLProject : HaxeProject
	{
		public OpenFLProject () : base()
		{
		}

		public OpenFLProject (ProjectCreateInformation info, XmlElement projectOptions) : base(info, projectOptions)
		{
		}
	}
}

