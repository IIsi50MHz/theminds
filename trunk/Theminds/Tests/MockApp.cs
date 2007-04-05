using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using Aspirations;

namespace Theminds.Tests {
   public sealed partial class MockApp : IAppControls, ITabsParent {
      string currentChannel;
      public string CurrentChannel {
         get { return currentChannel; }
         set { currentChannel = value; }
      }

      Buffer buffer; Quirk quirk;
      Tabber tabber;
      public MockApp() {
         QuirkStart mozNet = new QuirkStart();
         mozNet.Nick = "Tongue"; mozNet.Port = 6667;
         mozNet.Server = "irc.mozilla.org";
         mozNet.User = "USER cryptoliter2 8 * :Hi";
         quirk = new Quirk(mozNet);

         this.tabber = new Tabber(this, "(new)");
         tabber.NewTab += delegate {
         };
         tabber.Init();
         this.buffer = new Buffer(this);
      }

      public LogBox LogBox {
         get { return new LogBox(); }
      }

      public Tabber Tabber {
         get { return tabber; }
      }

      public Quirk Connection {
         get { return quirk; }
      }

      public Buffer Buffer {
         get { return buffer; }
      }

      public UserList UserList {
         get { throw new Exception("The method or operation is not implemented."); }
      }

      public Aspirations.InputBox InputBox {
         get { throw new Exception("The method or operation is not implemented."); }
      }

      public bool InvokeRequired {
         get { return true; }
      }

      public IAsyncResult BeginInvoke(Delegate d, params object[] args) {
         throw new Exception("The method or operation is not implemented.");
      }

      public object Invoke(Delegate d, params object[] args) {
         throw new Exception("The method or operation is not implemented.");
      }

      public void SwitchLogBox(Control c) {
         throw new Exception("The method or operation is not implemented.");
      }

      public void AddTab(Control button) {
         throw new Exception("The method or operation is not implemented.");
      }

      public void RemoveTab(Control button) {
         throw new Exception("The method or operation is not implemented.");
      }

      public ITab CreateTab(string label) {
         return new Tab(delegate {}, "(new)");
      }

      public int TabsWidth {
         get { return 5; }
      }

      public void SuspendLayout() {
         throw new Exception("The method or operation is not implemented.");
      }

      public void ResumeLayout() {
         throw new Exception("The method or operation is not implemented.");
      }

      public void GrabFocus() {}
   }
}