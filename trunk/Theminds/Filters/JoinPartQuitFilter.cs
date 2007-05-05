using System;
using System.Drawing;
using Aspirations;
using S = System.String;
using Sx = Aspirations.StringEx;

namespace Theminds.Filters {
   [DesiresAppControls]
   class JoinPartQuitFilter {
      Quirk quirk; IAppControls app;
      Ideas lion = App.Lion; LineDel fforde;
      public JoinPartQuitFilter(IAppControls app) {
         this.app = app; quirk = app.Connection;
         app.Buffer.Line += new LineDel(filter);
         fforde = new LineDel(suppressNewTab);
      }

      // line ~ "|:nick!ip join :#chan|"
      // line ~ "|:nick!ip part #chan :msg|"
      // line ~ "|:nick!ip quit :msg|"
      // If the join message happens to be from self
      // e.g. ":Tongue!ip join :#channel", we enter a special mode.
      // This is NOT the same as handling SelfLine.
      string nick, ip, line, mode; int[] spaces;
      int reasonIndex = 0; bool fromMe = false;
      protected void filter(ref BufferData data) {
         line = data.Line;
         if (!line.Contains(" ")) return;
         if (!line.StartsWith(":")) return;
         spaces = Sx.FindSpaces(line, 3);
         
         findNickAndIp();
         if (null == nick) return;
         fromMe = (nick == quirk.Info.Nick);

         mode = Sx.Tween(line, spaces[0], spaces[1] - 1).ToLowerInvariant();
         switch (mode) {
            case "join":
               data.Channel = line.Substring(spaces[1] + 1); break;
            case "part":
               data.Channel = Sx.Tween(line, spaces[1], spaces[2] - 1);
               reasonIndex = spaces[2] + 1; break;
            case "quit":
               reasonIndex = spaces[1] + 1; break;
            default: return;
         }
         data.Color = Color.Gray;
         findMessage(ref data); findReason(ref data);
         if ("part" == mode && fromMe) {
            messageToSuppress = data.Line;
            app.Buffer.PostLine += fforde;
         }
      }

      // index: index of the start of the message for us to parse
      void findReason(ref BufferData data) {
         int index = reasonIndex;
         if (0 == index || line.Length <= index) return;
         string reason = lion.Get(mode, "reason");
         reason = S.Format(reason, line.Substring(index));
         data.Line = S.Format(lion.Get(mode, "total"),
            data.Line, reason);
      }

      void findMessage(ref BufferData data) {
         if (fromMe) {
            data.Line = S.Format(lion.Get(mode, "self"),
               data.Channel); return;
         }
         data.Line = S.Format(lion.Get(mode, "others"),
            nick, ip, data.Channel);
      }

      void findNickAndIp() {
         string user = Sx.Tween(line, 0, spaces[0] - 1);
         // Roots out junk like ":<nick> MODE +x"
         if (!user.Contains("!")) return;
         nick = Sx.Tween(user, 1, user.IndexOf('!'));
         ip = user.Substring(user.IndexOf('!') + 1);
      }

      string messageToSuppress;
      void suppressNewTab(ref BufferData data) {
         if (messageToSuppress != data.Line) return;
         data.NeedsNewTab = false;
         app.Buffer.PostLine -= fforde;
      }
   }
}