using System;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Aspirations;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

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

      static Quirk quirk;
      static IBuffer buffer;
      static IAppControls app;
      public static void Init(IAppControls app) {
         buffer = app.Buffer; quirk = app.Connection;
         LogBoxFilters.app = app;

         buffer.Line += new LineDel(hostName);
         buffer.Line += new LineDel(ServerPrefix);

         buffer.Line += delegate(ref BufferData dc) {
            // Strip mIRC colors.
            string line = dc.Line;
            if (line.Contains("\u0003")) line = mircRegex.Replace(line, "");
         };

         buffer.Line += new LineDel(ping);
         buffer.Line += new LineDel(privmsg);

         buffer.SelfLine += new LineDel(privmsg);
         buffer.SelfLine += new LineDel(selfJoin);
      }

      public static void ServerPrefix(ref BufferData dc) {
         string line = dc.Line;
         string x = ":" + quirk.Info.hostName;

         if (false == line.StartsWith(x)) return;

         // Format: <server name> <command number> <optional. nick>
         line = line.Substring(x.Length + 1);
         line = serverPrefixNumberRegex.Replace(line, "");
         if (line.StartsWith(quirk.Info.nick)) {
            line = line.Substring(quirk.Info.nick.Length + 1);
         }
         line = "[server] " + line;
         dc.Line = line;
      }

      static void hostName(ref BufferData dc) {
         string line = dc.Line;
         if (line.StartsWith(":") == false) return;
         quirk.Info.hostName = line.Substring(1, line.IndexOf(' ') - 1);
         buffer.Line -= new LineDel(hostName);
      }

      static void ping(ref BufferData dc) {
         if (false == dc.Line.StartsWith("PING :")) return;

         pingMessage = dc.Line;
         buffer.PostLine += new MethodInvoker(sendPong);
         dc.Color = Color.Blue;
      }

      static string pingMessage;
      static void sendPong() {
         buffer.Line += new LineDel(colorPong);
         // Remove before Message or else recursion
         buffer.PostLine -= new MethodInvoker(sendPong);

         // PING : is six letters
         quirk.Message("PONG :" + pingMessage.Substring(6));
      }

      static void colorPong(ref BufferData dc) {
         if (false == dc.Line.StartsWith("PONG :")) return;
         dc.Color = Color.Blue;
         buffer.Line -= new LineDel(colorPong);
      }

      // Format: |PRIVMSG #channel msg| or |:nick!ip PRIVMSG #channel :msg|
      static void privmsg(ref BufferData dc) {
         string line = dc.Line;
         if (!line.Contains(" ")) return;
         bool fromSelf = line.StartsWith("PRIVMSG #");
         string[] tokens = line.Split(App.Space, 4);

         // Prevents matching something like |NOTICE PRIVMSG ...|
         bool fromOther = ("PRIVMSG" == tokens[1] && line.StartsWith(":"));
         if (!(fromOther || fromSelf)) return;

         dc.Channel = fromSelf ? tokens[1] : tokens[2];
         if (dc.Channel == quirk.Info.nick) {
            newIntimateFriend(ref dc); return;
         }

         string nick, msg;
         if (fromSelf) {
            nick = quirk.Info.nick;
            msg = line.Split(App.Space, 3)[2];
            dc.Color = Color.DarkRed;
         }
         else {
            nick = tokens[0].Substring(1, line.IndexOf('!') - 1);
            msg = tokens[3].Substring(1);
         }

         // ACTION uses a colon or not, depending on the source.
         string y = fromSelf ? ":\u0001ACTION" : "\u0001ACTION";
         if (!msg.StartsWith(y)) {
            dc.Line = String.Format("<{0}> {1}", nick, msg);
            return;
         }
         msg = tokens[2] + " " + tokens[3];
         dc.Line = String.Format("* {0} {1}", nick, identifyActions(msg));
         dc.Color = Color.Green;
      }

      // Not an event handler; a helper
      static string identifyActions(string line) {
         string msg = line.Trim().Split('\u0001')[1].Substring(7);
         return msg;
      }

      public delegate void NewChannelDel(string channel);
      public static event NewChannelDel NewChannel;
      static void selfJoin(ref BufferData dc) {
         string x = "JOIN "; string line = dc.Line;
         if (false == line.StartsWith("JOIN ")) return;
         dc.Color = Color.Blue;

         // Format: |JOIN #channel,#channel,#channel|
         string[] channels = line.Substring(x.Length).Split(',');
         NewChannel(channels[channels.Length - 1]);

         dc.Channel = channels[channels.Length - 1];
      }

      static void newIntimateFriend(ref BufferData dc) {
         string line = dc.Line;
         string[] tokens = line.Split(App.Space, 4);
         string nick = tokens[0].Substring(1).Substring(0,
            tokens[0].IndexOf('!'));
         string msg = tokens[3].Substring(0);
         app.BeginInvoke((MethodInvoker)delegate {
            NewChannel(nick);
         });
         dc.Channel = nick;
         dc.Line = string.Format("<{0}> {1}", nick, msg);
      }
   }
}