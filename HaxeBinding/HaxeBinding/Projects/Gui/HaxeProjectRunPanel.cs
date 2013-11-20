using System;
using System.IO;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Dialogs;
using Gtk;
using MonoDevelop.HaxeBinding.Tools;


namespace MonoDevelop.HaxeBinding.Projects.Gui
{

	public class HaxeProjectRunPanel : ItemOptionsPanel
	{
		HaxeProjectRunWidget mWidget;


		public override Gtk.Widget CreatePanelWidget ()
		{	
			mWidget = new HaxeProjectRunWidget ();
			mWidget.Load ((HaxeProject)ConfiguredProject);
			return mWidget;
		}


		public override void ApplyChanges ()
		{
			mWidget.Store ();
		}
	}

	[System.ComponentModel.Category("HaxeBinding")]
    [System.ComponentModel.ToolboxItem(true)]
    public partial class HaxeProjectRunWidget : Gtk.Bin
	{
		HaxeProject mProject;


		public HaxeProjectRunWidget ()
		{
			this.Build ();
		}


		public void Load (HaxeProject project)
		{
			mProject = project;
			
			OutputEntry.Text = mProject.OutputFile;
			ExecuteEntry.Text = mProject.ExecuteFile;
			AdditionalArgumentsEntry.Text = mProject.OutputArguments;
		}


		public void Store ()
		{
			if (mProject == null)
				return;
			
			mProject.OutputFile = OutputEntry.Text.Trim ();
			mProject.ExecuteFile = ExecuteEntry.Text.Trim ();
			mProject.OutputArguments = AdditionalArgumentsEntry.Text.Trim ();
		}

		
		protected void OnOutputButtonClicked (object sender, System.EventArgs e)
		{
			Gtk.FileChooserDialog fc =
				new Gtk.FileChooserDialog ("Select file", this.Toplevel as Gtk.Window, FileChooserAction.Open,
                    "Cancel", ResponseType.Cancel,
                    "Select", ResponseType.Accept);
			

			Gtk.FileFilter filterAll = new Gtk.FileFilter ();
			filterAll.Name = "All Files";
			filterAll.AddPattern ("*");
			
			fc.AddFilter (filterAll);
			
			if (mProject.OutputFile != "") {
				fc.SetFilename (System.IO.Path.Combine(mProject.BaseDirectory, mProject.OutputFile));
			} else {
				fc.SetFilename (mProject.BaseDirectory);
			}

			if (fc.Run () == (int)ResponseType.Accept)
			{
				string path = PathHelper.ToRelativePath (fc.Filename, mProject.BaseDirectory);
				
				OutputEntry.Text = path;
			}

			fc.Destroy ();
		}
		
	}
	
}