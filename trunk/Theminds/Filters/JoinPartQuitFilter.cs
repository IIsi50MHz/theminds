using System;
using System.Drawing;
using Aspirations;

namespace Theminds.Filters {
   [DesiresAppControls]
   class JoinPartQuitFilter {
      Quirk quirk; IAppControls app;
      Ideas lion = App.Lion;
      public JoinPartQuitFilter(IAppControls app) {
         this.app = app; quirk = app.Connection;
         app.Buffer.Line += new LineDel(filter);
      }

      string serverTruckTest = App.Lion.Get("all.server.test");
      // line ~ "|:nick!ip join :#chan|"
      // line ~ "|:nick!ip part #chan :msg|"
      // line ~ "|:nick!ip quit :msg|"
      // I'm not looking at my own lines. BUT!
      // But if the join message happens to be mine
      // e.g. ":Tongue!ip join :#channel", I need to have a
      // special, special way of handling that.
      void filter(ref BufferData dc) {
         // |LogBoxFilters.privmsg| eats this before me.
         // If it added a <, then I know this is not a JPQ.
         string line = dc.Line;
         if (line.StartsWith(serverTruckTest)) return;
         if (line.StartsWith("<")) return;
         
         string template = deduceTemplate(line);
         if (null == template) return;         
         dc.Color = Color.Gray;

         string[] tokens = line.Split(App.Space, 5);
         findNickAndIp(tokens[0]);

         dc.Channel = tokens[2];
         dc.Line = formLine(template, findReason(line));
         if ("join" == template)
            dc.Channel = dc.Channel.Substring(1);
         if ("quit" == template) dc.Channel = "";
      }

      string nick, ip;
      void findNickAndIp(string token) {
         string[] nickTokens = token.Split('!');
         nick = nickTokens[0].Substring(1);
         ip = nickTokens[1];
      }

      string formLine(string template, string reason) {
         if ("join" == template) reason = "";
         if (nick == quirk.Info.Nick) {
            return String.Format(lion.Get(template, "you"),
               app.CurrentChannel, reason);
         }
         return String.Format(lion.Get(template, "others"),
             nick, ip, app.CurrentChannel, reason);
      }

      string findReason(string line) {
         int omegaPos = line.IndexOf(':', 1);
         string argonaut = line.Substring(omegaPos + 1);
         //	No parentheses for empty part/quit messages.
         return (omegaPos != -1) ? " (" + argonaut + ")" : "";
      }

      string deduceTemplate(string line) {
         return lion.TestContains(line,
             new string[] { "join", "part", "quit" });
      }
   }
}