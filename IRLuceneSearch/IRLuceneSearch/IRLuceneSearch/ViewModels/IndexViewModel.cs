using System.Collections.Generic;
using IRLuceneSearch.Models;
using System.Web.WebPages.Html;

namespace IRLuceneSearch.ViewModels
{
    public class IndexViewModel
    {
        public int Limit { get; set; }
        public bool SearchDefault { get; set; }
        public Review SampleData { get; set; }
        public IEnumerable<Review> AllSampleData { get; set; }
        public IEnumerable<Review> SearchIndexData { get; set; }
        public IList<SelectedList> SearchFieldList { get; set; }
        public string SearchTerm { get; set; }
        public string SearchField { get; set; }
        public string Speed { get; set; }
        public string Size { get; set; }

        public string SelectedAnalyzer { get; set; }
        public IList<SelectListItem> AnalyzerFieldList
        {
            get
            {
                return new List<SelectListItem>() 
        { 
            new SelectListItem(){ Text="SimpleAnalyzer", Value="SimpleAnalyzer"} ,
            new SelectListItem(){ Text="KeywordAnalyzer", Value="KeywordAnalyzer"} ,
            new SelectListItem(){ Text="StandardAnalyzer", Value="StandardAnalyzer", Selected =true} ,
            new SelectListItem(){ Text="StopAnalyzer", Value="StopAnalyzer"} ,
            new SelectListItem(){ Text="WhiteSpaceAnalyzer", Value="WhiteSpaceAnalyzer"} ,
        };
            }
        }

    }
}