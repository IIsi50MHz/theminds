using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Theminds {
	public struct QuirkStart {
		public string serv;
		public string nick;
		public string user;
		public int port;

		public string hostName;
	}
	
	// An IRC client class
	public sealed partial class Quirk : IDisposable {
		public QuirkStart Info;

		// Everytime Connection has a new line, we send it to this event.
		// NOT LogBox's NewLine. This merely glues Connection and LogBox.
		public delegate void LineDel(string line);
		public event LineDel Line;

		static Random random;
		static Quirk() { random = new Random(); }
		
		bool dnsResolved;
		public Quirk(QuirkStart connectionInfo) {
			this.Info = connectionInfo;

			try {
				/*IPAddress[] x = Dns.GetHostEntry(connectionInfo.serv).AddressList;				
				int n = random.Next(0, x.Length);

				this.Info.serv = x[n].ToString();
				this.Info.hostName = Dns.GetHostEntry(this.Info.serv).HostName;*/
				dnsResolved = true;
			}
			catch (SocketException) {
				Line(String.Format(
					"Could not resolve {0} ({1}).", connectionInfo.serv, connectionInfo.hostName));
				dnsResolved = false;
				// TODO: throw exception for caller to catch.
			}
		}

		Thread connectThread;
		bool started;
		[Conditional("ENABLE_CONNECT")]
		public void Start() {
			if (false == dnsResolved) return;
			started = true;
			connectThread = new Thread(new ThreadStart(connect));
			connectThread.IsBackground = true;
			connectThread.Start();
		}

		// TODO: This can get called even if the connection gets closed.
		public void Message(string line) {
			string unlined = line;
			Line(line);
			writer.WriteLine(unlined);
		}

		// IDisposable
		bool disposed;
		public void Dispose() { Dispose(null); }
		public void Dispose(string quitMsg) {
			if (disposed || false == dnsResolved || false == started) return;

			if (null == quitMsg) Message("QUIT");
			else Message("QUIT " + quitMsg);

			abortConnect = true;
			connectThread.Join(1000);

			writer.Dispose();
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
				Line("Could not connect to \"" + Info.serv + "\".");
				return;
			}
			StreamReader reader = new StreamReader(stream);
			writer = new StreamWriter(stream);
			writer.AutoFlush = true;

			Timer pingTimer = new Timer((TimerCallback) delegate {
				Message("PING :" + Info.serv);
			}, null, 60000, 120000);

			Message("NICK " + Info.nick + "\n" + Info.user);

			string line; abortConnect = false;
			while (!abortConnect) {
				try { line = reader.ReadLine(); }
				// TODO: what is WSAConnection error?
				catch (IOException e) { Line(e.ToString()); break; }
				catch (OutOfMemoryException e) { Line(e.ToString()); break; }
				
				if (line == null || line.Length <= 1) continue;

				Line(line);
			}

			pingTimer.Dispose();
			writer.Close(); reader.Close();
		} // connect()
	}
}