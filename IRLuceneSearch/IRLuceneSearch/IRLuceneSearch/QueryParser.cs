using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Version = Lucene.Net.Util.Version;


namespace IRLuceneSearch
{
    public class QueryParserClass
    {
        private static List<string> SearchableFields  = new List<string>(){"text", "city"};

        public static BooleanQuery ParseQuery(string input, string latitude, string longitude)
        {

            Query query = null;
            BooleanQuery bQuery=new BooleanQuery();
            string FieldSearchPattern = "(.+?):(.+?)";
            Regex FieldSearchPatternRegex = new Regex(FieldSearchPattern);

            string phraseSearchPattern = @"[\""\""].+?[\""\""]|[^ ]+";
            Regex phrasSearchPatternRegex = new Regex(phraseSearchPattern);


            /*
             * Handle different query formates 
             * 1. Phrase query: if the query starts and ends with quotation marks
             * 2. Phrase query with Location: if the query starts and ends with quotation marks and contains locations filter
             * 3. Location query: if query contains only location filter
             * 4. Field Query: Query that is searching a specific field
             * 5. Free text query: all Other kinds of queries 
             */
            
            

          

            /* 
             * 3. Location query: if query contains only location filter
             */

            if (latitude != "" && longitude != "")
            {
                double dLatitude;
                double dLongitude;
                bool isNumLatitude = double.TryParse(latitude, out dLatitude);
                bool isNumLongtitude = double.TryParse(longitude, out dLongitude);

                if (isNumLatitude && isNumLatitude)
                {
                    bQuery.Add(QueryConstructor.CreateSpatialQuery(dLatitude, dLongitude, 10),BooleanClause.Occur.MUST);
                }

                // Empty Query String which means only contains location query
                if (input == "")
                {
                    return bQuery;
                }
            }

            /* 
            *  1. Phrase query: if the query starts and ends with quotation marks
            */
            bool m = Regex.IsMatch(input, @"[\""].+?[\""]");

            if (phrasSearchPatternRegex.IsMatch(input))
            {
                MatchCollection matches = Regex.Matches(input, phraseSearchPattern);
                for (int i = 0; i < matches.Count; i++)
                {
                    if (Regex.IsMatch(matches[i].Value, @"[\""].+?[\""]"))
                    {
                        bQuery.Add(QueryConstructor.CreateTermsQuery(matches[i].Value), BooleanClause.Occur.MUST);
                    }
                    else
                    {
                        bQuery.Add(QueryConstructor.CreateTermsQuery(matches[i].Value), BooleanClause.Occur.SHOULD);
                    }

                }
            }
           /* 
           * 4.  Field Query: Query that is searching a specific field
           */
            else if (FieldSearchPatternRegex.IsMatch(input))
            {
                string pattern = "(.+?:)";

                string[] substrings = Regex.Split(input, pattern);    // Split on hyphens
                foreach (string match in substrings)
                {
                    Console.WriteLine("'{0}'", match);
                }

                string[] queryTerms = input.Split(':');
                string[] searchTerms = new string[queryTerms.Length - 1];



                if (IsSearchableField(queryTerms[0]))
                {
                    Array.Copy(queryTerms, 1, searchTerms, 0, queryTerms.Length - 1);
                    query = QueryConstructor.CreateFieldSearchQuery(new[] { queryTerms[0] }, searchTerms);
                }
                else
                {
                    query = QueryConstructor.CreateTermsQuery(input);
                }


                //.Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
                //input = string.Join(" ", terms);

                //query = Queries.CreateTermQueryWithFields(latitude, longitude);
            }
            else
            {
                bQuery.Add(QueryConstructor.CreateTermsQuery(input), BooleanClause.Occur.SHOULD);
            }


            return bQuery;

        }

        private static bool IsSearchableField(string field)
        {
            if (SearchableFields.Contains(field))
                return true;
            else
                return false;
        }

       
    }
}