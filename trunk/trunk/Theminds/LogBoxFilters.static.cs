using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Theminds {
	static class LogBoxFilters {
		struct JPQT {
			public static string[] Join;
			public static string[] Part;
			public static string[] Quit;
		}

		static Bowel.MircRegex mircRegex;
		static Bowel.ServerPrefixNumberRegex serverPrefixNumberRegex;
		static LogBoxFilters() {
			// TODO: remove CurrentChannel from Parts once tabs are in.

			// Format: |:nick!crud join :#channel|
			JPQT.Join = new string[] { "JOIN :", "--> {0} ({1}) has joined {2}", "--> You have joined {0}" };
			
			// Format: |:nick!crud part #channel :msg|
			JPQT.Part = new string[] { "PART ", "<-- {0} ({1}) has left {2}{3}", "--> You have left {0}" };

			// Format: |:nick!crud quit :msg|
			JPQT.Quit = new string[] { "QUIT :", "<-- {0} ({1}) has quit ({3})", "--> You have quit {0} ({1})" };

			mircRegex = new Bowel.MircRegex();
			serverPrefixNumberRegex = new Bowel.ServerPrefixNumberRegex();
		}

		static Quirk connection;
		static LogBox logBox;
		static Page form;
		
		public static void Init(Quirk c, LogBox l, Page f) {
			logBox = l; connection = c; form = f;
			
			logBox.Line += new LogBox.LineDel(hostName);
			logBox.Line += new LogBox.LineDel(ServerPrefix);

			logBox.Line += delegate(ref string line) {
				// Strip mIRC colors.
				if (line.Contains("\u0003")) line = mircRegex.Replace(line, "");
			};

			logBox.Line += new LogBox.LineDel(initialPingPong);
			logBox.Line += new LogBox.LineDel(privmsg);
			logBox.Line += new LogBox.LineDel(joinPartQuit);

			logBox.SelfLine += new LogBox.LineDel(privmsg);
			logBox.SelfLine += new LogBox.LineDel(selfJoin);
		}

		public static void ServerPrefix(ref string line) {
			string x = ":" + connection.Info.hostName;

			if (false == line.StartsWith(x)) return;

			// |line|: <server name> <command number> <optional. nick>
			line = line.Substring(x.Length + 1);
			line = serverPrefixNumberRegex.Replace(line, "");
			if (line.StartsWith(connection.Info.nick)) {
				line = line.Substring(connection.Info.nick.Length + 1);
			}
			line = "[server] " + line;
		}

		static void hostName(ref string line) {
			if (line.StartsWith(":") == false) return;
			connection.Info.hostName = line.Substring(1, line.IndexOf(' ') - 1);
			logBox.Line -= new LogBox.LineDel(hostName);
		}

		static void initialPingPong(ref string line) {
			if (line.Contains("Welcome")) { logBox.Line -= new LogBox.LineDel(initialPingPong); return; }
			if (false == line.StartsWith("PING :")) { return; }

			// "PING :" is six characters long.
			connection.Message("PONG :" + line.Substring(6));
		}

		// Format: |PRIVMSG #channel msg| or |:nick!crud PRIVMSG #channel :msg|
		static void privmsg(ref string line) {
			if (false == line.Contains(" ")) return;
			bool selfMsg = line.StartsWith("PRIVMSG #");

			string[] lineTokens = line.Split(new char[] { ' ' }, 4);
			if ("PRIVMSG" != lineTokens[1] && selfMsg == false) return;
			
			string channel = selfMsg ? lineTokens[1] : lineTokens[2];
			Debug.WriteLine(channel, "privmsg channel finder");
			
			string nick, msg;
			if (selfMsg) {
				nick = connection.Info.nick;
				msg = lineTokens[2];
			}
			else {
				nick = lineTokens[0].Substring(1, line.IndexOf('!') - 1);
				msg = lineTokens[3].Substring(line.IndexOf(':', 2) + 1);
			}			

			// ACTION uses a colon or not, depending on the source.
			string y = selfMsg ? ":\u0001ACTION" : "\u0001ACTION";
			if (msg.StartsWith(y)) {
				msg = lineTokens[2] + " " + lineTokens[3];
				line = String.Format("* {0} {1}", nick, identifyActions(msg));
				return;
			}
			
			line = String.Format("<{0}> {1}", nick, msg);
		}

		static string identifyActions(string line) {
			string msg = line.Trim().Split('\u0001')[1].Substring(7);
			Debug.WriteLine(msg, "identifyActions msg");
			return msg;
		}

		// This is not called by NewSelfFilter. Self-parts are only filtered
		// through the server's response to the PART command.
		static void joinPartQuit(ref string line) {
			// this.privmsg must come before this (cf. event chain order).
			// If it's been through that, then it's not a JPQ.
			if (line.StartsWith("<")) return;

			string[] x;
			if (line.Contains(JPQT.Join[0])) x = JPQT.Join;
			else if (line.Contains(JPQT.Part[0])) x = JPQT.Part;
			else if (line.Contains(JPQT.Quit[0])) x = JPQT.Quit;
			else return;

			// Format: |:nick!IPcrud JOIN :#channel|
			int exclaimPos = line.IndexOf('!');
			string nick = line.Substring(1, exclaimPos - 1);
			string ipcrud = line.Substring(exclaimPos + 1, line.IndexOf(' ') - exclaimPos);

			// Deal with part/quit messages.
			string omega;
			if (x[0] != JPQT.Join[0]) {
				int omegaPos = line.IndexOf(':', 1);

				// Deal with empty part messages as well as
				// the possibility precludes using parentheses in the
				// JPQT.
				omega = (omegaPos != -1) ? " (" + line.Substring(omegaPos + 1) + ")" : "";
			}
			else omega = null; // No message given.

			// x[0] is the filter. x[1] is the template for others. x[2] is the template for user.
			if (nick != connection.Info.nick) {
				line = String.Format(x[1], nick, ipcrud, form.CurrentChannel, omega);
			}
			else {
				line = String.Format(x[2], form.CurrentChannel, omega);
			}
		} // joinPartQuit

		static void selfJoin(ref string line) {
			string x = "JOIN ";
			if (false == line.StartsWith("JOIN ")) return;

			// Format: |JOIN #channel,#channel,#channel|
			string[] channels = line.Substring(x.Length).Split(',');
			form.CurrentChannel = channels[channels.Length - 1];
		}
	}
}