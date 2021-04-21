#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace com.iris.common
{
    public static class AssetTesterGameObject
    {
		[MenuItem("GameObject/IRIS/Experience Tester", false, 27)]
		private static void AddFB()
		{
			
			PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath("Packages/com.iris.common/Resources/Creation/ExperienceTester.prefab", typeof(GameObject)));
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
		}
		/*
		[MenuItem("GameObject/" + Util.Constants.ASSET_NAME + "/" + Util.Constants.FB_SCENE_OBJECT_NAME, true)]
		private static bool AddFBValidator()
		{
			return !EditorHelper.isFileBrowserInScene;
		}
		*/
	}
}
#endif
