#r "packages/FAKE/tools/FakeLib.dll"
#load "packages/SourceLink.Fake/tools/SourceLink.fsx"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open SourceLink
 
let versionAssembly = "1.0.2.0"
let versionFile = "1.0.2.1"
 
Target "Clean" (fun _ -> !! "**/bin/" ++ "**/obj/" |> CleanDirs)
 
Target "AssemblyInfo" (fun _ ->
    CreateCSharpAssemblyInfo "AssemblyInfoCommon.cs"
        [   Attribute.Version versionAssembly
            Attribute.FileVersion versionFile ]
)
 
Target "Build" (fun _ ->
    !! "Nuget.Debug.Test/Nuget.Debug.Test.csproj"
    |> MSBuildRelease "" "Rebuild" |> ignore
)

Target "SourceLink" (fun _ ->
    use repo = new GitRepo(__SOURCE_DIRECTORY__)
    [ "Nuget.Debug.Test/Nuget.Debug.Test.csproj" ]
    |> Seq.iter (fun pf ->
        let proj = VsProj.LoadRelease pf
        logfn "source linking %s" proj.OutputFilePdb
        let files = (proj.Compiles -- "SolutionInfo.cs").SetBaseDirectory __SOURCE_DIRECTORY__
        repo.VerifyChecksums files
        proj.VerifyPdbChecksums files
        proj.CreateSrcSrv "https://raw.githubusercontent.com/susl/sourcelink-test/{0}/%var2%" repo.Commit (repo.Paths files)
        Pdbstr.exec proj.OutputFilePdb proj.OutputFilePdbSrcSrv
    )
)
 
Target "NuGet" (fun _ ->
    let bin = "../bin"
    Directory.CreateDirectory bin |> ignore
    NuGet (fun p -> 
        { p with
            Version = versionFile
            WorkingDir = "Nuget.Debug.Test/bin/Release"
            OutputPath = bin
        }) "Nuget.Debug.Test/Nuget.Debug.Test.nuspec"
)
 
"Clean"
//    ==> "AssemblyInfo"
    ==> "Build"
    ==> "SourceLink"
    ==> "NuGet"
 
RunTargetOrDefault "NuGet"
