using Aspirations;
using Color = System.Drawing.Color;

namespace Theminds {
   public struct TabId {
      public Quirk Connection;
      public string Channel;
      public LogBox LogBox;

      public TabId(Quirk c, string channel, LogBox l) {
         this.Connection = c;
         this.Channel = channel;
         this.LogBox = l;
      }

      public TabId(TabIdShell shell, LogBox l)
         : this(shell.Connection, shell.Channel, l) { }
   }

   public struct TabIdShell {
      public Quirk Connection;
      public string Channel;

      public TabIdShell(Quirk c, string channel) {
         this.Connection = c;
         this.Channel = channel;
      }

      public TabIdShell(TabId id)
         : this(id.Connection, id.Channel) { }
   }

   public struct BufferData {
      public Color Color;
      public string Channel;
      public string Line;
      public bool NeedsNewTab;
      public bool Ignore;
      public BufferData(string line) {
         this.Line = line;
         this.Color = Color.Black;
         this.Channel = null;
         this.NeedsNewTab = true;
         this.Ignore = false;
      }
   }
}