using System;
using Mono.Debugging.Client;

namespace MonoDevelop.HaxeBinding
{
	public class HxcppDebuggerStartInfo : DebuggerStartInfo
	{
		public Array Paths;

		public HxcppDebuggerStartInfo () : base()
		{
		}
	}
}

