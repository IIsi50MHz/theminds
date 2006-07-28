using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Aspirations {
	public delegate void TabDel(ITab t);
	public delegate void IntDel(int i);
	public class TabsController {
		List<ITab> tabs;

		ITabsParent parent;
		string defaultLabel;
		public TabsController(ITabsParent p, string s) {
			this.parent = p;
			this.defaultLabel = s;
			this.NewTab += new TabDel(OnNewTab);
			this.Moved += delegate { };
			this.Removed += delegate { };

			tabs = new List<ITab>();
		}

		public void Init() { Add(this.defaultLabel); }

		public ITab Current {
			get { return tabs[this.current]; }
		}

		// right: total width of all the buttons.
		// realWidth: total width without shrinkage
		// current: index in |tabs| of current tab
		int right, current, realWidth;
		public event TabDel NewTab;
		public void Add(string label) {
			if (tabs.Count > 0) {
				tabs[current].ForeColor = Color.Black;
			}

			// Math trick. This is before we add it, so it's
			// lastIndex + 1, or Count.
			current = tabs.Count;

			ITab x = this.parent.CreateTab(label);
			this.NewTab(x);

			this.right += x.Width;
			this.realWidth += x.TrueWidth;

			parent.FocusGrabber.Select();
			Resize();
		}

		public void Add(string[] labels) {
			foreach (string label in labels) Add(label);
		}
		public void Add() { Add(this.defaultLabel); }

		public event TabDel Moved;
		public void MoveTo(ITab tab) {
			int index = tabs.IndexOf(tab);
			c(delegate { current = index; parent.FocusGrabber.Select(); });
			this.Moved(tab);
		}
		public void MoveTo(int index) { MoveTo(tabs[index]); }

		public void MoveToNext() {
			c(delegate { current = (current + 1) % tabs.Count; });
		}

		public void MoveToPrev() {
			// -1 % 4 = -1. To make it 3, add |.Count|.
			c(delegate { current = (current - 1 + tabs.Count) % tabs.Count; });
		}

		public event IntDel Removed;
		public void RemoveCurrent() { Remove(current); }
		public void Remove(int index) {
			if (tabs.Count < current + 1) {
				Debug.WriteLine("Trying to remove tab at index " + index, "TabsController Remove()");
				throw new InvalidOperationException("Removing a tab that does not exist. Make sure to call Init.");
			}
			ITab x = tabs[index];
			int hashCode = x.GetHashCode();

			int width = x.Width;
			this.realWidth -= x.TrueWidth;

			parent.SuspendLayout();
			parent.RemoveTab((Control) x);
			tabs.Remove(x);

			int z = tabs.Count;
			for (int i = index; i < z; i++) {
				tabs[i].Left -= width;
			}

			// Stop when true:
			// * Close last, current one -> go to the new last tab.
			// * Close tab after current tab -> stay, no changes.
			// * Close tab before current tab -> update number.
			if (tabs.Count > 0) {
				if (index < current || current >= tabs.Count) {
					current -= 1;
				}
			}
			this.right -= width;

			MoveTo(current);
			Resize();
			Removed(hashCode);
			parent.ResumeLayout();
		}

		/**** Protected members ****/
		protected void OnNewTab(ITab t) {
			t.Left = right;
			t.MouseDown += new MouseEventHandler(Tab_Click);
			tabs.Add(t);
		}

		/**** Private members ****/
		// Magic over; tabs are all added/removed already
		void Resize() {
			int maxWidth = parent.TabsWidth;
			int realTotalWidth = 0;
			foreach (ITab tab in tabs) {
				realTotalWidth += tab.TrueWidth;
			}
			
			double sB = (double) maxWidth / realTotalWidth;
			this.right = 0;
			double shrinkBy = (sB < 1.0) ? sB : 1.0;
			foreach (ITab tab in tabs) {
				tab.Shrinkage = shrinkBy;
				tab.Left = right;
				right += tab.Width;
			}
		}

		delegate void ChangeCurrentDelegate();
		void c(ChangeCurrentDelegate del) {
			parent.SuspendLayout();
			if (tabs.Count != 0) {
				tabs[current].ForeColor = Color.Black;
				del(); // Here, current changes.
				tabs[current].ForeColor = Color.Red;
			}
			else { Add(); }
			parent.ResumeLayout();
		}

		void Tab_Click(object s, MouseEventArgs e) {
			ITab sender = (ITab) s;
			if (MouseButtons.Middle == e.Button) {
				Remove(tabs.IndexOf(sender));
				return;
			}
			MoveTo(sender);
		}
	} // class TabsController
} // namespace