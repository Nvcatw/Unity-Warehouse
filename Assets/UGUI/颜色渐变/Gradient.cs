using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UI
{

    [ExecuteAlways]
    public class Gradient : UIBehaviour, IMaterialModifier
    {

        [NonSerialized]
        private Graphic m_Graphic;

        /// <summary>
        /// The graphic associated with the Mask.
        /// </summary>
        public Graphic graphic
        {
            get { return m_Graphic ?? (m_Graphic = GetComponent<Graphic>()); }
        }

        [NonSerialized]
        private Material m_GradientMaterial;

        [NonSerialized]
        private Material m_BaseMaterial;

        public Vector2 offset;


        public float rotation
        {
            get { return m_Rotation; }
            set
            {
                if (m_Rotation != value)
                {
                    m_Rotation = value;
                    Refresh();
                }
            }
        }
        [SerializeField]
        private float m_Rotation;

        [SerializeField]
        private Texture2D m_GradientTex;
        public Texture2D gradientTex { get { return m_GradientTex; } set { if (m_GradientTex != value) { SetGradientTexture(value); } } }

        [NonSerialized]
        private Vector2 lastpivot;


        protected override void OnEnable()
        {
            base.OnEnable();
            m_BaseMaterial = null;
            if (graphic != null)
            {
                graphic.SetMaterialDirty();
                Canvas.willRenderCanvases += PerformUpdate;
            }
        }

        protected override void OnDisable()
        {
            if (graphic != null)
            {
            Canvas.willRenderCanvases -= PerformUpdate;
                graphic.SetMaterialDirty();
            }
        }

        private void PerformUpdate()
        {
            if(!IsActive())
                return;

            //检测到位置发生改变时，进行刷新
            Vector2 pivot = transform.position - graphic.canvas.transform.position;
            if (lastpivot != pivot && m_GradientMaterial != null)
            {
                Refresh();

                lastpivot = pivot;
            }
        }

        /// <summary>
        /// 刷新材质信息
        /// canvas中的网格将合并为一个大网格，shader中的世界位置是相对于canvas的位置
        /// 计算出旋转后的过渡半径
        /// 将过度半径，旋转角度，中轴直线方程斜率和b值传入shader，以便shader计算UV
        /// </summary>
        void Refresh()
        {
            Vector2 pivot = graphic.transform.position - graphic.canvas.transform.position;
            pivot /= new Vector2(graphic.canvas.transform.lossyScale.x, graphic.canvas.transform.lossyScale.y);
            Rect rect = graphic.rectTransform.rect;
            Vector2 min = rect.min + pivot;
            Vector2 max = rect.max + pivot;

            Vector2 centen = min + (new Vector2(0.5f, 0.5f) + offset) * rect.size;

            //计算过渡直线与矩形相交的两个点，求出过渡半径
            //y-kx+ka-d=0
            //b=ka-d
            //kx-b=y
            float k = Mathf.Tan(-rotation * Mathf.Deg2Rad);

            float b = k * centen.x - centen.y;

            Vector2 minPos;
            float leftY = k * min.x - b;
            if (leftY < min.y)
            {
                float x = (min.y + b) / k;
                minPos = new Vector2(x, min.y);
            }
            else if (leftY > max.y)
            {
                float x = (max.y + b) / k;
                minPos = new Vector2(x, max.y);
            }
            else
            {
                minPos = new Vector2(min.x, leftY);
            }

            Vector2 maxPos;
            float rightY = k * max.x - b;
            if (rightY < min.y)
            {
                float x = (min.y + b) / k;
                maxPos = new Vector2(x, min.y);
            }
            else if (rightY > max.y)
            {
                float x = (max.y + b) / k;
                maxPos = new Vector2(x, max.y);
            }
            else
            {
                maxPos = new Vector2(max.x, rightY);
            }
            float distance = Vector2.Distance(maxPos, minPos);


            float r = (((rotation) % 360) + 360) % 360;
            //求中轴直线方程斜率和b值
            float axisK = Mathf.Tan((-rotation - 90) * Mathf.Deg2Rad);
            float axisB = -(axisK * centen.x - centen.y);

            Vector4 dis = new Vector4(distance, r, axisK, axisB);
            
            m_GradientMaterial.SetVector("_AxisData", dis);
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (m_BaseMaterial != baseMaterial)
            {
                m_GradientMaterial = new Material(baseMaterial);
                m_GradientMaterial.name = "Gradient, "+baseMaterial.name;
                m_GradientMaterial.hideFlags = HideFlags.DontSave;
                m_GradientMaterial.EnableKeyword("UNITY_UI_GRADIENT");
                m_BaseMaterial = baseMaterial;
            }

            SetGradientTexture(m_GradientTex);
            Refresh();
            return m_GradientMaterial;
        }

        private void SetGradientTexture(Texture2D tex)
        {
            if (m_GradientMaterial != null)
                m_GradientMaterial.SetTexture("_GradientTex", tex);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;

            if (graphic != null)
            {

                graphic.SetMaterialDirty();
            }
        }

#endif
    }
}
