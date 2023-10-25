using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveDuplicateKISS
{
    public interface ILogItemsEngine
    {
        Task DirectoryProcess(string path);
        Task FileProcess(string path);
        Task FileDuplicated(StoreItem storeItem, string operationText, string stayFilename);
    }

    public class LogItemsEngineSimple : ILogItemsEngine
    {
        public LogItemsEngineSimple(StreamWriter logStream, TextWriter textOut)
        {
            this.logStream = logStream;
            this.textOut   = textOut;
        }

        public StreamWriter logStream { get; }
        public TextWriter   textOut   { get; }

        public async Task DirectoryProcess(string path)
        {
            await textOut.WriteLineAsync(path);
        }

        public async Task FileProcess(string path)
        {
            await textOut.WriteLineAsync(path);
        }

        public async Task FileDuplicated(StoreItem storeItem, string operationText, string stayFilename)
        {
            await logStream.WriteLineAsync($"{storeItem.path}\t{storeItem.hash}\t{operationText}\t[{stayFilename}]");
        }
    }
}
