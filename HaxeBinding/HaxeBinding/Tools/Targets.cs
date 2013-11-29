using System;

namespace MonoDevelop.HaxeBinding.Tools
{
	[Flags]
	public enum HaxeTarget {
		Flash,
		Js,
		Neko,
		Php,
		Cpp,
		Java,
		Cs
	}

	[Flags]
	public enum HaxeProjectTarget {
		OpenFL,
		Haxe
	}

	[Flags]
	public enum OpenFLTarget {
		Android, 
		BlackBerry,
		Flash, 
		HTML5, 
		iOS,
		Linux,
		Mac,
		webOS,
		Windows,
		Neko
	}
}

