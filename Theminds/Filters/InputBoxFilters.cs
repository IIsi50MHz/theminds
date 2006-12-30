using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Aspirations;

namespace Theminds {
	static class InputBoxFilters {
		public static void Init(Quirk connection, InputBox inputBox, Page form) {
			inputBox.StopPresses += delegate(ref bool shouldStop) {
				if (!connection.Started) shouldStop = true;
			};

			// TODO: Split messages up according to MaxLength
			inputBox.Command += delegate(string cmd, string msg) {
				if ("me" != cmd) return;
				connection.Message(String.Format("PRIVMSG {0} :\u0001ACTION {1}\u0001",
					form.CurrentChannel, msg));
			};
			inputBox.Message += delegate(string message) {
				if (form.CurrentChannel == null) return;
				connection.Message("PRIVMSG " + form.CurrentChannel + " " + message);
			};
			inputBox.Command += delegate(string cmd, string channels) {
				if ("j" == cmd) connection.Message("JOIN " + channels);
			};
			inputBox.Command += delegate(string cmd, string arg) {
				if ("raw" == cmd) connection.Message(arg);
			};
			inputBox.Command += delegate(string cmd, string quitMsg) {
				if ("q" == cmd) connection.Dispose(quitMsg);
			};
			inputBox.Command += delegate(string cmd, string arg) {
				if ("w" == cmd) form.SeedUserList();
			};
		} // Init
	}
}