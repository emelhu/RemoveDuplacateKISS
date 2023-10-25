using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveDuplicateKISS
{
    public class RdkParams
    {
        public List<string>     dirs        { get; private set; }
        public bool             recurse     { get; private set; }
        public bool             delete      { get; private set; }
        public string?          logFile     { get; private set; }
        public bool             verbose     { get; private set; }

        public RdkParams(List<string> dirs, bool recurse, bool delete, string? logFile, bool verbose)
        {
            this.dirs = dirs;
            this.recurse = recurse;
            this.delete = delete;
            this.logFile = logFile;
            this.verbose = verbose;
        }
    }
}
