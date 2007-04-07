using System.IO;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Aspirations {
   public class Ideas {
      public string Get(string id) { return dict[id]; }

      public string Get(string id, string id2) {
         return Get(id + "." + id2);
      }

      public Ideas(string file) {
         dict = new StringDictionary();
         using (TextReader t = new StreamReader(file)) {
            string line;
            while ((line = t.ReadLine()) != null) parseLine(line);
         }
      }

      /**** Private members ****/
      StringDictionary dict;
      void parseLine(string line) {
         if (!line.StartsWith("\"")) return;

         string[] tokens = line.Split('"');
         try { dict.Add(tokens[1], tokens[3]); }
         catch (System.ArgumentException) {
            dict[tokens[1]] = tokens[3];
         }
      }
   } // class Ideas
}