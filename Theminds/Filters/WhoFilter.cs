using System;
using System.Drawing;
using System.Windows.Forms;
using Sx = Aspirations.StringEx;

namespace Theminds.Filters {
   [DesiresAppControls]
   class WhoFilter {
      IAppControls app;
      LineDel filterDel;
      public WhoFilter(IAppControls app) {
         app.InputBox.Command += delegate(string cmd, string arg) {
            if ("w" != cmd) return;
            if (!Sx.IsChannel(app.CurrentChannel)) return;
            app.Buffer.Line += filterDel;
            app.Connection.Message("WHO " + app.CurrentChannel);
         };
         app.Buffer.SelfLine += new LineDel(whoMessageFilter);
         filterDel = new LineDel(filter);
         this.app = app;
      }

      void whoMessageFilter(ref BufferData data) {
         if (!data.Line.StartsWith("WHO ")) return;
         data.Channel = app.CurrentChannel;
      }

      // `userList` flickers with naive Clear&Add()s.
      // [0:server] [1:channel] [2:user] [3:host] [4:server] [5:nick]...
      readonly string serverPrefix = App.Lion.Get("server.prefix");
      void filter(ref BufferData data) {
         string[] tokens = data.Line.Split(App.Space, 7);
         if (serverPrefix != tokens[0]) return;
         if (!Sx.IsChannel(tokens[1])) return;

         data.Channel = tokens[1];
         data.Color = Color.DarkBlue;
         if (data.Line.Contains("End of /WHO"))
            app.Invoke(new MethodInvoker(stop));
         else
            app.UserList.Push(tokens[5]);
      }

      void stop() {
         app.UserList.Flush();
         app.Buffer.Line -= filterDel;
      }
   }
}