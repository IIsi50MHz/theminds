using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;

namespace Aspirations {
   public class UserList : ListBox {
      System.Collections.Generic.List<string> itemsBuffer;
      public UserList() {
         itemsBuffer = new List<string>();
      }

      public void Push(string user) { itemsBuffer.Add(user); }
      public void Flush() {
         Items.Clear(); Items.AddRange(itemsBuffer.ToArray());
         itemsBuffer.Clear();
      }
   }
}