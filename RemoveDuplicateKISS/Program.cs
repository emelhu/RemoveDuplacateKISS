using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using eMeL_CommandLineParams;
using RemoveDuplicateKISS;

//

Console.WriteLine($"==== RemoveDuplicateKISS ==== {Helper.GetVersion()} ==== (C) eMeL.hu [freeware] [www.emel.hu/utilities]\n\n");

var commandLineParams = new CommandLineParams("RemoveDuplicateKISS", "Remove duplicated files contained in path list.", Helper.GetVersion());

Option longerrmsgOption         = commandLineParams.AddOption("-lem|--longerrmsg", "Long error message on error.", 0, 0);
Option logDuplicatedItemsOption = commandLineParams.AddOption("-ldi|--logDuplicatedItems", "Logging duplicate items.", 0, 1);
Option recurseDirectoriesOption = commandLineParams.AddOption("-red|--recurseDirectories", "Recurse directories.", 0, 0);
Option fileDeleteEnableOption   = commandLineParams.AddOption("-fde|--fileDeleteEnable", "Enable delete duplicated files.", 0, 0);
Option verboseOption            = commandLineParams.AddOption("-verbose|--verbose", "Verbose messages from .", 0, 0);
Option savePermanentFileOption  = commandLineParams.AddOption("-spf|--savePermanentFile", "Save file's hash to permanent storage file.", 0, 1);
Option loadPermanentFileOption  = commandLineParams.AddOption("-lpf|--loadPermanentFile", "Load file's hash from permanent storage file.", 0, 100);


Argument pathsArgument = commandLineParams.AddArgument("paths", "Paths for scan files.", 1, 100);                                                       // Argument for Root/Main/Noname/empty command
pathsArgument.valueLimitationCode = ValueLimitationCode.ExistDirectoryName;

//

commandLineParams.AddCommand(new Command("CreateArgsFile|CAF", "Create an empty text file for 'args' strings.")).onExecute = () =>
{
    var fileName = CommandLineParams.GetArgsTextFileName(createFile: true);

    Console.WriteLine($"ARGS text file created: {fileName}");

    return (int)ExitCode.OK;
};

//

commandLineParams.onExecute = () =>
{   // When no commands are specified, this block will execute. This is the main "command"  --- warning: 'commandLineParams.dispHelpIfNoneCommand = true' is prevent execute this.

    if (pathsArgument.recognized)
    {
        foreach (var path in pathsArgument.values)
        {
            if (! Directory.Exists(path))
            {
                Console.WriteLine($"Directory is not exits! {path}");
                return (int)ExitCode.ParameterErr;
            }
        }
    }
    else
    {
        throw new CommandParsingException("The 'paths' argument is not found!");
    }

    string  filenameHelper      = Path.Combine(Environment.CurrentDirectory, "RemoveDuplicateKISS");
    string? logFile             = logDuplicatedItemsOption.recognized ? (logDuplicatedItemsOption.valuesCount > 0) ? logDuplicatedItemsOption.values[0] : filenameHelper + ".log" : null;
    string? savePermanentFile   = savePermanentFileOption.recognized ? (savePermanentFileOption.valuesCount > 0) ? savePermanentFileOption.values[0] : filenameHelper + ".store" : null;
    var     loadPermanentFiles  = new string[0];

    if (loadPermanentFileOption.recognized)
    {
        if (loadPermanentFileOption.valuesCount == 0)
        {
            loadPermanentFiles = new string[1];
            loadPermanentFiles[0] = filenameHelper + ".store";
        }
        else
        {
            loadPermanentFiles = loadPermanentFileOption.values.ToArray();
        }
    }

    foreach (var fileName in loadPermanentFiles )                                                                           // Check parameters are valid
    {
        if (! File.Exists(fileName))
        {
            Console.WriteLine($"Filename is not exits! {fileName}");
            return (int)ExitCode.ParameterErr;
        }
    }

    if (savePermanentFile is not null)                                                                                      // Check parameter is valid
    {
        //if (! Helper.IsValidFileName(savePermanentFile))
        //{
        //    Console.WriteLine($"Filename is not valid! {savePermanentFile} [invalid character]");
        //    return (int)ExitCode.ParameterErr;
        //}

        try
        {
            var fileInfo = new FileInfo(savePermanentFile);
        }
        catch (Exception e) 
        {
            Console.WriteLine($"Filename is not valid! {savePermanentFile} [{e.Message}]");
        }
    }

    PermanentFileDataSimple ? permanentFiledata = null;

    if ((savePermanentFile is not null) || (loadPermanentFiles.Count() > 0))
    {
        permanentFiledata = new PermanentFileDataSimple(savePermanentFile, loadPermanentFiles);
    }

    var rdkPars = new RdkParams(pathsArgument.values.ToList(), recurseDirectoriesOption.recognized, fileDeleteEnableOption.recognized, logFile, verboseOption.recognized);


    if ((logFile is not null) && File.Exists(logFile))
    {
        var saveName = Path.ChangeExtension(logFile, "---" + DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff") + Path.GetExtension(logFile));

        File.Move(logFile, saveName);
    }

    if ((savePermanentFile is not null) && File.Exists(savePermanentFile))
    {
        var saveName = Path.ChangeExtension(savePermanentFile, "---" + DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff") + Path.GetExtension(savePermanentFile));

        File.Move(savePermanentFile, saveName);
    }


    using (var logStream = (logFile is null) ? new StreamWriter(Stream.Null) : new StreamWriter(logFile, false, Encoding.UTF8))
    using (var storeFiledata = new StoreFiledata(permanentFiledata))
    {
        var logger = new LogItemsEngineSimple(logStream, Console.Out);
        var worker = new RemoveDuplicate(rdkPars, logger, storeFiledata);

        var task = Task.Run(() => worker.Run());
        Task.WaitAll(task);
    }    

    return (int)ExitCode.OK;
};

//

try
{
    commandLineParams.Run(args);
}
catch (CommandParsingException ex)                                                                              // https://github.com/anthonyreilly/ConsoleArgs/blob/master/Program.cs
{
    var errMsg = longerrmsgOption.recognized ? ex.ToString() : ex.Message;

    Console.WriteLine($"Command ERROR!\n\n{errMsg}");

    Environment.Exit((int)ExitCode.ParameterErr);
}
catch (Exception ex)
{
    var errMsg = longerrmsgOption.recognized ? ex.ToString() : ex.Message;

    Console.WriteLine($"Run ERROR!\n\n{errMsg}");

    Environment.Exit((int)ExitCode.Error);
}


Console.WriteLine("==== RemoveDuplicateKISS ====  end of run ====");

Environment.Exit((int)ExitCode.OK);  