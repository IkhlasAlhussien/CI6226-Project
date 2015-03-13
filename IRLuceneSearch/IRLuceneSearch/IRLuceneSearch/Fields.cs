using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IRLuceneSearch
{

    public struct Fields
    {
        public const string Latitude = "latitude";
        public const string Longitude = "longitude";
        public const string HasGeoCode = "hasGeoCode";
        public const string LocationTierPrefix = "LocationTierPrefix_";
    }

    public struct FieldFlags
    {
        public const string HasField = "true";
        public const string DoesNotHasField = "false";
    }

    public class CartesianVaraibles
    {
        public static double KmsToMiles = 0.621371192;
        public static double MaxKms = 5000 * KmsToMiles;
        public static double MinKms = 1 * KmsToMiles;
    }
}