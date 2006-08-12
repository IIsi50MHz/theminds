using System;
using System.Diagnostics;

public struct QuirkStart {
	public string serv;
	public string nick;
	public string user;
	public int port;

	public string hostName;

	// Sigh, FxCop.
	public override bool Equals(object obj) {
		QuirkStart o = (QuirkStart) obj;
		if (o.hostName != this.hostName) return false;
		if (o.port != this.port) return false;
		if (o.nick != this.nick) return false;
		if (o.serv != this.serv) return false;
		if (o.user != this.user) return false;
		return true;
	}

	public override int GetHashCode() {
		return base.GetHashCode();
	}

	public override string ToString() {
		return String.Format("Server: {0}; Nick: {1}; User: {2}; Port: {3}; hostName: {4}",
			new object[] { serv, nick, user, port, hostName });
	}
}