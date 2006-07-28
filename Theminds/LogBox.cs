using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace Theminds {
	sealed class LogBox : RichTextBox {
		public delegate void LineDel(ref string line, ref string channel);
		public event LineDel Line;
		public event LineDel SelfLine;

		// http://www.dotnet247.com/247reference/msgs/55/279639.aspx
		// http://www.dotnet4all.com/Dot-Net-Books/2004/08/how-to-auto-scroll-richtextbox-in.html
		
		// If it comes from a different thread, I know it's
		// from the server, not the user -> color = true.
		public void AddLine(string line) {
			if (InvokeRequired) {
				string channel = "";
				Line(ref line, ref channel);
				Debug.WriteLine(channel, "Channel");
				BeginInvoke(new AddLineDel(addLineHelper), new object[] { line, false });
			}
			else {
				string channel = "";
				SelfLine(ref line, ref channel);
				Debug.WriteLine(channel, "Channel");
				addLineHelper(line, true);
			}
		}

		delegate void AddLineDel(string line, bool red);
		void addLineHelper(string line, bool red) {
			// Any changes to |log| unselects any text the user has selected.
			int oldSelectStart = SelectionStart;
			int oldSelectLength = SelectionLength;
			if (red) {
				Select(TextLength, 0);

				this.SelectionColor = System.Drawing.Color.Red;
				this.SelectedText = line + "\n";
				this.SelectionColor = Color.Black;
			}
			else { AppendText(line + "\n"); }

			Select(oldSelectStart, oldSelectLength);
			NativeMethods.SendMessage(Handle, 0x115, (IntPtr) 7, IntPtr.Zero);
		}
	}
}