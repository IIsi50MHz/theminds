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
		void filterWho(ref string line) {
			if (InvokeRequired) {
				BeginInvoke(new LogBox.LineDel(filterWho), new object[] { line });
				return;
			}

			if (line.Contains("End of /WHO")) {
				userList.Items.Clear();
				userList.Items.AddRange(tmpUserListItems.ToArray());
				tmpUserListItems.Clear();
				logBox.Line -= new LogBox.LineDel(filterWho);
				return;
			}

			LogBoxFilters.ServerPrefix(ref line);

			string lint = line.Substring(line.IndexOf(connection.Info.hostName) +
				connection.Info.hostName.Length).Trim();

			Debug.WriteLine("Before:" + lint, "/WHO line, before");
			lint = lint.Substring(0, lint.IndexOf(' '));
			Debug.WriteLine("After:" + lint, "/WHO line, after");

			tmpUserListItems.Add(lint);
		}
	}
}