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
      }

      static Quirk quirk;
      static Buffer buffer;
      public static void Init(IAppControls app) {
         buffer = app.Buffer; quirk = app.Connection;

         buffer.Line += new LineDel(hostName);
         buffer.Line += new LineDel(ServerPrefix);
         buffer.Line += new LineDel(ping);
         buffer.SelfLine += new LineDel(selfJoin);
         buffer.Line += delegate(ref BufferData dc) {
            // Strip mIRC colors.
            string line = dc.Line;
            if (line.Contains("\u0003")) line = mircRegex.Replace(line, "");
         };
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

      static void selfJoin(ref BufferData dc) {
         string x = "JOIN "; string line = dc.Line;
         if (false == line.StartsWith("JOIN ")) return;
         dc.Color = Color.Blue;

         // Format: |JOIN #channel,#channel,#channel|
         string[] channels = line.Substring(x.Length).Split(',');
         buffer.AddChannel(channels[channels.Length - 1]);

         dc.Channel = channels[channels.Length - 1];
      }
   }
}