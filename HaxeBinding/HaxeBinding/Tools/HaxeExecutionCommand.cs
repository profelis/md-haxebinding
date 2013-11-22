using System;
using MonoDevelop.Core.Execution;
using System.Collections.Generic;
using HaxeBinding;
using MonoDevelop.HaxeBinding.Tools;

namespace MonoDevelop.HaxeBinding
{

	public class HaxeExecutionCommand : ProcessExecutionCommand
	{
		public Array Pathes;

		public HaxeProjectTarget HaxeExecuteTarget;
		public HaxeTarget? HaxeTarget;
		public OpenFLTarget? OpenFLTarget;

		public bool DebugMode;

		public HaxeExecutionCommand () : base()
		{
		}
		public HaxeExecutionCommand (string command) : base (command)
		{
		}
		public HaxeExecutionCommand (string command, string arguments) : base (command, arguments)
		{
		}
		public HaxeExecutionCommand (string command, string arguments, string workingDirectory) : base (command, arguments, workingDirectory)
		{
		}
		public HaxeExecutionCommand (string command, string arguments, string workingDirectory, IDictionary<string, string> environmentVariables) : base (command, arguments, workingDirectory, environmentVariables)
		{
		}
	}
}

