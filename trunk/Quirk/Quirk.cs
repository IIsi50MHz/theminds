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
      public delegate void NewLineDel(string line);
      public event NewLineDel NewLine;

      static Random rndAddressIndex = new Random();

      bool dnsResolved;
      public Quirk(QuirkStart connectionInfo) {
         this.Info = connectionInfo;

         try {
            IPAddress[] x = Dns.
               GetHostEntry(Info.Server).AddressList;
            this.Info.Server = 
               x[rndAddressIndex.Next(x.Length)].ToString();
            dnsResolved = true;
         }
         catch (SocketException) {
            NewLine(String.Format(
               "Could not resolve {0}.", Info.Server));
            dnsResolved = false;
         }
      }

      Thread connectThread;
      public bool Started = false;
      public void Start() {
         if (this.Started || !dnsResolved) return;
         connectThread = new Thread(new ThreadStart(connect));
         connectThread.IsBackground = true;
         connectThread.Start();
         this.Started = true;
      }

      public void Message(string line) {
         string unlined = line;
         NewLine(line);
         writer.WriteLine(unlined);
      }

      public void Message(string line, params object[] args) {
         Message(String.Format(line, args));
      }

      // IDisposable
      bool disposed = false;
      public void Dispose() { Dispose(null); }
      public void Dispose(string quitMsg) {
         if (disposed) return;
         if (null == connectThread) return;

         if (null != writer) {
            if (null == quitMsg) Message("QUIT");
            else Message("QUIT " + quitMsg);
         }

         disposed = true;
      }

      /**** Private members ****/
      StreamWriter writer;
      StreamReader reader;
      void connect() {
         Stream stream;
         try {
            stream = new TcpClient(Info.Server, Info.Port).GetStream();
         }
         catch (SocketException) {
            NewLine("Could not connect to \"" + Info.Server + "\".");
            return;
         }
         reader = new StreamReader(stream);
         writer = new StreamWriter(stream);
         writer.AutoFlush = true;

         Message("NICK {0}\n{1}", Info.Nick, Info.User);
         while (!disposed) pump();
         writer.Dispose(); reader.Dispose();
      }

      void pump() {
         string line = null;
         try { line = reader.ReadLine(); }
         catch (IOException e) { handleException(e); }
         catch (OutOfMemoryException e) { handleException(e); }

         if (line == null) return;
         NewLine(line);
      }

      void handleException(Exception e) {
         NewLine(e.ToString());
         this.Dispose();
      }
   }
}