using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace Theminds {
	sealed partial class Page : Form, Aspirations.ITabsParent {
		delegate void ClearUserListDel();
		System.Collections.Generic.List<string> tmpUserListItems;

		public void SeedUserList() {
			if (CurrentChannel == null) return;

			logBox.Line += new LogBox.LineDel(filterWho);
			connection.Message("WHO " + CurrentChannel);
		}

		// |userList| flickers if you just do a Clear() and lots of Add()s.
		// XXX: assign channel
		void filterWho(ref string line, ref string channel) {
			if (InvokeRequired) {
				channel = line.Split(' ')[1];
				this.BeginInvoke(new LogBox.LineDel(filterWho), new object[] { line, channel });
				return;
			}

			if (line.Contains("End of /WHO")) {
				userList.Items.Clear();
				userList.Items.AddRange(tmpUserListItems.ToArray());
				tmpUserListItems.Clear();
				logBox.Line -= new LogBox.LineDel(filterWho);
				return;
			}

			LogBoxFilters.ServerPrefix(ref line, ref channel);

			string[] tokens = line.Split(' ');
			tmpUserListItems.Add(tokens[5]);
		}
	}
}