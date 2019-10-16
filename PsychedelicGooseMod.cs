using System;
using Harmony;
using System.IO;
using System.Reflection;
using UnityEngine;
using VortexHarmonyInstaller.ModTypes;
using System.Collections.Generic;

namespace PsychedelicGooseMod
{
    public class Main
    {
        private static string DataPath { get; set; }
        private static Assembly AssemblyResolver(object sender, ResolveEventArgs args)
        {
            string asmName = args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
            FileInfo[] libFiles = new DirectoryInfo(DataPath)
                .GetFiles("*.dll", SearchOption.TopDirectoryOnly);
            foreach (FileInfo dll in libFiles)
            {
                if (dll.Name == asmName)
                {
                    return Assembly.LoadFile(dll.FullName);
                }
            }

            return null;
        }

        public static void RunPatch(VortexMod modInfo)
        {
            DataPath = modInfo.DataPath;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver;
            try
            {
                var harmony = HarmonyInstance.Create("com.blacktreegaming.vortex.mod.test");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception exc)
            {
                modInfo.LogError("Harmony patching failed", exc);
            }
        }
    }

    [HarmonyPatch(typeof(Goose))]
    [HarmonyPatch("Start")]
    class Patch
    {
        private static Color m_currentColor = Color.red;
        private static Color m_wantedColor = Color.blue;

        private static float m_fFadeInTime = 1f;
        private static float m_fProgress = 0.01f;

        private static SkinnedMeshRenderer m_skin = null;

        [HarmonyPostfix]
        static void PostFix(Goose __instance)
        {
            m_skin = __instance.GetComponentsInChildren<SkinnedMeshRenderer>()[0];
            Shader shader = Shader.Find("VertexLit");
            Material mat = new Material(shader);
            mat.SetColor("_Color", m_currentColor);
            mat.SetFloat("_EmissionColor", m_currentColor.r * 10f);
            //mat.SetTexture("_MainTex", null);
            m_skin.material = new Material(mat);
            StartRoutine(__instance);
        }

        public static void StartRoutine(Goose goose)
        {
            goose.StartCoroutine(ChangeWantedColor(goose));
            goose.StartCoroutine(LerpColor());
        }

        private static IEnumerator<WaitForSeconds> ChangeWantedColor(Goose goose)
        {
            while (true)
            {
                if (goose.gooseHonker.justQuacked)
                {
                    m_currentColor = m_skin.material.color;
                    m_fProgress = 0.01f;
                    m_wantedColor = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);
                }

                yield return null;
            }
        }

        private static IEnumerator<WaitForSeconds> LerpColor()
        {
            while (true)
            {
                float increment = Time.deltaTime;
                Color color = Color.Lerp(m_currentColor, m_wantedColor, m_fProgress / m_fFadeInTime);
                if (m_fProgress < 1f)
                {
                    m_skin.material.SetColor("_Color", color);
                    m_skin.material.SetFloat("_EmissionColor", color.r * 10f);
                    m_skin.material.EnableKeyword("_EMISSION");
                    m_fProgress += increment;
                }
                
                yield return null;
            }
        }
    }
}