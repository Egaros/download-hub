using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.AllDownloads
{
    public class CompletedDownload
    {
        public string fileName { get; set; }
        public string fileLocation { get; set; }
        public string fileType { get; set; }
        public CompletedDownload(string name, string location, string type)
        {
            fileName = name;
            fileLocation = location;
            fileType = type;
        }
    }
}
