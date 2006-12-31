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
         Tab t = new Tab(AddTab, label);
         t.MouseDown += new MouseEventHandler(tab_Click);
         return t;
      }

      void tab_Click(object sender, MouseEventArgs e) {
         ITab tab = (ITab)sender;
         if (MouseButtons.Middle == e.Button) {
            tabber.Remove(tab); return;
         }
         tabber.MoveTo(tab);
      }

      public int TabsWidth {
         get { return tabsPanel.Width; }
      }

      public Control FocusGrabber {
         get { return inputBox; }
      }
   }
}