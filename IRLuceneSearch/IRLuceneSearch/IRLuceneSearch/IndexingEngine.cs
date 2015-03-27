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
        private Analyzer indexingAnalyzer = null;
        private  int _startTier;
        private  int _endTier;
        private  Dictionary<int, CartesianTierPlotter> Plotters { get; set; }
        private DatasetParser datasetParser = new DatasetParser();


        #region TermsCount, DocsCount
        /**
         * TermsCount: Number of terms in the inverted index
         * DocsCount: Number of indexed documents
         */
        private  int termsCount = 0;
        private  int docsCount = 0;
       
        public  int TermsCount
        {
            get
            {
                return termsCount;
            }
            private set
            {
                termsCount = value;
            }
        }
        public  int DocsCount
        {
            get
            {
                return docsCount;
            }
            private set
            {
                docsCount = value;
            }
        }
        #endregion


        /**
         * Parse the analyzer type selected by the user and create index using that analyzer type
         * Sets the count of indexed documents (docsCount) and the count of indexed terms (TermsCount)
         * @AnalyzerType
         * @status
         */
        public  void CreateLuceneIndex(Analyzer analyzer, out string status)
        {
            /* Verify Analyzer is initilized */
            if (analyzer == null)
            {
                status = "Analyzer is not defined.";
                return;
            }

            indexingAnalyzer = analyzer;
            /* Checking the if the index already created then return the number of terms and docs indexed */
            if (IndexExists())
            {
                status = "Index already created.";
                CalculateIndexTermDocCount();
                return;
            }
            
            /* Construct region projection tiers*/
            constructPlotterTiers();

            /* Index Reviews parsed from dataset*/
            ParseAndIndexReviewDataset();

            if (IndexExists())
            {
                status = "Index was successfully created.";
                CalculateIndexTermDocCount();
            }
            else
            {
                status = "Error While Creating Index.";
            }
        }

        /**
         * Create lucene index for review data parsed from YELP dataset
         * @reviewList:  List of review data parsed and encoded as review objected
         */
        //private bool CreateReviewIndex(Dictionary<string, Review> reviewList)
        //{
        //    /*Checking the if the index already created. If true, then delete index content to start a new indexing*/
        //    if (IndexExists())
        //    {
        //        if (!ClearLuceneIndex())
        //            return false;
        //    }

        //    using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, indexingAnalyzer, IndexWriter.MaxFieldLength.UNLIMITED))
        //    {
        //        AddReviewToLuceneIndex(reviewList, writer);
        //    }

        //    return true;
        //}

        /**
         * Add the content of Bussiness objects to Lucene inverted index with differnet indexed fields
         */
        private  void AddBusinessToLuceneIndex(Business business, IndexWriter writer)
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

        /**
         * Add the content of Review objects to Lucene inverted index with differnet indexed fields
         */
        private void AddReviewToLuceneIndex(Review review, IndexWriter writer)
        {
            var doc = new Document();

            doc.Add(new Field("business_id", review.business_id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field("date", review.date, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("review_id", review.review_id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field("stars", review.stars.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("text", review.text, Field.Store.YES, Field.Index.ANALYZED));
            //doc.Add(new Field("type", review.type, Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field("user_id", review.user_id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field("cool", review.cool.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field("useful", review.useful.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("full_address", review.business.full_address, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("city", review.business.city, Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field("state", review.business.state, Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field("attributes", review.business.getAttributesString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("lan", review.business.latitude.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("long", review.business.longitude.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            AddSpatialLcnFields(review.business.latitude, review.business.longitude, doc);
            writer.AddDocument(doc);
        }

        /**
        * Add the lat, lon, and tier box id to the document
        * see http://www.nsshutdown.com/projects/lucene/whitepaper/locallucene_v2.html
        * @param lat
        * @param lon
        * @param document a geo document
        */
        private  void AddSpatialLcnFields(double lat, double lon, Document document)
        {
            document.Add(new Field(Fields.HAS_GEO_CODE, FieldFlags.HAS_FIELD, Field.Store.NO, Field.Index.NOT_ANALYZED));
            document.Add(new Field(Fields.LAT_FIELD, NumericUtils.DoubleToPrefixCoded(lat), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(Fields.LON_FIELD, NumericUtils.DoubleToPrefixCoded(lon), Field.Store.YES, Field.Index.NOT_ANALYZED));

            /* looped over the n plotters and got the identifier for each grid element that contains the latitude and longitude */
            for (var tier = _startTier; tier <= _endTier; tier++)
            {
                var ctp = Plotters[tier];
                var boxId = ctp.GetTierBoxId(lat, lon);
                document.Add(new Field(ctp.GetTierFieldName(),
                                NumericUtils.DoubleToPrefixCoded(boxId),
                                Field.Store.YES,
                                Field.Index.NOT_ANALYZED_NO_NORMS));
            }
        }

        /**
         * Check if the index file already exists.If so returns true
         */
        private  bool IndexExists()
        {
            if (IndexReader.IndexExists(IndexingDirectory.IndexFilePath))
            {
                return true;
            }
            else
                return false;
        }

        /**
         * Gets the number indexed terms and document
         */
        private  void CalculateIndexTermDocCount()
        {
            using (var reader = IndexReader.Open(IndexingDirectory.IndexFilePath, true))
            {
                TermEnum terms = reader.Terms();

                while (terms.Next())
                {
                    TermsCount += 1;
                }
                DocsCount = reader.MaxDoc();
            }
        }

        /**
         * Set up instances of CartesianTierPlotter class, one for each tier that will be indexed.
         */
        private void constructPlotterTiers()
        {
            IProjector projector = new SinusoidalProjector();
            var ctp = new CartesianTierPlotter(0, projector, Fields.LOC_TIER_PREFIX);
            _startTier = ctp.BestFit(CartesianVaraibles.MaxKms);
            _endTier = ctp.BestFit(CartesianVaraibles.MinKms);

            Plotters = new Dictionary<int, CartesianTierPlotter>();
            for (var tier = _startTier; tier <= _endTier; tier++)
            {
                Plotters.Add(tier, new CartesianTierPlotter(tier, projector, Fields.LOC_TIER_PREFIX));
            }
        }

        /**
         * Delete the content of lucene index
         */
        public bool ClearLuceneIndex()
        {
            try
            {
                using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, indexingAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // remove older index entries
                    writer.DeleteAll();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /**
        * Parse Review and Business datasets and add parsed data to "Review" objects list 
        * If a review does not belong to a specific business it will not be added to the list
        */
        public void ParseAndIndexReviewDataset()
        {
            Dictionary<string, Business> businessList = ParseBusinessDataset();

            if (businessList != null)
            {
                using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, indexingAnalyzer, IndexWriter.MaxFieldLength.UNLIMITED))
                {

                    using (System.IO.StreamReader sr = new System.IO.StreamReader(IndexingDirectory.ReviewDataPath))
                    {
                        while (sr.Peek() >= 0)
                        {
                            string line = sr.ReadLine();
                            Review review = System.Web.Helpers.Json.Decode<Review>(line);
                            if (businessList.ContainsKey(review.business_id))
                            {
                                review.business = businessList[review.business_id];
                            }
                            else
                            {
                                review.business = new Business() { full_address = "", city = "", latitude = 0, longitude = 0, state = "", attributes = "" };
                            }
                            AddReviewToLuceneIndex(review, writer);

                        }
                    }
                }
            }
        }

        /**
         * Parse Bussiness dataset and add parsed data to "Business" objects list
         */
        private Dictionary<string, Business> ParseBusinessDataset()
        {

            Dictionary<string, Business> businessList = new Dictionary<string, Business>();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(IndexingDirectory.BusinessDataPath))
            {
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    var business = System.Web.Helpers.Json.Decode<Business>(line);
                    businessList.Add(business.business_id, business);
                }
                sr.Dispose();
            }

            return businessList;
        }


        #region **************** Not used code ****************

        // add/update/clear search index data 
        public void AddUpdateLuceneIndex(Review sampleData, Analyzer analyzer)
        {
            AddUpdateLuceneIndex(new Dictionary<string, Review>() { { sampleData.review_id, sampleData } }, analyzer);
        }
        public void AddUpdateLuceneIndex(Dictionary<string, Review> sampleData, Analyzer analyzer)
        {
            // init lucene
            using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
               // AddReviewToLuceneIndex(sampleData, writer);
            }
        }
        public void ClearLuceneIndexRecord(int record_id)
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
        public void OptimizeIndex(Analyzer analyzer)
        {          
            if (IndexExists() && analyzer != null)
            {
                using (var writer = new IndexWriter(IndexingDirectory.IndexFilePath, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    writer.Optimize();
                    CalculateIndexTermDocCount();
                }
            }

        }

        #endregion
    }
}