using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BusinessAsUsual.Town.Editor
{
    public static class Phase8ABuilder
    {
        [MenuItem("Business as Usual/Phase 8A/Build Windows Living Workday")]
        public static void Build()
        {
            string path=Path.GetFullPath("../../Builds/Phase8A-Usability/BusinessAsUsual.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/Town/Scenes/DaylightTown.unity","Assets/Shopkeeper/Scenes/ShopkeeperShift.unity","Assets/Prototype/Scenes/TownPrototype.unity"},
                locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development });
            string evidence=Path.GetFullPath("../../Docs/Diagnostics/Phase8A/build.json");Directory.CreateDirectory(Path.GetDirectoryName(evidence));
            File.WriteAllText(evidence,JsonUtility.ToJson(new Summary {result=report.summary.result.ToString(),errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings,path=path},true));
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Phase8A build failed: "+report.summary.result);
            Debug.Log("Phase8A build: "+path);
        }
        [System.Serializable] class Summary {public string result,path;public int errors,warnings;}
    }
}
