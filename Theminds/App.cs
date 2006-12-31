//#define ENABLE_TEST

using System.Diagnostics;
using System.Windows.Forms;
using ThemindsTests;

[assembly: System.Reflection.AssemblyVersionAttribute("1.1")]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Security.Permissions.SecurityPermission(
  System.Security.Permissions.SecurityAction.RequestMinimum, Execution = true)]
[assembly: System.CLSCompliant(true)]
namespace Theminds {
   static class App {
      [System.STAThread]
      static void Main() {
         // http://www.codeproject.com/dotnet/unhandledexceptions.asp
         /*Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e) {
            MessageBox.Show("Error!");
         };*/
#if ENABLE_TEST
         Test();
#else
         Application.EnableVisualStyles();
         Application.Run(new Page());
#endif
      }

      static void Test() {
         Debug.Indent();
         new ThemindsTests.InputBox();
      }
   }
}