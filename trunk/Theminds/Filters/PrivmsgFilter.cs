using System;
using System.Drawing;
using Aspirations;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using S = System.String;

namespace Theminds.Filters {
   [DesiresAppControls]
   class PrivmsgFilter {
      IAppControls app; Quirk quirk;
      string speechAll, actionAll;
      public PrivmsgFilter(IAppControls app) {
         this.app = app;
         this.quirk = app.Connection;

         app.Buffer.Line += new LineDel(filter);
         app.Buffer.SelfLine += new LineDel(filter);

         speechAll = App.Lion.Get("speech.all");
         actionAll = App.Lion.Get("action.all");
      }

      // There are two separate functions. The code may seem
      // the same, but the differences are subtle and prolific
      // enough to preclude condensation.
      protected void filter(ref BufferData data) {
         string line = data.Line;
         if (!line.Contains(" ")) return;

         seedSpaces(line);
         if (line.StartsWith("PRIVMSG "))
            filterSelf(ref data);
         else if (line.StartsWith(":")
            && line.Substring(spaces[0]).StartsWith("PRIVMSG "))
            filterOthers(ref data);
         return;
      }

      // line ~ "PRIVMSG #channel msg"
      // line ~ "PRIVMSG #channel :\u0001ACTION <msg>\u0001"
      void filterSelf(ref BufferData data) {
         string line = data.Line;
         data.Channel = StringExcerpt(line, spaces[0], spaces[1] - 1);

         string nick = quirk.Info.Nick;
         string msg = line.Substring(spaces[1]);
         // Notice the colon! Weird protocol.
         if (msg.StartsWith(":\u0001ACTION")) {
            msg = StringExcerpt(line, spaces[2], line.Length - 1);
            data.Line = S.Format(actionAll, nick, msg);
            data.Color = Color.Green;
            return;
         }
         data.Line = S.Format(speechAll, nick, msg);
      }

      // line ~ ":nick!ip PRIVMSG #channel :msg"
      // line ~ ":nick!ip PRIVMSG #channel :\u0001ACTION <msg>\u0001"
      void filterOthers(ref BufferData data) {
         string line = data.Line;
         data.Channel = StringExcerpt(line, spaces[1], spaces[2] - 1);

         string nick = StringExcerpt(line, 1, line.IndexOf('!'));
         string msg = line.Substring(spaces[2] + 1);
         if (msg.StartsWith("\u0001ACTION")) {
            msg = StringExcerpt(line, spaces[3], line.Length - 1);
            data.Line = S.Format(actionAll, nick, msg);
            data.Color = Color.Green;
            return;
         }
         data.Line = S.Format(speechAll, nick, msg);
      }

      // `last` is one bigger than the actual indexOf
      // because I'm going to use it for Substring.
      int[] spaces = new int[4];
      void seedSpaces(string line) {
         int last = 0;
         for (int i = 0; i <= 3; ++i) {
            if (line.Length < last) return;
            last = line.IndexOf(' ', last) + 1;
            spaces[i] = last;
         }
      }

      // "0123456789", 3, 7 => "3456" (`end`th character removed)
      public static string
         StringExcerpt(string haystack, int begin, int end) {
         return haystack.Remove(end).Substring(begin);
      }
   }
}