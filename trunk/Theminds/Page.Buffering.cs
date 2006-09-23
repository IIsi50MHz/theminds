// Buffering separates *Line event logic from AddLine,
// thereby allowing me to match server-channel to each
// tab (different log view for each tab).

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace Theminds {
	sealed partial class Page : Form, IBuffer {
		delegate void AddLineDel(string line, Color color);

		// If it comes from a different thread, then the line is
		// from the server (Line event). Otherwise, it's from the user
		// (SelfLine event).
      void BufferLine(Quirk sender, string line) {
         string channel = ""; Color color = Color.Black;
         if (InvokeRequired) {
            Line(ref line, ref channel, ref color);
            LogBox l = logBoxes["server1." + channel];
            BeginInvoke(new AddLineDel(l.AddLine), new object[] { line, color });
         }
         else {
            color = Color.DarkRed;
            SelfLine(ref line, ref channel, ref color);
            LogBox l = logBoxes["server1." + channel];
            l.AddLine(line, color);
         }
         Debug.WriteLine(channel, "Channel (via FunnelLine)");
         PostLine();
      }

      void AddChannelTab(string channel) {
         currentChannel = channel;
         tabs["server1." + channel] = tabber.Add(channel);
         LogBox l = new LogBox();
         logBoxes["server1." + channel] = l;

         logBoxPanel.Controls.RemoveAt(0);
         logBoxPanel.Controls.Add(l);
      }

		public event LineDel Line;
		public event LineDel SelfLine;
		public event MethodInvoker PostLine;
	}

	public delegate void LineDel(ref string line, ref string channel, ref Color color);
	public interface IBuffer {
		event LineDel Line;
		event LineDel SelfLine;
		event MethodInvoker PostLine;

		string CurrentChannel { get; set;}
	}
}