using System;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Aspirations;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using S = System.String;
using Bowel;

namespace Theminds.Filters {
   [DesiresAppControls]
   class LogBoxFilters {
      MircRegex mircRegex = new MircRegex();
      ServerPrefixNumberRegex serverPrefixNumberRegex
         = new ServerPrefixNumberRegex();

      Quirk quirk;
      Buffer buffer;
      public LogBoxFilters(IAppControls app) {
         buffer = app.Buffer; quirk = app.Connection;

         buffer.PreLine += new LineDel(hostName);
         buffer.PreLine += new LineDel(serverPrefix);
         
         buffer.Line += new LineDel(ping);
         buffer.SelfLine += new LineDel(selfJoin);
         buffer.SelfLine += delegate(ref BufferData data) {
            data.Color = Color.DarkRed;
         };
         buffer.Line += delegate(ref BufferData dc) {
            // Strip mIRC colors.
            string line = dc.Line;
            if (line.Contains("\u0003")) line = mircRegex.Replace(line, "");
         };
      }

      string serverTruck = App.Lion.Get("all.server");
      void serverPrefix(ref BufferData dc) {
         string line = dc.Line;
         string x = ":" + quirk.Info.HostName;
         if (false == line.StartsWith(x)) return;

         // Format: <server name> <command number> <optional. nick>
         line = line.Substring(x.Length + 1);
         line = serverPrefixNumberRegex.Replace(line, "");
         if (line.StartsWith(quirk.Info.Nick)) {
            line = line.Substring(quirk.Info.Nick.Length + 1);
         }
         line = S.Format(serverTruck, line);
         dc.Line = line;
      }

      void hostName(ref BufferData dc) {
         string line = dc.Line;
         if (line.StartsWith(":") == false) return;
         quirk.Info.HostName = line.Substring(1, line.IndexOf(' ') - 1);
         buffer.PreLine -= new LineDel(hostName);
      }

      void ping(ref BufferData dc) {
         if (false == dc.Line.StartsWith("PING :")) return;

         pingMessage = dc.Line;
         buffer.PostLine += new LineDel(sendPong);
         dc.Color = Color.Blue;
      }

      string pingMessage;
      void sendPong(ref BufferData data) {
         buffer.Line += new LineDel(colorPong);
         // Remove before Message or else recursion
         buffer.PostLine -= new LineDel(sendPong);

         // PING : is six letters
         quirk.Message("PONG :" + pingMessage.Substring(6));
      }

      void colorPong(ref BufferData dc) {
         if (false == dc.Line.StartsWith("PONG :")) return;
         dc.Color = Color.Blue;
         buffer.Line -= new LineDel(colorPong);
      }

      void selfJoin(ref BufferData dc) {
         string x = "JOIN "; string line = dc.Line;
         if (false == line.StartsWith(x)) return;
         dc.Color = Color.Blue;

         // Format: |JOIN #channel,#channel,#channel|
         string[] channels = line.Substring(x.Length).Split(',');
         dc.Channel = channels[channels.Length - 1];
      }
   }
}