// Automatically seeds UserList with members of a channel
// upon joins via the /NAMES message (see RPL_NAMEREPLY
// and RPL_ENDOFNAMES in the IRC RFC).

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using S = System.String;
using Sx = Aspirations.StringEx;

namespace Theminds.Filters {
   [DesiresAppControls]
   class NamesFilter {
      IAppControls app;
      public NamesFilter(IAppControls app) {
         this.app = app;
         app.Buffer.Line += new LineDel(filter);
         app.Buffer.Broadcast += delegate(ref List<TabKey> tabs) {
         };
      }

      // line ~ "[server] = <channel> :[[@|+]<nick> [[@|+]<nick> [...]]]"
      // line ~ "[server] <channel> :End of /NAMES list"
      readonly string serverPrefix = App.Lion.Get("server.prefix");
      protected void filter(ref BufferData data) {
         if (data.Line.EndsWith(":End of /NAMES list.")) {
            data.Ignore = true;
            app.UserList.Flush();
            return;
         }

         string test = S.Format("{0} = ", serverPrefix);
         if (!data.Line.StartsWith(test)) return;
         
         int[] spaces = Sx.FindSpaces(data.Line, 4);
         // Remember the colon! Remember the weird tacked space!
         string[] nicks = data.Line.Substring(spaces[2] + 1).Trim().Split(' ');
         foreach (string nick in nicks) {
            app.UserList.Push(nick);
         }
         data.Ignore = true;
      }
   }
}