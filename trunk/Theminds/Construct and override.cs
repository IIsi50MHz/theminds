using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using Aspirations;

namespace Theminds {
   public sealed partial class App : Form {
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
      Buffer buffer;
      public App() {
         this.SetUpForm(); // MainForm.SetUpForm.cs
         Lion = new Ideas(@"lion.txt");

         QuirkStart mozNet = new QuirkStart();
         mozNet.nick = "Tongue"; mozNet.port = 6667;
         mozNet.serv = "irc.mozilla.org";
         mozNet.user = "USER cryptoliter2 8 * :Hi";
         connection = new Quirk(mozNet);
         connection.NewLine += new Quirk.NewLineDel(
            delegate(Quirk q, string s) { Buffer.Add(s); });

         this.buffer = new Buffer(this);
         LogBoxFilters.Init(this);
         JoinPartQuitFilter.Init(connection, Buffer);
         InputBoxFilters.Init(this);
         WhoFilter.Init(this);
         PrivmsgFilter.Init(this);

         connection.Start();
      }

      /**** Event handlers ****/
      protected override void OnClosing(
         System.ComponentModel.CancelEventArgs e) {
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