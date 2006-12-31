// Buffering separates *Line event logic from AddLine,
// thereby allowing me to match server-channel to each
// tab (different log view for each tab).

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Aspirations;

namespace Theminds {
   sealed partial class Page : Form, IBuffer {
      struct TabId {
         public Quirk Connection;
         public string Channel;

         public TabId(Quirk c, string channel) {
            this.Connection = c;
            this.Channel = channel;
         }
         public TabId(Quirk c) : this(c, "") { }
      }

      delegate void AddLineDel(string line, Color color);

      // If it comes from a different thread, then the line is
      // from the server (Line event). Otherwise, it's from the user
      // (SelfLine event).
      void bufferLine(Quirk sender, string line) {
         string channel = ""; Color color = Color.Black;
         TabId tId = new TabId(connection);
         if (InvokeRequired) {
            Line(ref line, ref tId.Channel, ref color);
            LogBox l = logBoxes[tId];
            BeginInvoke(new AddLineDel(l.AddLine), new object[] { line, color });
         }
         else {
            color = Color.DarkRed;
            SelfLine(ref line, ref tId.Channel, ref color);
            LogBox l = logBoxes[tId];
            l.AddLine(line, color);
         }
         PostLine();
      }

      void addChannelTab(string channel) {
         currentChannel = channel;
         TabId tId = new TabId(connection, channel);
         tabs[tId] = tabber.Add(channel);
         channelNames[tabber.Current] = tId;

         LogBox l = new LogBox();
         logBoxes[tId] = l;

         switchLogBox(l);
      }

      // If no key exists, `t` is a new tab.
      // AddChannelTab will handle instead.
      void moveChannelTab(ITab t) {
         if (!channelNames.ContainsKey(t)) return;

         TabId tId = channelNames[t];
         switchLogBox(logBoxes[tId]);
      }

      void switchLogBox(Control c) {
         if (c == logBoxPanel.Controls[0]) return;
         this.SuspendLayout();
         logBoxPanel.Controls.RemoveAt(0);
         logBoxPanel.Controls.Add(c);
         this.ResumeLayout();
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