//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var environment = Argument<string>("environment", "Local");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var source = Directory("./src");
var buildDir = source + Directory("TwitterApi/bin") + Directory(configuration);
var buildDirCore = source + Directory("TwitterApiCore/bin") + Directory(configuration);
var solution = "TwitterApi.sln";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(buildDir);
        CleanDirectory(buildDirCore);
    });

Task("Restore-NuGet-Package")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore($"./{solution}");
    });

Task("Push-NuGet-Package")
    .IsDependentOn("Build")
    .Does(() => 
    {
        if (environment != "Local" && environment != "Remote")
        {
            Warning("Unknown environment. Skipping nuget push.");
            return;
        }

        foreach (var packageFile in GetFiles(source.ToString() + "/**/*.nupkg"))
        {
            Information($"FullPath: {packageFile.FullPath}");
            if (environment == "Local")
            {
                NuGetPush(packageFile, new NuGetPushSettings { Source = @"c:\projects\local-nuget" });
            }
            else
            {
                NuGetPush(packageFile, new NuGetPushSettings { Source = "https://pelism.pkgs.visualstudio.com/_packaging/PelismFeed/nuget/v3/index.json", ApiKey = "VSTS" });
            }
        }
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Package")
    .Does(() =>
    {
	   MSBuild("./TwitterApi.sln", settings => settings.SetConfiguration(configuration));
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Push-NuGet-Package")
	.Does(() =>
    {
	   Information("Done");
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information($"Using environment {environment}");
});

RunTarget(target);