using System;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using System.ComponentModel;

namespace MG
{
    /// <summary>
    /// AmbientCG Importer para Unity URP.
    /// Descarga materiales de ambientCG y crea automáticamente
    /// un material compatible con Universal Render Pipeline.
    /// </summary>
    public class AmbientCGImporterEditor : EditorWindow
    {
        private struct UserInput
        {
            public string textureUrl;
            public int resolutionIndex;
            public bool logging;
        }

        private UserInput m_userInput = new UserInput();

        private const string m_baseUrl =
            "https://ambientcg.com/get?file=";

        private string[] m_resolutions =
        {
            "1K",
            "2K",
            "4K",
            "8K",
            "12K",
            "16K"
        };

        public string TexureName
        {
            get
            {
                if (string.IsNullOrEmpty(m_userInput.textureUrl))
                    return "";

                string[] parts =
                    m_userInput.textureUrl.Split('=');

                if (parts.Length < 2)
                    return "";

                return parts[1];
            }
        }

        public string FolderPath
        {
            get
            {
                return UnityEngine.Application.dataPath +
                    "/AmbientCGImporter/Imported/" +
                    TexureName;
            }
        }

        public string RelativePath
        {
            get
            {
                return "Assets/AmbientCGImporter/Imported/" +
                    TexureName;
            }
        }

        // =========================================================
        // MENU
        // =========================================================

        [MenuItem("Tools/AmbientCG Importer")]
        public static void OpenWindow()
        {
            EditorWindow ew =
                EditorWindow.GetWindow(
                    typeof(AmbientCGImporterEditor)
                );

            ew.titleContent =
                new UnityEngine.GUIContent(
                    "AmbientCG Importer"
                );
        }

        // =========================================================
        // GUI
        // =========================================================

        private void OnGUI()
        {
            UnityEngine.GUILayout.Space(10);

            m_userInput.textureUrl =
                EditorGUILayout.TextField(
                    "Url",
                    m_userInput.textureUrl
                );

            UnityEngine.GUILayout.Space(5);

            m_userInput.resolutionIndex =
                EditorGUILayout.Popup(
                    "Resolution",
                    m_userInput.resolutionIndex,
                    m_resolutions
                );

            UnityEngine.GUILayout.Space(5);

            m_userInput.logging =
                EditorGUILayout.Toggle(
                    "Logging",
                    m_userInput.logging
                );

            UnityEngine.GUILayout.Space(20);

            if (UnityEngine.GUILayout.Button("Import"))
            {
                Import();
            }
        }

        // =========================================================
        // IMPORT
        // =========================================================

        private void Import()
        {
            if (string.IsNullOrEmpty(TexureName))
            {
                UnityEngine.Debug.LogError(
                    "Introduce una URL válida de ambientCG."
                );

                return;
            }

            // Crear carpeta principal
            if (!AssetDatabase.IsValidFolder(
                "Assets/AmbientCGImporter"))
            {
                AssetDatabase.CreateFolder(
                    "Assets",
                    "AmbientCGImporter"
                );
            }

            // Crear carpeta Imported
            if (!AssetDatabase.IsValidFolder(
                "Assets/AmbientCGImporter/Imported"))
            {
                AssetDatabase.CreateFolder(
                    "Assets/AmbientCGImporter",
                    "Imported"
                );
            }

            string url =
                CreateDownloadLink();

            DownloadFile(url);
        }

        // =========================================================
        // DOWNLOAD LINK
        // =========================================================

        private string CreateDownloadLink()
        {
            string resolution =
                m_resolutions[
                    m_userInput.resolutionIndex
                ];

            return m_baseUrl +
                TexureName +
                "_" +
                resolution +
                "-PNG.zip";
        }

        // =========================================================
        // DOWNLOAD
        // =========================================================

        private void DownloadFile(string url)
        {
            WebClient client =
                new WebClient();

            Uri uri =
                new Uri(url);

            client.DownloadFileCompleted +=
                new AsyncCompletedEventHandler(
                    OnDownloadComplete
                );

            client.DownloadFileTaskAsync(
                uri,
                FolderPath + ".zip"
            );

            if (m_userInput.logging)
            {
                UnityEngine.Debug.Log(
                    "Downloading texture " +
                    TexureName +
                    " from " +
                    url
                );
            }
        }

        // =========================================================
        // DOWNLOAD COMPLETE
        // =========================================================

        private void OnDownloadComplete(
            object sender,
            AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                UnityEngine.Debug.LogError(
                    "Error descargando la textura: " +
                    e.Error.Message
                );

                return;
            }

            if (m_userInput.logging)
            {
                UnityEngine.Debug.Log(
                    "Download complete. Extracting textures"
                );
            }

