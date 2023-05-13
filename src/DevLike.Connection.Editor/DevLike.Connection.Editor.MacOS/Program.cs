using System;
using Eto.Forms;

namespace DevLike.Connection.Editor.Mac
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application(Eto.Platforms.macOS).Run(new MainForm());
		}
	}
}
