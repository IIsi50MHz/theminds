using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Reflection.AssemblyVersionAttribute("1.1")]
[assembly: System.CLSCompliant(true)]
[assembly: System.Security.Permissions.SecurityPermission(
  System.Security.Permissions.SecurityAction.RequestMinimum, Execution = true)]
namespace Aspirations {
   public delegate void TabDel(ITab t);
   public delegate void IntDel(int i);
   public class Tabber {
      List<ITab> tabs;
      ITabsParent parent;
      string defaultLabel;
      public Tabber(ITabsParent p, string label) {
         this.parent = p;
         this.defaultLabel = label;
         this.NewTab += new TabDel(OnNewTab);
         this.Moved += delegate { };
         this.Removed += delegate { };

         tabs = new List<ITab>();
      }

      public void Init() { Add(this.defaultLabel); }

      public ITab Current {
         get { return tabs[this.current]; }
      }

      // right: total width of all the buttons.
      // realWidth: total width without shrinkage
      // current: index in |tabs| of current tab
      int right, current, realWidth;
      public event TabDel NewTab;
      public ITab Add(string label) {
         if (null == label) label = defaultLabel;
         if (tabs.Count > 0) tabs[current].BecomeOld();

         // Math trick. This is before we add it, so it's
         // lastIndex + 1, or Count.
         current = tabs.Count;

         ITab x = this.parent.CreateTab(label);
         this.NewTab(x);

         this.right += x.Width;
         this.realWidth += x.TrueWidth;

         parent.GrabFocus();
         resize();

         return x;
      }

      public void Add(string[] labels) {
         foreach (string label in labels) Add(label);
      }
      public void Add() { Add(this.defaultLabel); }

      public event TabDel Moved;
      public void MoveTo(ITab tab) {
         int index = tabs.IndexOf(tab);
         c(delegate { current = index; parent.GrabFocus(); });
         this.Moved(tab);
      }
      public void MoveTo(int index) { MoveTo(tabs[index]); }

      public void MoveToNext() {
         c(delegate { current = (current + 1) % tabs.Count; });
         Moved(this.Current);
      }

      public void MoveToPrev() {
         // Adding tabs.Count prevents negative numbers.
         c(delegate { current = (current - 1 + tabs.Count) % tabs.Count; });
         Moved(this.Current);
      }

      public event TabDel Removed;
      public void RemoveCurrent() { Remove(current); }
      public void Remove(int index) {
         if (tabs.Count < current + 1) {
            throw new InvalidOperationException(
               "I am removing a tab that does not exist. Make sure to call Init.");
         }
         ITab x = tabs[index];
         int width = x.Width;
         this.realWidth -= x.TrueWidth;

         parent.SuspendLayout();
         // Places to clean up:
         // * Parent's collection
         // * Our collection
         parent.RemoveTab((System.Windows.Forms.Control)x);
         tabs.Remove(x);
         tabs.ForEach(delegate(ITab t) { t.Left -= width; });
         
         removeHelperForCounters(index, width);
         MoveTo(current); resize();
         // On the ITab's lifetime:
         //  It should die out after this event call unless
         //  an event help stores it, which is a Bad Idea.
         Removed(x);
         parent.ResumeLayout();
      }
      public void Remove(ITab t) { Remove(tabs.IndexOf(t)); }

      /**** Protected members ****/
      protected void OnNewTab(ITab t) {
         t.Left = right;
         tabs.Add(t);
      }

      /**** Private members ****/
      // Magic over; tabs are all added/removed already
      void resize() {
         int maxWidth = parent.TabsWidth;
         int realTotalWidth = 0;
         tabs.ForEach(delegate(ITab tab) {
            realTotalWidth += tab.TrueWidth;
         });

         double sB = (double)maxWidth / realTotalWidth;
         this.right = 0;
         double shrinkBy = (sB < 1.0) ? sB : 1.0;
         tabs.ForEach(delegate(ITab tab) {
            tab.Shrinkage = shrinkBy;
            tab.Left = right;
            right += tab.Width;
         });
      }

      delegate void ChangeCurrentDel();
      void c(ChangeCurrentDel del) {
         parent.SuspendLayout();
         tabs[current].BecomeOld();
         del(); // Here, current changes.
         tabs[current].BecomeNew();
         parent.ResumeLayout();
      }

      void removeHelperForCounters(int index, int width) {
         // Stop when true:
         // * Close last, current one -> go to the new last tab.
         // * Close tab after current tab -> stay, no changes.
         // * Close tab before current tab -> update number.
         if (tabs.Count > 0) {
            if (index < current || current >= tabs.Count) {
               current -= 1;
            }
            this.right -= width;
         }
         else {
            this.right = 0;
            Add(); current = 0;
         }
      }
   } // class TabsController
} // namespace