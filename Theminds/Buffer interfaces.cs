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

      public override int GetHashCode() {
         if (Channel == null) return 42;
         else return Channel.GetHashCode();
      }

      public override bool Equals(object obj) {
         if (obj == null) return false;
         if (typeof(TabId) != obj.GetType()) return false;
         TabId o = (TabId)obj;
         return (o.Connection == this.Connection) &&
            (o.Channel == this.Channel);
      }
   }

   public struct BufferData {
      public Color Color;
      public string Channel;
      public string Line;
      public bool Ignore;
      public bool Broadcast;
      public BufferData(string line) {
         this.Line = line;
         this.Color = Color.Black;
         this.Channel = null;
         this.Ignore = false;
         this.Broadcast = false;
      }
   }
}