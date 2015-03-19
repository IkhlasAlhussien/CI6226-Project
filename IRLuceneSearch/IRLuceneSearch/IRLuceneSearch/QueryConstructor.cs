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
    public class QueryConstructor
    {
        /**
         * build a query that searchs for all review for business with loction 
         * It does not search reviews themselfs
         */
        public static BooleanQuery CreateSpatialQuery(double latitude, double longitude, int distance)
        {
            var spatialQuery = new BooleanQuery();
           
            /*  Builder allows us to build a polygon which we will use to limit  
            * search scope on our cartesian tiers, this is like putting a grid 
            * over a map */
            var builder = new CartesianPolyFilterBuilder(Fields.LOC_TIER_PREFIX);

            /*  Bounding area draws the polygon, this can be thought of as working  
            * out which squares of the grid over a map to search */
            //TODO:: set the diameter to some other value
            var boundingArea = builder.GetBoundingArea(latitude, longitude, 10 * CartesianVaraibles.KmsToMiles);
            //Shape shap= builder.get(latitude, longitude, 10 * CartesianVaraibles.KmsToMiles);


            /*  We refine, this is the equivalent of drawing a circle on the map,  
             *  within our grid squares, ignoring the parts the squares we are  
             *  searching that aren't within the circle - ignoring extraneous corners 
             *  and such */
            var distFilter = new LatLongDistanceFilter(boundingArea,
                                                distance * CartesianVaraibles.KmsToMiles,
                                                latitude,
                                                longitude,
                                                Fields.LAT_FIELD,
                                                Fields.LON_FIELD);


            /*  We add a query stating we will only search against products that have GeoCode information */
            spatialQuery.Add(new TermQuery(new Term(Fields.HAS_GEO_CODE, FieldFlags.HAS_FIELD)), BooleanClause.Occur.MUST);

            /*  Add our filter, this will stream through our results and determine eligibility */
           
            spatialQuery.Add(new ConstantScoreQuery(distFilter), BooleanClause.Occur.MUST);

            return spatialQuery;
        }

        /**
         * 
         */
        public static Query CreateFieldSearchQuery(string[] fields, string[] terms)
        {
            Query query;
            //TODO:: change analyzer
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
            var parser = new QueryParser(Version.LUCENE_29, "text", analyzer);
            var query  = parser.Parse(searchQuery);
            analyzer.Close();
            return query;
        }

        public static PhraseQuery CreatePhraseQuery(string searchQuery)
        {
            //var analyzer = new KeywordAnalyzer();
            //var parser = new QueryParser(Version.LUCENE_29, "text", analyzer);
            //var query = parser.Parse(searchQuery);
            //analyzer.Close();
            PhraseQuery query = new PhraseQuery();
            //query.Add(new Term(searchQuery);
            return query;
        }


    }
}