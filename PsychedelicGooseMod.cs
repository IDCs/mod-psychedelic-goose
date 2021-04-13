using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace PsychedelicGooseMod
{
  public static class ModInfo
  {
    public const string GUID = "com.blacktreegaming.psychedelicgoose";
    public const string Name = "Psychedelic Goose";
    public const string Version = "1.0.1";
  }

  [BepInPlugin (ModInfo.GUID, ModInfo.Name, ModInfo.Version)]
  [HarmonyPatch]
  public class PsychedelicGoose : BaseUnityPlugin
  {
    private static Color m_currentColor = Color.red;
    private static Color m_wantedColor = Color.blue;

    private static float m_fFadeInTime = 1f;
    private static float m_fProgress = 0.01f;

    private static SkinnedMeshRenderer m_skin = null;

    private void Awake()
    {
      new Harmony (ModInfo.GUID).PatchAll (typeof (PsychedelicGoose));
    }

    [HarmonyPostfix]
    [HarmonyPatch (typeof (Goose), "Start")]
    private static void ChangeSkin(Goose __instance)
    {
      m_skin = __instance.GetComponentsInChildren<SkinnedMeshRenderer> () [0];
      Shader shader = Shader.Find ("VertexLit");
      Material mat = new Material (shader);
      mat.SetColor ("_Color", m_currentColor);
      mat.SetFloat ("_EmissionColor", m_currentColor.r * 10f);
      //mat.SetTexture("_MainTex", null);
      m_skin.material = new Material (mat);
      StartRoutine (__instance);
    }

    private static void StartRoutine(Goose goose)
    {
      goose.StartCoroutine (ChangeWantedColor (goose));
      goose.StartCoroutine (LerpColor ());
    }

    private static IEnumerator<WaitForSeconds> ChangeWantedColor(Goose goose)
    {
      while (true)
      {
        if (goose.gooseHonker.justQuacked)
        {
          m_currentColor = m_skin.material.color;
          m_fProgress = 0.01f;
          m_wantedColor = UnityEngine.Random.ColorHSV (0f, 1f, 0f, 1f, 0f, 1f);
        }

        yield return null;
      }
    }

    private static IEnumerator<WaitForSeconds> LerpColor()
    {
      while (true)
      {
        float increment = Time.deltaTime;
        Color color = Color.Lerp (m_currentColor, m_wantedColor, m_fProgress / m_fFadeInTime);
        if (m_fProgress < 1f)
        {
          m_skin.material.SetColor ("_Color", color);
          m_skin.material.SetFloat ("_EmissionColor", color.r * 10f);
          m_skin.material.EnableKeyword ("_EMISSION");
          m_fProgress += increment;
        }

        yield return null;
      }
    }
  }
}