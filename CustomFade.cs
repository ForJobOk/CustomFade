using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class CustomFade : MonoBehaviour
{
    [Serializable]
    public class FadeParameter
    {
        [SerializeField]
        public Color panelColor;

        [SerializeField]
        public float fadeTime;

        [SerializeField]
        public float alpha_Panel;
    }

    [SerializeField, HideInInspector]
    FadeParameter[] fadeParameters;

    [SerializeField, Tooltip("Test number of element")]
    int debugElementNum;

    [SerializeField, Tooltip("Change True When you do test. O is FadeOut, I is FadeIn.")]
    bool debug;

    const float FIXEDUPDATE_DELTATIME = 0.02f;

    Image facePanel;

    Coroutine coroutine;


    void Awake()
    {
        if (GameObject.Find("OnlyUIRenderingCamera"))
        {
            Destroy(GameObject.Find("OnlyUIRenderingCamera"));
        }
    }


    void Start()
    {
        GameObject camera_G = new GameObject("OnlyUIRenderingCamera");
        Camera faceCamera = camera_G.AddComponent<Camera>();
        faceCamera.clearFlags = CameraClearFlags.Depth;
        faceCamera.cullingMask = (1 << LayerMask.NameToLayer("UI"));

        camera_G.transform.parent = this.gameObject.transform;
        
        GameObject canvas_G = new GameObject("FaceCanvas");
        Canvas faceCanvas = canvas_G.AddComponent<Canvas>();
        canvas_G.AddComponent<CanvasRenderer>();

        
        Vector3 canvasPosition = canvas_G.transform.position;
        canvasPosition.x = 0;
        canvasPosition.y = 0;
        canvasPosition.z = 0.1f;
        canvas_G.transform.localPosition = canvasPosition;
        
        faceCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        faceCanvas.worldCamera = faceCamera;
        
        GameObject panel_G = new GameObject("FacePanel");
        facePanel = panel_G.AddComponent<Image>();

        Color tmpColor = facePanel.color;
        tmpColor.a = 0f;
        facePanel.color = tmpColor;
        
        panel_G.transform.SetParent(canvas_G.transform);
        
        Vector3 panelPosition = panel_G.transform.localPosition;
        Vector3 panelScale = panel_G.transform.localScale;
        panelPosition.x = 0;
        panelPosition.y = 0;
        panelPosition.z = 0;
        panelScale = new Vector3(22, 24, 22);
        panel_G.transform.localPosition = panelPosition;
        panel_G.transform.localScale = panelScale;
        
        panel_G.transform.SetParent(canvas_G.transform);
        
        canvas_G.layer = LayerMask.NameToLayer("UI");
        panel_G.layer = LayerMask.NameToLayer("UI");

    }

    void Update()
    {
        Time.fixedDeltaTime = FIXEDUPDATE_DELTATIME;
    }

    void FixedUpdate()
    {
        if (Input.anyKey == false)
        {
            return;
        }

        Test(debugElementNum);

    }

    void Test(int num)
    {
        if (coroutine == null && debug)
        {
            if (Input.GetKeyDown(KeyCode.O) && fadeParameters[num].panelColor.a == 0)
            {
                FadeOut(num);
            }

            if (Input.GetKeyDown(KeyCode.I) && fadeParameters[num].panelColor.a == fadeParameters[num].alpha_Panel)
            {
                FadeIn(num);
            }
        }
    }


    public void FadeOut(int num)
    {
        if (fadeParameters[num].panelColor.a == 0)
        {
            coroutine = StartCoroutine(FadeOutCoroutine(num));
        }
    }


    public void FadeIn(int num)
    {
        if (fadeParameters[num].panelColor.a == fadeParameters[num].alpha_Panel)
        {
            coroutine = StartCoroutine(FadeInCoroutine(num));
        }
    }

    IEnumerator FadeOutCoroutine(int num)
    {

        yield return new WaitForFixedUpdate();
        while (facePanel.color.a < fadeParameters[num].alpha_Panel - 0.00005f)
        {
            yield return new WaitForFixedUpdate();
            fadeParameters[num].panelColor.a += fadeParameters[num].alpha_Panel / (fadeParameters[num].fadeTime * 50);
            facePanel.color = fadeParameters[num].panelColor;

        }

        fadeParameters[num].panelColor.a = fadeParameters[num].alpha_Panel;

        StopCoroutine(coroutine);
        coroutine = null;
    }


    IEnumerator FadeInCoroutine(int num)
    {

        yield return new WaitForFixedUpdate();
        while (fadeParameters[num].panelColor.a > 0 + 0.00005f)
        {
            yield return new WaitForFixedUpdate();
            fadeParameters[num].panelColor.a -= fadeParameters[num].alpha_Panel / (fadeParameters[num].fadeTime * 50);
            facePanel.color = fadeParameters[num].panelColor;

        }

        fadeParameters[num].panelColor.a = 0;

        StopCoroutine(coroutine);
        coroutine = null;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomFade))]
    public class CustomFadeEditor : Editor
    {
        ReorderableList reorderableList;

        void OnEnable()
        {
            SerializedProperty prop = serializedObject.FindProperty("fadeParameters");

            reorderableList = new ReorderableList(serializedObject, prop);
            reorderableList.elementHeight = 78;
            reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "FadeList");
            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = prop.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, element);
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            reorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FadeParameter))]
    public class CharacterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                EditorGUIUtility.labelWidth = 100;

                position.height = EditorGUIUtility.singleLineHeight;

                Rect panelColorRect = new Rect(position)
                {
                    y = position.y + EditorGUIUtility.singleLineHeight + 1
                };

                Rect fadeTimeRect = new Rect(panelColorRect)
                {
                    y = panelColorRect.y + EditorGUIUtility.singleLineHeight + 2
                };

                Rect alpha_PanelRect = new Rect(fadeTimeRect)
                {
                    y = fadeTimeRect.y + EditorGUIUtility.singleLineHeight + 2
                };

                SerializedProperty panelColorProperty = property.FindPropertyRelative("panelColor");
                SerializedProperty fadeTimeProperty = property.FindPropertyRelative("fadeTime");
                SerializedProperty alpha_PanelProperty = property.FindPropertyRelative("alpha_Panel");

                panelColorProperty.colorValue = EditorGUI.ColorField(panelColorRect, "FadeColor", panelColorProperty.colorValue);
                fadeTimeProperty.floatValue = EditorGUI.FloatField(fadeTimeRect, "FadeDuration", fadeTimeProperty.floatValue);
                alpha_PanelProperty.floatValue = EditorGUI.Slider(alpha_PanelRect, "FadeAlpha", alpha_PanelProperty.floatValue, 0, 1);
            }
        }
    }
#endif
}