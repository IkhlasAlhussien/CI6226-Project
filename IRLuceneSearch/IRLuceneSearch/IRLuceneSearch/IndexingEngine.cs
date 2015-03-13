using IRLuceneSearch.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Tier.Projectors;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Version = Lucene.Net.Util.Version;

namespace IRLuceneSearch
{
   
    public class IndexingEngine
    {
        private static Lucene.Net.Analysis.Analyzer anz = null;
        private static int _startTier;
        private static int _endTier;
        private static Dictionary<int, CartesianTierPlotter> Plotters { get; set; }


        public static void CreateLuceneIndex(string AnalyzerType, out int termsCount, out int docsCount, out string status)
        {
            termsCount = 0;
            docsCount = 0;

            anz = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);

            if (AnalyzerType == "StandardAnalyzer") // 1144.7470562 seconds, 1648 MB
            {
                anz = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }
            else if (AnalyzerType == "KeywordAnalyzer")
            {
                anz = new KeywordAnalyzer();
            }
            else if (AnalyzerType == "SimpleAnalyzer")
            {
                anz = new SimpleAnalyzer();
            }
            else if (AnalyzerType == "StopAnalyzer")
            {
                anz = new StopAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }
            else if (AnalyzerType == "WhiteSpaceAnalyzer")
            {
                anz = new WhitespaceAnalyzer();
            }

            //======================================================================

            IProjector projector = new SinusoidalProjector();
            var ctp = new CartesianTierPlotter(0, projector,Fields.LocationTierPrefix);
            _startTier = ctp.BestFit(CartesianVaraibles.MaxKms);
            _endTier = ctp.BestFit(CartesianVaraibles.MinKms);

            Plotters = new Dictionary<int, CartesianTierPlotter>();
            for (var tier = _startTier; tier <= _endTier; tier++)
            {
                Plotters.Add(tier, new CartesianTierPlotter(tier, projector, Fields.LocationTierPrefix));
            }
            //======================================================================

            if (StartIndexing(anz, out termsCount, out docsCount))
            {
                status = "Index was successfully created.";
            }
            else { status = "Index already created."; }


        }


        public static bool StartIndexing(Analyzer analyzer, out int termsCount, out int docsCount)
        {
            /*Checking the if the index already created then return the number of terms and docs indexed */
            if (IndexExists(out termsCount, out docsCount))
            {
                return false;
            }
            /*--------------------------------------------
             * Create Index
             ---------------------------------------------*/
            using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {

                /*-------------------------------------------------------
                 * Parse Bussiness objects and add them to a list
                 --------------------------------------------------------*/

                Dictionary<string, Business> listBusiness = new Dictionary<string, Business>();
                using (System.IO.StreamReader sr = new System.IO.StreamReader(IndexingDirectory.BusinessDataPath))
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        var business = System.Web.Helpers.Json.Decode<Business>(line);
                        listBusiness.Add(business.business_id, business);
                    }
                    sr.Dispose();
                }

