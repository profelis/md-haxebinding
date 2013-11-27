using System;

namespace MonoDevelop.HaxeBinding.Tools
{
	[Flags]
	public enum HaxeTarget {
		Flash = 0x1,
		Js = 0x2,
		Neko = 0x3,
		Php = 0x4,
		Cpp = 0x5,
		Java = 0x6,
		Cs = 0x7
	}

	[Flags]
	public enum HaxeProjectTarget {
		OpenFL = 0x1,
		Haxe = 0x2
	}

	[Flags]
	public enum OpenFLTarget {
		Android = 0x1, 
		BlackBerry = 0x2,
		Flash = 0x3, 
		HTML5 = 0x4, 
		iOS = 0x5,
		Linux = 0x6,
		Mac = 0x7,
		webOS = 0x8,
		Windows = 0x9
	}
}

