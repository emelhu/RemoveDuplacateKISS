using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RemoveDuplicateKISS
{
    public class RemoveDuplicate
    {
        private RdkParams       parameters;
        private ILogItemsEngine logger;
        private IStoreFileData storeFileData;

        public RemoveDuplicate(RdkParams par, ILogItemsEngine logger, IStoreFileData storeFileData)
        {
            this.parameters     = par;
            this.logger         = logger;
            this.storeFileData  = storeFileData;
        }

        public async Task Run()
        {
            foreach (var dirName in parameters.dirs)
            {
                await ProcessDir(dirName, 1);
            }
        }

        private async Task ProcessDir(string dirName, int level)
        {
            string preTextDir  = new String('»', level);
            string preTextFile = new String(' ', level);

            dirName = Path.GetFullPath(dirName);

            await logger.DirectoryProcess($"{preTextDir} {dirName}");

            foreach (string fileName in Directory.GetFiles(dirName))
            {
                if (parameters.verbose)
                { 
                    await logger.FileProcess(preTextFile + ' ' + Path.GetFileName(fileName));
                }

                try
                {
                    var storeItem = new StoreItem(fileName);

                    var stayFilename = storeFileData.StoreIfUniq(storeItem);

                    if (stayFilename is not null)
                    {   // Duplicated file
                        string operationText = String.Empty;

                        if (File.Exists(stayFilename))
                        {
                            if (parameters.delete)
                            {
                                operationText = "deleted";

                                try
                                {
                                    File.Delete(fileName);                                              // remove
                                }
                                catch (Exception ex) 
                                {
                                    operationText = "delete error: " + ex.Message;
                                }
                            }
                            else
                            {
                                operationText = "duplicated";
                            }
                        }
                        else
                        {
                            operationText = "original file not found!";
                        }

                        if (! parameters.verbose)
                        {
                            await logger.FileProcess(preTextFile + ' ' + Path.GetFileName(fileName));
                        }
                        await logger.FileProcess(preTextFile + "> " + operationText);
                        await logger.FileDuplicated(storeItem, operationText, stayFilename);
                    }
                }
                catch (Exception ex) 
                {
                    await logger.FileProcess(preTextFile + "! " + ex.Message);
                    continue;
                }
            }

            if (parameters.recurse)
            {
                foreach (string directory in Directory.GetDirectories(dirName))
                {
                    await ProcessDir(directory, level + 1);
                }
            }
        }
    }
}
