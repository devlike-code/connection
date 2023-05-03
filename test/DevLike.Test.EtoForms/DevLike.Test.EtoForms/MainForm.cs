using System;
using Eto.Forms;
using Eto.Drawing;
using DevLike.Connection.Control;

namespace DevLike.Test.EtoForms
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			Title = "My Eto Form";
			MinimumSize = new Size(640, 480);

			var graphControl = new ConnectionGraphControl();
			graphControl.Width = 640;
			graphControl.Height = 480;
			
			var stackLatout = new StackLayout();
			stackLatout.Items.Add(new StackLayoutItem(new Label { Text = "start" }, false));
            stackLatout.Items.Add(new StackLayoutItem(new Scrollable { Content = graphControl }, true));
            stackLatout.Items.Add(new StackLayoutItem(new Label { Text = "end" }, false));
            Content = stackLatout;

            // create a few commands that can be used for the menu and toolbar
            var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
			clickMe.Executed += (sender, e) => MessageBox.Show(this, "I was clicked!");

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(this);

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new SubMenuItem { Text = "&File", Items = { clickMe } },
					// new SubMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
					// new SubMenuItem { Text = "&View", Items = { /* commands/items */ } },
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
					new ButtonMenuItem { Text = "&Preferences..." },
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};

			// create toolbar			
			ToolBar = new ToolBar { Items = { clickMe } };
		}
	}
}
