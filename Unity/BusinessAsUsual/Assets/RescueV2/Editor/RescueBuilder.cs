using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace BusinessAsUsual.RescueV2.Editor
{
    public static class RescueBuilder
    {
        const string Base="Assets/RescueV2/Resources/RescueV2";
        [MenuItem("Business as Usual/Rescue V2/Prepare UI assets")]
        public static void PrepareUI()
        {
            if(!TMP_Settings.instance){var package=UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_FontAsset).Assembly);AssetDatabase.ImportPackage(package.resolvedPath+"/Package Resources/TMP Essential Resources.unitypackage",false);EditorApplication.delayCall+=PrepareUI;return;}
            Directory.CreateDirectory(Base+"/Fonts");
            foreach(var file in Directory.GetFiles(Base+"/Icons","*.png")){var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));if(!importer)continue;importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.spritePixelsPerUnit=100;importer.SaveAndReimport();}
            var panel=(TextureImporter)AssetImporter.GetAtPath(Base+"/Panel.png");if(panel){panel.textureType=TextureImporterType.Sprite;panel.spriteImportMode=SpriteImportMode.Single;panel.spriteBorder=new Vector4(12,12,12,12);panel.mipmapEnabled=false;panel.alphaIsTransparency=true;panel.textureCompression=TextureImporterCompression.Uncompressed;panel.SaveAndReimport();}
            var skin=AssetDatabase.LoadAssetAtPath<RescueSkin>(Base+"/WorkdaySkin.asset");if(!skin){skin=ScriptableObject.CreateInstance<RescueSkin>();AssetDatabase.CreateAsset(skin,Base+"/WorkdaySkin.asset");}
            skin.display=Font("BricolageGrotesque/BricolageGrotesque96pt-ExtraBold.ttf","Display");
            skin.labels=Font("BricolageGrotesque/BricolageGrotesque12pt-Bold.ttf","Labels");
            skin.reading=Font("AtkinsonHyperlegible/AtkinsonHyperlegible-Regular.ttf","Reading");
            skin.readingBold=Font("AtkinsonHyperlegible/AtkinsonHyperlegible-Bold.ttf","ReadingBold");
            skin.comparisonHeadings=new[]{Font("DelaGothicOne/DelaGothicOne-Regular.ttf","CompareDela"),Font("Fredoka/Fredoka-Bold.ttf","CompareFredoka")};EditorUtility.SetDirty(skin);
            Directory.CreateDirectory(Base+"/Components");
            var panelPrefab=new GameObject("PaperPanel",typeof(RectTransform),typeof(Image));var panelImage=panelPrefab.GetComponent<Image>();panelImage.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Base+"/Panel.png");panelImage.type=Image.Type.Sliced;panelImage.color=skin.paper;panelImage.raycastTarget=false;PrefabUtility.SaveAsPrefabAsset(panelPrefab,Base+"/Components/PaperPanel.prefab");UnityEngine.Object.DestroyImmediate(panelPrefab);
            var labelPrefab=new GameObject("ReadingText",typeof(RectTransform),typeof(TextMeshProUGUI));var label=labelPrefab.GetComponent<TextMeshProUGUI>();label.font=skin.reading;label.fontSize=18;label.color=skin.ink;label.raycastTarget=false;PrefabUtility.SaveAsPrefabAsset(labelPrefab,Base+"/Components/ReadingText.prefab");UnityEngine.Object.DestroyImmediate(labelPrefab);
            var go=new GameObject("WorkdayCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(RescueInterface));
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=80;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;go.GetComponent<RescueInterface>().skin=skin;
            PrefabUtility.SaveAsPrefabAsset(go,Base+"/WorkdayCanvas.prefab");UnityEngine.Object.DestroyImmediate(go);AssetDatabase.SaveAssets();
            Debug.Log("Rescue UI assets prepared: six real SDF fonts, licensed sources, shared skin and canvas prefab.");
        }
        static TMP_FontAsset Font(string source,string name)
        {
            string path=Base+"/Fonts/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);if(existing)return existing;
            var font=AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/RescueV2/Fonts/"+source);if(!font)throw new Exception("Missing font "+source);
            var asset=TMP_FontAsset.CreateFontAsset(font,90,9,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);asset.name=name;
            AssetDatabase.CreateAsset(asset,path);AssetDatabase.AddObjectToAsset(asset.material,asset);
            foreach(var atlas in asset.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,asset);
            string characters=new string(Enumerable.Range(32,95).Select(n=>(char)n).ToArray())+"×·→‹›◇●—–’…";asset.TryAddCharacters(characters,out string missing);if(missing.Length>0)Debug.LogWarning(name+" missing optional characters: "+missing);
            EditorUtility.SetDirty(asset);return asset;
        }
        [MenuItem("Business as Usual/Rescue V2/Build Windows")]
        public static void Build()
        {
            if(EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed)throw new Exception("Editor scripts must compile successfully before building Rescue V2.");
            PrepareUI();RescueArtBuilder.PrepareGoldenPrint();var path=Path.GetFullPath("../../Builds/Phase8A-RescueV2/BusinessAsUsual.exe");Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Town/Scenes/DaylightTown.unity","Assets/Shopkeeper/Scenes/ShopkeeperShift.unity","Assets/Prototype/Scenes/TownPrototype.unity"},target=BuildTarget.StandaloneWindows64,locationPathName=path,options=BuildOptions.Development});
            string output=Path.GetFullPath("../../Docs/Diagnostics/Phase8ARescueV2/build.json");Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(new BuildEvidence{result=report.summary.result.ToString(),errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings,path=path},true));
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Rescue build "+report.summary.result);
        }
        [Serializable]class BuildEvidence{public string result,path;public int errors,warnings;}
    }
}
