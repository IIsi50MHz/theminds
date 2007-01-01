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
      public TabId(Quirk c) : this(c, "") { }
   }

   public class Buffer : IBuffer {
      delegate void AddLineDel(string line, Color color);

      IAppControls app;
      Dictionary<TabId, LogBox> logBoxes;
      Dictionary<TabId, ITab> tabs;
      Dictionary<ITab, TabId> channelNames;
      public Buffer(IAppControls app) {
         this.app = app;
         logBoxes = new Dictionary<TabId, LogBox>(5);
         tabs = new Dictionary<TabId, ITab>(5);
         channelNames = new Dictionary<ITab, TabId>(5);

         PostLine += delegate { };
         Line += delegate { };c
         SelfLine += delegate { };

         TabId tId = new TabId(app.Connection);
         logBoxes[tId] = app.LogBox;
         tabs[tId] = app.Tabber.Current;
         channelNames[app.Tabber.Current] = tId;

         // Page.Buffering events.
         LogBoxFilters.NewChannel +=
            new LogBoxFilters.NewChannelDel(addChannelTab);
         app.Tabber.Moved += new TabDel(moveChannelTab);
      }

      public string CurrentChannel {
         get { return app.CurrentChannel; }
      }

      // If it comes from a different thread, then the line is
      // from the server (Line event). Otherwise, it's from the user
      // (SelfLine event).
      public void Add(string line) {
         Color color = Color.Black;
         TabId tId = new TabId(app.Connection);
         if (app.InvokeRequired) {
            Line(ref line, ref tId.Channel, ref color);
            LogBox l = logBoxes[tId];
            app.BeginInvoke(new AddLineDel(l.AddLine), line, color);
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
         app.CurrentChannel = channel;
         TabId tId = new TabId(app.Connection, channel);
         tabs[tId] = app.Tabber.Add(channel);
         channelNames[app.Tabber.Current] = tId;

         LogBox l = new LogBox();
         logBoxes[tId] = l;

         app.SwitchLogBox(l);
      }

      // If no key exists, `t` is a new tab.
      // AddChannelTab will handle instead.
      void moveChannelTab(ITab t) {
         if (!channelNames.ContainsKey(t)) return;

         TabId tId = channelNames[t];
         app.SwitchLogBox(logBoxes[tId]);
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

      string CurrentChannel { get; }
   }
}