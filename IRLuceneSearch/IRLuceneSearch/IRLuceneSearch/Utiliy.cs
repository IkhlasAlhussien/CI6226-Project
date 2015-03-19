using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
namespace IRLuceneSearch
{
    public class Utiliy
    {
        public static Analyzer GetAnalyzer(string analyzerType)
        {
            Analyzer analyzer = null;
            if (analyzerType == "StandardAnalyzer") // 1144.7470562 seconds, 1648 MB
            {
                analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }
            else if (analyzerType == "KeywordAnalyzer")
            {
                analyzer = new KeywordAnalyzer();
            }
            else if (analyzerType == "SimpleAnalyzer")
            {
                analyzer = new SimpleAnalyzer();
            }
            else if (analyzerType == "StopAnalyzer")
            {
                analyzer = new StopAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }
            else if (analyzerType == "WhiteSpaceAnalyzer")
            {
                analyzer = new WhitespaceAnalyzer();
            }
            return analyzer;
        }
    }
}