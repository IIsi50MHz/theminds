using System;
using System.Drawing;
using System.Diagnostics;
using Aspirations;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

namespace Theminds.Filters {
   [DesiresAppControls]
   class InputBoxFilters {
      public static event MethodInvoker Who = delegate { };
      public InputBoxFilters(IAppControls form) {
         InputBox inputBox = form.InputBox;
         Quirk connection = form.Connection;

         inputBox.StopPresses += delegate(ref bool shouldStop) {
            if (!connection.Started) shouldStop = true;
         };

         // TODO: Split messages up according to MaxLength
         inputBox.Command += delegate(string cmd, string msg) {
            if ("me" != cmd) return;
            connection.Message("PRIVMSG {0} :\u0001ACTION {1}\u0001",
               form.CurrentChannel, msg);
         };

         inputBox.Message += delegate(string message) {
            if (form.CurrentChannel == null) return;
            connection.Message(
               "PRIVMSG " + form.CurrentChannel + " " + message);
         };

         inputBox.Command += delegate(string cmd, string arg) {
            switch (cmd) {
               // arg is a channel.
               case "j": connection.Message("JOIN " + arg); break;
               case "raw": connection.Message(arg); break;
               // arg is a quit message.
               case "q": connection.Dispose(arg); break;
               case "w": Who(); break;
            }
         };
      }
   }
}