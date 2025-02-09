using UnityEditor;
using UnityEngine;
using System.IO;

namespace GenerateMaskMap
{
    public class GenerateMaskMapTexture : EditorWindow
    {
        string filename = "MaskMap";
        Texture2D diffuseMap;
        Texture2D normalMap;
        Texture2D metallicMap;
        Texture2D roughnessMap;
        Texture2D ambientOcclusionMap;
        Texture2D maskMap;

        public float roughnessSliderValue = 0.5f; // Imposta un valore predefinito, puoi cambiarlo a tuo piacimento

        public enum MaskMapType
        {
            Metallic,
            Roughness
        }

        private MaskMapType selectedMaskMapType = MaskMapType.Metallic; // Imposta il valore predefinito a Metallic

        [MenuItem("Window/Mask Map Generator")]
        public static void ShowWindow()
        {
            GetWindow<GenerateMaskMapTexture>("Generate Mask Map");
        }

        private void OnGUI()
        {
            filename = EditorGUILayout.TextField("Texture Name", filename);
            GUILayout.Space(20);
            selectedMaskMapType = (MaskMapType)EditorGUILayout.EnumPopup("Mask Map Type", selectedMaskMapType);
            GUILayout.Space(20);
            GUILayout.Label("Select Input Maps", EditorStyles.boldLabel);
            GUILayout.Space(20);

            switch (selectedMaskMapType)
            {
                case MaskMapType.Metallic:
                    // Diffuse Map
                    diffuseMap = (Texture2D)EditorGUILayout.ObjectField("Diffuse Map", diffuseMap, typeof(Texture2D), false);

                    // Normal Map
                    normalMap = (Texture2D)EditorGUILayout.ObjectField("Normal Map", normalMap, typeof(Texture2D), false);

                    // Metallic Map
                    metallicMap = (Texture2D)EditorGUILayout.ObjectField("Metallic Map", metallicMap, typeof(Texture2D), false);

                    // Ambient Occlusion Map
                    ambientOcclusionMap = (Texture2D)EditorGUILayout.ObjectField("Ambient Occlusion Map", ambientOcclusionMap, typeof(Texture2D), false);
                    break;
                case MaskMapType.Roughness:
                    // Diffuse Map
                    diffuseMap = (Texture2D)EditorGUILayout.ObjectField("Diffuse Map", diffuseMap, typeof(Texture2D), false);

                    // Normal Map
                    normalMap = (Texture2D)EditorGUILayout.ObjectField("Normal Map", normalMap, typeof(Texture2D), false);

                    // Roughness Map
                    roughnessMap = (Texture2D)EditorGUILayout.ObjectField("Roughness Map", roughnessMap, typeof(Texture2D), false);

                    GUILayout.Space(10);

                    roughnessSliderValue = EditorGUILayout.Slider("Roughness Value", roughnessSliderValue, 0.001f, 0.999f);

                    // Ambient Occlusion Map
                    ambientOcclusionMap = (Texture2D)EditorGUILayout.ObjectField("Ambient Occlusion Map", ambientOcclusionMap, typeof(Texture2D), false);
                    break;
                default:
                    // Diffuse Map
                    diffuseMap = (Texture2D)EditorGUILayout.ObjectField("Diffuse Map", diffuseMap, typeof(Texture2D), false);

                    // Normal Map
                    normalMap = (Texture2D)EditorGUILayout.ObjectField("Normal Map", normalMap, typeof(Texture2D), false);

                    // Metallic Map
                    metallicMap = (Texture2D)EditorGUILayout.ObjectField("Metallic Map", metallicMap, typeof(Texture2D), false);

                    // Ambient Occlusion Map
                    ambientOcclusionMap = (Texture2D)EditorGUILayout.ObjectField("Ambient Occlusion Map", ambientOcclusionMap, typeof(Texture2D), false);
                    break;
            }

            GUILayout.Space(20);

            // Generate Mask Map button
            if (GUILayout.Button("Generate Mask Map"))
            {
                if (selectedMaskMapType == MaskMapType.Metallic)
                {
                    GenerateFromMetallic();
                }
                else if (selectedMaskMapType == MaskMapType.Roughness)
                {
                    GenerateFromRoughness(roughnessSliderValue);
                }
            }

            GUILayout.Space(20);

            GUILayout.Label("Generated Mask Map", EditorStyles.boldLabel);

            GUILayout.Space(20);

            // Mostra il pulsante "Save" solo se la Mask Map è stata generata correttamente
            if (GUILayout.Button("Save Mask Map"))
            {
                SaveMaskMap();
            }

            GUILayout.Space(20);

            GUILayout.Label("Save Mask Map", EditorStyles.boldLabel);


        }

