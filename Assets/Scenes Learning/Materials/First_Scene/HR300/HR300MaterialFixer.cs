using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>
/// HR300MaterialFixerPro
/// ---------------------
/// Ajusta los materiales extraídos del FBX del Mitutoyo HR-300 para URP/Lit.
///
/// Mejoras frente a la primera versión:
/// - Materiales físicamente más creíbles.
/// - Acero y acero oscuro con metallic/smoothness específicos.
/// - Esmalte blanco con Clear Coat cuando URP lo soporta.
/// - Plásticos, caucho, grafito, bronce y LCD tratados por separado.
/// - Emisión HDR para la pantalla.
/// - Fuerza Specular Highlights y Environment Reflections activas.
/// - Limpia keywords incompatibles.
/// - Incluye una herramienta OPCIONAL para crear/refrescar un Reflection Probe,
///   importante para que los metales se vean realmente metálicos.
///
/// INSTALACIÓN:
/// Assets/Editor/HR300MaterialFixerPro.cs
///
/// MENÚ:
/// Tools > HR300 > 1. Aplicar materiales PRO
/// Tools > HR300 > 2. Crear/Actualizar Reflection Probe
/// </summary>
public static class HR300MaterialFixerPro
{
    // -------------------------------------------------------------------------
    // PRESET DE MATERIAL
    // -------------------------------------------------------------------------
    private class Preset
    {
        public Color color;
        public float metallic;
        public float smoothness;

        public float clearCoat;
        public float clearCoatSmoothness;

        public bool emission;
        public Color emissionColor;

        public bool doubleSidedGI;

        public Preset(
            Color color,
            float metallic,
            float smoothness,
            float clearCoat = 0f,
            float clearCoatSmoothness = 0.7f,
            bool emission = false,
            Color emissionColor = default(Color),
            bool doubleSidedGI = false)
        {
            this.color = color;
            this.metallic = metallic;
            this.smoothness = smoothness;
            this.clearCoat = clearCoat;
            this.clearCoatSmoothness = clearCoatSmoothness;
            this.emission = emission;
            this.emissionColor = emissionColor;
            this.doubleSidedGI = doubleSidedGI;
        }
    }

