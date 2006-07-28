using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Aspirations;

namespace Theminds {
	sealed partial class Page : Form {
		void FunnelLine(string line) {
			logBox.AddLine(line);
		}
	}
}