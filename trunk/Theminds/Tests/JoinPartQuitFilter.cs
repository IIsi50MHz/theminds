using System;
using System.Diagnostics;
using Theminds;
using Aspirations;
using S = System.String;

namespace Theminds.Tests {
   [DesiresTestingWithMockAppAttribute]
   class JoinPartQuitFilter : Filters.JoinPartQuitFilter {
      string channel = "#spreadbutter";
      public JoinPartQuitFilter(IAppControls app)
         : base(app) {

         string joinOthers = App.Lion.Get("join.others");
         string joinSelf = App.Lion.Get("join.self");
         string partSelf = App.Lion.Get("part.self");
         string partOthers = App.Lion.Get("part.others");
         string partReason = App.Lion.Get("part.reason");
         string partTotal = App.Lion.Get("part.total");
         string quitSelf = App.Lion.Get("quit.self");
         string quitOthers = App.Lion.Get("quit.others");
         string quitReason = App.Lion.Get("quit.reason");
         string quitTotal = App.Lion.Get("quit.total");
         QuirkStart info = app.Connection.Info;
         info.Nick = "Tongue";

         // join, others
         test(":maria!ip JOIN :#spreadbutter",
            S.Format(joinOthers, "maria", "ip", channel), "maria");
         // join, self
         test(":Tongue!ip JOIN :#spreadbutter",
            S.Format(joinSelf, channel), "tips");
         // part, others, no reason
         test(":Tongues!ip PART #spreadbutter",
            S.Format(partOthers, "Tongues", "ip", channel),
            "roses");
         // part, others, a reason
         test(":Tongues!ip PART #spreadbutter :Too angsty",
            S.Format(partTotal,
               S.Format(partOthers, "Tongues", "ip", channel),
               S.Format(partReason, "Too angsty")), "angst");
         // part, self, no reason
         test(":Tongue!ip PART #spreadbutter",
            S.Format(partSelf, channel), "soulz");
         // part, self, a reason
         test(":Tongue!ip PART #spreadbutter :Too verklempt",
            S.Format(partTotal,
               S.Format(partSelf, channel),
               S.Format(partReason, "Too verklempt")),
            "verklempt");

         // And now those that do not need no stinking channel.
         this.channel = null;
         // quit, others, a reason
         test(":t!ip QUIT :Goodbye cruelle world",
            S.Format(quitTotal, S.Format(quitOthers, "t", "ip"),
               S.Format(quitReason, "Goodbye cruelle world")),
            "poverty");
         // quit, others, no reason
         test(":Tongues!ip QUIT", S.Format(quitOthers, "Tongues", "ip"),
            "slip");
         // quit, self, a reason
         test(":Tongue!ip QUIT :Goodbye cruelle monkey",
            S.Format(quitTotal, S.Format(quitSelf),
               S.Format(quitReason, "Goodbye cruelle monkey")),
            "jane");
         // quit, self, no reason
         test(":Tongue!ip QUIT",
            S.Format(quitSelf), "short");
         // should ignore
         test(":Tongue!ip MODE :+x", "estar en moda");
      }

      void test(string line, string id) {
         test(line, null, id);
      }

      void test(string line, string msg, string id) {
         if (null == msg) msg = line;

         BufferData data = new BufferData(line);
         filter(ref data);
         if (channel == data.Channel && msg == data.Line) return;
         throw new InvalidOperationException("JoinPartQuit failure in filter() " + id);
      }
   }
}