using System;
using System.Drawing;
using Aspirations;

namespace Theminds {
   static class JoinPartQuitFilter {
      static Quirk connection;
      static IBuffer buffer;
      static Ideas lion;
      public static void Init(Quirk c, IBuffer l) {
         buffer = l; connection = c;
         buffer.Line += new LineDel(OnLine);
         lion = App.Lion;
      }

      static void OnLine(ref BufferData dc) {
         // |LogBoxFilters.privmsg| eats this before me.
         // If it added a <, then I know this is not a JPQ.
         string line = dc.Line;
         if (line.StartsWith("<")) return;
         string template = deductTemplate(line);
         if (null == template) return;         
         dc.Color = Color.Gray;

         string[] tokens = line.Split(App.Space, 5);
         findNickAndIp(tokens[0]);

         dc.Channel = tokens[2];
         dc.Line = formLine(template, findReason(line));
         if ("join" == template) dc.Channel = dc.Channel.Substring(1);
         if ("quit" == template) dc.Channel = "";
      }

      static string nick, ip;
      static void findNickAndIp(string token) {
         string[] nickTokens = token.Split('!');
         nick = nickTokens[0].Substring(1);
         ip = nickTokens[1];
      }

      static string formLine(string template, string reason) {
         if ("join" == template) reason = "";
         if (nick == connection.Info.nick) {
            return String.Format(lion.Get(template, "you"),
               buffer.CurrentChannel, reason);
         }
         return String.Format(lion.Get(template, "others"),
             nick, ip, buffer.CurrentChannel, reason);
      }

      static string findReason(string line) {
         int omegaPos = line.IndexOf(':', 1);
         string argonaut = line.Substring(omegaPos + 1);
         //	No parentheses for empty part/quit messages.
         string reason = (omegaPos != -1) ? " (" + argonaut + ")" : "";
         return reason;
      }

      static string deductTemplate(string line) {
         return lion.TestContains(line,
             new string[] { "join", "part", "quit" });
      }
   }
}