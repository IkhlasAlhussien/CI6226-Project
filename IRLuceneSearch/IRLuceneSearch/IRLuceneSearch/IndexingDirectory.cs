using Lucene.Net.Index;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace IRLuceneSearch
{
    /**
     * IndexingDirectory saves Directories for:
     * 1. Bussiness dataset
     * 2. Review dataset
     * 3. Created Index
     */
    public class IndexingDirectory
    {
        // properties
        private static string luceneIndexDir = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "..\\..\\lucene_index");
        private static string reviewDataPath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "..\\..\\yelp_dataset_challenge_academic_dataset", "yelp_academic_dataset_review.json");
        private static string businessDataPath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "..\\..\\yelp_dataset_challenge_academic_dataset", "yelp_academic_dataset_business.json");

        private static FSDirectory _indexFilePathTemp;
        public static FSDirectory IndexFilePath
        {
            get
            {
                if (_indexFilePathTemp == null)
                {
                    _indexFilePathTemp = FSDirectory.Open(new DirectoryInfo(luceneIndexDir));
                }
                if (IndexWriter.IsLocked(_indexFilePathTemp))
                {
                    IndexWriter.Unlock(_indexFilePathTemp);
                }
                var lockFilePath = Path.Combine(luceneIndexDir, "write.lock");
                if (File.Exists(lockFilePath))
                {
                    File.Delete(lockFilePath);
                }
                return _indexFilePathTemp;
            }
        }

        public static string LuceneIndexDir
        {
            get
            {
                return luceneIndexDir;
            }
        }
        public static string ReviewDataPath
        {
            get
            {
                return reviewDataPath;
            }
        }
        public static string BusinessDataPath
        {
            get
            {
                return businessDataPath;
            }
        }
    }
}