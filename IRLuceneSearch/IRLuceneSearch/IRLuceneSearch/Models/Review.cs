using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IRLuceneSearch.Models
{
    public class Review 
    {
        public string type { get; set; }
        public string business_id { get; set; }
        public string user_id { get; set; }
        public string review_id { get; set; }
        public int stars { get; set; }
        public string date { get; set; }
        public string text { get; set; }
        
        public int funny { get; set; }
        public int useful { get; set; }
        public int cool { get; set; }

        public float scores { get; set; }
        public int docid { get; set; }
        public int rank { get; set; }

        public Business business { get; set; }
    }

    public class votes {
        public int funny { get; set; }
        public int useful { get; set; }
        public int cool { get; set; }
    }
}