using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using IRLuceneSearch.Models;
using Version = Lucene.Net.Util.Version;
using Lucene.Net.Analysis;
using Lucene.Net.Spatial.Tier;
using System.Threading;
using Lucene.Net.Spatial.Tier.Projectors;
using Lucene.Net.Util;
using System.Text.RegularExpressions;



namespace IRLuceneSearch.Service
{
        
	public class LuceneService 
    {
        		// search methods
        public static IEnumerable<Review> GetAllIndexRecords()
        {
			// validate search index
            if (!System.IO.Directory.EnumerateFiles(IndexingDirectory.LuceneIndexDir).Any()) return new List<Review>();

			// set up lucene searcher
            var searcher = new IndexSearcher(IndexingDirectory.IndexFilePath, false);
            var reader = IndexReader.Open(IndexingDirectory.IndexFilePath, false);
			var docs = new List<Document>();
			var term = reader.TermDocs();
            int count = 0;

            while (term.Next() && count <1000)
            {
                docs.Add(searcher.Doc(term.Doc()));
                count++;
            }
			reader.Dispose();
			searcher.Dispose();
			return _mapLuceneToDataList(docs);
		}

        public static IEnumerable<Review> Search(string input,int limit)
        {
            /* Check:if the seach query is empty then return*/
            if (string.IsNullOrEmpty(input))
            {
                return new List<Review>();
            }

            /* Called QueryParser to create query out of th query string */
            var query =QueryParserClass.ParseQuery(input);

            /*start searching */
            using (var searcher = new IndexSearcher(IndexingDirectory.IndexFilePath, false))
            {
                if (query != null)
                {
                    searcher.SetDefaultFieldSortScoring(true, true);
                    TopDocs topHits = searcher.Search(query, null, limit, Sort.RELEVANCE);
                    var results = _mapLuceneToDataList(topHits, searcher);
                    searcher.Dispose();
                    return results;
                }
                else
                {
                    return null;
                }
            }
            
         //   return _search(input, limit,fieldName);
            
		}

     


        public static IEnumerable<Review> SearchDefault(string input,int limit, string fieldName = "")
        {
            return string.IsNullOrEmpty(input) ? new List<Review>() : _search(input,limit, fieldName);
		}
        
		// main search method
        private static IEnumerable<Review> _search(string searchQuery, int limit,string searchField = "")
        {
            
           // searchField = "business_id";
			// validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<Review>();
            
            using (var searcher = new IndexSearcher(IndexingDirectory.IndexFilePath, false))
            {

               // /*  Builder allows us to build a polygon which we will use to limit  
               // * search scope on our cartesian tiers, this is like putting a grid 
               // * over a map */
               // var builder = new CartesianPolyFilterBuilder(Fields.LocationTierPrefix);

               // /*  Bounding area draws the polygon, this can be thought of as working  
               // * out which squares of the grid over a map to search */
               // var boundingArea = builder.GetBoundingArea(40.4116918, -79.9123428, 10 * CartesianVaraibles.KmsToMiles);


               // /*  We refine, this is the equivalent of drawing a circle on the map,  
               //  *  within our grid squares, ignoring the parts the squares we are  
               //  *  searching that aren't within the circle - ignoring extraneous corners 
               //  *  and such */
               // var distFilter = new LatLongDistanceFilter(boundingArea,
               //                                     10 * CartesianVaraibles.KmsToMiles,
               //                                     40.4116918,
               //                                     -79.9123428,
               //                                     "latitude",
               //                                     "longitude");


               // var masterQuery = new BooleanQuery();
               // /*  We add a query stating we will only search against products that have GeoCode information */
               // masterQuery.Add(new TermQuery(new Term(Fields.HasGeoCode, FieldFlags.HasField)), BooleanClause.Occur.MUST);

               // /*  Add our filter, this will stream through our results and determine eligibility */
               // masterQuery.Add(new ConstantScoreQuery(distFilter), BooleanClause.Occur.MUST);

               // int maxdoc = searcher.MaxDoc();
               // var results2 = searcher.Search(masterQuery, null, limit);
               // var results3 = _mapLuceneToDataList(results2, searcher);
               // return results3;
               //ScoreDoc[] doc =  results2.ScoreDocs;
                
                //***************************************************************************************************************
                
                //TODO:: use other analyzer with tokenizer stemmer

                var analyzer = new KeywordAnalyzer();
                //var analyzer = new StandardAnalyzer(Version.LUCENE_29);
				// search by single field
				if (!string.IsNullOrEmpty(searchField)) 
                {
					var parser = new QueryParser(Version.LUCENE_29, searchField, analyzer);
					var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, limit);
					var results = _mapLuceneToDataList(hits, searcher);
					analyzer.Close();
					searcher.Dispose();
					return results;
				}
				// search by multiple fields (ordered by RELEVANCE)
				else {
					var parser = new MultiFieldQueryParser
						(Version.LUCENE_29, new[] {
                            "business_id", 
                            //"review_id", 
                            //"full_address",                           
                            //"city",
                            "longitude",
                            "latitude",
                            //"attributes",
                            //"type",
                            //"user_id",
                            //"review_id",                  
                            "text"}, analyzer);
               
					var query = parseQuery(searchQuery, parser); 
                    searcher.SetDefaultFieldSortScoring(true, true);
                    
                    TopDocs docs = searcher.Search(query, null, limit,Sort.RELEVANCE);
                    var results = _mapLuceneToDataList(docs, searcher);
					analyzer.Close();
					searcher.Dispose();
					return results;
				}
			}
		}
		
        private static Query parseQuery(string searchQuery, QueryParser parser) {
			Query query;
			try {
				query = parser.Parse(searchQuery.Trim());
			}
			catch (ParseException) {
				query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
			}
			return query;
		}


        // map Lucene search index to data
        private static IEnumerable<Review> _mapLuceneToDataList(IEnumerable<Document> hits)
        {
            //return hits.Select(_mapLuceneDocumentToData).ToList();
            return null;
        }

        private static IEnumerable<Review> _mapLuceneToDataList(TopDocs docs, IndexSearcher searcher)
        {
          // v 2.9.4: use 'hit.doc'
          // v 3.0.3: use 'hit.Doc'

              ScoreDoc[] filterScoreDosArray = docs.ScoreDocs;
              List<Review> hits=new List<Review>();
              for (int i = 0; i < filterScoreDosArray.Length; ++i)
              {
                  Document doc = searcher.Doc(filterScoreDosArray[i].doc);
                  int docid = filterScoreDosArray[i].doc;
                  float score = filterScoreDosArray[i].score;
                  float rank = 0;
                  Review result=_mapLuceneDocumentToData(doc,score,docid,rank);
                  //TODO:: add only summry of the review and location
                  result.text = result.business_id+ result.text;
                  hits.Add(result);
              }
          
            return hits ;
			
		}
        private static Review _mapLuceneDocumentToData(Document doc, float scores, int docid, float rank)
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

            //business.longitude = Convert.ToDouble(doc.Get("longitude"));
            //business.latitude = Convert.ToDouble(doc.Get("latitude"));
            business.state = doc.Get("state");
            business.attributes = doc.Get("attributes");
            review.business = business;

            return review;
        }
                
	}
}