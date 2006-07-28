using System.Windows.Forms;

[assembly: System.Reflection.AssemblyVersionAttribute("0.1")]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Security.Permissions.SecurityPermission(
  System.Security.Permissions.SecurityAction.RequestMinimum, Execution = true)]
[assembly: System.CLSCompliant(true)]
namespace Theminds {
	sealed partial class Page : Form {
		[System.STAThread]
		static void Main() {
			// http://www.codeproject.com/dotnet/unhandledexceptions.asp
			/*Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e) {
				MessageBox.Show("Error!");
			};*/
			Application.Run(new Page());
		}
	}
}