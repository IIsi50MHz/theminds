using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Aspirations;

namespace Theminds {
	sealed partial class Page : Form, ITabsParent {
		public void AddTab(Control c) { tabsPanel.Controls.Add(c); }
		public void RemoveTab(Control c) { tabsPanel.Controls.Remove(c); }
		public ITab CreateTab(string label) {
			return new Tab(delegate(Control c) {
				this.tabsPanel.Controls.Add(c);
			}, label);
		}

		public int TabsWidth {
			get { return tabsPanel.Width; }
		}

		public Control FocusGrabber {
			get { return inputBox; }
		}
	}
}