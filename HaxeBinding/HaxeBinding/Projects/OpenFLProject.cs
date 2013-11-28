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

		public override string ProjectType {
			get { return "OpenFL"; }
		}

		public override string[] SupportedLanguages {
			get { return new string[] { "", "Haxe", "OpenFL" }; }
		}

		protected override void buildConfigurations ()
		{
			string[] targets = new string[] {
				"Android",
				"BlackBerry",
				"Flash",
				"HTML5",
				"iOS",
				"Linux",
				"Mac",
				"webOS",
				"Windows",
				"Neko"
			};
			OpenFLTarget[] targetFlags = new OpenFLTarget[] {
				OpenFLTarget.Android,
				OpenFLTarget.BlackBerry,
				OpenFLTarget.Flash,
				OpenFLTarget.HTML5,
				OpenFLTarget.iOS,
				OpenFLTarget.Linux,
				OpenFLTarget.Mac,
				OpenFLTarget.webOS,
				OpenFLTarget.Windows,
				OpenFLTarget.Neko
			};

			HaxeProjectConfiguration configuration;

			for (int i = 0; i < targets.Length; i++) {
				string target = targets [i];
				OpenFLTarget targetFlag = targetFlags [i];
				configuration = (HaxeProjectConfiguration)CreateConfiguration ("Debug");
				configuration.DebugMode = true;
				configuration.Platform = target;
				configuration.OpenFLTarget = targetFlag;
				configuration.HaxeProjectTarget = HaxeProjectTarget.OpenFL;

				if (target == "iOS") {
					configuration.AdditionalArguments = "-simulator";
				}
				Configurations.Add (configuration);
			}

			for (int i = 0; i < targets.Length; i++) {
				string target = targets [i];
				OpenFLTarget targetFlag = targetFlags [i];
				configuration = (HaxeProjectConfiguration)CreateConfiguration ("Release");
				configuration.DebugMode = false;
				configuration.Platform = target;
				configuration.OpenFLTarget = targetFlag;
				configuration.HaxeProjectTarget = HaxeProjectTarget.OpenFL;

				if (target == "iOS") {
					configuration.AdditionalArguments = "-simulator";
				}
				Configurations.Add (configuration);
			}
		}

		public override void updateDefaultRunConfig (HaxeProjectConfiguration configuration)
		{
			ExecuteFile = String.Empty;
			OutputFile = BuildFile;
		}

		public override HxmlParser getHxml (HaxeProjectConfiguration configuration)
		{
			string hxmlContent = OpenFLCommandLineToolsManager.GetHXMLData (this, configuration);
			HxmlParser hxml = new HxmlParser ();
			hxml.Parse (hxmlContent);
			return hxml;
		}

		protected override BuildResult DoBuild (IProgressMonitor monitor, ConfigurationSelector configurationSelector)
		{
			HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configurationSelector);
			if (DefaultRun) {
				updateDefaultRunConfig (haxeConfig);
			}
			return OpenFLCommandLineToolsManager.Compile (this, haxeConfig, monitor);
		}

		protected override void DoClean (IProgressMonitor monitor, ConfigurationSelector configuration)
		{
			HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configuration);
			OpenFLCommandLineToolsManager.Clean (this, haxeConfig, monitor);
		}

		protected override void DoExecute (IProgressMonitor monitor, ExecutionContext context, ConfigurationSelector configurationSelector)
		{
			HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configurationSelector);
			if (DefaultRun) {
				updateDefaultRunConfig (haxeConfig);
			}
			pathes = HaxeCompilerManager.GetClassPaths (this, haxeConfig); // hxml need too?

			OpenFLCommandLineToolsManager.Run (this, haxeConfig, monitor, context);
		}

		protected override bool OnGetCanExecute (ExecutionContext context, ConfigurationSelector configurationSelector)
		{
			HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configurationSelector);
			if (DefaultRun) {
				updateDefaultRunConfig (haxeConfig);
			}
			return OpenFLCommandLineToolsManager.CanRun (this, haxeConfig, context);
		}
	}
}

