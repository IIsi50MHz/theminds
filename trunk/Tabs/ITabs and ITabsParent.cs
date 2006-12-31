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
      int Width { get; set;}
      int TrueWidth { get; }
      int Left { get; set;}
      double Shrinkage { get; set;}

      int GetHashCode();
      void BecomeOld();
      void BecomeNew();

      System.Drawing.Color ForeColor { get; set;}
      System.Drawing.Font Font { get; set;}

      event System.Windows.Forms.MouseEventHandler MouseDown;

   }
}