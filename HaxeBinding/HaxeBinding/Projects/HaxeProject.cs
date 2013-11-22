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

	[DataInclude(typeof(HaxeProjectConfiguration))]
    public class HaxeProject : Project
	{
		
		[ItemProperty("AdditionalArguments", DefaultValue="")]
		string mAdditionalArguments = string.Empty;

		public string AdditionalArguments {
			get { return mAdditionalArguments;  }
			set { mAdditionalArguments = value; }
		}
		
		[ItemProperty("BuildFile", DefaultValue="")]
		string mBuildFile = string.Empty;
		
		public string BuildFile {
			get { return mBuildFile;  }
			set { 
				mBuildFile = value;
				// TODO: check file content
				if (mBuildFile.EndsWith(".hxml")) {
					ProjectTarget = HaxeProjectTarget.Haxe;
				} else if (mBuildFile.EndsWith(".xml")) {
					ProjectTarget = HaxeProjectTarget.OpenFL;
				}
			}
		}


		[ItemProperty("OutputFile", DefaultValue="")]
		string mOutputFile = string.Empty;

		public string OutputFile {
			get { return mOutputFile; }
			set { mOutputFile = value; }
		}

		[ItemProperty("ExecuteFile", DefaultValue="")]
		string mExecuteFile = string.Empty;

		public string ExecuteFile {
			get { return mExecuteFile; }
			set { mExecuteFile = value; }
		}
		
		[ItemProperty("OutputArguments", DefaultValue="")]
		string mOutputArguments = string.Empty;

		public string OutputArguments {
			get { return mOutputArguments; }
			set { mOutputArguments = value; }
		}

		[ItemProperty("ProjectTarget", DefaultValue="")]
		HaxeProjectTarget mProjectTarget = HaxeProjectTarget.Haxe;

		public HaxeProjectTarget ProjectTarget {
			get { return mProjectTarget; }
			set { 
				if (mProjectTarget == value)
					return;
				mProjectTarget = value;
				HaxeProjectConfiguration configuration;

				switch (mProjectTarget) {
				case HaxeProjectTarget.OpenFL:

					string[] targets = new string[] {
						"Android",
						"BlackBerry",
						"Flash",
						"HTML5",
						"iOS",
						"Linux",
						"Mac",
						"webOS",
						"Windows"
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
						OpenFLTarget.Windows
					};

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
					pathes.Add (this.BaseDirectory);
					break;
				case HaxeProjectTarget.Haxe:

					configuration = (HaxeProjectConfiguration)CreateConfiguration ("Debug");
					configuration.DebugMode = true;
					configuration.HaxeProjectTarget = HaxeProjectTarget.Haxe;
					Configurations.Add (configuration);

					configuration = (HaxeProjectConfiguration)CreateConfiguration ("Release");
					configuration.DebugMode = false;
					configuration.HaxeProjectTarget = HaxeProjectTarget.Haxe;
					Configurations.Add (configuration);
					break;
				}
			}
		}

		public List<string> pathes = new List<string> ();

		public string ModuleName {
			get;
			private set;
		}

		public HaxeProject () : base()
		{
			
		}

		public override void Dispose ()
		{
			HaxeCompilerManager.StopServer ();
			base.Dispose ();
		}


		public HaxeProject (ProjectCreateInformation info, XmlElement projectOptions) : base()
		{
			if (projectOptions.Attributes ["BuildFile"] != null)
			{
				BuildFile = GetOptionAttribute (info, projectOptions, "BuildFile");
			}
			
			if (projectOptions.Attributes ["AdditionalArguments"] != null)
			{
				AdditionalArguments = GetOptionAttribute (info, projectOptions, "AdditionalArguments");
			}

			if (projectOptions.Attributes ["OutputFile"] != null)
			{
				OutputFile = GetOptionAttribute (info, projectOptions, "OutputFile");
			}

			if (projectOptions.Attributes ["ExecuteFile"] != null)
			{
				ExecuteFile = GetOptionAttribute (info, projectOptions, "ExecuteFile");
			}

			ModuleName = info.ProjectName.Substring (0, 1).ToUpper () + info.ProjectName.Substring (1);
		}

		protected string GetOptionAttribute (ProjectCreateInformation info, XmlElement projectOptions, string attributeName)
		{
			string value = projectOptions.Attributes [attributeName].InnerText;
			return HaxeFileDescriptionTemplate.FormatString (value, this, info);
		}

		public override SolutionItemConfiguration CreateConfiguration (string name)
		{
			HaxeProjectConfiguration conf = new HaxeProjectConfiguration ();
			conf.Name = name;
			return conf;
		}
		
		
		protected override BuildResult DoBuild (IProgressMonitor monitor, ConfigurationSelector configurationSelector)
		{
			HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configurationSelector);

			switch (ProjectTarget) {
			case HaxeProjectTarget.Haxe:
				return HaxeCompilerManager.Compile (this, haxeConfig, monitor);
			case HaxeProjectTarget.OpenFL:
				return OpenFLCommandLineToolsManager.Compile (this, haxeConfig, monitor);
			default:
				return null;
			}
		}
		
		
		protected override void DoClean (IProgressMonitor monitor, ConfigurationSelector configuration)
		{
			switch (ProjectTarget) {
			case HaxeProjectTarget.Haxe:
				break;
				//base.DoClean (monitor, configuration);
			case HaxeProjectTarget.OpenFL:
				HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configuration);
				OpenFLCommandLineToolsManager.Clean (this, haxeConfig, monitor);
				break;
			}
		}
		
		
		protected override void DoExecute (IProgressMonitor monitor, ExecutionContext context, ConfigurationSelector configurationSelector)
		{
			HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configurationSelector);
			pathes = HaxeCompilerManager.GetClassPaths (this, haxeConfig); // hxml need too?

			switch (ProjectTarget) {
			case HaxeProjectTarget.Haxe:
				HaxeCompilerManager.Run (this, haxeConfig, monitor, context);
				break;
			case HaxeProjectTarget.OpenFL:
				OpenFLCommandLineToolsManager.Run (this, haxeConfig, monitor, context);
				break;
			}
		}
		
		public override bool IsCompileable (string fileName)
		{
			return true;
		}
		
		
		protected override bool OnGetCanExecute (ExecutionContext context, ConfigurationSelector configurationSelector)
		{
			HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configurationSelector);
			switch (ProjectTarget) {
			case HaxeProjectTarget.Haxe:
				return HaxeCompilerManager.CanRun (this, haxeConfig, context);
			case HaxeProjectTarget.OpenFL:
				return OpenFLCommandLineToolsManager.CanRun (this, haxeConfig, context);
			default:
				return false;
			}
		}

		public HxmlParser getHxml(HaxeProjectConfiguration configuration) {
			string hxmlContent = null;

			switch (this.ProjectTarget) {
			case HaxeProjectTarget.Haxe:
				string path = Path.GetFullPath (BuildFile);

				if (!File.Exists (path)) {
					path = Path.Combine (BaseDirectory, BuildFile);
				}
				hxmlContent = File.ReadAllText (path);
				break;
			case HaxeProjectTarget.OpenFL:

				hxmlContent = OpenFLCommandLineToolsManager.GetHXMLData (this, configuration);
				break;
			}
			if (hxmlContent == null) {
				throw new Exception ("can't get hxml file");
			}
			HxmlParser hxml = new HxmlParser ();
			hxml.Parse (hxmlContent);
			return hxml;
		}


		public override string ProjectType {
			get { return "Haxe"; }
		}
		

		public override string[] SupportedLanguages {
			get { return new string[] { "", "Haxe", "HXML", "OpenFL" }; }
		}
		
	}
	
}