using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace Theminds {
	sealed class InputBox : TextBox {
		public delegate void CommandDel(string cmd, string arg);
		public delegate void MessageDel(string message);

		// Utter hack, but it avoids passing |connection| into here.
		public delegate void StopPressesDel(ref bool shouldStop);

		public event CommandDel Command;
		public event MessageDel Message;
		public event StopPressesDel StopPresses;

		protected override void OnKeyDown(KeyEventArgs e) {
			// Press Esc, line turns empty. Similar to Command Prompt.
			if (Keys.Escape == e.KeyCode) Text = null;
			if (Keys.Enter != e.KeyCode) return;

			bool shouldStop = false;
			StopPresses(ref shouldStop);
			if (shouldStop) return;

			if (Text.StartsWith("/")) {
				int y = Text.IndexOf(' ');

				// Check command for any parameters.
				string command = (-1 != y) ? Text.Substring(1, y - 1) : Text.Substring(1);
				string arg = (-1 != y) ? Text.Substring(y + 1) : "";

				Command(command, arg);

				// TODO: Erase text once we implement command history.
			}

			// Precondition: Connection must be non-null.
			else if (Text.Length > 0) Message(Text);

			// Mandatory per Windows Forms docs.
			base.OnKeyDown(e);
		} // OnKeyDown
	}
}