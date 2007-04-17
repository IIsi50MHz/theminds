using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using Aspirations;

namespace Theminds {
   public sealed partial class App : Form {
      public static char[] Space = new char[] { ' ' };

      string currentChannel;
      public string CurrentChannel {
         get { return currentChannel; }
         set { currentChannel = value; }
      }

      public static Ideas Lion = new Ideas(@"lion.txt");

      Buffer buffer;
      Quirk quirk;
      public App() {
         this.SetUpForm(); // MainForm.SetUpForm.cs

         QuirkStart mozNet = new QuirkStart();
         mozNet.Nick = "Tongue"; mozNet.Port = 6667;
         mozNet.Server = "irc.mozilla.org";
         mozNet.User = "USER cryptoliter2 8 * :Hi";
         quirk = new Quirk(mozNet);

         this.buffer = new Buffer(this);
         quirk.NewLine += new Quirk.NewLineDel(Buffer.AddLine);
         App.LoadAttributeLovers(
            typeof(DesiresAppControlsAttribute), this);

         quirk.Start();
      }


      /**** Event handlers ****/
      protected override void OnClosing(
         System.ComponentModel.CancelEventArgs e) {
         quirk.Dispose();
         base.OnClosing(e);
      }

      protected override void OnKeyDown(KeyEventArgs e) {
         if (false == e.Control) goto noControl;
         e.SuppressKeyPress = true;
         switch (e.KeyCode) {
            case Keys.PageUp: tabber.MoveToPrev(); break;
            case Keys.PageDown: tabber.MoveToNext(); break;
            case Keys.T: buffer.AddChannel(); break;
            case Keys.W: buffer.Remove(tabber.Current); break;
            case Keys.Q: this.Close(); break;
            default: e.SuppressKeyPress = false; break;
         }

      noControl:
         base.OnKeyDown(e);
      }

      protected override void OnMouseWheel(MouseEventArgs e) {
         logBox.Select();
         base.OnMouseWheel(e);
      }

      /**** Static members ****/
      public static void Alert(object alert) {
         MessageBox.Show(alert.ToString());
      }

      public static void LoadAttributeLovers(Type attribute, 
         params object[] args) {
         Type[] types = Assembly.GetExecutingAssembly().GetTypes();
         try {
            foreach (Type type in types) {
               object[] oneNightStands =
                  type.GetCustomAttributes(attribute, false);
               if (oneNightStands.Length < 1) continue;
               Activator.CreateInstance(type, args);
            }
         }
         catch (TargetInvocationException e) {
            Exception x = e.InnerException;
            throw new InvalidOperationException(x.Message + x.StackTrace);
         }
      }
   }
}