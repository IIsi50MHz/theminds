using System;
using System.Drawing;
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
			JPQT.Part = new string[] { "PART ", "<-- {0} ({1}) has left {2}{3}", "<-- You have left {0}" };

			// Format: |:nick!crud quit :msg|
			JPQT.Quit = new string[] { "QUIT :", "<-- {0} ({1}) has quit{3}", "<-- You have quit {0}{1}" };

			mircRegex = new Bowel.MircRegex();
			serverPrefixNumberRegex = new Bowel.ServerPrefixNumberRegex();
		}

		static Quirk connection;
		static IBuffer buffer;
		
		public static void Init(Quirk c, IBuffer l) {
			buffer = l; connection = c;
			
			buffer.Line += new LineDel(hostName);
			buffer.Line += new LineDel(ServerPrefix);

			buffer.Line += delegate(ref string line, ref string channel, ref Color color) {
				// Strip mIRC colors.
				if (line.Contains("\u0003")) line = mircRegex.Replace(line, "");
			};

			buffer.Line += new LineDel(ping);
			buffer.Line += new LineDel(privmsg);
			buffer.Line += new LineDel(joinPartQuit);

			buffer.SelfLine += new LineDel(privmsg);
			buffer.SelfLine += new LineDel(selfJoin);
		}

		public static void ServerPrefix(ref string line, ref string channel, ref Color color) {
			string x = ":" + connection.Info.hostName;

			if (false == line.StartsWith(x)) return;

			// Format: <server name> <command number> <optional. nick>
			line = line.Substring(x.Length + 1);
			line = serverPrefixNumberRegex.Replace(line, "");
			if (line.StartsWith(connection.Info.nick)) {
				line = line.Substring(connection.Info.nick.Length + 1);
			}
			line = "[server] " + line;
		}

		static void hostName(ref string line, ref string channel, ref Color color) {
			if (line.StartsWith(":") == false) return;
			connection.Info.hostName = line.Substring(1, line.IndexOf(' ') - 1);
			buffer.Line -= new LineDel(hostName);
		}

		static void ping(ref string line, ref string channel, ref Color color) {
			if (false == line.StartsWith("PING :")) return;

			pingMessage = line;
			buffer.PostLine += new System.Windows.Forms.MethodInvoker(sendPong);
			color = Color.Blue;
		}

		static string pingMessage;
		static void sendPong() {
			buffer.Line += new LineDel(colorPong);
			// Remove before Message or else recursion
			buffer.PostLine -= new System.Windows.Forms.MethodInvoker(sendPong);

			// PING : is six letters
			connection.Message("PONG :" + pingMessage.Substring(6));
		}

		static void colorPong(ref string l, ref string chan, ref Color c) {
			if (false == l.StartsWith("PONG :")) return;
			c = Color.Blue;
			buffer.Line -= new LineDel(colorPong);
		}

		// Format: |PRIVMSG #channel msg| or |:nick!crud PRIVMSG #channel :msg|
		static void privmsg(ref string line, ref string channel, ref Color color) {
			if (false == line.Contains(" ")) return;
			bool selfMsg = line.StartsWith("PRIVMSG #");

			string[] lineTokens = line.Split(Page.Space, 4);

			// Prevents matching something like |NOTICE PRIVMSG ...|
			if (("PRIVMSG" == lineTokens[1] && line.StartsWith(":")) == false && selfMsg == false) return;
			
			channel = selfMsg ? lineTokens[1] : lineTokens[2];
			
			string nick, msg;
			if (selfMsg) {
				nick = connection.Info.nick;
				msg = line.Split(Page.Space, 3)[2];
				color = Color.DarkRed;
			}
			else {
				nick = lineTokens[0].Substring(1, line.IndexOf('!') - 1);
				msg = lineTokens[3].Substring(1);
			}			

			// ACTION uses a colon or not, depending on the source.
			string y = selfMsg ? ":\u0001ACTION" : "\u0001ACTION";
			if (msg.StartsWith(y)) {
				msg = lineTokens[2] + " " + lineTokens[3];
				line = String.Format("* {0} {1}", nick, identifyActions(msg));
				color = Color.Green;
				return;
			}
			
			line = String.Format("<{0}> {1}", nick, msg);
		}

		// Not an event handler; a helper
		static string identifyActions(string line) {
			string msg = line.Trim().Split('\u0001')[1].Substring(7);
			return msg;
		}

		// This is not called by NewSelfFilter. Self-parts are only filtered
		// through the server's response to the PART command.
		static void joinPartQuit(ref string line, ref string channel, ref Color color) {
			// |privmsg| comes before this (cf. event chain order).
			// If it's went through that, then it's not a JPQ.
			if (line.StartsWith("<")) return;

			// ASSIGN TEMPLATE.
			string[] template;
			if (line.Contains(JPQT.Join[0])) template = JPQT.Join;
			else if (line.Contains(JPQT.Part[0])) template = JPQT.Part;
			else if (line.Contains(JPQT.Quit[0])) {
				template = JPQT.Quit; channel = "<>";
			}
			else return;
			color = Color.Gray;

			// FIND NICK. Format: |:nick!IPcrud ...|
			string[] tokens = line.Split(Page.Space, 5);
			string[] nickTokens = tokens[0].Split('!');
			string nick = nickTokens[0].Substring(1);
			string ipcrud = nickTokens[1];

			// FIND MESSAGE.
			int omegaPos = line.IndexOf(':', 1);
			string argonaut = line.Substring(omegaPos + 1);
			//		No parentheses by default so I can handle empty part/quit messages.
			string omega = (omegaPos != -1) ? " (" + argonaut + ")" : "";
			
			// ASSIGN CHANNEL.
			if (JPQT.Join[0] == template[0]) channel = argonaut;
			else if (JPQT.Part[0] == template[0]) channel = tokens[2];
			//		Quit's channel already assigned to <>

			// PUT INTO TEMPLATE. [1] = for others; [2] = for user
			if (nick != connection.Info.nick) {
				line = String.Format(template[1], nick, ipcrud, buffer.CurrentChannel, omega);
				return;
			}
			
			line = String.Format(template[2], buffer.CurrentChannel, omega);
		} // joinPartQuit

		static void selfJoin(ref string line, ref string channel, ref Color color) {
			string x = "JOIN ";
			if (false == line.StartsWith("JOIN ")) return;
			color = Color.Blue;

			// Format: |JOIN #channel,#channel,#channel|
			string[] channels = line.Substring(x.Length).Split(',');
			buffer.CurrentChannel = channels[channels.Length - 1];
			channel = channels[channels.Length - 1];
		}
	}
}