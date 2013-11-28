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

// TODO: compiler managers (Haxe and OpenFL) make with Similar interface
// TODO: and create link to one of the fabrics

namespace MonoDevelop.HaxeBinding.Projects
{

	[DataInclude(typeof(HaxeProjectConfiguration))]
    public class HaxeProject : Project
	{

		// HaxeProjectOptionsPanel
		[ItemProperty("AdditionalArguments", DefaultValue="")]
		string mAdditionalArguments = string.Empty;

		public string AdditionalArguments {
			get { return mAdditionalArguments;  }
			set { mAdditionalArguments = value; }
		}

		// HaxeProjectOptionsPanel
		[ItemProperty("BuildFile", DefaultValue="")]
		string mBuildFile = string.Empty;
		
		public string BuildFile {
			get { return mBuildFile;  }
			set { 
				mBuildFile = value;
				if (DefaultRun) {
					updateDefaultRunConfig ((HaxeProjectConfiguration)DefaultConfiguration);
				}
			}
		}

		// HaxeProjectRunPanel
		[ItemProperty("DefaultRun", DefaultValue="")]
		bool mDefaultRun = false;

		public bool DefaultRun {
			get { return mDefaultRun; }
			set {
				mDefaultRun = value;
				if (value) {
					updateDefaultRunConfig ((HaxeProjectConfiguration)DefaultConfiguration);
				}
			}
		}

		// HaxeProjectRunPanel
		[ItemProperty("OutputFile", DefaultValue="")]
		string mOutputFile = string.Empty;

		public string OutputFile {
			get { return mOutputFile; }
			set { mOutputFile = value; }
		}

		// HaxeProjectRunPanel
		[ItemProperty("ExecuteFile", DefaultValue="")]
		string mExecuteFile = string.Empty;

		public string ExecuteFile {
			get { return mExecuteFile; }
			set { mExecuteFile = value; }
		}

		// HaxeProjectRunPanel
		[ItemProperty("OutputArguments", DefaultValue="")]
		string mOutputArguments = string.Empty;

		public string OutputArguments {
			get { return mOutputArguments; }
			set { mOutputArguments = value; }
		}
		
		public List<string> pathes = new List<string> ();
		
		public string ModuleName {
			get;
			private set;
		}

		public HaxeProject () : base()
		{
			buildConfigurations ();
		}

		public override void Dispose ()
		{
			HaxeCompilerManager.StopServer ();
			base.Dispose ();
		}

		public HaxeProject (ProjectCreateInformation info, XmlElement projectOptions) : base()
		{
			if (projectOptions == null) 
			{
				return;
			}

			ModuleName = info.ProjectName.Substring (0, 1).ToUpper () + info.ProjectName.Substring (1);

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

			if (projectOptions.Attributes ["BuildFile"] != null)
			{
				BuildFile = GetOptionAttribute (info, projectOptions, "BuildFile");
			}

			if (projectOptions.Attributes ["DefaultRun"] != null)
			{
				mDefaultRun = GetOptionAttribute (info, projectOptions, "DefaultRun") == "true";
			}

			buildConfigurations ();
		}

		protected override void OnEndLoad ()
		{
			DefaultRun = mDefaultRun;

			base.OnEndLoad ();
		}

		protected virtual void buildConfigurations ()
		{
			HaxeProjectConfiguration configuration = (HaxeProjectConfiguration)CreateConfiguration ("Debug");
			configuration.DebugMode = true;
			configuration.HaxeProjectTarget = HaxeProjectTarget.Haxe;
			configuration.Platform = "Haxe";
			Configurations.Add (configuration);

			configuration = (HaxeProjectConfiguration)CreateConfiguration ("Release");
			configuration.DebugMode = false;
			configuration.HaxeProjectTarget = HaxeProjectTarget.Haxe;
			configuration.Platform = "Haxe";
			Configurations.Add (configuration);
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
			if (DefaultRun) {
				updateDefaultRunConfig (haxeConfig);
			}

			return HaxeCompilerManager.Compile (this, haxeConfig, monitor);
		}

		protected override void DoClean (IProgressMonitor monitor, ConfigurationSelector configuration)
		{
			//base.DoClean (monitor, configuration);
		}

		protected override void DoExecute (IProgressMonitor monitor, ExecutionContext context, ConfigurationSelector configurationSelector)
		{
			HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configurationSelector);
			if (DefaultRun) {
				updateDefaultRunConfig (haxeConfig);
			}
			//pathes = HaxeCompilerManager.GetClassPaths (this, haxeConfig); // hxml need too?

			HaxeCompilerManager.Run (this, haxeConfig, monitor, context);
		}
		
		public override bool IsCompileable (string fileName)
		{
			return true;
		}
		
		
		protected override bool OnGetCanExecute (ExecutionContext context, ConfigurationSelector configurationSelector)
		{
			HaxeProjectConfiguration haxeConfig = (HaxeProjectConfiguration)GetConfiguration (configurationSelector);
			if (DefaultRun) {
				updateDefaultRunConfig (haxeConfig);
			}
			return HaxeCompilerManager.CanRun (this, haxeConfig, context);
		}

		public virtual HxmlParser getHxml(HaxeProjectConfiguration configuration) {

			string path = Path.GetFullPath (BuildFile);

			if (!File.Exists (path)) {
				path = Path.Combine (BaseDirectory, BuildFile);
			}
			string hxmlContent = File.ReadAllText (path);

			HxmlParser hxml = new HxmlParser ();
			hxml.Parse (hxmlContent);
			return hxml;
		}

		public virtual void updateDefaultRunConfig(HaxeProjectConfiguration configuration) {
			HxmlParser hxml = getHxml (configuration);
			// TODO: optimize
			switch (hxml.Target) {
			case HaxeTarget.Flash:
				ExecuteFile = String.Empty;
				OutputFile = hxml.Out;
				break;
			case HaxeTarget.Js:
				ExecuteFile = String.Empty;
				OutputFile = Path.Combine(Path.GetDirectoryName(hxml.Out), "index.html");
				break;
			case HaxeTarget.Cpp:
				ExecuteFile = String.Empty;
				OutputFile = Path.Combine (hxml.Out, Name);
				break;
			case HaxeTarget.Cs:
				ExecuteFile = String.Empty;
				OutputFile = Path.Combine (hxml.Out, Name);
				break;
			case HaxeTarget.Neko:
				ExecuteFile = "neko";
				OutputFile = hxml.Out;
				break;
			case HaxeTarget.Java:
				ExecuteFile = "java -jar";
				OutputFile = Path.Combine (hxml.Out, "java.jar");
				break;
			case HaxeTarget.Php:
				ExecuteFile = String.Empty;
				OutputFile = "http://127.0.0.1";
				break;
			}
		}
		
		public override string ProjectType {
			get { return "Haxe"; }
		}

		public override string[] SupportedLanguages {
			get { return new string[] { "", "Haxe", "HXML" }; }
		}
		
	}
	
}