using System;
using System.Drawing;
using Aspirations;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

namespace Theminds.Filters {
   [DesiresAppControls]
   class PrivmsgFilter {
      IAppControls app;
      Quirk quirk;
      Buffer buffer;
      public PrivmsgFilter(IAppControls app) {
         this.app = app;
         this.quirk = app.Connection;
         this.buffer = app.Buffer;

         buffer.Line += new LineDel(filter);
         buffer.SelfLine += new LineDel(filter);
      }

      // Format: |PRIVMSG #channel msg| or |:nick!ip PRIVMSG #channel :msg|
      BufferData dc;
      string thisLock = "ants";
      void filter(ref BufferData bufferData) {
         lock (thisLock) {
            dc = bufferData;
            string line = dc.Line;

            // Check to see if I can Split at all.
            if (!line.Contains(" ")) return;
            string[] tokens = line.Split(App.Space, 4);
            if (!isValid(line, tokens)) return;

            dc.Channel = fromSelf ? tokens[1] : tokens[2];
            if (dc.Channel == quirk.Info.nick)
               newIntimateFriend(line, tokens);
            else
               newPrivmsg(line, tokens);
            bufferData = dc;
         }
      }

      void newPrivmsg(string line, string[] tokens) {
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
         if (!msg.StartsWith(y))
            dc.Line = String.Format("<{0}> {1}", nick, msg);
         else {
            msg = tokens[2] + " " + tokens[3];
            dc.Line = String.Format("* {0} {1}", nick,
               identifyActions(msg));
            dc.Color = Color.Green;
         }
      }

      bool fromSelf;
      bool isValid(string line, string[] tokens) {
         fromSelf = line.StartsWith("PRIVMSG #");
         // Prevents matching something like |NOTICE PRIVMSG ...|
         bool fromOther = ("PRIVMSG" == tokens[1] && line.StartsWith(":"));
         if (fromOther || fromSelf) return true;
         else return false;
      }

      // Not an event handler; a helper
      string identifyActions(string line) {
         return line.Trim().Split('\u0001')[1].Substring(7);
      }

      void newIntimateFriend(string line, string[] tokens) {
         string nick = tokens[0].Substring(1).Substring(0,
            tokens[0].IndexOf('!') - 1);
         string msg = tokens[3].Substring(1);
         app.Invoke((MethodInvoker)delegate {
            buffer.AddChannel(nick);
         });
         dc.Channel = nick;
         dc.Line = string.Format("<{0}> {1}", nick, msg);
      }
   }
}