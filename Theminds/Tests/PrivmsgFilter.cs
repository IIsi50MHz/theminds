using System;
using System.Diagnostics;
using Theminds;
using Aspirations;
using S = System.String;

namespace Theminds.Tests {
   [DesiresTesting]
   class PrivmsgFilter : Filters.PrivmsgFilter {
      public PrivmsgFilter()
         : this(new MockApp()) { }
      public PrivmsgFilter(IAppControls app)
         : base(app) {

         string speechAll = App.Lion.Get("speech.all");
         string actionAll = App.Lion.Get("action.all");
         string channel = "#spreadbutter";
         QuirkStart info = app.Connection.Info;

         // speech, others
         testFilter(":maria!ip PRIVMSG #spreadbutter :Hello! #moto rola",
            speechAll, channel, "maria", "Hello! #moto rola", "maria");

         // action, others
         testFilter(":maria!ip PRIVMSG #spreadbutter :\u0001ACTION #dances !butterfly",
            actionAll, channel, "maria", "#dances !butterfly", "butterfly");

         // speech, self
         testFilter("PRIVMSG #spreadbutter PEOPLE WANNA DANCE #spreadbutter",
            speechAll, channel, info.Nick, "PEOPLE WANNA DANCE #spreadbutter", "dance");

         // action, self
         testFilter("PRIVMSG #spreadbutter :\u0001ACTION #plucks !alfalfa!",
            actionAll, channel, info.Nick, "#plucks !alfalfa!", "alfalfa");
      }

      void testFilter(string line, string template,
         string channel, string nick, string msg, string id) {
         BufferData data = new BufferData(line);
         filter(ref data);
         if (channel == data.Channel &&
            S.Format(template, nick, msg) == data.Line) return;
         Debug.Fail("PrimvsgFilter failure in filter() " + id);
      }
   }
}