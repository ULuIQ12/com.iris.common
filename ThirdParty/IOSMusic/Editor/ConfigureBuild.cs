using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Collections;
using System.IO;

namespace com.hodges.iosmusic
{
	public class ConfigureBuild : MonoBehaviour
	{

		internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
		{
			if (Directory.Exists(dstPath))
				Directory.Delete(dstPath);
			if (File.Exists(dstPath))
				File.Delete(dstPath);

			Directory.CreateDirectory(dstPath);

			foreach (var file in Directory.GetFiles(srcPath))
				File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));

			foreach (var dir in Directory.GetDirectories(srcPath))
				CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
		}

		[PostProcessBuild]
		public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
		{
			if (buildTarget == BuildTarget.iOS)
			{
				string projPath = PBXProject.GetPBXProjectPath(path);
				PBXProject proj = new PBXProject();
				proj.ReadFromString(File.ReadAllText(projPath));

				// old method, for Unity 2019.2 and below
				//string target = proj.TargetGuidByName("Unity-iPhone");

				// new method
				string target = proj.GetUnityFrameworkTargetGuid();

				proj.AddFrameworkToProject(target, "StoreKit.framework", false);
				proj.AddFrameworkToProject(target, "MediaPlayer.framework", false);
				File.WriteAllText(projPath, proj.WriteToString());

				/* Info.plist */

				// Read the Info.plist file
				string plistPath = path + "/Info.plist";
				PlistDocument plist = new PlistDocument();
				plist.ReadFromString(File.ReadAllText(plistPath));

				// Get root of plist
				PlistElementDict rootDict = plist.root;

				// add the Apple Music privacy prompt entry
				rootDict.SetString("NSAppleMusicUsageDescription", "iOS Music would like to access this device's media library to play music.");

				//Write out the Info.plist file
				plist.WriteToFile(plistPath);
			}
		}
	}
}