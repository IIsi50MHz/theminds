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
			consoleInputSplitter = new Splitter();
			consoleUsersSplitter = new Splitter();

			inputBox.Dock = DockStyle.Bottom;
			inputBox.Font = new System.Drawing.Font("Lucida Console", 10);
			inputBox.Multiline = false;
			inputBox.BackColor = Color.White;
			inputBox.Height = inputBox.PreferredHeight;
			inputBox.TabIndex = 0;

			logBox.Dock = DockStyle.Fill;
			logBox.Font = new System.Drawing.Font("Calibri", 11);
			logBox.WordWrap = true;
			logBox.SelectionHangingIndent = 30;
			logBox.ReadOnly = true;
			logBox.TabStop = false;
			NativeMethods.ParaFormat fmt = new NativeMethods.ParaFormat();
				fmt.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(fmt);
				fmt.dwMask = 0x00000100; // PFM_LINESPACING
				fmt.dyLineSpacing = 20; // 20 = single-space, 40 = double-, &c
				fmt.bLineSpacingRule = 5;
			NativeMethods.SendMessage(logBox.Handle, 1095, (IntPtr) 0, ref fmt);


			/* The users, the notices, the panel, the myth */
			userNotice.Dock = DockStyle.Top;
			userNotice.BackColor = Color.Blue;

			userList.ScrollAlwaysVisible = true;
			userList.IntegralHeight = false;
			userList.Dock = DockStyle.Fill;
			userList.Width = 200;
			userList.TabIndex = 1;

			userPanel.Controls.Add(userList);
			userPanel.Controls.Add(userNotice);
			userPanel.Dock = DockStyle.Right;

			/* Splitters! */
			consoleInputSplitter.Dock = DockStyle.Bottom;			
			consoleUsersSplitter.Dock = DockStyle.Right;

			tabsPanel = new Panel();
			tabsPanel.Dock = DockStyle.Bottom;
			tabsPanel.AutoSize = true;

			tabber = new TabsController(this, "(New)");
			tabber.NewTab += new TabDel(tabber.MoveTo);
			tabber.Init();

			// Order is critical
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
				logBox, consoleUsersSplitter, userPanel , consoleInputSplitter,
				inputBox, tabsPanel});
			this.ResumeLayout(); // this.suspendLayout() above
		}
	}
}