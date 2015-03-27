using IRLuceneSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IRLuceneSearch
{
    public class DatasetParser
    {
        /**
         * Parse Review and Business datasets and add parsed data to "Review" objects list 
         * If a review does not belong to a specific business it will not be added to the list
         */
        //public Dictionary<string, Review> ParseReviewDataset()
        public IEnumerable<Review> ParseReviewDataset()
        {
           // Dictionary<string, Review> reviewList = new Dictionary<string, Review>();
            List<Review> reviewList = new List<Review>();
            Dictionary<string, Business> businessList = ParseBusinessDataset();
            if (businessList != null)
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
                        reviewList.Add(review);
                        //reviewList.Add(review.review_id, review);
                    }
                }
            }
            return reviewList;
        }

        /**
         * Parse Bussiness dataset and add parsed data to "Business" objects list
         */
        private Dictionary<string, Business> ParseBusinessDataset()
        {

            Dictionary<string, Business> businessList = new Dictionary<string,Business>();
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
    }
}