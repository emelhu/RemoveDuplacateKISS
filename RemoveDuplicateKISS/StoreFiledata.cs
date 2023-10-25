using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;

namespace RemoveDuplicateKISS
{
    public interface IStoreFileData 
    {
        public string? StoreIfUniq(StoreItem item);
    }


    public class StoreFiledata : IDisposable, IStoreFileData
    {
        private SortedList<string, string>  storage = new SortedList<string, string>();
        private IPermanentFileData? permanentEngine;

        //

        public StoreFiledata(IPermanentFileData? permanentEngine = null)
        {
            this.permanentEngine = permanentEngine;

            LoadList();
        }

        private void LoadList()
        {
            if (permanentEngine is not null)
            {
                storage = permanentEngine.Load();
            }
        }

        private void SaveList()
        {
            if (permanentEngine is not null)
            {
                permanentEngine.Save(storage);
            }
        }

        /// <summary>
        /// Store filename and hash of file if hash is uniq or return name of preceding file with same hash
        /// </summary>
        /// <param name="item"></param>
        /// <returns>null if hash of file doesn't stored previously or retunn filename of previously stored identical hash</returns>
        public string? StoreIfUniq(StoreItem item)
        {
            if (storage.ContainsKey(item.hash))
            {
                return storage[item.hash];
            }
            else
            {
                storage.Add(item.hash, item.path);
                return null;
            }
        }

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {                    
                    SaveList();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~StoreFiledata()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion  
    }

    public class StoreItem
    {
        public string hash { get; private set; } 
        public string path { get; private set; }

        //

        public StoreItem(string filePath)
        {
            this.hash = GetHash(filePath);
            this.path = filePath;
        }

        private static string GetHash(string filePath)
        {
            using (var stream = new BufferedStream(File.OpenRead(filePath), 1024 * 1024))
            {
                using (var sha256Hash = SHA256.Create())
                {
                    byte[] hashBytes = sha256Hash.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower(); 
                }
            }
        }
    }    

    public interface IPermanentFileData
    {
        public SortedList<string, string> Load();
        public void Save(SortedList<string, string> data);
    }

    public class PermanentFileDataSimple : IPermanentFileData
    {
        private string?     savePermanentFile;
        private string[]?   loadPermanentFiles;

        public PermanentFileDataSimple(string? savePermanentFile, string[]? loadPermanentFiles) 
        {
            this.savePermanentFile  = savePermanentFile;
            this.loadPermanentFiles = loadPermanentFiles;
        }


        #region IPermanentFileData
        public SortedList<string, string> Load()
        {
            var ret = new SortedList<string, string>();

            if ((loadPermanentFiles is not null) && (loadPermanentFiles.Count() > 0))
            {
                foreach (var fileName in loadPermanentFiles)
                {
                    var lines = File.ReadAllLines(fileName);
      
                    foreach (var line in lines) 
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        var items = line.Split('\t');

                        Debug.Assert(items.Length == 2);

                        ret.Add(items[0], items[1]);    
                    }
                }
            }

            return ret;
        }

        public void Save(SortedList<string, string> data)
        {
            if (savePermanentFile is not null)
            {
                using (var writer = new StreamWriter(savePermanentFile, true, Encoding.UTF8))                          // append is exists
                {
                    foreach (var item in data)
                    {
                        writer.WriteLine(item.Key + '\t' + item.Value);
                    }
                }
            }
        }
        #endregion
    }
}
