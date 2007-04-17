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
   public class Buffer {
      IAppControls app;

      // These two are the inverses of each other. Sweet, I know.
      //  We need the inverse for AddLine.
      TwoWayDictionary<ITab, TabId> proust;
      Dictionary<TabIdShell, LogBox> logBoxes;
      public Buffer(IAppControls app) {
         this.app = app;
         proust = new TwoWayDictionary<ITab, TabId>(5);
         logBoxes = new Dictionary<TabIdShell, LogBox>(5);

         TabId id = new TabId(app.Connection, null, app.LogBox);
         proust[app.Tabber.Current] = id;
         logBoxes[new TabIdShell(id)] = app.LogBox;

         // Page.Buffering events.
         app.Tabber.Moved += new TabDel(MoveToTab);
      }

      // If it comes from a different thread, then the line is
      // from the server (Line event). Otherwise, it's from the user
      // (SelfLine event).
      delegate void AddLineDel(string line, Color color);
      public void AddLine(string line) {
         BufferData data = new BufferData(line);

         // The hub
         PreLine(ref data);
         if (app.InvokeRequired) Line(ref data);
         else SelfLine(ref data);
         PostLine(ref data);

         TabIdShell shell = new TabIdShell(app.Connection, data.Channel);
         // *Line events allows clients to modify data.NeedsNewTab.
         //  This allows them to signal to their mother ship, us.
         handleNewTab(ref shell, data.NeedsNewTab);

         LogBox l = logBoxes[shell];
         l.Invoke(new AddLineDel(l.AddLine),
            data.Line, data.Color);
      }

      void handleNewTab(ref TabIdShell shell, bool needsNewTab) {
         if (logBoxes.ContainsKey(shell)) return;
         if (!needsNewTab) shell.Channel = app.CurrentChannel;
         else {
            TabId id = new TabId(shell, null);
            app.Invoke((MethodInvoker)delegate {
               AddChannel(id.Channel);
            });
         }
      }

      public void AddChannel() { AddChannel(null); }

      public void AddChannel(string channel) {
         app.CurrentChannel = channel;
         ITab newTab = app.Tabber.Add(channel);
         TabId id = new TabId(app.Connection, channel, new LogBox());
         app.SwitchLogBox(id.LogBox);

         proust[newTab] = id;
         logBoxes[new TabIdShell(id)] = id.LogBox;
      }

      // If no key exists, `t` is a new tab.
      // AddChannelTab will handle instead.
      public void MoveToTab(ITab t) {
         if (!proust.ContainsKey(t)) return;

         TabId id = proust[t];
         app.SwitchLogBox(id.LogBox);
         app.CurrentChannel = id.Channel;
      }

      public void Remove(ITab t) {
         if (proust.Count == 1) return;
         if (!proust.ContainsKey(t)) return;

         string channel = proust[t].Channel;
         logBoxes.Remove(new TabIdShell(proust[t]));
         proust.Remove(t);
         app.Tabber.Remove(t);
         if (StringEx.IsChannel(channel))
            app.Connection.Message("PART {0}", channel);
      }

      public event LineDel PreLine = delegate { };
      public event LineDel Line = delegate { };
      public event LineDel SelfLine = delegate { };
      public event LineDel PostLine = delegate { };
   }

   public delegate void LineDel(ref BufferData data);
   public delegate void StringDel(string s);
}