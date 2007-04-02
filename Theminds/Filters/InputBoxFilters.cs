using System;
using System.Drawing;
using System.Diagnostics;
using Aspirations;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using S = System.String;

namespace Theminds.Filters {
   [DesiresAppControls]
   class InputBoxFilters {
      public static event MethodInvoker Who = delegate { };
      IAppControls app;
      public InputBoxFilters(IAppControls app) {
         this.app = app;
         InputBox inputBox = app.InputBox;
         Quirk connection = app.Connection;

         inputBox.StopPresses += delegate(ref bool shouldStop) {
            if (!connection.Started) shouldStop = true;
         };

         inputBox.Command += delegate(string cmd, string msg) {
            if ("me" != cmd) return;
            connection.Message("PRIVMSG {0} :\u0001ACTION {1}\u0001",
               app.CurrentChannel, msg);
         };

         inputBox.Message += new InputBox.MessageDel(PrivmsgCurrentChannel);
         inputBox.Command += delegate(string cmd, string arg) {
            switch (cmd) {
               // arg is a channel.
               case "j": connection.Message("JOIN " + arg); break;
               case "raw": connection.Message(arg); break;
               // arg is a quit message.
               case "q": connection.Dispose(arg); break;
               case "w": Who(); break;
               default: PrivmsgCurrentChannel(
                  S.Format("{0} {1}", cmd, arg)); break;
            }
         };
      }

      void PrivmsgCurrentChannel(string msg) {
         if (app.CurrentChannel == null) return;
         app.Connection.Message(
            "PRIVMSG " + app.CurrentChannel + " " + msg);
      }
   }
}