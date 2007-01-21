using System;
using System.Drawing;
using System.Windows.Forms;
using Aspirations;

namespace Theminds.Filters {
   [DesiresAppControls]
   class WhoFilter {
      IAppControls app;
      public WhoFilter(IAppControls parent) {
         InputBoxFilters.Who += new MethodInvoker(start);
         this.app = parent;
      }

      void start() {
         if (app.CurrentChannel == null) return;

         app.Buffer.Line += new LineDel(filter);
         app.Connection.Message("WHO " + app.CurrentChannel);
      }

      // |userList| flickers if you just do a Clear() and lots of Add()s.
      void filter(ref BufferData dc) {
         string[] tokens = dc.Line.Split(' ');
         dc.Color = Color.Blue;
         dc.Channel = tokens[1];

         if (dc.Line.Contains("End of /WHO")) stop();
         else {
            //LogBoxFilters.ServerPrefix(ref dc);
            app.UserList.Push(tokens[5]);
         }
      }

      void stop() {
         if (app.InvokeRequired)
            app.BeginInvoke(new MethodInvoker(stop));
         else {
            app.UserList.Flush();
            app.Buffer.Line -= new LineDel(filter);
         }
      }
   }
}