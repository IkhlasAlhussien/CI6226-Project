using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IRLuceneSearch.ViewModels;
using IRLuceneSearch.Models;
using IRLuceneSearch.Service;
using Lucene.Net.Analysis.Standard;
using System.Threading.Tasks;
using Lucene.Net.Analysis;

namespace IRLuceneSearch.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        
        public ActionResult Index(IndexViewModel model)
        {
            return View(model);
        }

        public ActionResult Create(string strAnalyzer)
        {

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int termsCount = 0;
            int docsCount = 0;
            string indexstr = "";
            IndexingEngine.CreateLuceneIndex(strAnalyzer, out termsCount, out docsCount, out indexstr);
            
            sw.Stop();
                       
            ViewData["result"] = "<b>" + indexstr + "</b>total terms:<b>" + termsCount + "</b>, total docs:<b>" + docsCount + "</b>, speed: <b>" + sw.Elapsed.TotalMinutes.ToString() + "</b> minutes, size: <b>1000</b> MB";
            return PartialView("Result");
        }

        public ActionResult OptimizeIndex(string strAnalyzer)
        {
         
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int termsCount = 0;
            int docsCount = 0;
            IndexingEngine.OptimizeIndex(strAnalyzer, out termsCount, out docsCount);
            ViewData["result"] = "<b>Index was successfully optimized.</b>total terms:<b>" + termsCount + "</b>, total docs:<b>" + docsCount + "</b>, speed: <b>" + sw.Elapsed.TotalMinutes.ToString() + "</b> minutes, size: <b>1000</b> MB";
            sw.Stop();
            return PartialView("Result");
        }
       
        public ActionResult Search(string input,int limit, string fieldName,IndexViewModel model)
        {
          

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            model.SearchIndexData =  LuceneService.Search(input,limit);
            sw.Stop();
            model.Speed = sw.Elapsed.TotalSeconds.ToString ();
            return PartialView("Index",model);
        }
    }
}
