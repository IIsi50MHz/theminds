using System;
using System.Drawing;
using Aspirations;
using S = System.String;
using Sx = Theminds.StringEx;

namespace Theminds.Filters {
   [DesiresAppControls]
   class JoinPartQuitFilter {
      Quirk quirk; IAppControls app;
      Ideas lion = App.Lion;
      public JoinPartQuitFilter(IAppControls app) {
         this.app = app; quirk = app.Connection;
         app.Buffer.Line += new LineDel(filter);
      }

      // line ~ "|:nick!ip join :#chan|"
      // line ~ "|:nick!ip part #chan :msg|"
      // line ~ "|:nick!ip quit :msg|"
      // If the join message happens to be from self
      // e.g. ":Tongue!ip join :#channel", we enter a special mode.
      // This is NOT the same as handling SelfLine.
      string nick, ip, line; int[] spaces;
      protected void filter(ref BufferData data) {
         line = data.Line;
         if (!line.Contains(" ")) return;
         if (!line.StartsWith(":")) return;
         spaces = Sx.FindSpaces(line, 3);
         
         findNickAndIp(out nick, out ip);
         // Roots out e.g. ":Tongue MODE +x"
         if ("" == nick) return;
         bool isSelf = (nick == quirk.Info.Nick);

         string mode = Sx.Tween(line, spaces[0], spaces[1] - 1).ToLowerInvariant();
         int reasonIndex = 0;
         switch (mode) {
            case "join":
               data.Channel = line.Substring(spaces[1] + 1);
               data.Color = Color.Gray;
               break;
            case "part":
               data.Channel = Sx.Tween(line, spaces[1], spaces[2] - 1);
               reasonIndex = spaces[2] + 1;
               data.Color = Color.Gray;
               break;
            case "quit":
               reasonIndex = spaces[1] + 1;
               data.Color = Color.Gray;
               break;
            default: return;
         }
         findMessage(ref data, isSelf, mode);
         findReason(ref data, mode, reasonIndex);
      }

      // index: index of the start of the message for us to parse
      void findReason(ref BufferData data, string mode, int index) {
         if (0 == index || line.Length <= index) return;
         string reason = lion.Get(mode, "reason");
         reason = S.Format(reason, line.Substring(index));
         data.Line = S.Format(lion.Get(mode, "total"),
            data.Line, reason);
      }

      void findMessage(ref BufferData data, bool isSelf, string mode) {
         if (isSelf) {
            data.Line = S.Format(lion.Get(mode, "self"),
               data.Channel);
            return;
         }
         data.Line = S.Format(lion.Get(mode, "others"),
            nick, ip, data.Channel);
      }

      void findNickAndIp(out string nick, out string ip) {
         string user = Sx.Tween(line, 0, spaces[0] - 1);
         if (!user.Contains("!")) { nick = ip = ""; return; }
         nick = Sx.Tween(user, 1, user.IndexOf('!'));
         ip = user.Substring(user.IndexOf('!') + 1);
      }
   }
}