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
         if (Keys.Escape == e.KeyCode) {
            Text = null; e.SuppressKeyPress = true;
            return;
         } 
         if (Keys.Enter != e.KeyCode) return;

         
         bool shouldStop = false;
         StopPresses(ref shouldStop);
         if (shouldStop) return;

         if (Text.StartsWith("/")) {
            int y = Text.IndexOf(' ');

            // Check for parameters.
            string command = (-1 != y) ? Text.Substring(1, y - 1) : Text.Substring(1);
            string arg = (-1 != y) ? Text.Substring(y + 1) : "";

            Command(command, arg);
         }
         else if (Text.Length > 0) Message(Text);
         base.OnKeyDown(e);
      }
	}
}