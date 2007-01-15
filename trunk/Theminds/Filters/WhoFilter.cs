using System;
using System.Drawing;
using System.Windows.Forms;
using Aspirations;

namespace Theminds {
   static class WhoFilter {
      static IAppControls parent;
      public static void Init(IAppControls parent) {
         InputBoxFilters.Who += new MethodInvoker(start);
         WhoFilter.parent = parent;
      }

      static void start() {
         if (parent.CurrentChannel == null) return;

         parent.Buffer.Line += new LineDel(filter);
         parent.Connection.Message("WHO " + parent.CurrentChannel);
      }

      // |userList| flickers if you just do a Clear() and lots of Add()s.
      static void filter(ref string line, ref string channel, ref Color color) {
         string[] tokens = line.Split(' ');
         color = Color.Blue;
         if (line.Contains("End of /WHO")) {
            channel = tokens[1];
            stop(); return;
         }

         LogBoxFilters.ServerPrefix(ref line, ref channel, ref color);

         channel = tokens[1];
         parent.UserList.Push(tokens[5]);
      }

      static void stop() {
         if (parent.InvokeRequired) {
            parent.BeginInvoke(new MethodInvoker(stop));
            return;
         }
         parent.UserList.Flush();
         parent.Buffer.Line -= new LineDel(filter);
      }
   }
}