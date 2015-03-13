using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Tier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using Lucene.Net.QueryParsers;
using Version = Lucene.Net.Util.Version;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;

namespace IRLuceneSearch
{
    public class Queries
    {
        
        public static Query CreateSpatialQuery(double latitude, double longitude, int distance)
        {
            var spatialQuery = new BooleanQuery();
           
            /*  Builder allows us to build a polygon which we will use to limit  
            * search scope on our cartesian tiers, this is like putting a grid 
            * over a map */
            var builder = new CartesianPolyFilterBuilder(Fields.LocationTierPrefix);

            /*  Bounding area draws the polygon, this can be thought of as working  
            * out which squares of the grid over a map to search */
            var boundingArea = builder.GetBoundingArea(40.4116918, -79.9123428, 10 * CartesianVaraibles.KmsToMiles);


            /*  We refine, this is the equivalent of drawing a circle on the map,  
             *  within our grid squares, ignoring the parts the squares we are  
             *  searching that aren't within the circle - ignoring extraneous corners 
             *  and such */
            var distFilter = new LatLongDistanceFilter(boundingArea,
                                                distance * CartesianVaraibles.KmsToMiles,
                                                latitude,
                                                longitude,
                                                Fields.Latitude,
                                                Fields.Longitude);


            /*  We add a query stating we will only search against products that have GeoCode information */
            spatialQuery.Add(new TermQuery(new Term(Fields.HasGeoCode, FieldFlags.HasField)), BooleanClause.Occur.MUST);

            /*  Add our filter, this will stream through our results and determine eligibility */
            spatialQuery.Add(new ConstantScoreQuery(distFilter), BooleanClause.Occur.MUST);

            return spatialQuery;
        }



        public static Query CreateFieldSearchQuery(string[] fields, string[] terms)
        {
            Query query;
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            var multiFieldParser = new MultiFieldQueryParser(Version.LUCENE_29, fields, analyzer);
            try
            {
                query = multiFieldParser.Parse(string.Join(" ", terms));

            }
            catch (ParseException)
            {
                query = multiFieldParser.Parse(QueryParser.Escape(string.Join(" ", terms).Trim()));
            }
            analyzer.Close();
            return query;
        }

        public static Query CreateTermsQuery(string searchQuery)
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            var parser = new QueryParser(Version.LUCENE_29,"text", analyzer);
            var query  = parser.Parse(searchQuery);
            return query;
        }


    }
}