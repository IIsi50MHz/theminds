#define ENABLE_CONNECT

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Aspirations;

namespace Theminds {
	sealed partial class Page : Form, IBuffer {
		public static char[] Space = new char[] { ' ' };
		Quirk connection;

		public string CurrentChannel {
			get { return currentChannel; }
			set { currentChannel = value; }
		}

		// Testcase: Join a channel, try messaging.
		public string currentChannel;
		public Page() {
			SetUp(); // MainForm.SetUpForm.cs

			// Prevent empty event list exceptions
			PostLine += delegate { };

			QuirkStart mozNet = new QuirkStart();
				mozNet.nick = "Tongue"; mozNet.port = 6667;
				mozNet.serv = "irc.mozilla.org";
				mozNet.user = "USER cryptoliter2 8 * :Hi";
			connection = new Quirk(mozNet);
			connection.Line += new Quirk.LineDel(BufferLine);

			// Converts server garbage to post-impressionist garbage
			LogBoxFilters.Init(connection, this);

			// For commands on rtbInput
			InputBoxFilters.Init(connection, inputBox, this);

			// For StartSeedingUserList()
			tmpUserListItems = new System.Collections.Generic.List<string>();

			connection.Start();
		}

		/**** Event handlers ****/
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			connection.Dispose();
			base.OnClosing(e);
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			if (false == e.Control) goto noControl;
			switch (e.KeyCode) {
				case Keys.PageUp: tabber.MoveToPrev(); break;
				case Keys.PageDown: tabber.MoveToNext(); break;
				case Keys.T: tabber.Add(); break;
				case Keys.W: tabber.RemoveCurrent(); break;
				case Keys.Q: this.Close(); break;
			}

		noControl:
			base.OnKeyDown(e);
		}

		/**** Static members ****/
		public static void Alert(object alert) { MessageBox.Show(alert.ToString()); }
	}
}