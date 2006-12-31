using System;
using System.Drawing;
using System.Windows.Forms;
using Aspirations;

namespace Theminds {
   sealed partial class Page : Form {
      LogBox logBox;
      InputBox inputBox;
      ListBox userList;

      RichTextBox userNotice;
      Panel userPanel;
      Panel tabsPanel;
      Panel logBoxPanel;
      Splitter consoleInputSplitter;
      Splitter consoleUsersSplitter;

      TabsController tabber;
      void SetUp() {
         this.WindowState = FormWindowState.Maximized;
         this.KeyPreview = true;
         this.SuspendLayout();

         logBox = new LogBox();
         userList = new ListBox();
         inputBox = new InputBox();
         userNotice = new RichTextBox();
         userPanel = new Panel();
         logBoxPanel = new Panel();
         consoleInputSplitter = new Splitter();
         consoleUsersSplitter = new Splitter();

         inputBox.Dock = DockStyle.Bottom;
         inputBox.Font = new System.Drawing.Font("Lucida Console", 10);
         inputBox.Multiline = false;
         inputBox.BackColor = Color.White;
         inputBox.Height = inputBox.PreferredHeight;
         inputBox.TabIndex = 0;

         logBoxPanel.Controls.Add(logBox);
         logBoxPanel.Dock = DockStyle.Fill;
         logBoxPanel.BorderStyle = BorderStyle.None;
         logBoxPanel.AutoSize = true;

         /* The users, the notices, the panel, the myth */
         userNotice.Dock = DockStyle.Top;
         userNotice.BackColor = Color.Blue;

         userList.ScrollAlwaysVisible = true;
         userList.IntegralHeight = false;
         userList.Dock = DockStyle.Fill;
         userList.Width = 200;
         userList.TabIndex = 1;
         userList.Font = SystemFonts.MessageBoxFont;

         userPanel.Controls.Add(userList);
         userPanel.Controls.Add(userNotice);
         userPanel.Dock = DockStyle.Right;

         /* Splitters! */
         consoleInputSplitter.Dock = DockStyle.Bottom;
         consoleUsersSplitter.Dock = DockStyle.Right;

         tabsPanel = new Panel();
         tabsPanel.Dock = DockStyle.Bottom;
         tabsPanel.AutoSize = true;

         tabber = new TabsController(this, "(new)");
         tabber.NewTab += new TabDel(tabber.MoveTo);
         tabber.Init();

         // Order is critical
         this.Controls.AddRange(new System.Windows.Forms.Control[] {
				logBoxPanel, consoleUsersSplitter, userPanel , consoleInputSplitter,
				inputBox, tabsPanel});
         this.ResumeLayout(); // this.suspendLayout() above
      }
   }
}