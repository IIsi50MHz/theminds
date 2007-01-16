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
      public TabId(Quirk c) : this(c, null) { }
   }

   public struct BufferData {
      public Color Color;
      public string Channel;
      public string Line;
      public BufferData(string line) {
         this.Line = line;
         this.Color = Color.Black;
         this.Channel = null;
      }
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
         Line += delegate { };
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
         TabId tId = new TabId(app.Connection);
         BufferData dc = new BufferData(line);
         if (app.InvokeRequired) {
            Line(ref dc); tId.Channel = dc.Channel;
            LogBox l = logBoxes[tId];
            app.BeginInvoke(new AddLineDel(l.AddLine),
               dc.Line, dc.Color);
         }
         else {
            dc.Color = Color.DarkRed;
            SelfLine(ref dc); tId.Channel = dc.Channel;
            LogBox l = logBoxes[tId];
            l.AddLine(dc.Line, dc.Color);
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

   public delegate void LineDel(ref BufferData dc);
   public interface IBuffer {
      event LineDel Line;
      event LineDel SelfLine;
      event MethodInvoker PostLine;

      string CurrentChannel { get; }
   }
}