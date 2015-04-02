using IRLuceneSearch.Models;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;

namespace IRLuceneSearch
{
    public class SearchResultMapper
    {

        //TODO:: update the whole class
        /**
         * 
         */ 
        public static IEnumerable<Review> MapLuceneToDataList(TopDocs docs, IndexSearcher searcher, string input)
        {
            // v 2.9.4: use 'hit.doc'
            // v 3.0.3: use 'hit.Doc'
            string phraseSearchPattern = @"[\""\""].+?[\""\""]|[^ ]+";
            Regex phrasSearchPatternRegex = new Regex(phraseSearchPattern);
            MatchCollection matches = Regex.Matches(input, phraseSearchPattern);

            string[] words = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                if (Regex.IsMatch(matches[i].Value, @"[\""].+?[\""]"))
                {
                    words[i] = matches[i].Value.Substring(1, matches[i].Value.Length-2);
                }
                else
                {
                    words[i] = matches[i].Value;
                }
            }

            ScoreDoc[] filterScoreDosArray = docs.ScoreDocs;
            List<Review> hits = new List<Review>();
            for (int i = 0; i < filterScoreDosArray.Length; ++i)
            {
                Document doc = searcher.Doc(filterScoreDosArray[i].doc);
                int docid = filterScoreDosArray[i].doc;
                float score = filterScoreDosArray[i].score;
                int rank = i + 1;
                Review result = _mapLuceneDocumentToData(doc, score, docid, rank);
                //TODO:: add only summry of the review and location
                result.text = text_summarry(result.text, words);
                hits.Add(result);
            }

            return hits;

        }
        public static Review _mapLuceneDocumentToData(Document doc, float scores, int docid, int rank)
        {
            Review review = new Review();
            Business business = new Business();

            review.business_id = doc.Get("business_id");
            review.date = "";//doc.Get("date"),
            review.review_id = doc.Get("review_id");
            review.stars = Convert.ToInt32(doc.Get("stars"));
            review.text = doc.Get("text");
            review.type = doc.Get("type");
            review.user_id = doc.Get("user_id");
            review.cool = Convert.ToInt32(doc.Get("cool"));
            review.funny = Convert.ToInt32(doc.Get("funny"));
            review.useful = Convert.ToInt32(doc.Get("useful"));
            review.scores = scores;
            review.docid = docid;
            review.rank = rank;

            business.full_address = doc.Get("full_address");
            business.city = doc.Get("city");
            var obj1 = doc.Get("longitude");
            var obj2 = doc.Get("longitude");

            business.longitude = Convert.ToDouble(doc.Get("long"));
            business.latitude = Convert.ToDouble(doc.Get("lan"));
            business.state = doc.Get("state");
            business.attributes = doc.Get("attributes");
            review.business = business;

            return review;
        }

        public static string text_summarry(string text, string[] words)
        {
            if (text.Length < 400)
                return text;

            string[] sentences = Regex.Split(text, @"(?<=[\.!\?])\s+");
            int i = 0;
            while (text.Length > 400)
            {
                if (i == sentences.Length)
                {
                    text = text.Remove(400);
                    break;
                }
                int indicator = 0; // indicate whether the sentence contain the key word or not
                foreach (string word in words)
                {
                    if (sentences[i].IndexOf(word) > 0)
                        indicator++;
                }
                if (indicator == 0)
                {
                    if ((i != 0) && ((sentences[i - 1] == "...") || (sentences[i - 1] == "")))
                        sentences[i] = "";
                    else
                        sentences[i] = "...";
                }
                text = "";
                foreach (string sentence in sentences)
                {
                    text += sentence;
                }
                i++;
            }
            return text;
        }
    }
}