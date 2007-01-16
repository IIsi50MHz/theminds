using System;
using System.Drawing;
using System.Windows.Forms;
using Aspirations;

namespace Theminds {
   public interface IAppControls {
      LogBox LogBox { get; }
      Tabber Tabber { get; }
      Quirk Connection { get;}
      Buffer Buffer { get;}
      UserList UserList { get;}
      InputBox InputBox { get;}
      string CurrentChannel { get; set;}
      bool InvokeRequired { get;}

      IAsyncResult BeginInvoke(Delegate d, params object[] args);
      void SwitchLogBox(Control c);
   }

   public partial class App : Form, IAppControls {
      public LogBox LogBox {
         get { return this.logBox; }
      }

      public Tabber Tabber {
         get { return this.tabber; }
      }

      public Quirk Connection {
         get { return this.connection; }
      }

      public UserList UserList {
         get { return this.userList; }
      }

      public InputBox InputBox {
         get { return this.inputBox; }
      }

      public Buffer Buffer {
         get { return buffer; }
      }

      public void SwitchLogBox(Control c) {
         if (c == logBoxPanel.Controls[0]) return;
         this.SuspendLayout();
         logBoxPanel.Controls.RemoveAt(0);
         logBoxPanel.Controls.Add(c);
         this.ResumeLayout();
      }
   }
}