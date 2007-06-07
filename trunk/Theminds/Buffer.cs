// Buffering separates *Line event logic from AddLine,
// thereby allowing me to match server-channel to each
// tab (different log view for each tab).

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using Aspirations;
using M = System.Windows.Forms.MethodInvoker;

namespace Theminds {
   public class Buffer {
      IAppControls app;

      // I need the two-way-osity for AddLine.
      TwoWayDictionary<ITab, TabKey> proust = new TwoWayDictionary<ITab, TabKey>(5);
      public Buffer(IAppControls app) {
         this.app = app;

         TabKey id = new TabKey(app.Connection, null, app.LogBox);
         proust[app.Tabber.Current] = id;

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
         
         if (data.Ignore) return;
         if (data.BroadcastId != null)
            broadcastHelper(data.Line, data.Color);
         else {
            TabKey tab = new TabKey(app.Connection, data.Channel, null);
            if (!proust.ContainsKey(tab))
               app.Invoke((M)delegate { AddChannel(tab); });

            // AddChannel now guarantees `tab` is inside
            // `proust`, ripe for picking. Forward & reverse
            // ensures that tab.LogBox is not null.
            tab = proust[proust[tab]];
            app.Invoke(new AddLineDel(tab.LogBox.AddLine),
                  data.Line, data.Color);
         }
      }

      private void broadcastHelper(string line, Color color) {
         List<TabKey> tabs = proust.Values;
         Broadcast(ref tabs);
         app.BeginInvoke((M)delegate {
            tabs.ForEach((Action<TabKey>)delegate(TabKey tab) {
               tab.LogBox.AddLine(line, color);
            });
         });
      }

      public void AddChannel() { 
         AddChannel(new TabKey(app.Connection, null, null)); 
      }
      public void AddChannel(TabKey tab) {
         app.CurrentChannel = tab.Channel;
         ITab newTab = app.Tabber.Add(tab.Channel);
         tab.LogBox = new LogBox();
         app.SwitchLogBox(tab.LogBox);

         proust[newTab] = tab;
         NewChannel(tab.Channel);
      }

      // If no key exists, `t` is a new tab.
      // AddChannelTab will handle instead.
      public void MoveToTab(ITab t) {
         if (!proust.ContainsKey(t)) return;

         TabKey id = proust[t];
         app.SwitchLogBox(id.LogBox);
         app.CurrentChannel = id.Channel;
      }

      public void Remove(ITab t) {
         if (proust.Count == 1) return;
         if (!proust.ContainsKey(t)) return;

         string channel = proust[t].Channel;
         proust.Remove(t);
         app.Tabber.Remove(t);
         if (StringEx.IsChannel(channel))
            app.Connection.Message("PART {0}", channel);
      }

      public event LineDel PreLine = delegate { };
      public event LineDel Line = delegate { };
      public event LineDel SelfLine = delegate { };
      public event LineDel PostLine = delegate { };
      public event Action<String> NewChannel = delegate { };
      public event BroadcastDel Broadcast = delegate { };
   }

   public delegate void LineDel(ref BufferData data);
   public delegate void BroadcastDel(ref List<TabKey> tabs);
}