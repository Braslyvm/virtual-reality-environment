using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class ConfigurarLamparaRealista
{
    private class DatosMaterial
    {
        public string nombre;
        public Color color;
        public float metallic;
        public float smoothness;
        public bool emission;
        public Color emissionColor;

        public DatosMaterial(
            string nombre,
            string hexColor,
            float metallic,
            float smoothness,
            bool emission = false,
            string hexEmission = "#000000")
        {
            this.nombre = nombre;
            ColorUtility.TryParseHtmlString(hexColor, out color);
            this.metallic = metallic;
            this.smoothness = smoothness;
            this.emission = emission;
            ColorUtility.TryParseHtmlString(hexEmission, out emissionColor);
        }
    }

    private static readonly List<DatosMaterial> materiales =
        new List<DatosMaterial>()
        {
            new DatosMaterial(
                "Lampara_Color_MaderaPrincipal",
                "#8C6339",
                0.0f,
                0.28f
            ),

            new DatosMaterial(
                "Lampara_Color_MaderaSecundaria",
                "#654425",
                0.0f,
                0.24f
            ),

            new DatosMaterial(
                "Lampara_Color_MetalGrafito",
                "#2E3134",
                0.78f,
                0.68f
            ),

            new DatosMaterial(
                "Lampara_Color_Bombillo",
                "#FFF2D7",
                0.0f,
                0.78f,
                true,
                "#FFD89A"
            )
        };

    [MenuItem("Tools/Lampara/Aplicar materiales realistas y luz")]
    public static void AplicarMaterialesYLuz()
    {
        Dictionary<string, Material> materialesEncontrados =
            new Dictionary<string, Material>();

        foreach (DatosMaterial datos in materiales)
        {
            Material mat = BuscarMaterial(datos.nombre);

            if (mat == null)
            {
                Debug.LogWarning("No se encontró el material: " + datos.nombre);
                continue;
            }

            ConfigurarMaterial(mat, datos);
            materialesEncontrados[datos.nombre] = mat;

            Debug.Log("Material configurado: " + datos.nombre);
        }

        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Selecciona la lámpara en la Hierarchy.");
            return;
        }

        GameObject raiz = Selection.activeGameObject;
        Renderer[] renderers = raiz.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] mats = renderer.sharedMaterials;
            bool huboCambios = false;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;

                string nombre = LimpiarNombre(mats[i].name);

                if (materialesEncontrados.ContainsKey(nombre))
                {
                    mats[i] = materialesEncontrados[nombre];
                    huboCambios = true;
                }
            }

            if (huboCambios)
            {
                Undo.RecordObject(renderer, "Reasignar materiales lámpara");
                renderer.sharedMaterials = mats;
                EditorUtility.SetDirty(renderer);
            }
        }

        CrearOLocalizarLuz(raiz, materialesEncontrados);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Lámpara configurada correctamente.");
    }

    [MenuItem("Tools/Lampara/Solo actualizar materiales")]
    public static void SoloActualizarMateriales()
    {
        foreach (DatosMaterial datos in materiales)
        {
            Material mat = BuscarMaterial(datos.nombre);

            if (mat == null)
            {
                Debug.LogWarning("No se encontró el material: " + datos.nombre);
                continue;
            }

            ConfigurarMaterial(mat, datos);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Materiales de lámpara actualizados.");
    }

    private static Material BuscarMaterial(string nombre)
    {
        string[] guids = AssetDatabase.FindAssets(nombre + " t:Material");

        foreach (string guid in guids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);

            if (mat != null && mat.name == nombre)
                return mat;
        }

        return null;
    }

    private static void ConfigurarMaterial(Material mat, DatosMaterial datos)
    {
        Undo.RecordObject(mat, "Configurar material lámpara");

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", datos.color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", datos.color);

        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", datos.metallic);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", datos.smoothness);

        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", datos.smoothness);

        if (mat.HasProperty("_BaseMap"))
            mat.SetTexture("_BaseMap", null);

        if (mat.HasProperty("_MainTex"))
            mat.SetTexture("_MainTex", null);

        if (datos.emission)
        {
            mat.EnableKeyword("_EMISSION");

            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", datos.emissionColor * 1.8f);

            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", Color.black);
        }

        EditorUtility.SetDirty(mat);
    }

    private static string LimpiarNombre(string nombre)
    {
        return nombre.Replace(" (Instance)", "").Trim();
    }

    private static void CrearOLocalizarLuz(GameObject raiz, Dictionary<string, Material> matsEncontrados)
    {
        Renderer mejorRenderer = null;
        Bounds? mejorBounds = null;

        Renderer[] renderers = raiz.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat == null) continue;

                string nombre = LimpiarNombre(mat.name);

                if (nombre == "Lampara_Color_Bombillo")
                {
                    mejorRenderer = renderer;
                    mejorBounds = renderer.bounds;
                    break;
                }
            }

            if (mejorRenderer != null) break;
        }

        Vector3 posicion;

        if (mejorBounds.HasValue)
        {
            posicion = mejorBounds.Value.center;
        }
        else
        {
            // Fallback: usar el renderer más alto
            float maxY = float.MinValue;

            foreach (Renderer renderer in renderers)
            {
                if (renderer.bounds.center.y > maxY)
                {
                    maxY = renderer.bounds.center.y;
                    posicion = renderer.bounds.center;
                    mejorRenderer = renderer;
                    mejorBounds = renderer.bounds;
                }
            }

            if (mejorBounds.HasValue)
                posicion = mejorBounds.Value.center;
            else
                posicion = raiz.transform.position;
        }

        Transform luzExistente = raiz.transform.Find("Lampara_Bombillo_Light");

        GameObject luzObj;

        if (luzExistente != null)
        {
            luzObj = luzExistente.gameObject;
        }
        else
        {
            luzObj = new GameObject("Lampara_Bombillo_Light");
            Undo.RegisterCreatedObjectUndo(luzObj, "Crear luz lámpara");
            luzObj.transform.SetParent(raiz.transform);
        }

        luzObj.transform.position = posicion;

        Light light = luzObj.GetComponent<Light>();
        if (light == null)
            light = luzObj.AddComponent<Light>();

        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.92f, 0.76f);
        light.range = 2.5f;
        light.intensity = 3.0f;
        light.shadows = LightShadows.Soft;

#if UNITY_2020_1_OR_NEWER
        light.useColorTemperature = true;
        light.colorTemperature = 2700f;
#endif

        EditorUtility.SetDirty(luzObj);
    }
}