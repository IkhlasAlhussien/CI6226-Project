using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IRLuceneSearch
{
    public class ServiceAnalyzer
    {
       // public static Analyzer analayzer = null;
    }
    public struct Fields
    {
        public const string LAT_FIELD = "latitude";
        public const string LON_FIELD = "longitude";
        public const string HAS_GEO_CODE = "hasGeoCode";
        public const string LOC_TIER_PREFIX = "LocationTierPrefix_";
    }

    public struct FieldFlags
    {
        public const string HAS_FIELD = "true";
        public const string DOES_NOT_HAS_FIELD = "false";
    }

    public class CartesianVaraibles
    {
        public static double KmsToMiles = 0.621371192;
        public static double MaxKms = 5000 * KmsToMiles;
        public static double MinKms = 1 * KmsToMiles;
    }
}