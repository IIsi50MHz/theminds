namespace Aspirations {
	public interface ITabsParent {
		void AddTab(System.Windows.Forms.Control button);
		void RemoveTab(System.Windows.Forms.Control button);
		ITab CreateTab(string label);

		int TabsWidth { get; }
		System.Windows.Forms.Control FocusGrabber { get; }

		void SuspendLayout();
		void ResumeLayout();
	}

	public delegate void KissWidthDel(System.Windows.Forms.Control b);
	public interface ITab {
		System.Drawing.Color ForeColor { get; set;}
		int Width { get; set;}
		int TrueWidth { get; }
		int Left { get; set;}
		System.Drawing.Font Font { get; set;}
		event System.Windows.Forms.MouseEventHandler MouseDown;

		double Shrinkage { get; set;}

		int GetHashCode();
	}
}