using System;
using System.Drawing;
using Aspirations;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using S = System.String;

namespace Theminds.Filters {
   [DesiresAppControls]
   class PrivmsgFilter {
      IAppControls app;
      Quirk quirk; Ideas lion = App.Lion;
      string speechAll, actionAll;

      public PrivmsgFilter(IAppControls app) {
         this.app = app;
         this.quirk = app.Connection;
         
         Buffer buffer = app.Buffer;
         buffer.Line += new LineDel(filter);
         buffer.SelfLine += new LineDel(filter);

         speechAll = lion.Get("speech.all");
         actionAll = lion.Get("action.all");
      }

      // line ~ "PRIVMSG #channel msg" or ":nick!ip PRIVMSG #channel :msg"
      BufferData dc;
      string thisLock = "ants";
      protected void filter(ref BufferData bufferData) {
         lock (thisLock) {
            dc = bufferData;
            string line = dc.Line;

            // Check to see if I can Split at all.
            if (!line.Contains(" ")) return;
            string[] tokens = line.Split(App.Space, 4);
            if (!isValid(line, tokens)) return;

            dc.Channel = fromSelf ? tokens[1] : tokens[2];
            if (dc.Channel == quirk.Info.Nick)
               newIntimateFriend(line, tokens);
            else
               newPrivmsg(line, tokens);
            bufferData = dc;
         }
      }

      void newPrivmsg(string line, string[] tokens) {
         string nick, msg;
         if (fromSelf) {
            nick = quirk.Info.Nick;
            msg = line.Split(App.Space, 3)[2];
         }
         else {
            nick = nickFinder(tokens[0]);
            msg = msgFinder(tokens[3]);
         }

         // ACTION uses a colon or not, depending on the source.
         string y = fromSelf ? ":\u0001ACTION" : "\u0001ACTION";
         if (!msg.StartsWith(y))
            dc.Line = S.Format(speechAll, nick, msg);
         else {
            msg = tokens[2] + " " + tokens[3];
            dc.Line = S.Format(actionAll, nick,
               identifyActions(msg));
            dc.Color = Color.Green;
         }
      }

      bool fromSelf;
      bool isValid(string line, string[] tokens) {
         fromSelf = line.StartsWith("PRIVMSG ");
         // Prevents matching something like "NOTICE PRIVMSG ..."
         bool fromOther = ("PRIVMSG" == tokens[1] && line.StartsWith(":"));
         if (fromOther || fromSelf) return true;
         else return false;
      }

      // Not an event handler; a helper
      string identifyActions(string line) {
         return line.Trim().Split('\u0001')[1].Substring(7);
      }

      void newIntimateFriend(string line, string[] tokens) {
         // |line| ~ "PRIVMSG <nick> <msg>"
         // Edge case: what if I'm talking to myself?
         if (line.StartsWith("PRIVMSG ")) {
            dc.Channel = app.Connection.Info.Nick;
            string msg = line.Substring(line.IndexOf(' ', 8 + dc.Channel.Length) + 1);
            dc.Line = S.Format(speechAll, dc.Channel, msg);
            return;
         }
         else {
            string nick = nickFinder(tokens[0]);
            string msg = msgFinder(tokens[3]);
            dc.Channel = nick;
            dc.Line = S.Format(speechAll, nick, msg);
         }
      }

      string nickFinder(string token) {
         return token.Substring(1, token.IndexOf('!') - 1);
      }
      string msgFinder(string token) {
         return token.Substring(1);
      }
   }
}