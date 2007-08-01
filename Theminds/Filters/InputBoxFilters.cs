using System;
using System.Drawing;
using System.Diagnostics;
using Aspirations;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using S = System.String;

namespace Theminds.Filters {
   [DesiresAppControls]
   class InputBoxFilters {
      IAppControls app;
      public InputBoxFilters(IAppControls app) {
         this.app = app;
         InputBox inputBox = app.InputBox;
         Quirk connection = app.Connection;

         inputBox.StopPresses += (ref bool shouldStop) => {
            if (!connection.Started) shouldStop = true;
         };

         inputBox.Command += (cmd, msg) => {
            if ("me" != cmd) return;
            connection.Message("PRIVMSG {0} :\u0001ACTION {1}\u0001",
               app.CurrentChannel, msg);
         };

         inputBox.Message += new InputBox.MessageDel(PrivmsgCurrentChannel);
         inputBox.Command += (cmd, arg) => {
            switch (cmd) {
               // arg is a channel.
               case "j": connection.Message("JOIN " + arg); break;
               case "raw": connection.Message(arg); break;
               // arg is a quit message.
               case "q": connection.Dispose(arg); break;
            }
         };
         inputBox.Command += (cmd, arg) => {
            if (!cmd.StartsWith("/")) return;
            if (arg.Length != 0) arg = " " + arg;
            PrivmsgCurrentChannel(S.Format("{0}{1}", cmd, arg));
         };
      }

      void PrivmsgCurrentChannel(string msg) {
         if (app.CurrentChannel == null)
            app.LogBox.AddLine(App.Lion.Get("error.cannot.privmsg"), Color.Purple);
         else
            app.Connection.Message("PRIVMSG " + app.CurrentChannel + " " + msg);
      }
   }
}