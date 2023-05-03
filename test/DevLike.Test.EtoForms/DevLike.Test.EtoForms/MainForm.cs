using DevLike.Connection.Control;
using Eto.Drawing;
using Eto.Forms;

namespace DevLike.Test.EtoForms
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			Title = "My Eto Form";
			MinimumSize = new Size(640, 480);

			var graphControl = new ConnectionGraphControl();
            Content = graphControl;

            var newCommand = new Command { MenuText = "New", ToolBarText = "New" };
            newCommand.Executed += (o, e) => graphControl.Clear();

            var openCommand = new Command { MenuText = "Open", ToolBarText = "Open" };
            openCommand.Executed += (o, e) => graphControl.StartLoad();

            var saveCommand = new Command { MenuText = "Save", ToolBarText = "Save" };
            saveCommand.Executed += (o, e) => graphControl.StartSave();

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (o, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (o, e) => new AboutDialog().ShowDialog(this);
			
			Menu = new MenuBar
			{
				Items =
				{
					new SubMenuItem { Text = "&File", Items = { newCommand, openCommand, saveCommand } },
				},
				ApplicationItems =
				{
					new ButtonMenuItem { Text = "&Preferences..." },
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};
		}
	}
}
