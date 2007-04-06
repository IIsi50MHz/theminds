using System;
using System.Diagnostics;
using Theminds;
using Aspirations;
using S = System.String;

namespace Theminds.Tests {
   [DesiresTesting]
   class JoinPartQuitFilter : Filters.JoinPartQuitFilter {
      public JoinPartQuitFilter()
         : this(new MockApp()) { }
      public JoinPartQuitFilter(IAppControls app)
         : base(app) {

         string joinOthers = App.Lion.Get("join.others");
         string joinSelf = App.Lion.Get("join.self");
         string partOthers = App.Lion.Get("part.others");
         string partReason = App.Lion.Get("part.reason");
         string partTotal = App.Lion.Get("part.total");
         string quitOthers = App.Lion.Get("quit.others");
         string quitReason = App.Lion.Get("quit.reason");
         string quitTotal = App.Lion.Get("quit.total");
         string channel = "#spreadbutter";
         QuirkStart info = app.Connection.Info;
         info.Nick = "Tongue";

         // join, others
         testFilter(":maria!ip JOIN :#spreadbutter",
            S.Format(joinOthers, "maria", "ip", channel), channel, "maria");

         // join, self
         testFilter(":Tongue!ip JOIN :#spreadbutter",
            S.Format(joinSelf, channel), channel, "tips");

         // part, others, no reason
         testFilter(":Tongues!ip PART #spreadbutter",
            S.Format(partTotal, S.Format(partOthers, "Tongues", "ip", channel), ""),
            channel, "roses");

         // part, others, a reason
         testFilter(":Tongues!ip PART #spreadbutter :Too angsty",
            S.Format(partTotal,
               S.Format(partOthers, "Tongues", "ip", channel),
               S.Format(partReason, "Too angsty")),
            channel, "angst");

         // quit, others, no reason
         testFilter(":Tongues!ip QUIT",
            S.Format(quitTotal, S.Format(quitOthers, "Tongues", "ip", channel, null), ""),
            null, "slip");
      }

      void testFilter(string line, string id) {
         testFilter(line, null, null, id);
      }

      void testFilter(string line, string msg,
         string channel, string id) {
         if (null == msg) msg = line;

         BufferData data = new BufferData(line);
         filter(ref data);
         if (channel == data.Channel && msg == data.Line) return;
         throw new InvalidOperationException("JoinPartQuit failure in filter() " + id);
      }
   }
}