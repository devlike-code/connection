using System;
using DevLike.Connection.Control;
using Eto.Drawing;
using Eto.Forms;

namespace DevLike.Connection.Editor
{
	public partial class MainForm : Form
	{
        private ConnectionGraphControl _connectionGraphControl;
        private GridView _gridView;

        public MainForm()
		{
			Title = "DevLike Connection Editor";
			MinimumSize = new Size(1024, 600);

			_connectionGraphControl = new ConnectionGraphControl();

            _gridView = new GridView();
            _gridView.ShowHeader = true;

            var hsplitter = new Splitter();
            hsplitter.Orientation = Orientation.Horizontal;
            hsplitter.FixedPanel = SplitterFixedPanel.Panel2;
            hsplitter.Panel1 = _connectionGraphControl;
            hsplitter.Panel1MinimumSize = 100;
            hsplitter.Panel2 = _gridView;
            hsplitter.Panel2MinimumSize = 100;
            hsplitter.Position = 800;
            Content = hsplitter;

            var newCommand = new Command { MenuText = "New", ToolBarText = "New" };
            newCommand.Executed += (o, e) => _connectionGraphControl.Clear();

            var openCommand = new Command { MenuText = "Open", ToolBarText = "Open" };
            openCommand.Executed += (o, e) => _connectionGraphControl.StartLoad();

            var saveCommand = new Command { MenuText = "Save", ToolBarText = "Save" };
            saveCommand.Executed += (o, e) => _connectionGraphControl.StartSave();

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
