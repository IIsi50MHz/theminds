// Buffering separates *Line event logic from AddLine,
// thereby allowing me to match server-channel to each
// tab (different log view for each tab).

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using Aspirations;

namespace Theminds {
   public struct TabId {
      public Quirk Connection;
      public string Channel;

      public TabId(Quirk c, string channel) {
         this.Connection = c;
         this.Channel = channel;
      }
   }

   public struct BufferData {
      public Color Color;
      public string Channel;
      public string Line;
      public bool NeedsNewTab;
      public BufferData(string line) {
         this.Line = line;
         this.Color = Color.Black;
         this.Channel = null;
         this.NeedsNewTab = true;
      }
   }

   public class Buffer {
      IAppControls app;
      Dictionary<ITab, LogBox> logBoxes;

      // These two are the inverses of each other. Sweet, I know.
      //  We need the inverse for AddLine.
      TwoWayDictionary<ITab, TabId> proust;
      public Buffer(IAppControls app) {
         this.app = app;
         logBoxes = new Dictionary<ITab, LogBox>(5);
         proust = new TwoWayDictionary<ITab, TabId>(5);

         TabId tId = new TabId(app.Connection, null);
         logBoxes[app.Tabber.Current] = app.LogBox;
         proust[app.Tabber.Current] = tId;

         // Page.Buffering events.
         app.Tabber.Moved += new TabDel(MoveToTab);
      }

      // If it comes from a different thread, then the line is
      // from the server (Line event). Otherwise, it's from the user
      // (SelfLine event).
      delegate void AddLineDel(string line, Color color);
      public void AddLine(string line) {
         BufferData data = new BufferData(line);

         PreLine(ref data);
         if (app.InvokeRequired) Line(ref data);
         else {
            data.Color = Color.DarkRed;
            SelfLine(ref data);
         }

         TabId id = new TabId(app.Connection, data.Channel);

         // *Line events allows clients to modify data.NeedsNewTab.
         //  This allows them to signal to their mother ship, us.
         handleNewTab(ref id, data.NeedsNewTab);
         LogBox l = logBoxes[proust[id]];
         app.Invoke(new AddLineDel(l.AddLine), 
            data.Line, data.Color);
         PostLine();
      }

      public static event
         NewChannelDel NewChannel = delegate { };
      void handleNewTab(ref TabId id, bool needsNewTab) {
         if (proust.ContainsKey(id)) return;
         if (!needsNewTab) {
            id.Channel = app.CurrentChannel;
         }
         else {
            TabId id2 = id;
            app.Invoke((MethodInvoker)delegate {
               AddChannel(id2.Channel);
               NewChannel(id2.Channel);
            });
         }
      }

      public void AddChannel(string channel) {
         app.CurrentChannel = channel;
         ITab newTab = app.Tabber.Add(channel);
         TabId id = new TabId(app.Connection, channel);
         proust[newTab] = id;

         LogBox l = new LogBox();
         logBoxes[newTab] = l;
         app.SwitchLogBox(l);
      }

      // If no key exists, `t` is a new tab.
      // AddChannelTab will handle instead.
      public void MoveToTab(ITab t) {
         if (!proust.ContainsKey(t)) return;

         TabId tId = proust[t];
         app.SwitchLogBox(logBoxes[t]);
      }

      public void Remove(ITab t) {
         proust.Remove(t);
         app.Tabber.Remove(t);
      }

      public event LineDel PreLine = delegate { };
      public event LineDel Line = delegate { };
      public event LineDel SelfLine = delegate { };
      public event MethodInvoker PostLine = delegate { };
   }

   public delegate void LineDel(ref BufferData dc);
   public delegate void NewChannelDel(string channel);
}