                /*-------------------------------------------------------
                 * Parse Review objects and add them to index
                 --------------------------------------------------------*/
                using (System.IO.StreamReader sr = new System.IO.StreamReader(IndexingDirectory.ReviewDataPath))
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        Review review = System.Web.Helpers.Json.Decode<Review>(line);
                        if (listBusiness.ContainsKey(review.business_id))
                        {
                            review.business = listBusiness[review.business_id];
                        }
                        else
                        {
                            review.business = new Business() { full_address = "", city = "", latitude = 0, longitude = 0, state = "", attributes = "" };
                        }
                        _addReviewToLuceneIndex(review, writer);
                    }
                    sr.Dispose();
                }
                // close handles
                analyzer.Close();
                writer.Dispose();

                IndexReader reader = IndexReader.Open(IndexingDirectory.IndexFilePath, true);
                TermEnum terms = reader.Terms();

                while (terms.Next()) termsCount += 1;
                docsCount = reader.MaxDoc();
                reader.Dispose();
            }

            return true;
        }

        private static void _addBusinessToLuceneIndex(Business business, IndexWriter writer)
        {
            // remove older index entry
            //var searchQuery = new TermQuery(new Term("review_id", review.review_id.ToString()));
            //writer.DeleteDocuments(searchQuery);

            // add new index entry
            var doc = new Document();
            doc.Add(new Field("business_id", business.business_id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("full_address", business.full_address, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("hours", business.getHoursString(), Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));
            doc.Add(new Field("open", business.open.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("categories", string.Join(",", business.categories), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("city", business.city, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("review_count", business.review_count.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("neighborhoods", string.Join(",", business.neighborhoods), Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));
            doc.Add(new Field("longitude", business.longitude.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("latitude", business.latitude.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("state", business.state, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("stars", business.stars.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("attributes", business.getAttributesString(), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("type", business.type, Field.Store.YES, Field.Index.NOT_ANALYZED));
            // add entry to index
            writer.AddDocument(doc);
        }
      
        private static void _addReviewToLuceneIndex(Review review, IndexWriter writer)
        {
            var doc = new Document();

            doc.Add(new Field("business_id", review.business_id, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("date", review.date, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("review_id", review.review_id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("stars", review.stars.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("text", review.text, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("type", review.type, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("user_id", review.user_id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("cool", review.cool.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("funny", review.funny.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("useful", review.useful.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("full_address", review.business.full_address, Field.Store.YES, Field.Index.NO));
            doc.Add(new Field("city", review.business.city, Field.Store.YES, Field.Index.NO));
            doc.Add(new Field("state", review.business.state, Field.Store.YES, Field.Index.NO));
            doc.Add(new Field("attributes", review.business.getAttributesString(), Field.Store.YES, Field.Index.NO));

            //Add the latitude and longitude fields to the index
            doc.Add(new Field(Fields.HasGeoCode, FieldFlags.HasField, Field.Store.NO, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(Fields.Latitude, NumericUtils.DoubleToPrefixCoded(review.business.latitude), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(Fields.Longitude, NumericUtils.DoubleToPrefixCoded(review.business.longitude), Field.Store.YES, Field.Index.NOT_ANALYZED));
            AddCartesianTiers(review.business.latitude, review.business.longitude, doc);

            writer.AddDocument(doc);
        }



        private static void AddCartesianTiers(double latitude,double longitude, Document document)
        {
            for (var tier = _startTier; tier <= _endTier; tier++)
            {
                var ctp = Plotters[tier];
                var boxId = ctp.GetTierBoxId(latitude, longitude);
                document.Add(new Field(ctp.GetTierFieldName(),
                                NumericUtils.DoubleToPrefixCoded(boxId),
                                Field.Store.YES,
                                Field.Index.NOT_ANALYZED_NO_NORMS));
            }
        }


        // add/update/clear search index data 
        public static void AddUpdateLuceneIndex(Review sampleData)
        {
            AddUpdateLuceneIndex(new List<Review> { sampleData });
        }
        public static void AddUpdateLuceneIndex(IEnumerable<Review> sampleDatas)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entries if any)
                foreach (var sampleData in sampleDatas) _addReviewToLuceneIndex(sampleData, writer);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }
        public static void ClearLuceneIndexRecord(int record_id)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // remove older index entry
                var searchQuery = new TermQuery(new Term("Id", record_id.ToString()));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }
        public static bool ClearLuceneIndex()
        {
            try
            {
                var analyzer = new StandardAnalyzer(Version.LUCENE_29);
                using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // remove older index entries
                    writer.DeleteAll();

                    // close handles
                    analyzer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static void OptimizeIndex(string AnalyzerType, out int termsCount, out int docs)
        {
            if (IndexExists(out termsCount, out docs))
            {

                using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, anz, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    anz.Close();
                    writer.Optimize();
                    writer.Dispose();

                    IndexReader reader = IndexReader.Open(IndexingDirectory.IndexFilePath, true);
                    TermEnum terms = reader.Terms();

                    while (terms.Next())
                    {
                        termsCount += 1;
                    }
                    docs = reader.MaxDoc();
                    reader.Dispose();
                }
            }
        
        }
                           
        private static bool IndexExists(out int termsCount, out int docsCount)
        {
            termsCount = 0;
            docsCount = 0;
            if (IndexReader.IndexExists(IndexingDirectory.IndexFilePath))
            {
                IndexReader reader = IndexReader.Open(IndexingDirectory.IndexFilePath, true);
                TermEnum terms = reader.Terms();

                while (terms.Next())
                {
                    termsCount += 1;
                }
                docsCount = reader.MaxDoc();

                return true;
            }
            else
                return false;
        }

    }
}