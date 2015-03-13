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

        public static Query ParseQuery(string input)
        {
            /*
             * Handle differente query formates 
             * 1. Phrase query: if the query starts and ends with qutation marks
             * 2. Phrase query with Location: if the query starts and ends with qutation marks and contains locations filter
             * 3. Location query: if query contains only location filter
             * 4. Field Query: Query that is searching a specific field
             * 5. Free text query: all Other kinds of queries 
             */
            
            // Empty Query String
            if (input == null)
            {
                return null;
            }


            Query query = null;
            string FieldSearchPattern = "(.+?):(.+?)";
            Regex FieldSearchPatternRegex = new Regex(FieldSearchPattern);

            /* 
             * 3. Location query: if query contains only location filter
             */

            if (input.Contains("cord:") && input.Contains(","))
            {
                var terms = input.Trim().Replace("cord:", "").Split(',');
                if (terms.Length < 2 || terms.Length > 2)
                {
                    // normal query
                }
                else
                {
                    double latitude;
                    double longitude;
                    bool isNumLatitude = double.TryParse(terms[0], out latitude);
                    bool isNumLongtitude = double.TryParse(terms[1], out longitude);
                    if (isNumLatitude && isNumLatitude)
                    {
                        query = Queries.CreateSpatialQuery(latitude, longitude,10);
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
                    query = Queries.CreateFieldSearchQuery(new[] { queryTerms[0] }, searchTerms);
                }
                else
                {
                    query = Queries.CreateTermsQuery(input);
                }


                //    .Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
                //input = string.Join(" ", terms);

                //query = Queries.CreateTermQueryWithFields(latitude, longitude);
            }
            else
            {
                query = Queries.CreateTermsQuery(input);
            }


            return query;

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