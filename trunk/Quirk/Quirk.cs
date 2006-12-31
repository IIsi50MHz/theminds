// Core server logic for Theminds. She takes a QuirkStart for input
// and outputs via the Line event. Very self-contained.

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;


[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Reflection.AssemblyVersionAttribute("1.1")]
[assembly: System.CLSCompliant(true)]
[assembly: System.Security.Permissions.SecurityPermission(
  System.Security.Permissions.SecurityAction.RequestMinimum, Execution = true)]
namespace Aspirations {
   // An IRC client class
   public sealed partial class Quirk : IDisposable {
      public QuirkStart Info;

      // Everytime Connection has a new line, we send it to this event.
      // NOT LogBox's NewLine. This merely glues Connection and LogBox.
      public delegate void LineDel(Quirk sender, string line);
      public event LineDel Line;

      static Random rndAddressIndex = new Random();

      bool dnsResolved;
      public Quirk(QuirkStart connectionInfo) {
         this.Info = connectionInfo;

         try {
            IPAddress[] x = Dns.GetHostEntry(connectionInfo.serv).AddressList;
            this.Info.serv = x[rndAddressIndex.Next(x.Length)].ToString();
            dnsResolved = true;
         }
         catch (SocketException) {
            Line(this, String.Format(
               "Could not resolve {0} ({1}).", connectionInfo.serv, connectionInfo.hostName));
            dnsResolved = false;
            // TODO: throw exception for caller to catch.
         }
      }

      Thread connectThread;
      public bool Started = false;
      public void Start() {
         if (this.Started) return;
         if (false == dnsResolved) return;
         connectThread = new Thread(new ThreadStart(connect));
         connectThread.IsBackground = true;
         connectThread.Start();
         this.Started = true;
      }

      public void Message(string line) {
         string unlined = line;
         Line(this, line);
         writer.WriteLine(unlined);
      }

      // IDisposable
      bool disposed;
      public void Dispose() { Dispose(null); }
      public void Dispose(string quitMsg) {
         if (disposed) return;
         if (null == connectThread) return;

         if (null != writer) {
            if (null == quitMsg) Message("QUIT");
            else Message("QUIT " + quitMsg);
         }

         abortConnect = true;
         connectThread.Join();

         if (null != writer) writer.Dispose();
         disposed = true;
      }

      /**** Private members ****/
      volatile bool abortConnect; StreamWriter writer;
      void connect() {
         Stream stream;
         try {
            stream = new TcpClient(Info.serv, Info.port).GetStream();
         }
         catch (SocketException) {
            Line(this, "Could not connect to \"" + Info.serv + "\".");
            return;
         }
         StreamReader reader = new StreamReader(stream);
         writer = new StreamWriter(stream);
         writer.AutoFlush = true;

         Message("NICK " + Info.nick + "\n" + Info.user);
         string line; abortConnect = false;
         while (!abortConnect) {
            try { line = reader.ReadLine(); }
            // TODO: what is WSAConnection error?
            catch (IOException e) { Line(this, e.ToString()); break; }
            catch (OutOfMemoryException e) { Line(this, e.ToString()); break; }

            if (line == null || line.Length <= 1) continue;
            Line(this, line);
         }
         writer.Close(); reader.Close();
      } // connect()
   }
}