    // -------------------------------------------------------------------------
    // COLORES / PROPIEDADES AJUSTADAS PARA UNITY URP
    //
    // Nota:
    // En un metal real, el "color" por sí solo NO produce el aspecto metálico.
    // El resultado depende mucho de Metallic + Smoothness + reflexiones del entorno.
    // -------------------------------------------------------------------------
    private static readonly Dictionary<string, Preset> Materials =
        new Dictionary<string, Preset>()
    {
        // Pintura principal de la máquina:
        // esmalte blanco ligeramente cálido, con una capa clara suave.
        {
            "HR300_Blanco",
            new Preset(
                new Color(0.775f, 0.778f, 0.755f, 1f),
                metallic: 0.00f,
                smoothness: 0.52f,
                clearCoat: 0.18f,
                clearCoatSmoothness: 0.76f)
        },

        // Gris claro pintado / piezas secundarias.
        {
            "HR300_BlancoGris",
            new Preset(
                new Color(0.620f, 0.628f, 0.612f, 1f),
                metallic: 0.00f,
                smoothness: 0.40f,
                clearCoat: 0.08f,
                clearCoatSmoothness: 0.68f)
        },

        // Negro plástico.
        {
            "HR300_Negro",
            new Preset(
                new Color(0.012f, 0.013f, 0.016f, 1f),
                metallic: 0.03f,
                smoothness: 0.34f)
        },

        // Plástico/antracita oscuro del panel y mandos.
        {
            "HR300_Antracita",
            new Preset(
                new Color(0.040f, 0.043f, 0.050f, 1f),
                metallic: 0.08f,
                smoothness: 0.38f)
        },

        // Grafito: algo más reflectante que el plástico, sin parecer acero cromado.
        {
            "HR300_Grafito",
            new Preset(
                new Color(0.085f, 0.092f, 0.105f, 1f),
                metallic: 0.42f,
                smoothness: 0.46f)
        },

        // Caucho negro mate.
        {
            "HR300_Caucho",
            new Preset(
                new Color(0.010f, 0.011f, 0.013f, 1f),
                metallic: 0.00f,
                smoothness: 0.12f)
        },

        // ACERO PRINCIPAL.
        //
        // Metallic alto + smoothness moderado:
        // evita que parezca "plástico gris" o un espejo perfecto.
        {
            "HR300_Acero",
            new Preset(
                new Color(0.560f, 0.575f, 0.595f, 1f),
                metallic: 0.98f,
                smoothness: 0.66f)
        },

        // Acero ennegrecido / pavonado.
        {
            "HR300_AceroOscuro",
            new Preset(
                new Color(0.095f, 0.105f, 0.120f, 1f),
                metallic: 0.92f,
                smoothness: 0.54f)
        },

        // Bronce / latón oscuro.
        {
            "HR300_Bronce",
            new Preset(
                new Color(0.430f, 0.285f, 0.190f, 1f),
                metallic: 0.72f,
                smoothness: 0.50f)
        },

        // LCD con emisión HDR.
        {
            "HR300_LCD",
            new Preset(
                new Color(0.300f, 0.410f, 0.135f, 1f),
                metallic: 0.00f,
                smoothness: 0.32f,
                emission: true,
                emissionColor: new Color(0.45f, 0.80f, 0.16f, 1f) * 1.8f)
        },

        // Colores de señalización.
        {
            "HR300_Naranja",
            new Preset(
                new Color(0.900f, 0.180f, 0.040f, 1f),
                metallic: 0.00f,
                smoothness: 0.42f)
        },

        {
            "HR300_Rojo",
            new Preset(
                new Color(0.700f, 0.055f, 0.045f, 1f),
                metallic: 0.00f,
                smoothness: 0.42f)
        },

        {
            "HR300_Verde",
            new Preset(
                new Color(0.120f, 0.570f, 0.185f, 1f),
                metallic: 0.00f,
                smoothness: 0.40f)
        },

        // Textos.
        {
            "HR300_TextoClaro",
            new Preset(
                new Color(0.880f, 0.885f, 0.890f, 1f),
                metallic: 0.00f,
                smoothness: 0.30f,
                doubleSidedGI: true)
        },

        {
            "HR300_TextoOscuro",
            new Preset(
                new Color(0.035f, 0.037f, 0.040f, 1f),
                metallic: 0.00f,
                smoothness: 0.28f,
                doubleSidedGI: true)
        },

        // Material auxiliar de cotas.
        {
            "HR300_Cota",
            new Preset(
                new Color(0.040f, 0.350f, 0.950f, 1f),
                metallic: 0.00f,
                smoothness: 0.28f)
        },

        // Piso auxiliar, deliberadamente mate.
        {
            "HR300_Piso",
            new Preset(
                new Color(0.180f, 0.185f, 0.195f, 1f),
                metallic: 0.00f,
                smoothness: 0.18f)
        },
    };

    // -------------------------------------------------------------------------
    // MENÚ 1: APLICAR MATERIALES
    // -------------------------------------------------------------------------
    [MenuItem("Tools/HR300/1. Aplicar materiales PRO")]
    public static void ApplyMaterials()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");

        if (urpLit == null)
        {
            EditorUtility.DisplayDialog(
                "HR300",
                "No se encontró el shader Universal Render Pipeline/Lit.\n\n" +
                "Este script está preparado para un proyecto URP.",
                "Aceptar");
            return;
        }

        int changed = 0;
        int missing = 0;

        foreach (KeyValuePair<string, Preset> entry in Materials)
        {
            string materialName = entry.Key;
            Preset preset = entry.Value;

            Material mat = FindMaterial(materialName);

            if (mat == null)
            {
                Debug.LogWarning("[HR300] No se encontró: " + materialName);
                missing++;
                continue;
            }

            Undo.RecordObject(mat, "HR300 Material PRO");

            // Shader URP/Lit
            if (mat.shader != urpLit)
                mat.shader = urpLit;

            ConfigureOpaqueURP(mat);
            ApplyPreset(mat, preset);

            EditorUtility.SetDirty(mat);
            changed++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[HR300] Materiales PRO terminados. Actualizados: " +
            changed + " | No encontrados: " + missing);

