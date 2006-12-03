using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;

namespace Theminds {
   class Ideas {
      public string Get(string id) {
         return dict[id];
      }

      public string Get(string id, string id2) {
         return Get(id + "." + id2);
      }

      public bool TestContains(string haystack, string id) {
         id += ".test";
         return haystack.Contains(Get(id));
      }

      public string TestContains(string haystack, string[] ids) {
         foreach (string id in ids) {
            if (TestContains(haystack, id)) return id;
         }
         return null;
      }

      public Ideas(string file) {
         dict = new Dictionary<string, string>(20);
         using (TextReader t = new StreamReader(file)) {
            string line;
            while ((line = t.ReadLine()) != null) parseLine(line);
         }
      }

      /**** Private members ****/
      Dictionary<string, string> dict;
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