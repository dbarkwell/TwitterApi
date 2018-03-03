//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var source = Directory("./src");
var buildDir = source + Directory("TwitterApi/bin") + Directory(configuration);
var buildDirCore = source + Directory("TwitterApiCore/bin") + Directory(configuration);
var solution = "TwitterApi.sln";
var isVSTS = TFBuild.IsRunningOnVSTS || TFBuild.IsRunningOnTFS;
var userName = "dbarkwell@hotmail.com";

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

Task("Push-NuGet-Packages")
    .WithCriteria(isVSTS)
    .Does(() => 
    {
        foreach (var packageFile in GetFiles(buildDir.ToString() + "/**/*.nupkg"))
        {
            Information($"FullPath: {packageFile.FullPath}");
           
            NuGetPush(packageFile, new NuGetPushSettings 
            { 
                Source = "PelismFeed", 
                ApiKey = "VSTS",
                Verbosity = NuGetVerbosity.Detailed
            });
        }
    });

Task("Push-NuGet-Local")
    .WithCriteria(!isVSTS)
    .Does(() => 
    {
        foreach (var packageFile in GetFiles(buildDir.ToString() + "/**/*.nupkg"))
        {
            Information($"FullPath: {packageFile.FullPath}");
            NuGetPush(packageFile, new NuGetPushSettings { Source = @"c:\projects\local-nuget" });
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
    .IsDependentOn("Build")
    .IsDependentOn("Push-NuGet-Packages")
    .IsDependentOn("Push-Nuget-Local")
    .Does(() =>
    {
        Information("Done");
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information($"Running Local: {!isVSTS}");
});

RunTarget(target);