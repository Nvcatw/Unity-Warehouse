using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    public static class UIMaterialFactory
    {
        private class MatEntry
        {
            public Material baseMat;
            public Material customMat;
            public int count;

            public CullMode cullMode = CullMode.Off;
            public CompareFunction compareFunction = CompareFunction.LessEqual;
            public int useFog = 0;
        }

        private static List<MatEntry> m_List = new List<MatEntry>();

        private static Material Add(Material baseMat, CullMode cullMode, CompareFunction compareFunction, int useFog)
        {
            if (baseMat == null)
                return baseMat;

            if (!baseMat.HasProperty("_CullMode"))
            {
                LogWarningWhenNotInBatchmode("Material " + baseMat.name + " doesn't have _CullMode property", baseMat);
                return baseMat;
            }
            if (!baseMat.HasProperty("_ZTestMode"))
            {
                LogWarningWhenNotInBatchmode("Material " + baseMat.name + " doesn't have _ZTestMode property", baseMat);
                return baseMat;
            }
            if (!baseMat.HasProperty("_UseUIFog"))
            {
                LogWarningWhenNotInBatchmode("Material " + baseMat.name + " doesn't have _UseUIFog property", baseMat);
                return baseMat;
            }

            var listCount = m_List.Count;
            for (int i = 0; i < listCount; ++i)
            {
                MatEntry ent = m_List[i];

                if (ent.baseMat == baseMat
                    && ent.cullMode == cullMode
                    && ent.compareFunction == compareFunction
                    && ent.useFog == useFog)
                {
                    ++ent.count;
                    return ent.customMat;
                }
            }

            var newEnt = new MatEntry();
            newEnt.count = 1;
            newEnt.baseMat = baseMat;
            newEnt.customMat = new Material(baseMat);
            newEnt.customMat.hideFlags = HideFlags.HideAndDontSave;
            newEnt.cullMode = cullMode;
            newEnt.compareFunction = compareFunction;
            newEnt.useFog = useFog;

            newEnt.customMat.name = string.Format("Cull Mode:{0}, Compare Function:{1}, Use Fog:{2} ({3})", cullMode, compareFunction, useFog, baseMat.name);

            newEnt.customMat.SetFloat("_CullMode", (float)cullMode);
            newEnt.customMat.SetFloat("_ZTestMode", (float)compareFunction);
            newEnt.customMat.SetFloat("_UseUIFog", (float)useFog);

            m_List.Add(newEnt);
            return newEnt.customMat;
        }

        public static void Remove(Material customMat)
        {
            if (customMat == null)
                return;

            var listCount = m_List.Count;
            for (int i = 0; i < listCount; ++i)
            {
                MatEntry ent = m_List[i];

                if (ent.customMat != customMat)
                    continue;

                if (--ent.count == 0)
                {
                    GameObject.DestroyImmediate(ent.customMat);
                    ent.baseMat = null;
                    m_List.RemoveAt(i);
                }
                return;
            }
        }


        static void LogWarningWhenNotInBatchmode(string warning, Object context)
        {
            // Do not log warnings in batchmode (case 1350059)
            if (!Application.isBatchMode)
                Debug.LogWarning(warning, context);
        }
    }
}
