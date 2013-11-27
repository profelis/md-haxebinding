using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Projects;
using MonoDevelop.HaxeBinding.Projects;
using HaxeBinding;

// TODO: haxe compiler server to separate file

namespace MonoDevelop.HaxeBinding.Tools
{

	static class HaxeCompilerManager
	{
		
		private static Process compilationServer;
		private static int compilationServerPort;
		
		private static string cacheArgumentsGlobal;
		private static string cacheArgumentsPlatform;
		private static string cacheHXML;
		private static string cachePlatform;
		private static DateTime cacheNMMLTime;
		
		private static Regex mErrorFull = new Regex (@"^(?<file>.+)\((?<line>\d+)\):\s(col:\s)?(?<column>\d*)\s?(?<level>\w+):\s(?<message>.*)\.?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		// example: test.hx:11: character 7 : Unterminated string
		private static Regex mErrorFileChar = new Regex (@"^(?<file>.+):(?<line>\d+):\s(character\s)(?<column>\d*)\s:\s(?<message>.*)\.?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		// example: test.hx:11: characters 0-5 : Unexpected class
		private static Regex mErrorFileChars = new Regex (@"^(?<file>.+):(?<line>\d+):\s(characters\s)(?<column>\d+)-(\d+)\s:\s(?<message>.*)\.?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		// example: test.hx:10: lines 10-28 : Class not found : Sprite
		private static Regex mErrorFile = new Regex (@"^(?<file>.+):(?<line>\d+):\s(?<message>.*)\.?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		
		private static Regex mErrorCmdLine = new Regex (@"^command line: (?<level>\w+):\s(?<message>.*)\.?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		private static Regex mErrorSimple = new Regex (@"^(?<level>\w+):\s(?<message>.*)\.?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		private static Regex mErrorIgnore = new Regex (@"^(Updated|Recompile|Reason|Files changed):.*", RegexOptions.Compiled);

		
		public static BuildResult Compile (HaxeProject project, HaxeProjectConfiguration configuration, IProgressMonitor monitor)
		{
			string exe = "haxe";
			//string args = project.TargetHXMLFile;

			HxmlParser hxml = project.getHxml (configuration);
			hxml.GetProjectPath (project, true);

			string args = hxml.Args;
			
			if (configuration.DebugMode)
			{
				args += " -debug";
			}
			
			args += " " + project.AdditionalArguments + " " + configuration.AdditionalArguments;

			string error = "";
			int exitCode = DoCompilation (exe, args, project.BaseDirectory, monitor, ref error);
			
			BuildResult result = ParseOutput (project, error);
			if (result.CompilerOutput.Trim ().Length != 0)
				monitor.Log.WriteLine (result.CompilerOutput);

			if (result.ErrorCount == 0 && exitCode != 0)
			{
				string errorMessage = File.ReadAllText (error);
				if (!string.IsNullOrEmpty (errorMessage))
					result.AddError (errorMessage); 
				else
					result.AddError ("Build failed. Go to \"Build Output\" for more information");
			}
			
			FileService.DeleteFile (error);
			return result;
		}
		
		
		private static BuildError CreateErrorFromString (HaxeProject project, string text)
		{
			Match match = mErrorIgnore.Match (text);
			if (match.Success)
				return null;

			match = mErrorFull.Match (text);
			if (!match.Success)
				match = mErrorCmdLine.Match (text);
			if (!match.Success)
				match = mErrorFileChar.Match (text);
			if (!match.Success)
				match = mErrorFileChars.Match (text);
			if (!match.Success)
				match = mErrorFile.Match (text);
			    
			if (!match.Success)
				match = mErrorSimple.Match (text);
			if (!match.Success)
				return null;

			int n;

			BuildError error = new BuildError ();
			error.FileName = match.Result ("${file}") ?? "";
			error.IsWarning = match.Result ("${level}").ToLower () == "warning";
			error.ErrorText = match.Result ("${message}");
			
			if (error.FileName == "${file}")
			{
				error.FileName = "";
			}
			else
			{
				if (!File.Exists (error.FileName))
				{
					if (File.Exists (Path.GetFullPath (error.FileName)))
					{						
						error.FileName = Path.GetFullPath (error.FileName);
					}
					else
					{
						error.FileName = Path.Combine (project.BaseDirectory, error.FileName);
					}
				}
			}

			if (Int32.TryParse (match.Result ("${line}"), out n))
				error.Line = n;
			else
				error.Line = 0;

			if (Int32.TryParse (match.Result ("${column}"), out n))
				error.Column = n+1; //haxe counts zero based
			else
				error.Column = -1;

			return error;
		}
		
		
		private static int DoCompilation (string cmd, string args, string wd, IProgressMonitor monitor, ref string error)
		{
			int exitcode = 0;
			error = Path.GetTempFileName ();
			StreamWriter errwr = new StreamWriter (error);

			ProcessStartInfo pinfo = new ProcessStartInfo (cmd, args);
			pinfo.UseShellExecute = false;
			pinfo.RedirectStandardOutput = true;
			pinfo.RedirectStandardError = true;
			pinfo.WorkingDirectory = wd;

			using (MonoDevelop.Core.Execution.ProcessWrapper pw = Runtime.ProcessService.StartProcess(pinfo, monitor.Log, errwr, null))
			{
				pw.WaitForOutput ();
				exitcode = pw.ExitCode;
			}
			errwr.Close ();

			return exitcode;
		}
		
		
		public static string GetCompletionData (HaxeProject project, string classPath, string fileName, int position)
		{
			if (!PropertyService.HasValue ("HaxeBinding.EnableCompilationServer"))
			{
				PropertyService.Set ("HaxeBinding.EnableCompilationServer", true);
				PropertyService.Set ("HaxeBinding.CompilationServerPort", 6000);
	            PropertyService.SaveProperties();
			}
			
			string exe = "haxe";
			string args = "";

			if (project.ProjectTarget == HaxeProjectTarget.OpenFL) {
				
				HaxeProjectConfiguration configuration = project.GetConfiguration (MonoDevelop.Ide.IdeApp.Workspace.ActiveConfiguration) as HaxeProjectConfiguration;
				
				string platform = configuration.Platform.ToLower ();
				string path = project.BuildFile;
				
				if (!File.Exists (path))
				{
					path = Path.Combine (project.BaseDirectory, path);
				}
				
				DateTime time = File.GetLastWriteTime (Path.GetFullPath (path));
				
				if (!time.Equals (cacheNMMLTime) || platform != cachePlatform || configuration.AdditionalArguments != cacheArgumentsPlatform || project.AdditionalArguments != cacheArgumentsGlobal)
				{
					cacheHXML = OpenFLCommandLineToolsManager.GetHXMLData (project, configuration).Replace (Environment.NewLine, " ");
					cacheNMMLTime = time;
					cachePlatform = platform;
					cacheArgumentsGlobal = project.AdditionalArguments;
					cacheArgumentsPlatform = configuration.AdditionalArguments;
				}
				
				args = cacheHXML + " -D code_completion";
				
			} else if (project.ProjectTarget == HaxeProjectTarget.Haxe) {
				
				HaxeProjectConfiguration configuration = project.GetConfiguration (MonoDevelop.Ide.IdeApp.Workspace.ActiveConfiguration) as HaxeProjectConfiguration;
				
				args = "\"" + ((HaxeProject)project).BuildFile + "\" " + ((HaxeProject)project).AdditionalArguments + " " + configuration.AdditionalArguments;
				
			} else {
				
				return "";
				
			}
			
			args += " -cp \"" + classPath + "\" --display \"" + fileName + "\"@" + position + " -D use_rtti_doc";
			
			if (PropertyService.Get<bool> ("HaxeBinding.EnableCompilationServer")) {
				
				var port = PropertyService.Get<int> ("HaxeBinding.CompilationServerPort");
				
				if (compilationServer == null || compilationServer.HasExited || port != compilationServerPort)
				{
					StartServer ();
				}
				
				try
	            {
					if (!compilationServer.HasExited)
					{
		                var client = new TcpClient("127.0.0.1", port);
		                var writer = new StreamWriter(client.GetStream());
		                writer.WriteLine("--cwd " + project.BaseDirectory);
		                
		                // Instead of replacing spaces with newlines, replace only
		                // if the space is followed by a dash.
		                // TODO: Even more intelligent replacement so you can use folders
						// 		 that contain the string sequence " -".
						writer.Write(args.Replace(" -", "\n-"));
						
						//writer.WriteLine("--connect " + port);
		                writer.Write("\0");
		                writer.Flush();
		                var reader = new StreamReader(client.GetStream());
		                var lines = reader.ReadToEnd().Split('\n');
		                client.Close();
		                return String.Join ("\n", lines);
					}
	            }
	            catch(Exception)
	            {
					//MonoDevelop.Ide.MessageService.ShowError (ex.ToString ());
	                //TraceManager.AddAsync(ex.Message);
	                //if (!failure && FallbackNeeded != null)
	                   // FallbackNeeded(false);
	                //failure = true;
	                //return "";
	            }
				
			}
			
			//MonoDevelop.Ide.MessageService.ShowError ("Falling back to standard completion");
			
			Process process = new Process ();
			process.StartInfo.FileName = exe;
			process.StartInfo.Arguments = args;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.WorkingDirectory = project.BaseDirectory;
			process.Start ();
			
			string result = process.StandardError.ReadToEnd ();
			process.WaitForExit ();
			
			return result;
		}


		static BuildResult ParseOutput (HaxeProject project, string stderr)
		{
			BuildResult result = new BuildResult ();

			StringBuilder output = new StringBuilder ();
			ParserOutputFile (project, result, output, stderr);

			result.CompilerOutput = output.ToString ();

			return result;
		}
		
		
		static void ParserOutputFile (HaxeProject project, BuildResult result, StringBuilder output, string filename)
		{
			StreamReader reader = File.OpenText (filename);

			string line;
			while ((line = reader.ReadLine()) != null)
			{
				output.AppendLine (line);

				line = line.Trim ();
				if (line.Length == 0 || line.StartsWith ("\t"))
					continue;

				BuildError error = CreateErrorFromString (project, line);
				if (error != null)
					result.Append (error);
			}

			reader.Close ();
		}
		
		
		private static HaxeExecutionCommand CreateExecutionCommand (HaxeProject project, HaxeProjectConfiguration configuration)
		{
			if (configuration == null) {
				return null;
			}

			HaxeExecutionCommand cmd = new HaxeExecutionCommand ();
			cmd.haxeExecuteTarget = HaxeProjectTarget.Haxe;
			cmd.DebugMode = configuration.DebugMode;

			HxmlParser hxml = project.getHxml (configuration);
			cmd.haxeTarget = hxml.Target;

			string output = project.OutputFile;

			if (output.Length > 0 && !output.StartsWith ("http://") && !File.Exists (Path.GetFullPath (output))) {
				output = Path.Combine (project.BaseDirectory, output);
			}

			if (project.ExecuteFile.Length > 0) {
				cmd.Command = project.ExecuteFile;
				cmd.Arguments = "\"" + output + "\"";
			} else {
				cmd.Command = output;
				cmd.Arguments = "";
			}

			cmd.Arguments += project.OutputArguments;

			// cmd.WorkingDirectory = Path.GetDirectoryName (output);
			if (cmd.Command.Length > 0) {
				cmd.WorkingDirectory = Path.GetDirectoryName (cmd.Command);
			}

			if (configuration.DebugMode) {
				//	cmd.EnvironmentVariables.Add ("HXCPP_DEBUG_HOST", "gdb");
				cmd.EnvironmentVariables.Add ("HXCPP_DEBUG", "1");
			}
			// output += "-debug";

			return cmd;
		}
		
		
		public static bool CanRun (HaxeProject project, HaxeProjectConfiguration configuration, ExecutionContext context)
		{
			// need to optimize so this caches the result
			
			HaxeExecutionCommand cmd = CreateExecutionCommand (project, configuration);
			if (cmd == null) {
				return false;
			}
			return configuration.DebugMode ? context.ExecutionHandler.CanExecute (cmd) : true;
		}
		

		public static void Run (HaxeProject project, HaxeProjectConfiguration configuration, IProgressMonitor monitor, ExecutionContext context)
		{
			HaxeExecutionCommand cmd = CreateExecutionCommand (project, configuration);

			if (cmd.DebugMode && (cmd.haxeTarget == HaxeTarget.Cpp || cmd.haxeTarget == HaxeTarget.Neko))
			{
				IConsole console;
				if (configuration.ExternalConsole)
					console = context.ExternalConsoleFactory.CreateConsole (false);
				else
					console = context.ConsoleFactory.CreateConsole (false);
	
				AggregatedOperationMonitor operationMonitor = new AggregatedOperationMonitor (monitor);
	
				try
				{
					if (!context.ExecutionHandler.CanExecute (cmd))
					{
						monitor.ReportError (String.Format ("Cannot execute '{0}'.", cmd.Target), null);
						return;
					}
					
					IProcessAsyncOperation operation = context.ExecutionHandler.Execute (cmd, console);
					
					operationMonitor.AddOperation (operation);
					operation.WaitForCompleted ();
	
					monitor.Log.WriteLine ("Player exited with code {0}.", operation.ExitCode);
				}
				catch (Exception)
				{
					monitor.ReportError (String.Format ("Error while executing '{0}'.", cmd.Target), null);
				}
				finally
				{
					operationMonitor.Dispose ();
					console.Dispose ();
				}
			}
			else
			{
				Process p = new Process ();
				p.StartInfo.Arguments = cmd.Arguments;
				p.StartInfo.WorkingDirectory = cmd.WorkingDirectory;
				p.StartInfo.FileName = cmd.Command;
				//p.StartInfo.UseShellExecute = true;
				p.Start ();
				/*bool canExe = context.ExecutionHandler.CanExecute (cmd);
				IConsole console = context.ConsoleFactory.CreateConsole(false);
				IProcessAsyncOperation op = context.ExecutionHandler.Execute (cmd, console);
				op.WaitForCompleted ();
				console.Dispose ();*/
				//Process.Start (cmd);
			}
		}
		
		
		private static void StartServer ()
		{
			if (compilationServer != null && !compilationServer.HasExited)
			{
				StopServer ();
			}
			
			compilationServerPort = PropertyService.Get<int>("HaxeBinding.CompilationServerPort");
			compilationServer = new Process ();
			compilationServer.StartInfo.FileName = "haxe";
			compilationServer.StartInfo.Arguments = "--wait " + compilationServerPort;
			compilationServer.StartInfo.CreateNoWindow = true;
			compilationServer.StartInfo.UseShellExecute = false;
			compilationServer.StartInfo.RedirectStandardOutput = true;
			//MonoDevelop.Ide.MessageService.ShowMessage ("sldifj");
			compilationServer.Start ();
			//System.Threading.Thread.Sleep (100);
			//compilationServer.StandardOutput.ReadLine ();
		}
		
		
		public static void StopServer ()
		{
			try
			{
				if (compilationServer != null)
				{
					compilationServer.CloseMainWindow ();
				}
			} catch (Exception) {}
		}

		public static List<string> GetClassPaths(HaxeProject project, HaxeProjectConfiguration configuration)
		{
			List<string> paths = new List<string> ();
			HxmlParser hxml = project.getHxml (configuration);
			foreach (string lib in hxml.Libs) {
				paths.AddRange(HaxelibTools.GetLibraryPath(lib));
			}
			foreach (string path in hxml.ClassPaths) {
				paths.Add(Path.Combine(project.BaseDirectory, path));
			}
			return paths;
		}
		
	}
	
}