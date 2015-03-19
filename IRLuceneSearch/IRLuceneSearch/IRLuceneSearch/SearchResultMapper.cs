using IRLuceneSearch.Models;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
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
        public static IEnumerable<Review> MapLuceneToDataList(TopDocs docs, IndexSearcher searcher)
        {
            // v 2.9.4: use 'hit.doc'
            // v 3.0.3: use 'hit.Doc'

            ScoreDoc[] filterScoreDosArray = docs.ScoreDocs;
            List<Review> hits = new List<Review>();
            for (int i = 0; i < filterScoreDosArray.Length; ++i)
            {
                Document doc = searcher.Doc(filterScoreDosArray[i].doc);
                int docid = filterScoreDosArray[i].doc;
                float score = filterScoreDosArray[i].score;
                float rank = 0;
                Review result = _mapLuceneDocumentToData(doc, score, docid, rank);
                //TODO:: add only summry of the review and location
                result.text = result.business_id + ": " + result.business.latitude + " | " + result.business.longitude + "   " +result.text;
                hits.Add(result);
            }

            return hits;

        }
        public static Review _mapLuceneDocumentToData(Document doc, float scores, int docid, float rank)
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
    }
}