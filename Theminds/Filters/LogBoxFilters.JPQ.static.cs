using System;
using System.Drawing;
using Aspirations;

namespace Theminds{
   static class JoinPartQuitFilter {
      static Quirk connection;
      static IBuffer buffer;
      static Ideas lion;
      public static void Init(Quirk c, IBuffer l) {
         buffer = l; connection = c;
         buffer.Line += new LineDel(filter);
         lion = Page.Lion;
      }

      // NewSelfFilter does not call this. Self-parts are only filtered
      // through the server's response to the PART command.
      static void filter(ref string line, ref string channel, ref Color color) {
         // |LogBoxFilters.privmsg| eats this before me.
         // If it added a <, then I know this is not a JPQ.
         if (line.StartsWith("<")) return;

         string template = deductTemplate(line);
         if (null == template) return;
         
         // Now and only now can I assign color.
         color = Color.Gray;

         // FIND NICK. Format: |:nick!IPcrud ...|
         string[] tokens = line.Split(Page.Space, 5);
         string[] nickTokens = tokens[0].Split('!');
         string nick = nickTokens[0].Substring(1);
         string ipcrud = nickTokens[1];

         // FIND MESSAGE.
         int omegaPos = line.IndexOf(':', 1);
         string argonaut = line.Substring(omegaPos + 1);
         //	No parentheses by default so I can handle empty part/quit messages.
         string omega = (omegaPos != -1) ? " (" + argonaut + ")" : "";

         // ASSIGN CHANNEL.
         if ("join" == template) channel = argonaut;
         else if ("part" == template) channel = tokens[2];
         //	Quit's channel already assigned as "<>"

         // PUT INTO TEMPLATE.
         if (nick != connection.Info.nick) {
            line = String.Format(lion.Get(template, "others"), 
               nick, ipcrud, buffer.CurrentChannel, omega);
            return;
         }

         line = String.Format(lion.Get(template, "you"),
            buffer.CurrentChannel, omega);
      }

      static string deductTemplate(string line) {
         string template = null;
         if (lion.TestContains(line, "join")) template = "join";
         else if (lion.TestContains(line, "part")) template = "part";
         else if (lion.TestContains(line, "quit")) {
            template = "quit";
         }
         return template;
      }
   }
}