        EditorUtility.DisplayDialog(
            "HR300 - Materiales PRO",
            "Materiales actualizados: " + changed +
            "\nNo encontrados: " + missing +
            "\n\nIMPORTANTE PARA EL ACERO:\n" +
            "Si todavía se ve gris/plano, ejecuta también:\n" +
            "Tools > HR300 > 2. Crear/Actualizar Reflection Probe",
            "Aceptar");
    }

    // -------------------------------------------------------------------------
    // BUSCAR MATERIAL EXACTO
    // -------------------------------------------------------------------------
    private static Material FindMaterial(string materialName)
    {
        string[] guids = AssetDatabase.FindAssets(
            "\"" + materialName + "\" t:Material");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material candidate = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (candidate != null &&
                candidate.name.Equals(
                    materialName,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        // Segundo intento, más amplio.
        guids = AssetDatabase.FindAssets(materialName + " t:Material");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material candidate = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (candidate != null &&
                candidate.name.Equals(
                    materialName,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // CONFIGURACIÓN BASE DE URP/LIT
    // -------------------------------------------------------------------------
    private static void ConfigureOpaqueURP(Material mat)
    {
        // Metallic workflow.
        SetFloatIfExists(mat, "_WorkflowMode", 1.0f);

        // Opaque.
        SetFloatIfExists(mat, "_Surface", 0.0f);
        SetFloatIfExists(mat, "_Blend", 0.0f);
        SetFloatIfExists(mat, "_AlphaClip", 0.0f);
        SetFloatIfExists(mat, "_Cutoff", 0.5f);

        // Front faces.
        SetFloatIfExists(mat, "_Cull", 2.0f);

        // ZWrite.
        SetFloatIfExists(mat, "_ZWrite", 1.0f);

        // Smoothness desde el valor del material, no desde BaseMap Alpha.
        SetFloatIfExists(mat, "_SmoothnessTextureChannel", 0.0f);

        // Mantener sombras.
        SetFloatIfExists(mat, "_ReceiveShadows", 1.0f);

        // MUY IMPORTANTE PARA METALES:
        // En URP estos valores en 0 significan "NO desactivar".
        SetFloatIfExists(mat, "_SpecularHighlights", 0.0f);
        SetFloatIfExists(mat, "_EnvironmentReflections", 0.0f);

        mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
        mat.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");

        mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHATEST_ON");

        mat.SetOverrideTag("RenderType", "Opaque");
        mat.renderQueue = -1;

        mat.enableInstancing = true;
    }

    // -------------------------------------------------------------------------
    // APLICAR PRESET
    // -------------------------------------------------------------------------
    private static void ApplyPreset(Material mat, Preset p)
    {
        // COLOR BASE -----------------------------------------------------------
        SetColorIfExists(mat, "_BaseColor", p.color);
        SetColorIfExists(mat, "_Color", p.color);

        // METAL ---------------------------------------------------------------
        SetFloatIfExists(mat, "_Metallic", p.metallic);

        // Si existe un mapa metallic viejo, lo quitamos.
        // Así el valor numérico de Metallic realmente controla el material.
        if (mat.HasProperty("_MetallicGlossMap"))
            mat.SetTexture("_MetallicGlossMap", null);

        mat.DisableKeyword("_METALLICSPECGLOSSMAP");

        // SMOOTHNESS ----------------------------------------------------------
        SetFloatIfExists(mat, "_Smoothness", p.smoothness);
        SetFloatIfExists(mat, "_Glossiness", p.smoothness);

        // SPECULAR COLOR:
        // URP en Metallic Workflow calcula físicamente el specular a partir
        // de BaseColor + Metallic, por eso no forzamos un color especular manual.

        // CLEAR COAT ----------------------------------------------------------
        if (mat.HasProperty("_ClearCoatMask"))
        {
            mat.SetFloat("_ClearCoatMask", p.clearCoat);
            SetFloatIfExists(
                mat,
                "_ClearCoatSmoothness",
                p.clearCoatSmoothness);

            if (p.clearCoat > 0.001f)
                mat.EnableKeyword("_CLEARCOAT");
            else
                mat.DisableKeyword("_CLEARCOAT");
        }

        // EMISSION ------------------------------------------------------------
        if (p.emission)
        {
            mat.EnableKeyword("_EMISSION");
            SetColorIfExists(
                mat,
                "_EmissionColor",
                p.emissionColor);

            mat.globalIlluminationFlags =
                MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            SetColorIfExists(
                mat,
                "_EmissionColor",
                Color.black);

            mat.globalIlluminationFlags =
                MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }

        // OCLUSIÓN ------------------------------------------------------------
        SetFloatIfExists(mat, "_OcclusionStrength", 1.0f);

        // Escala normal neutra si no hay textura normal.
        if (mat.HasProperty("_BumpMap") &&
            mat.GetTexture("_BumpMap") == null)
        {
            SetFloatIfExists(mat, "_BumpScale", 1.0f);
            mat.DisableKeyword("_NORMALMAP");
        }

        mat.doubleSidedGI = p.doubleSidedGI;

        // Garantizar reflexiones.
        mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
        mat.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
    }

    // -------------------------------------------------------------------------
    // MENÚ 2: REFLECTION PROBE
    //
    // Un metal sin algo que reflejar se ve como plástico gris.
    // Esta opción crea un probe de escena pensado para una habitación/laboratorio.
    // -------------------------------------------------------------------------
    [MenuItem("Tools/HR300/2. Crear/Actualizar Reflection Probe")]
    public static void CreateOrUpdateReflectionProbe()
    {
        const string probeName = "HR300_ReflectionProbe";

        GameObject go = GameObject.Find(probeName);
        ReflectionProbe probe;

        if (go == null)
        {
            go = new GameObject(probeName);
            Undo.RegisterCreatedObjectUndo(
                go,
                "Crear HR300 Reflection Probe");

            probe = go.AddComponent<ReflectionProbe>();
        }
        else
        {
            probe = go.GetComponent<ReflectionProbe>();

            if (probe == null)
                probe = Undo.AddComponent<ReflectionProbe>(go);
        }

        // Si hay un objeto seleccionado, colocarlo cerca de ese modelo.
        Bounds? selectedBounds = GetSelectedBounds();

        if (selectedBounds.HasValue)
        {
            Bounds b = selectedBounds.Value;

            go.transform.position = b.center;

            // El probe debe abarcar el modelo y parte del entorno.
            Vector3 s = b.size;
            probe.size = new Vector3(
                Mathf.Max(3.0f, s.x * 5.0f),
                Mathf.Max(3.0f, s.y * 5.0f),
                Mathf.Max(3.0f, s.z * 4.0f));
        }
        else
        {
            // Valor seguro para una escena tipo habitación/laboratorio.
            go.transform.position = new Vector3(0f, 1.5f, 0f);
            probe.size = new Vector3(5f, 3f, 5f);
        }

        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;

        probe.resolution = 256;
        probe.intensity = 1.05f;
        probe.boxProjection = true;
        probe.importance = 2;
        probe.nearClipPlane = 0.10f;
        probe.farClipPlane = 100f;

        // Fondo/culling normal.
        probe.clearFlags = ReflectionProbeClearFlags.Skybox;
        probe.cullingMask = ~0;

        EditorUtility.SetDirty(probe);

        // Refrescar captura.
        probe.RenderProbe();

        Selection.activeGameObject = go;

        Debug.Log(
            "[HR300] Reflection Probe creado/actualizado y refrescado.");

        EditorUtility.DisplayDialog(
            "HR300 - Reflection Probe",
            "Reflection Probe configurado.\n\n" +
            "Esto es especialmente importante para HR300_Acero, " +
            "HR300_AceroOscuro y HR300_Bronce.\n\n" +
            "Si moviste mucho la máquina o cambiaste la iluminación, " +
            "puedes volver a ejecutar esta opción.",
            "Aceptar");
    }

    // -------------------------------------------------------------------------
    // BOUNDS DE SELECCIÓN
    // -------------------------------------------------------------------------
    private static Bounds? GetSelectedBounds()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
            return null;

        Renderer[] renderers =
            selected.GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
            return null;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------
    private static void SetFloatIfExists(
        Material mat,
        string property,
        float value)
    {
        if (mat.HasProperty(property))
            mat.SetFloat(property, value);
    }

    private static void SetColorIfExists(
        Material mat,
        string property,
        Color value)
    {
        if (mat.HasProperty(property))
            mat.SetColor(property, value);
    }
}