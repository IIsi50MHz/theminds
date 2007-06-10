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

         // Remember the colon! Rememebr the weird tacked space!
         int[] spaces = Sx.FindSpaces(data.Line, 4);
         data.Channel = Sx.Tween(data.Line, spaces[1], spaces[2] - 1);
         string[] nicks = data.Line.Substring(spaces[2] + 1).Trim().Split(' ');
         Users.Instance.Clear(data);
         foreach (string nick in nicks) {
            Users.Instance[data] = nick;
            app.UserList.Push(nick);
         }
         data.Ignore = true;
      }
   }
}