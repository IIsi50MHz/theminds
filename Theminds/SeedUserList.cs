using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace Theminds {
	sealed partial class App : Form, Aspirations.ITabsParent {
		delegate void ClearUserListDel();
		System.Collections.Generic.List<string> tmpUserListItems;

		public void SeedUserList() {
			if (currentChannel == null) return;

			Buffer.Line += new LineDel(filterWho);
			connection.Message("WHO " + currentChannel);
		}

		// |userList| flickers if you just do a Clear() and lots of Add()s.
		void filterWho(ref string line, ref string channel, ref Color color) {
			string[] tokens = line.Split(' ');
			color = Color.Blue;
			if (line.Contains("End of /WHO")) {
				channel = tokens[1];
				stopFilteringWho();
				return;
			}

			LogBoxFilters.ServerPrefix(ref line, ref channel, ref color);

			channel = tokens[1];
			tmpUserListItems.Add(tokens[5]);
		}

		void stopFilteringWho() {
			if (this.InvokeRequired) {
				this.BeginInvoke((MethodInvoker) delegate {
					stopFilteringWho();
				});
				return;
			}
			userList.Items.Clear();
			userList.Items.AddRange(tmpUserListItems.ToArray());
			tmpUserListItems.Clear();
			Buffer.Line -= new LineDel(filterWho);
		}
	}
}