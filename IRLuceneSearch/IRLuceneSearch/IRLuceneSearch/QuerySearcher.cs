using IRLuceneSearch.Models;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IRLuceneSearch
{
    public class QuerySearcher
    {
        /**
         * This Method takes the query parameters entered by the user, parse the query, execuate it 
         * and return the search result as "Review" list
         */
        public IEnumerable<Review> SearchQuery(string input, string latitude, string longitude, int resultLimit)
        {
            IEnumerable<Review> searchResult = null;
            /* Check:if the search query is empty then return*/
            if (string.IsNullOrEmpty(input) && latitude == "" && longitude == "")
            {
                return searchResult;
            }

            /* Called QueryParser to create query out of th query string */
            BooleanQuery query = QueryParserClass.ParseQuery(input, latitude, longitude);

            /*start searching */
            using (var searcher = new IndexSearcher(IndexingDirectory.IndexFilePath, false))
            {
                if (query != null)
                {
                    searcher.SetDefaultFieldSortScoring(true, true);
                    TopDocs topHits = searcher.Search(query, null, resultLimit, Sort.RELEVANCE);
                    searchResult = SearchResultMapper.MapLuceneToDataList(topHits, searcher,input);
                }
            }

            return searchResult;
        }
    }
}