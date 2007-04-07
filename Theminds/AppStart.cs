//#define ENABLE_TEST

using System.Diagnostics;
using System.Windows.Forms;
using Theminds.Tests;

[assembly: System.Reflection.AssemblyVersionAttribute("1.3")]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Security.Permissions.SecurityPermission(
  System.Security.Permissions.SecurityAction.RequestMinimum, Execution = true)]
[assembly: System.CLSCompliant(true)]
namespace Theminds {
   static class AppStart {
      [System.STAThread]
      static void Main() {
#if ENABLE_TEST
         Debug.Indent();
         App.LoadAttributeLovers(typeof(DesiresTestingAttribute));
         App.LoadAttributeLovers(typeof(DesiresTestingWithMockAppAttribute), new MockApp());
#else
         Application.EnableVisualStyles();
         Application.Run(new App());
#endif
      }
   }
}