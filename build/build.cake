var target = Argument("target", "Default");
var configuration = Argument("Configuration", "Release");
var softhsmVersion = Argument("SofthsmVersion", "2.5.0");

using System.IO.Compression;

//*****************************************************************************
// Constants

string artefacts = "./Artefacts";

// ****************************************************************************
var netCoreBuildSettings = new DotNetCoreBuildSettings()
{
    Configuration = configuration,
    NoDependencies = false,
    NoIncremental = true,
    NoRestore = false
};

var netCoreDotNetCoreTestSettings = new  DotNetCoreTestSettings()
{
    //TODO: lcov
    Configuration = configuration
};

var netCoreDotNetCorePackSettings = new  DotNetCorePackSettings ()
{
    Configuration = configuration,
    OutputDirectory = artefacts,
    IncludeSource = false,
    IncludeSymbols = false,
    NoBuild = false
};

// ****************************************************************************
// Tasks

Task("Clean")
     .Does(()=>
     {
          foreach(var projFile in GetFiles("../src/Src/*/*.csproj"))
          {
              var projDirectory = projFile.GetDirectory();
              Information($"Clear {projDirectory}");
              CleanDirectory(projDirectory + Directory("/obj"));
              CleanDirectory(projDirectory + Directory("/bin"));
          }
     
          foreach(var projFile in GetFiles("../src/Test/*/*.csproj"))
          {
              var projDirectory = projFile.GetDirectory();
              Information($"Clear {projDirectory}");
              CleanDirectory(projDirectory + Directory("/obj"));
              CleanDirectory(projDirectory + Directory("/bin"));
          }
     
          CleanDirectory(artefacts);
     });

Task("Build")
    .IsDependentOn("Clean")
    .Does(()=>
    {
        DotNetCoreBuild("../src/Src/SoftHSMv2ForTesting/SoftHSMv2ForTesting.csproj", netCoreBuildSettings);
        DotNetCoreBuild("../src/Test/SoftHSMv2ForTesting.MsTest/SoftHSMv2ForTesting.MsTest.csproj", netCoreBuildSettings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(()=>
    {
        DotNetCoreTest("../src/Test/SoftHSMv2ForTesting.MsTest/SoftHSMv2ForTesting.MsTest.csproj", netCoreDotNetCoreTestSettings);
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(()=>
    {
        DotNetCorePack("../src/Src/SoftHSMv2ForTesting/SoftHSMv2ForTesting.csproj", netCoreDotNetCorePackSettings);
    });

static void CompressGz(string inFile, string outFile)
{
    using (System.IO.FileStream rfs = new System.IO.FileStream(inFile, FileMode.Open, FileAccess.Read))
    {
        using (System.IO.FileStream wfs = new System.IO.FileStream(outFile, FileMode.Create, FileAccess.Write))
        {
            using (GZipStream gziped = new GZipStream(wfs, CompressionMode.Compress))
            {
                rfs.CopyTo(gziped);
            }
        }
    }
}

Task("DownloadSoftHsm")
    .IsDependentOn("Clean")
    .Does(()=>
    {
        string downloadUrl = $"https://github.com/disig/SoftHSM2-for-Windows/releases/download/v{softhsmVersion}/SoftHSM2-{softhsmVersion}-portable.zip";
        Verbose($"Starting download {downloadUrl}");
        var softhsmFile = DownloadFile(downloadUrl);
        Verbose("Downloading complete");
        Unzip(softhsmFile, "./publisTmp");
        CleanDirectory($"{artefacts}/Windows");

        CompressGz("./publisTmp/SoftHSM2/README.txt", $"{artefacts}/Windows/README.txt.gz");
        CompressGz("./publisTmp/SoftHSM2/lib/softhsm2.dll", $"{artefacts}/Windows/softhsm2.dll.gz");
        CompressGz("./publisTmp/SoftHSM2/lib/softhsm2-x64.dll", $"{artefacts}/Windows/softhsm2-x64.dll.gz");
        CompressGz("./publisTmp/SoftHSM2/bin/softhsm2-util.exe", $"{artefacts}/Windows/softhsm2-util.exe.gz");

        DeleteDirectory("./publisTmp", new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    });

Task("Default")
    .IsDependentOn("Pack");

//*****************************************************************************
// EXECUTION

RunTarget(target);