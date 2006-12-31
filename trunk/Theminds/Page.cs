using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using Aspirations;

namespace Theminds {
   sealed partial class Page : Form, IBuffer {
      public static char[] Space = new char[] { ' ' };
      Quirk connection;

      string currentChannel;
      public string CurrentChannel {
         get { return currentChannel; }
         set { currentChannel = value; }
      }

      public static Ideas Lion;

      // TODO: handle tab close viv parting a channel.
      // TODO: set up a preference system to remove hardcoded lion.txt
      // TODO: depressed button state?
      public Page() {
         this.SetUp(); // MainForm.SetUpForm.cs
         PostLine += delegate { };
         Lion = new Ideas(@"lion.txt");

         QuirkStart mozNet = new QuirkStart();
         mozNet.nick = "Tongue"; mozNet.port = 6667;
         mozNet.serv = "irc.mozilla.org";
         mozNet.user = "USER cryptoliter2 8 * :Hi";
         connection = new Quirk(mozNet);
         connection.Line += new Quirk.LineDel(bufferLine);

         LogBoxFilters.Init(connection, this);
         JoinPartQuitFilter.Init(connection, this);
         InputBoxFilters.Init(connection, inputBox, this);
         InitBuffering();

         // For StartSeedingUserList()
         tmpUserListItems = new List<string>();

         connection.Start();
      }

      /**** Construction workers ****/
      Dictionary<TabId, LogBox> logBoxes;
      Dictionary<TabId, ITab> tabs;
      Dictionary<ITab, TabId> channelNames;
      void InitBuffering() {
         // Page.Buffering
         logBoxes = new Dictionary<TabId, LogBox>(5);
         tabs = new Dictionary<TabId, ITab>(5);
         channelNames = new Dictionary<ITab, TabId>(5);

         TabId tId = new TabId(connection);
         logBoxes[tId] = logBox;
         tabs[tId] = tabber.Current;
         channelNames[tabber.Current] = tId;

         // Page.Buffering events.
         LogBoxFilters.NewChannel +=
            new LogBoxFilters.NewChannelDel(addChannelTab);
         tabber.Moved += new TabDel(moveChannelTab);
      }

      /**** Event handlers ****/
      protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
         connection.Dispose();
         base.OnClosing(e);
      }

      protected override void OnKeyDown(KeyEventArgs e) {
         if (false == e.Control) goto noControl;
         e.SuppressKeyPress = true;
         switch (e.KeyCode) {
            case Keys.PageUp: tabber.MoveToPrev(); break;
            case Keys.PageDown: tabber.MoveToNext(); break;
            case Keys.T: tabber.Add(); break;
            case Keys.W: tabber.RemoveCurrent(); break;
            case Keys.Q: this.Close(); break;
            default: e.SuppressKeyPress = false; break;
         }

      noControl:
         base.OnKeyDown(e);
      }

      /**** Static members ****/
      public static void Alert(object alert) {
         MessageBox.Show(alert.ToString());
      }
   }
}