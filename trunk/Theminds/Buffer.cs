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
      LineDel fforde;
      public Buffer(IAppControls app) {
         this.app = app;
         logBoxes = new Dictionary<ITab, LogBox>(5);
         proust = new TwoWayDictionary<ITab, TabId>(5);

         TabId tId = new TabId(app.Connection, null);
         logBoxes[app.Tabber.Current] = app.LogBox;
         proust[app.Tabber.Current] = tId;

         // Page.Buffering events.
         app.Tabber.Moved += new TabDel(MoveToTab);
         fforde = new LineDel(suppressNewTabForPart);
      }

      // If it comes from a different thread, then the line is
      // from the server (Line event). Otherwise, it's from the user
      // (SelfLine event).
      delegate void AddLineDel(string line, Color color);
      public void AddLine(string line) {
         BufferData data = new BufferData(line);

         PreLine(ref data);
            if (app.InvokeRequired) Line(ref data);
            else SelfLine(ref data);
         PostLine(ref data);
         TabId id = new TabId(app.Connection, data.Channel);
         // *Line events allows clients to modify data.NeedsNewTab.
         //  This allows them to signal to their mother ship, us.
         handleNewTab(ref id, data.NeedsNewTab);
         
         LogBox l = logBoxes[proust[id]];
         l.Invoke(new AddLineDel(l.AddLine), 
            data.Line, data.Color);
      }

      public static event
         StringDel NewChannel = delegate { };
      void handleNewTab(ref TabId id, bool needsNewTab) {
         if (proust.ContainsKey(id)) return;
         if (!needsNewTab) id.Channel = app.CurrentChannel;
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

         TabId id = proust[t];
         app.SwitchLogBox(logBoxes[t]);
         app.CurrentChannel = id.Channel;
      }

      public void Remove(ITab t) {
         if (proust.Count == 1) return;
         string channel = proust[t].Channel;
         proust.Remove(t);
         app.Tabber.Remove(t);

         // Make sure `channel` is not null or a user.
         // Assumption: If we have a channel tab, all the
         //   nitty-gritty server talking is done.
         if (StringEx.IsChannel(channel)) {
            app.Connection.Message("PART {0}", channel);
            PostLine += fforde;
         }
      }

      void suppressNewTabForPart(ref BufferData data) {
         if(String.Format(App.Lion.Get("part.self"), data.Channel)
            != data.Line) return;
         data.NeedsNewTab = false;
         PostLine -= fforde;
      }


      public event LineDel PreLine = delegate { };
      public event LineDel Line = delegate { };
      public event LineDel SelfLine = delegate { };
      public event LineDel PostLine = delegate { };
   }

   public delegate void LineDel(ref BufferData data);
   public delegate void StringDel(string s);
}