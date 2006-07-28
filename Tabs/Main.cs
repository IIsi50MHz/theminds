using System;
using System.Windows.Forms;
using System.Drawing;

namespace Aspirations {
	class Oo : Form, ITabsParent {
		// Used for ButtonTabs
		public TextBox input;

		TabsController tabs;
		Panel tabsPanel;
		public Oo()
			: base() {
			this.SuspendLayout();
			this.Width = 800;
			this.Height = 500;
			this.KeyPreview = true;

			input = new TextBox();
			input.Dock = DockStyle.Bottom;
			this.Controls.Add(input);

			tabsPanel = new Panel();
			tabsPanel.Dock = DockStyle.Top;
			tabsPanel.AutoSize = true;
			Controls.Add(tabsPanel);

			tabs = new TabsController(this, "(New)");
			tabs.Add("(New Tabbbbbbb)");

			this.KeyDown += new KeyEventHandler(TabNavigate);
			this.Resize += delegate { tabs.Resize(); };
			this.ResumeLayout();
		}

		void TabNavigate(object sender, KeyEventArgs e) {
			if (true != e.Control) return;
			switch (e.KeyCode) {
				case Keys.PageDown: tabs.Next(); break;
				case Keys.PageUp: tabs.Prev(); break;
				case Keys.W: tabs.RemoveCurrent(); break;
				case Keys.Q: this.Close(); break;
				case Keys.T: tabs.Add("(New Tabbbbbbb)"); break;
			}
		}

		public static void Alert(object o) { MessageBox.Show(o.ToString()); }

		[STAThread]
		public static void Main() {
			Application.Run(new Oo());
		}

		/* ITabsParent */
		public void AddTab(Control button) { tabsPanel.Controls.Add(button); }
		public void RemoveTab(Control button) { tabsPanel.Controls.Remove(button); }
		public int TabsWidth {
			get {
				return tabsPanel.Width;
			}
		}
		Control ITabsParent.FocusGrabber { get { return input; } }
	}
}