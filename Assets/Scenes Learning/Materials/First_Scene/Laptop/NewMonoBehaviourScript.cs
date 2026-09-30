using UnityEngine;
using UnityEditor;

public static class CorregirMaterialesLaptop
{
    // ============================================================
    // CARPETA EXACTA DE TUS MATERIALES
    // ============================================================

    private const string CARPETA =
        "Assets/Scenes Learning/Materials/First_Scene/Laptop/";


    // ============================================================
    // MENÚ
    // ============================================================

    [MenuItem("Tools/Laptop/CORREGIR TECLADO 009-033")]
    public static void CorregirTeclado()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError(
                "Selecciona Laptop_Keyboard en la Hierarchy."
            );

            return;
        }


        GameObject objeto =
            Selection.activeGameObject;


        Renderer renderer =
            objeto.GetComponent<Renderer>();


        if (renderer == null)
        {
            Debug.LogError(
                "El objeto seleccionado no tiene Renderer."
            );

            return;
        }


        Debug.Log(
            "Objeto seleccionado: " +
            objeto.name
        );


        Debug.Log(
            "Slots encontrados: " +
            renderer.sharedMaterials.Length
        );


        // Laptop_Keyboard debe tener 25 slots:
        //
        // Slot 0  = laptop009
        // Slot 1  = laptop010
        // ...
        // Slot 24 = laptop033

        Material[] nuevos =
            new Material[renderer.sharedMaterials.Length];


        for (int slot = 0;
             slot < renderer.sharedMaterials.Length;
             slot++)
        {
            int numero =
                9 + slot;


            string nombre =
                "laptop" +
                numero.ToString("000");


            Material material =
                CargarMaterial(nombre);


            if (material == null)
            {
                Debug.LogError(
                    "NO encontré: " +
                    nombre
                );

                nuevos[slot] =
                    renderer.sharedMaterials[slot];

                continue;
            }


            // Configurar visualmente
            ConfigurarMaterial(
                material,
                numero
            );


            // FORZAR material al slot
            nuevos[slot] =
                material;


            Debug.Log(
                "Slot " +
                slot +
                " -> " +
                nombre
            );
        }


        Undo.RecordObject(
            renderer,
            "Corregir materiales teclado Laptop"
        );


        renderer.sharedMaterials =
            nuevos;


        EditorUtility.SetDirty(
            renderer
        );


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


        Debug.Log(
            "====================================\n" +
            "TECLADO CORREGIDO\n" +
            "laptop009 -> laptop033\n" +
            "===================================="
        );
    }



    // ============================================================
    // CARGAR MATERIAL EXACTAMENTE DE TU CARPETA
    // ============================================================

    private static Material CargarMaterial(
        string nombre
    )
    {
        string ruta =
            CARPETA +
            nombre +
            ".mat";


        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(
                ruta
            );


        // Caso especial de laptop007 no aplica aquí,
        // pero dejamos búsqueda alternativa por seguridad.
        if (mat == null)
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    nombre + " t:Material",
                    new string[]
                    {
                        CARPETA.TrimEnd('/')
                    }
                );


            foreach (string guid in guids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(
                        guid
                    );


                Material encontrado =
                    AssetDatabase.LoadAssetAtPath<Material>(
                        path
                    );


                if (
                    encontrado != null &&
                    encontrado.name == nombre
                )
                {
                    return encontrado;
                }
            }
        }


        return mat;
    }



    // ============================================================
    // CONFIGURACIÓN
    // ============================================================

    private static void ConfigurarMaterial(
        Material mat,
        int numero
    )
    {
        Undo.RecordObject(
            mat,
            "Configurar material teclado"
        );


        // ========================================================
        // COLOR
        // ========================================================
        //
        // Tu laptop es clara.
        // Por eso las letras/símbolos deben ser OSCUROS.
        //

        Color color;


        // Material principal del teclado
        if (numero == 9)
        {
            ColorUtility.TryParseHtmlString(
                "#11171C",
                out color
            );
        }

        // Algunos símbolos ligeramente más claros
        else if (numero >= 10 &&
                 numero <= 15)
        {
            ColorUtility.TryParseHtmlString(
                "#1A232A",
                out color
            );
        }

        else if (numero >= 16 &&
                 numero <= 23)
        {
            ColorUtility.TryParseHtmlString(
                "#202B32",
                out color
            );
        }

        else
        {
            ColorUtility.TryParseHtmlString(
                "#151D22",
                out color
            );
        }


        // ========================================================
        // BASE COLOR
        // ========================================================

        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor(
                "_BaseColor",
                color
            );
        }


        if (mat.HasProperty("_Color"))
        {
            mat.SetColor(
                "_Color",
                color
            );
        }


        // ========================================================
        // QUITAR TEXTURAS ANTIGUAS
        // ========================================================

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture(
                "_BaseMap",
                null
            );
        }


        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture(
                "_MainTex",
                null
            );
        }


        // ========================================================
        // METALLIC
        // ========================================================

        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat(
                "_Metallic",
                0.0f
            );
        }


        // ========================================================
        // SMOOTHNESS
        // ========================================================

        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat(
                "_Smoothness",
                0.35f
            );
        }


        if (mat.HasProperty("_Glossiness"))
        {
            mat.SetFloat(
                "_Glossiness",
                0.35f
            );
        }


        // ========================================================
        // SIN EMISIÓN
        // ========================================================

        mat.DisableKeyword(
            "_EMISSION"
        );


        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor(
                "_EmissionColor",
                Color.black
            );
        }


        EditorUtility.SetDirty(
            mat
        );
    }



    // ============================================================
    // EXTRA:
    // PONER TODOS NEGROS DE UNA VEZ
    // ============================================================

    [MenuItem("Tools/Laptop/Teclado/TODOS NEGRO OSCURO")]
    public static void TodosNegros()
    {
        Color color;

        ColorUtility.TryParseHtmlString(
            "#151B20",
            out color
        );


        for (int numero = 9;
             numero <= 33;
             numero++)
        {
            string nombre =
                "laptop" +
                numero.ToString("000");


            Material mat =
                CargarMaterial(nombre);


            if (mat == null)
                continue;


            Undo.RecordObject(
                mat,
                "Oscurecer teclado"
            );


            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor(
                    "_BaseColor",
                    color
                );
            }


            if (mat.HasProperty("_Color"))
            {
                mat.SetColor(
                    "_Color",
                    color
                );
            }


            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat(
                    "_Metallic",
                    0.0f
                );
            }


            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat(
                    "_Smoothness",
                    0.35f
                );
            }


            EditorUtility.SetDirty(
                mat
            );
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


        Debug.Log(
            "laptop009 - laptop033 oscurecidos."
        );
    }
}