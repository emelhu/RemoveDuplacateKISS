using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemoveDuplicateKISS
{
    public static class Helper
    {
        public static string GetVersion()
        {
            return string.Format("Version {0}", Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "???");
        }

        //public static bool IsValidFileName(string fileName)                                                                                   // Don't tested yet!
        //{
        //    var invalidChars = Path.GetInvalidFileNameChars();
        //    return !string.IsNullOrEmpty(fileName) && fileName.IndexOfAny(invalidChars) < 0;
        //}
    }

    public enum ExitCode
    {
        OK = 0,
        HelpHappened = 5,
        ParameterErr = 10,
        Error = 100
    };
}