            // Crear carpeta del material
            if (!AssetDatabase.IsValidFolder(
                RelativePath))
            {
                AssetDatabase.CreateFolder(
                    "Assets/AmbientCGImporter/Imported",
                    TexureName
                );
            }

            // Abrir ZIP
            using (
                ZipArchive archive =
                new ZipArchive(
                    File.OpenRead(
                        FolderPath + ".zip"
                    ),
                    ZipArchiveMode.Read
                )
            )
            {
                ExtractAmbientCG(
                    archive,
                    FolderPath,
                    TexureName
                );
            }

            // Eliminar ZIP
            if (File.Exists(
                FolderPath + ".zip"))
            {
                File.Delete(
                    FolderPath + ".zip"
                );
            }

            AssetDatabase.Refresh();

            CreateMaterial();
        }

        // =========================================================
        // CREATE URP MATERIAL
        // =========================================================

        private void CreateMaterial()
        {
            AssetDatabase.Refresh();

            // =====================================================
            // URP SHADER
            // =====================================================

            UnityEngine.Shader shader =
                UnityEngine.Shader.Find(
                    "Universal Render Pipeline/Lit"
                );

            if (shader == null)
            {
                UnityEngine.Debug.LogError(
                    "No se encontró el shader " +
                    "'Universal Render Pipeline/Lit'. " +
                    "Comprueba que tu proyecto utiliza URP."
                );

                return;
            }

            // Crear material
            UnityEngine.Material material =
                new UnityEngine.Material(shader);

            material.name =
                TexureName;

            // =====================================================
            // RUTAS
            // =====================================================

            string colorPath =
                RelativePath +
                "/" +
                TexureName +
                "_alb.png";

            string normalPath =
                RelativePath +
                "/" +
                TexureName +
                "_nml.png";

            string mosPath =
                RelativePath +
                "/" +
                TexureName +
                "_mos.png";

            string heightPath =
                RelativePath +
                "/" +
                TexureName +
                "_plx.png";

            // =====================================================
            // NORMAL MAP
            // =====================================================

            TextureImporter normalImporter =
                AssetImporter.GetAtPath(
                    normalPath
                ) as TextureImporter;

            if (normalImporter != null)
            {
                normalImporter.textureType =
                    TextureImporterType.NormalMap;

                normalImporter.sRGBTexture =
                    false;

                normalImporter.SaveAndReimport();
            }

            // =====================================================
            // CARGAR TEXTURAS
            // =====================================================

            UnityEngine.Texture2D color =
                AssetDatabase.LoadAssetAtPath<
                    UnityEngine.Texture2D
                >(colorPath);

            UnityEngine.Texture2D normal =
                AssetDatabase.LoadAssetAtPath<
                    UnityEngine.Texture2D
                >(normalPath);

            UnityEngine.Texture2D mos =
                AssetDatabase.LoadAssetAtPath<
                    UnityEngine.Texture2D
                >(mosPath);

            UnityEngine.Texture2D height =
                AssetDatabase.LoadAssetAtPath<
                    UnityEngine.Texture2D
                >(heightPath);

            // =====================================================
            // BASE MAP
            // =====================================================

            if (color != null)
            {
                material.SetTexture(
                    "_BaseMap",
                    color
                );

                material.SetColor(
                    "_BaseColor",
                    UnityEngine.Color.white
                );
            }

            // =====================================================
            // NORMAL
            // =====================================================

            if (normal != null)
            {
                material.SetTexture(
                    "_BumpMap",
                    normal
                );

                material.SetFloat(
                    "_BumpScale",
                    1.0f
                );
            }

            // =====================================================
            // METALLIC / AO / SMOOTHNESS
            // =====================================================

            if (mos != null)
            {
                // MOS:
                //
                // R = Metallic
                // G = AO
                // B = Detail Mask
                // A = Smoothness
                //

                material.SetTexture(
                    "_MetallicGlossMap",
                    mos
                );

                material.SetTexture(
                    "_OcclusionMap",
                    mos
                );

                material.SetFloat(
                    "_Metallic",
                    1.0f
                );

                material.SetFloat(
                    "_Smoothness",
                    1.0f
                );
            }

            // =====================================================
            // HEIGHT
            // =====================================================

            if (height != null)
            {
                material.SetTexture(
                    "_ParallaxMap",
                    height
                );

                material.SetFloat(
                    "_Parallax",
                    0.02f
                );
            }

            // =====================================================
            // CREAR MAT
            // =====================================================

            string materialPath =
                RelativePath +
                "/" +
                TexureName +
                ".mat";

            // Si existe uno anterior, eliminarlo
            if (AssetDatabase.LoadAssetAtPath<
                UnityEngine.Material>(
                    materialPath) != null)
            {
                AssetDatabase.DeleteAsset(
                    materialPath
                );
            }

            AssetDatabase.CreateAsset(
                material,
                materialPath
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (m_userInput.logging)
            {
                UnityEngine.Debug.Log(
                    "======================================"
                );

                UnityEngine.Debug.Log(
                    "Material URP creado correctamente:"
                );

                UnityEngine.Debug.Log(
                    materialPath
                );

                UnityEngine.Debug.Log(
                    "======================================"
                );
            }

            Selection.activeObject =
                material;
        }

        // =========================================================
        // EXTRACT AMBIENTCG
        // =========================================================

        private void ExtractAmbientCG(
            ZipArchive arc,
            string dir,
            string name)
        {
            static bool tryFindEntry(
                ZipArchive arc,
                string suffix,
                out ZipArchiveEntry e)
            {
                e =
                    arc.Entries.FirstOrDefault(
                        x =>
                            x.Name
                                .ToLowerInvariant()
                                .EndsWith(
                                    suffix
                                )
                    );

                return e != null;
            }

            static ZipArchiveEntry findEntryOrNull(
                ZipArchive arc,
                string suffix)
            {
                tryFindEntry(
                    arc,
                    suffix,
                    out ZipArchiveEntry e
                );

                return e;
            }

            static void copyEntry(
                ZipArchive arc,
                string suffix,
                string outFile,
                bool throwIfNotFound)
            {
                if (File.Exists(outFile))
                    File.Delete(outFile);

                if (tryFindEntry(
                    arc,
                    suffix,
                    out ZipArchiveEntry e))
                {
                    using Stream IN =
                        e.Open();

                    using Stream OUT =
                        File.OpenWrite(outFile);

                    IN.CopyTo(OUT);
                }
                else if (throwIfNotFound)
                {
                    throw new Exception(
                        "Could not find an entry ending with " +
                        suffix
                    );
                }
            }

            static byte[] readStreamBytes(
                ZipArchiveEntry e)
            {
                if (e == null)
                    return null;

                using Stream es =
                    e.Open();

                using MemoryStream ms =
                    new MemoryStream();

                es.CopyTo(ms);

                return ms.ToArray();
            }

            string colorOut =
                $"{dir}/{name}_alb.png";

            string mosOut =
                $"{dir}/{name}_mos.png";

            string normalOut =
                $"{dir}/{name}_nml.png";

            string plxOut =
                $"{dir}/{name}_plx.png";

            // Buscar mapas
            ZipArchiveEntry metalness =
                findEntryOrNull(
                    arc,
                    "_metalness.png"
                );

            ZipArchiveEntry roughness =
                findEntryOrNull(
                    arc,
                    "_roughness.png"
                );

            ZipArchiveEntry ao =
                findEntryOrNull(
                    arc,
                    "_ambientocclusion.png"
                );

            // Crear MOS
            if (File.Exists(mosOut))
                File.Delete(mosOut);

            makeMosMap(
                readStreamBytes(metalness),
                readStreamBytes(roughness),
                readStreamBytes(ao),
                mosOut
            );

            // Color
            copyEntry(
                arc,
                "_color.png",
                colorOut,
                true
            );

            // Normal
            copyEntry(
                arc,
                "_normalgl.png",
                normalOut,
                true
            );

            // Displacement
            copyEntry(
                arc,
                "_displacement.png",
                plxOut,
                false
            );
        }

        // =========================================================
        // CREATE MOS MAP
        // =========================================================

        private void makeMosMap(
            byte[] metalBytes,
            byte[] roughBytes,
            byte[] aoBytes,
            string outFile)
        {
            static Bitmap bytesToBitmap(
                byte[] bytes)
            {
                if (bytes == null)
                    return null;

                using MemoryStream ms =
                    new MemoryStream(bytes);

                return new Bitmap(
                    ms,
                    false
                );
            }

            static Bitmap resize(
                Bitmap b,
                int w,
                int h)
            {
                Rectangle destRect =
                    new Rectangle(
                        0,
                        0,
                        w,
                        h
                    );

                Bitmap destImage =
                    new Bitmap(
                        w,
                        h
                    );

                using Graphics graphics =
                    Graphics.FromImage(
                        destImage
                    );

                graphics.CompositingMode =
                    CompositingMode.SourceCopy;

                graphics.CompositingQuality =
                    CompositingQuality.HighQuality;

                graphics.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

                graphics.SmoothingMode =
                    SmoothingMode.HighQuality;

                graphics.PixelOffsetMode =
                    PixelOffsetMode.HighQuality;

                using ImageAttributes wrapMode =
                    new ImageAttributes();

                wrapMode.SetWrapMode(
                    WrapMode.TileFlipXY
                );

                graphics.DrawImage(
                    b,
                    destRect,
                    0,
                    0,
                    b.Width,
                    b.Height,
                    GraphicsUnit.Pixel,
                    wrapMode
                );

                return destImage;
            }

            static void matchSizes(
                ref Bitmap a,
                ref Bitmap b,
                ref Bitmap c,
                out int w,
                out int h)
            {
                w = 0;
                h = 0;

                if (a != null)
                {
                    w = Math.Max(
                        w,
                        a.Width
                    );

                    h = Math.Max(
                        h,
                        a.Height
                    );
                }

                if (b != null)
                {
                    w = Math.Max(
                        w,
                        b.Width
                    );

                    h = Math.Max(
                        h,
                        b.Height
                    );
                }

                if (c != null)
                {
                    w = Math.Max(
                        w,
                        c.Width
                    );

                    h = Math.Max(
                        h,
                        c.Height
                    );
                }

                if (
                    a != null &&
                    (
                        a.Width != w ||
                        a.Height != h
                    ))
                {
                    Bitmap n =
                        resize(
                            a,
                            w,
                            h
                        );

                    a.Dispose();

                    a = n;
                }

                if (
                    b != null &&
                    (
                        b.Width != w ||
                        b.Height != h
                    ))
                {
                    Bitmap n =
                        resize(
                            b,
                            w,
                            h
                        );

                    b.Dispose();

                    b = n;
                }

                if (
                    c != null &&
                    (
                        c.Width != w ||
                        c.Height != h
                    ))
                {
                    Bitmap n =
                        resize(
                            c,
                            w,
                            h
                        );

                    c.Dispose();

                    c = n;
                }
            }

            static Color[] readColors(
                Bitmap bmp,
                int w,
                int h)
            {
                Color[] a =
                    new Color[
                        w * h
                    ];

                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        a[
                            (y * w) + x
                        ] =
                            bmp.GetPixel(
                                x,
                                y
                            );
                    }
                }

                return a;
            }

