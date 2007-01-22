using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Theminds {
   public class Ideas {
      public string Get(string id) { return dict[id]; }

      public string Get(string id, string id2) {
         return Get(id + "." + id2);
      }

      public delegate bool TestDel(string s);
      public bool Test(string id, TestDel d) {
         id += ".test";
         return d(Get(id));
      }
      public string Test(string[] ids, TestDel d) {
         foreach (string id in ids) {
            if (Test(id, d)) return id;
         }
         return null;
      }

      public string TestContains(string haystack, string[] ids) {
         return Test(ids, new TestDel(haystack.Contains));
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