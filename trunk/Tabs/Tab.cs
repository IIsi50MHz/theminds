using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Aspirations {
   public sealed class Tab : Button, ITab {
      double shrinkage = 1.0;
      public double Shrinkage {
         get { return shrinkage; }
         set {
            if (value == shrinkage) return;
            this.shrinkage = value;
            this.Width = (int)(this.trueWidth * value);
         }
      }


      // del: Add |this| to an object that will calculate its
      //    autosized width (a Form).
      int trueWidth;
      public Tab(KissWidthDel del)
         : base() {
         this.AutoSize = true;
         del(this);
         this.trueWidth = this.Width;

         this.Width = (int)(trueWidth * Shrinkage);
         this.Font = SystemFonts.MessageBoxFont;
      }

      public Tab(KissWidthDel del, string text)
         : this(del) { this.Text = text; }

      public int TrueWidth { get { return trueWidth; } }
   }
}