            static void writeColors(
                Bitmap bmp,
                int w,
                int h,
                Color[] a)
            {
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        bmp.SetPixel(
                            x,
                            y,
                            a[
                                (y * w) + x
                            ]
                        );
                    }
                }
            }

            static Color[] fakeColors(
                int len,
                int value)
            {
                Color[] a =
                    new Color[len];

                for (int i = 0; i < len; ++i)
                {
                    a[i] =
                        Color.FromArgb(
                            255,
                            value,
                            value,
                            value
                        );
                }

                return a;
            }

            static Color combineMosColor(
                Color metal,
                Color rough,
                Color ao)
            {
                // Unity:
                //
                // RED   = Metallic
                // GREEN = AO
                // BLUE  = Detail Mask
                // ALPHA = Smoothness
                //
                // Smoothness = 1 - Roughness

                return Color.FromArgb(
                    255 - rough.R,
                    metal.R,
                    ao.R,
                    0
                );
            }

            static Color[] combineMosColors(
                Color[] metal,
                Color[] rough,
                Color[] ao)
            {
                int len =
                    metal.Length;

                Color[] mos =
                    new Color[len];

                for (int i = 0; i < len; ++i)
                {
                    mos[i] =
                        combineMosColor(
                            metal[i],
                            rough[i],
                            ao[i]
                        );
                }

                return mos;
            }

            Bitmap metalBmp = null;
            Bitmap roughBmp = null;
            Bitmap aoBmp = null;

            Color[] metalColors;
            Color[] roughColors;
            Color[] aoColors;

            int width;
            int height;

            try
            {
                metalBmp =
                    bytesToBitmap(
                        metalBytes
                    );

                roughBmp =
                    bytesToBitmap(
                        roughBytes
                    );

                aoBmp =
                    bytesToBitmap(
                        aoBytes
                    );

                matchSizes(
                    ref metalBmp,
                    ref roughBmp,
                    ref aoBmp,
                    out width,
                    out height
                );

                metalColors =
                    metalBmp != null
                    ? readColors(
                        metalBmp,
                        width,
                        height
                    )
                    : fakeColors(
                        width * height,
                        0
                    );

                roughColors =
                    roughBmp != null
                    ? readColors(
                        roughBmp,
                        width,
                        height
                    )
                    : fakeColors(
                        width * height,
                        127
                    );

                aoColors =
                    aoBmp != null
                    ? readColors(
                        aoBmp,
                        width,
                        height
                    )
                    : fakeColors(
                        width * height,
                        255
                    );
            }
            finally
            {
                metalBmp?.Dispose();
                roughBmp?.Dispose();
                aoBmp?.Dispose();
            }

            using Bitmap mosBmp =
                new Bitmap(
                    width,
                    height
                );

            Color[] mosColors =
                combineMosColors(
                    metalColors,
                    roughColors,
                    aoColors
                );

            writeColors(
                mosBmp,
                width,
                height,
                mosColors
            );

            mosBmp.Save(
                outFile,
                ImageFormat.Png
            );
        }
    }
}