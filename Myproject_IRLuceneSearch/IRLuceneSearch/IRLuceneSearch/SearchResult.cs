using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IRLuceneSearch
{
    public class SpatialSearchResult
    {
        public double Score { get; set; }
        public int LocationId { get; set; }
        public double DistanceInKms { get; set; }

        public SpatialSearchResult()
        {

        }

        public SpatialSearchResult(double score, int id, double distanceInKms)
        {
            Score = score;
            LocationId = id;
            DistanceInKms = distanceInKms;
        }
    }
}