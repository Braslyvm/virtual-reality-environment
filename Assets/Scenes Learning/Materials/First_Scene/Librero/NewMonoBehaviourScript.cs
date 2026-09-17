using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class AsignarColoresLibreria
{
    // =========================================================
    // CONFIGURACIÓN DE LOS MATERIALES
    // =========================================================

    private class DatosMaterial
    {
        public string nombre;
        public Color color;
        public float metallic;
        public float smoothness;

        public DatosMaterial(
            string nombre,
            string hexadecimal,
            float metallic,
            float smoothness)
        {
            this.nombre = nombre;

            ColorUtility.TryParseHtmlString(
                hexadecimal,
                out color
            );

            this.metallic = metallic;
            this.smoothness = smoothness;
        }
    }


    private static readonly List<DatosMaterial> materiales =
        new List<DatosMaterial>()
        {
            // ESTRUCTURA INDUSTRIAL
            new DatosMaterial(
                "Libreria_Color_Estructura",
                "#2B3033",
                0.45f,
                0.35f
            ),

            // PÁGINAS
            new DatosMaterial(
                "Libreria_Color_Paginas",
                "#BEBBAF",
                0.0f,
                0.18f
            ),

            // LIBROS
            new DatosMaterial(
                "Libreria_Color_Libro_AzulGris",
                "#465B64",
                0.0f,
                0.25f
            ),

            new DatosMaterial(
                "Libreria_Color_Libro_AzulOscuro",
                "#263A4A",
                0.0f,
                0.25f
            ),

            new DatosMaterial(
                "Libreria_Color_Libro_Beige",
                "#776C58",
                0.0f,
                0.20f
            ),

            new DatosMaterial(
                "Libreria_Color_Libro_Borgona",
                "#592B2B",
                0.0f,
                0.22f
            ),

            new DatosMaterial(
                "Libreria_Color_Libro_Carbon",
                "#282B2E",
                0.0f,
                0.28f
            ),

            new DatosMaterial(
                "Libreria_Color_Libro_Gris",
                "#555A5D",
                0.0f,
                0.25f
            ),

            new DatosMaterial(
                "Libreria_Color_Libro_VerdeGris",
                "#48564D",
                0.0f,
                0.22f
            )
        };


    // =========================================================
    // MENÚ DE UNITY
    // =========================================================

    [MenuItem("Tools/Libreria/Aplicar colores industriales")]
    public static void AplicarColores()
    {
        Debug.Log(
            "=== INICIANDO CONFIGURACIÓN DE LIBRERÍA ==="
        );


        // -----------------------------------------------------
        // 1. CONFIGURAR LOS MATERIALES
        // -----------------------------------------------------

        Dictionary<string, Material> materialesEncontrados =
            new Dictionary<string, Material>();


        foreach (DatosMaterial datos in materiales)
        {
            Material material = BuscarMaterial(datos.nombre);

            if (material == null)
            {
                Debug.LogWarning(
                    "No se encontró el material: "
                    + datos.nombre
                );

                continue;
            }


            ConfigurarMaterial(
                material,
                datos
            );


            materialesEncontrados[datos.nombre] = material;


            Debug.Log(
                "Material configurado: "
                + datos.nombre
            );
        }


        // -----------------------------------------------------
        // 2. REASIGNAR A LA LIBRERÍA SELECCIONADA
        // -----------------------------------------------------

        if (Selection.activeGameObject != null)
        {
            GameObject raiz = Selection.activeGameObject;

            Renderer[] renderers =
                raiz.GetComponentsInChildren<Renderer>(true);


            int reemplazos = 0;


            foreach (Renderer renderer in renderers)
            {
                Material[] actuales =
                    renderer.sharedMaterials;


                bool huboCambios = false;


                for (int i = 0; i < actuales.Length; i++)
                {
                    if (actuales[i] == null)
                        continue;


                    string nombreActual =
                        LimpiarNombreMaterial(
                            actuales[i].name
                        );


                    if (
                        materialesEncontrados.ContainsKey(
                            nombreActual
                        )
                    )
                    {
                        actuales[i] =
                            materialesEncontrados[nombreActual];

                        reemplazos++;

                        huboCambios = true;
                    }
                }


                if (huboCambios)
                {
                    Undo.RecordObject(
                        renderer,
                        "Asignar materiales librería"
                    );

                    renderer.sharedMaterials = actuales;

                    EditorUtility.SetDirty(renderer);
                }
            }


            Debug.Log(
                "Materiales reasignados en la librería: "
                + reemplazos
            );
        }
        else
        {
            Debug.LogWarning(
                "No hay una librería seleccionada en la Jerarquía. "
                + "Los colores de los materiales sí fueron actualizados, "
                + "pero no se realizó reasignación."
            );
        }


        // -----------------------------------------------------
        // GUARDAR CAMBIOS
        // -----------------------------------------------------

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


        Debug.Log(
            "=== LIBRERÍA ACTUALIZADA CORRECTAMENTE ==="
        );
    }


    // =========================================================
    // BUSCAR MATERIAL
    // =========================================================

    private static Material BuscarMaterial(string nombre)
    {
        string[] guids =
            AssetDatabase.FindAssets(
                nombre + " t:Material"
            );


        foreach (string guid in guids)
        {
            string ruta =
                AssetDatabase.GUIDToAssetPath(guid);


            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    ruta
                );


            if (
                material != null
                &&
                material.name == nombre
            )
            {
                return material;
            }
        }


        return null;
    }


    // =========================================================
    // CONFIGURAR MATERIAL
    // =========================================================

    private static void ConfigurarMaterial(
        Material material,
        DatosMaterial datos)
    {
        Undo.RecordObject(
            material,
            "Configurar material librería"
        );


        // -----------------------------------------------------
        // COLOR
        // -----------------------------------------------------

        // URP Lit
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor(
                "_BaseColor",
                datos.color
            );
        }


        // Built-in Standard
        if (material.HasProperty("_Color"))
        {
            material.SetColor(
                "_Color",
                datos.color
            );
        }


        // -----------------------------------------------------
        // METALLIC
        // -----------------------------------------------------

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat(
                "_Metallic",
                datos.metallic
            );
        }


        // -----------------------------------------------------
        // SMOOTHNESS URP
        // -----------------------------------------------------

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat(
                "_Smoothness",
                datos.smoothness
            );
        }


        // -----------------------------------------------------
        // SMOOTHNESS STANDARD
        // -----------------------------------------------------

        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat(
                "_Glossiness",
                datos.smoothness
            );
        }


        // -----------------------------------------------------
        // QUITAR TEXTURA BASE SI BLENDER DEJÓ UNA ROTA
        // -----------------------------------------------------

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture(
                "_BaseMap",
                null
            );
        }


        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture(
                "_MainTex",
                null
            );
        }


        EditorUtility.SetDirty(material);
    }


    // =========================================================
    // LIMPIAR NOMBRE
    // =========================================================

    private static string LimpiarNombreMaterial(
        string nombre)
    {
        // Unity a veces añade "(Instance)"
        nombre = nombre.Replace(
            " (Instance)",
            ""
        );


        // Quitar espacios
        nombre = nombre.Trim();


        return nombre;
    }


    // =========================================================
    // OPCIÓN: SOLO ACTUALIZAR MATERIALES
    // =========================================================

    [MenuItem("Tools/Libreria/Solo actualizar colores de materiales")]
    public static void SoloActualizarMateriales()
    {
        foreach (DatosMaterial datos in materiales)
        {
            Material material =
                BuscarMaterial(datos.nombre);


            if (material == null)
            {
                Debug.LogWarning(
                    "No se encontró: "
                    + datos.nombre
                );

                continue;
            }


            ConfigurarMaterial(
                material,
                datos
            );
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


        Debug.Log(
            "Colores de Libreria actualizados."
        );
    }
}