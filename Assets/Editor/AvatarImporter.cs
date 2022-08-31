using System;
using System.IO;
using GLTFast;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YouTubeACCT
{
    public class AvatarImporter: EditorWindow
    {
        const string k_Title = "YT-ACCT Avatar Importer";
        const string k_VisualTreeFile = "Assets/Editor/AvatarImporter.uxml";
        const string k_StyleSheetFile = "Assets/Editor/AvatarImporter.uss";
        const string k_URLLabelName = "URL";
        const string k_PreviewBtnName = "PreviewBtn";
    
        TextField m_URLTextField;
        Button m_PreviewBtn;
        GltfAsset m_GLTFAsset;

        [MenuItem("Window/YouTube ACCT/Avatar Importer")]
        public static void ShowAvatarImporter() =>
            GetWindow<AvatarImporter>().titleContent = new GUIContent(k_Title);

        public void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_VisualTreeFile).Instantiate();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetFile);
            rootVisualElement.Add(visualTree);
            rootVisualElement.styleSheets.Add(styleSheet);

            m_URLTextField = visualTree.Q<TextField>(k_URLLabelName);
            m_PreviewBtn = visualTree.Q<Button>(k_PreviewBtnName);
            m_URLTextField.RegisterCallback<ChangeEvent<string>>(ev => m_GLTFAsset.url = ev.newValue);
            m_PreviewBtn.clicked += PreviewModel;

            OnSelectionChange();
        }

        async void PreviewModel()
        {
            if(m_GLTFAsset==null)
                return;
            
            var filePath = m_GLTFAsset.FullUrl;
            var data = await File.ReadAllBytesAsync(filePath);
            var gltfImporter = new GltfImport();
            var success = await gltfImporter.LoadGltfBinary(data, new Uri(filePath));
            if (success) {
                success = gltfImporter.InstantiateMainScene((Transform)null);
            }
            Debug.Log("Loading GTLF model "+(success?"succeeded":"failed"));
        }

        void OnSelectionChange()
        {
            var selection = Selection.activeObject as GameObject;
            bool valid = false;
            
            if (selection != null)
            {
                m_GLTFAsset = selection.GetComponent<GltfAsset>();
                valid |= m_GLTFAsset != null;
                OnFocus();
            }

            rootVisualElement.visible = valid;
        }

        void OnFocus()  // Just in case the user modified the URL via the default inspector
        {
            if (m_URLTextField != null && m_GLTFAsset != null)
                m_URLTextField.SetValueWithoutNotify(m_GLTFAsset.url);
        }
    }
}
