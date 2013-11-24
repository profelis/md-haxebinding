using System;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.HaxeBinding.Languages
{
	public class OpenFLLanguageBinding : ILanguageBinding
	{
		public OpenFLLanguageBinding ()
		{
		}

		public string SingleLineCommentTag { get { return null; } }
		public string BlockCommentStartTag { get { return "<!--"; } }
		public string BlockCommentEndTag { get { return "-->"; } }

		public string Language { get { return "OpenFL"; } }

		
		public FilePath GetFileName (FilePath baseName)
		{
			return new FilePath (baseName.FileNameWithoutExtension + ".xml");
		}
		
		
		public bool IsSourceCodeFile (FilePath fileName)
		{
			return fileName.Extension == "xml";
		}
	}
}

