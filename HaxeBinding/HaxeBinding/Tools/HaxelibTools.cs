using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HaxeBinding
{
	public class HaxelibTools
	{
		private HaxelibTools ()
		{
		}

		// TODO: cache result
		public static List<string> GetLibraryPath(string library)
		{
			ProcessStartInfo info = new ProcessStartInfo ();

			info.FileName = "haxe";
			info.Arguments = "--run tools.haxelib.Main path " + library;
			info.UseShellExecute = false;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			//info.WindowStyle = ProcessWindowStyle.Hidden;
			info.CreateNoWindow = true;
			string data;
			using (Process process = Process.Start (info))
			{
				data = process.StandardOutput.ReadToEnd ();
				process.WaitForExit ();
			}
			var libsPaths = new List<string> ();
			var dataList = data.Split (Environment.NewLine.ToCharArray());
			foreach (string line in dataList) {
				if (!line.StartsWith ("-D ") && !line.StartsWith ("-L ")) {
					libsPaths.Add (line);
				}
			}
			return libsPaths;
		}
	}
}

