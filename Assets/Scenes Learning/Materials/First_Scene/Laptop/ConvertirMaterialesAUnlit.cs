using UnityEngine;
using UnityEditor;

public class ConvertirMaterialesAUnlit
{
    [MenuItem("Tools/Materiales/Convertir seleccionados a URP Unlit")]
    private static void Convertir()
    {
        Shader shaderUnlit =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shaderUnlit == null)
        {
            Debug.LogError(
                "No se encontró Universal Render Pipeline/Unlit."
            );
            return;
        }

        Object[] seleccionados =
            Selection.GetFiltered<Object>(
                SelectionMode.DeepAssets
            );

        int cantidad = 0;

        foreach (Object objeto in seleccionados)
        {
            Material material =
                objeto as Material;

            if (material == null)
                continue;

            Texture textura = null;
            Color color = Color.white;

            if (material.HasProperty("_BaseMap"))
                textura = material.GetTexture("_BaseMap");
            else if (material.HasProperty("_MainTex"))
                textura = material.GetTexture("_MainTex");

            if (material.HasProperty("_BaseColor"))
                color = material.GetColor("_BaseColor");
            else if (material.HasProperty("_Color"))
                color = material.GetColor("_Color");

            Undo.RecordObject(
                material,
                "Convertir Material a URP Unlit"
            );

            material.shader = shaderUnlit;

            if (
                textura != null &&
                material.HasProperty("_BaseMap")
            )
            {
                material.SetTexture(
                    "_BaseMap",
                    textura
                );
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    color
                );
            }

            EditorUtility.SetDirty(material);

            cantidad++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            cantidad +
            " materiales convertidos a URP/Unlit."
        );
    }
}