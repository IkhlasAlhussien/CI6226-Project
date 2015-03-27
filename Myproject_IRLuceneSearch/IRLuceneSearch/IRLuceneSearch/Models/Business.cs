using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IRLuceneSearch.Models
{
    public class Business
    {
        public string business_id { get; set; }
        public string full_address { get; set; }        
        public dynamic hours { get; set; }
        public string getHoursString()
        {
            string str = "";
            foreach (KeyValuePair<string, object> kvp in hours)
            { // enumerating over it exposes the Properties and Values as a KeyValuePair
                str += string.Format("{0} ", kvp.Key);
                dynamic val = kvp.Value;
                foreach (KeyValuePair<string, object> v in val) {
                    str += string.Format("{0} {1} ", v.Key ,v.Value);
                }
            }
            return str;
        }
        public bool open { get; set; }
        public string[] categories { get; set; }
        public string city { get; set; }
        public int review_count { get; set; }
        public string[] neighborhoods { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string state { get; set; }
        public string stars { get; set; }
        public dynamic attributes { get; set; }
        public string getAttributesString() {
            string str = "";
            foreach (KeyValuePair<string, object> kvp in attributes) // enumerating over it exposes the Properties and Values as a KeyValuePair
                str=string.Format("{0} {1} ", kvp.Key, kvp.Value);
            return str;
        }
        public string type { get; set; }        
    }
}