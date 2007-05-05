// Automatically seeds UserList with members of a channel
// upon joins via the /NAMES message (see RPL_NAMEREPLY
// and RPL_ENDOFNAMES in the IRC RFC).

using System;
using S = System.String;
using Sx = Aspirations.StringEx;

namespace Theminds.Filters {
   [DesiresAppControls]
   class NamesFilter {
      LineDel del; string channel; IAppControls app;
      public NamesFilter(IAppControls app) {
         this.app = app;
         del = new LineDel(filter);
         app.Buffer.NewChannel += delegate(string channel) {
            this.channel = channel;
            app.Buffer.Line += del;
         };
      }

      // line ~ "[server] = <channel> :[[@|+]<nick> [[@|+]<nick> [...]]]"
      // line ~ "[server] <channel> :End of /NAMES list"
      readonly string serverPrefix = App.Lion.Get("server.prefix");
      protected void filter(ref BufferData data) {
         //System.Diagnostics.Debug.WriteLine("Stop calling me");
         if (S.Format("{0} {1} :End of /NAMES list.",
            serverPrefix, channel) == data.Line) {
            data.Ignore = true;
            app.Buffer.Line -= del;
            app.UserList.Flush();
            return;
         }

         string test = S.Format("{0} = {1} :", serverPrefix, channel);
         if (!data.Line.StartsWith(test)) return;
         
         int[] spaces = Sx.FindSpaces(data.Line, 4);
         // Remember the colon! Remember the weird tacked space!
         string[] nicks = data.Line.Substring(spaces[2] + 1).Trim().Split(' ');
         foreach (string nick in nicks) app.UserList.Push(nick);
         data.Ignore = true;
      }
   }
}