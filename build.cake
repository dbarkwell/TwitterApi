//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var environment = EnvironmentVariable("build.environment") ?? "Local";
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
        foreach (var packageFile in GetFiles(source.ToString() + "/**/*.nupkg"))
        {
            Information($"FullPath: {packageFile.FullPath}");
            if (environment == "Local")
            {
                NuGetPush(packageFile, new NuGetPushSettings { Source = @"c:\projects\local-nuget" });
            }
            else
            {
                NuGetPush(packageFile, new NuGetPushSettings { Source = "PelismFeed", ApiKey = "VSTS" });
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

RunTarget(target);