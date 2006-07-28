using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Aspirations {
	public class Tab : Button, ITab {
		public double Shrinkage {
			get { return shrinkage; }
			set {
				if (value == shrinkage) return;
				this.shrinkage = value;
				this.Width = (int) (this.trueWidth * value);
			}
		}
		double shrinkage = 1.0;

		int trueWidth;

		// del: Add |this| to an object that will calculate its
		//    autosized width.
		public Tab(KissWidthDel del)
			: base() {
			this.AutoSize = true;
			del(this);
			this.trueWidth = this.Width;
			this.AutoSize = false;

			this.Width = (int) (trueWidth * Shrinkage);
		}

		public Tab(KissWidthDel del, string text)
			: this(del) {
			this.Text = text;
		}

		public int TrueWidth { get { return trueWidth; } }
	}
}