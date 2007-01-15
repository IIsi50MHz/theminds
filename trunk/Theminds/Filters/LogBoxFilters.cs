using System;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Aspirations;

namespace Theminds {
   static class LogBoxFilters {
      static Bowel.MircRegex mircRegex;
      static Bowel.ServerPrefixNumberRegex serverPrefixNumberRegex;
      static LogBoxFilters() {
         // TODO: remove CurrentChannel from Parts once tabs are in;
         mircRegex = new Bowel.MircRegex();
         serverPrefixNumberRegex = new Bowel.ServerPrefixNumberRegex();

         NewChannel += delegate { };
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

      // Format: |PRIVMSG #channel msg| or |:nick!ip PRIVMSG #channel :msg|
      static void privmsg(ref string line, ref string channel, ref Color color) {
         if (!line.Contains(" ")) return;
         bool fromSelf = line.StartsWith("PRIVMSG #");
         string[] tokens = line.Split(App.Space, 4);

         // Prevents matching something like |NOTICE PRIVMSG ...|
         bool fromOther = ("PRIVMSG" == tokens[1] && line.StartsWith(":"));
         if (!(fromOther || fromSelf)) return;

         channel = fromSelf ? tokens[1] : tokens[2];

         string nick, msg;
         if (fromSelf) {
            nick = connection.Info.nick;
            msg = line.Split(App.Space, 3)[2];
            color = Color.DarkRed;
         }
         else {
            nick = tokens[0].Substring(1, line.IndexOf('!') - 1);
            msg = tokens[3].Substring(1);
         }

         // ACTION uses a colon or not, depending on the source.
         string y = fromSelf ? ":\u0001ACTION" : "\u0001ACTION";
         if (!msg.StartsWith(y)) {
            line = String.Format("<{0}> {1}", nick, msg);
            return;
         }
         msg = tokens[2] + " " + tokens[3];
         line = String.Format("* {0} {1}", nick, identifyActions(msg));
         color = Color.Green;
      }

      // Not an event handler; a helper
      static string identifyActions(string line) {
         string msg = line.Trim().Split('\u0001')[1].Substring(7);
         return msg;
      }

      public delegate void NewChannelDel(string channel);
      public static event NewChannelDel NewChannel;
      static void selfJoin(ref string line, ref string channel, ref Color color) {
         string x = "JOIN ";
         if (false == line.StartsWith("JOIN ")) return;
         color = Color.Blue;

         // Format: |JOIN #channel,#channel,#channel|
         string[] channels = line.Substring(x.Length).Split(',');
         NewChannel(channels[channels.Length - 1]);

         //buffer.CurrentChannel = channels[channels.Length - 1];
         channel = channels[channels.Length - 1];
      }
   }
}