        private void GenerateFromMetallic()
        {
            if (diffuseMap == null || normalMap == null || metallicMap == null || ambientOcclusionMap == null)
            {
                Debug.LogError("Please select all input maps!");
                return;
            }

            // Check if the dimensions of all maps are the same
            int width = diffuseMap.width;
            int height = diffuseMap.height;

            if (width != normalMap.width || width != metallicMap.width || width != ambientOcclusionMap.width ||
                height != normalMap.height || height != metallicMap.height || height != ambientOcclusionMap.height)
            {
                Debug.LogError("All input maps must have the same dimensions!");
                return;
            }

            try
            {
                // Create the Mask Map texture
                maskMap = new Texture2D(width, height, TextureFormat.ARGB32, false);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Get pixel values from each map
                        Color normalColor = normalMap.GetPixel(x, y);
                        Color metallicColor = metallicMap.GetPixel(x, y);
                        Color ambientOcclusionColor = ambientOcclusionMap.GetPixel(x, y);

                        // Extract specific information from each map (You may need to adjust these based on your specific maps)
                        float metallicValue = metallicColor.r;
                        float ambientOcclusionValue = ambientOcclusionColor.g;
                        float smoothnessValue = 1.0f - (normalColor.g + normalColor.b) / 2.0f;

                        // Combine the information to create the Mask Map pixel color
                        Color maskColor = new Color(metallicValue, ambientOcclusionValue, smoothnessValue, 1.0f);

                        // Set the pixel color in the Mask Map
                        maskMap.SetPixel(x, y, maskColor);
                    }
                }

                maskMap.Apply();

                Debug.Log("Mask Map generated successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error generating Mask Map: " + e);
            }
        }

        private void GenerateFromRoughness(float roughness)
        {
            if (diffuseMap == null || normalMap == null || roughnessMap == null || ambientOcclusionMap == null)
            {
                Debug.LogError("Please select all input maps!");
                return;
            }

            // Check if the dimensions of all maps are the same
            int width = diffuseMap.width;
            int height = diffuseMap.height;

            if (width != normalMap.width || width != roughnessMap.width || width != ambientOcclusionMap.width ||
                height != normalMap.height || height != roughnessMap.height || height != ambientOcclusionMap.height)
            {
                Debug.LogError("All input maps must have the same dimensions!");
                return;
            }

            try
            {
                // Create the Mask Map texture
                maskMap = new Texture2D(width, height, TextureFormat.ARGB32, false);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Get pixel values from each map
                        Color normalColor = normalMap.GetPixel(x, y);
                        Color roughnessColor = roughnessMap.GetPixel(x, y);
                        Color ambientOcclusionColor = ambientOcclusionMap.GetPixel(x, y);

                        // Extract specific information from each map (You may need to adjust these based on your specific maps)
                        float roughnessValue = roughnessColor.r;
                        float ambientOcclusionValue = ambientOcclusionColor.g;
                        float smoothnessValue = roughness;

                        // Combine the information to create the Mask Map pixel color
                        Color maskColor = new Color(roughnessValue, ambientOcclusionValue, smoothnessValue, 1.0f);

                        // Set the pixel color in the Mask Map
                        maskMap.SetPixel(x, y, maskColor);
                    }
                }

                maskMap.Apply();

                Debug.Log("Mask Map generated successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error generating Mask Map: " + e);
            }
        }

        private void SaveMaskMap()
        {
            if (maskMap == null)
            {
                Debug.LogError("No Mask Map generated!");
                return;
            }

            string folderPath = "Assets/MaskMapGenerate/MaskMaps"; // Cartella in cui salvare la Mask Map

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/MaskMapGenerate", "MaskMaps");
            }

            string fileName = filename + ".png";
            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
            {
                // Il file esiste già, chiedi all'utente se vuole sovrascriverlo
                bool overwrite = EditorUtility.DisplayDialog("File Already Exists",
                    "The file already exists. Do you want to overwrite it?", "Yes", "No");

                if (!overwrite)
                {
                    return;
                }
            }

            byte[] bytes = maskMap.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);

            AssetDatabase.Refresh();

            Debug.Log("Mask Map saved to: " + filePath);
        }
    }
}
