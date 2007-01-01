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
   static class AppStart {
      [System.STAThread]
      static void Main() {
#if ENABLE_TEST
         Test();
#else
         Application.EnableVisualStyles();
         Application.Run(new App());
#endif
      }

      static void Test() {
         Debug.Indent();
         new ThemindsTests.InputBox();
      }
   }
}