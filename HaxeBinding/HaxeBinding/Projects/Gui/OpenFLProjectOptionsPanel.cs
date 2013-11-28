using System;
using System.IO;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Dialogs;
using Gtk;


namespace MonoDevelop.HaxeBinding.Projects.Gui
{

	public class OpenFLProjectOptionsPanel : ItemOptionsPanel
	{
		OpenFLProjectOptionsWidget mWidget;


		public override Gtk.Widget CreatePanelWidget ()
		{	
			mWidget = new OpenFLProjectOptionsWidget ();
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
	public partial class OpenFLProjectOptionsWidget : Gtk.Bin
	{
		HaxeProject mProject;


		public OpenFLProjectOptionsWidget ()
		{
			this.Build ();
		}


		public void Load (HaxeProject project)
		{
			mProject = project;
			
			TargetEntry.Text = mProject.BuildFile;
			AdditionalArgumentsEntry.Text = mProject.AdditionalArguments;
		}


		public void Store ()
		{
			if (mProject == null)
				return;

			mProject.AdditionalArguments = AdditionalArgumentsEntry.Text.Trim ();
			mProject.BuildFile = TargetEntry.Text.Trim ();

		}

		
		protected void OnBuildFileButtonClick (object sender, System.EventArgs e)
		{
			Gtk.FileChooserDialog fc =
				new Gtk.FileChooserDialog ("OpenFL project", this.Toplevel as Gtk.Window, FileChooserAction.Open,
                    "Cancel", ResponseType.Cancel,
					"Ok", ResponseType.Accept);
			
			Gtk.FileFilter filterHaxe = new Gtk.FileFilter ();
			filterHaxe.Name = "OpenFL project";
			filterHaxe.AddPattern ("*.xml");
			
			Gtk.FileFilter filterAll = new Gtk.FileFilter ();
			filterAll.Name = "All Files";
			filterAll.AddPattern ("*");
			
			fc.AddFilter (filterHaxe);
			fc.AddFilter (filterAll);
			
			if (mProject.BuildFile != "")
			{
				fc.SetFilename (System.IO.Path.Combine(mProject.BaseDirectory, mProject.BuildFile));
			}
			else
			{
				fc.SetFilename (mProject.BaseDirectory);
			}

			if (fc.Run () == (int)ResponseType.Accept)
			{
				string path = PathHelper.ToRelativePath (fc.Filename, mProject.BaseDirectory);
				
				TargetEntry.Text = path;
				Store ();
				Load (mProject);
			}

			fc.Destroy ();
		}
	}
	
}