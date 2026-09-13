using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Game.Health;

/// <summary>
/// Monta o HUD de vida por codigo, salva como prefab e coloca uma instancia na cena.
///
/// Por que por codigo e nao arrastando na mao: prefab e cena sao YAML enorme e ilegivel,
/// entao duas pessoas mexendo geram conflito impossivel de resolver. Um script que constroi
/// e codigo normal, com diff revisavel, e nasce identico em qualquer maquina.
/// Se o HUD quebrar, roda o menu de novo.
/// </summary>
public static class ConstruirHudDeVida
{
    const string PastaUI = "Assets/UI";
    const string CaminhoPrefab = PastaUI + "/HealthHUD.prefab";
    const string CaminhoCena = "Assets/Scenes/GameScene.unity";
    const int Coracoes = 5;

    static readonly Color Trilho = new Color(0.15f, 0.16f, 0.20f, 0.85f);

    [MenuItem("Game/Health/Construir HUD de vida")]
    public static void Construir()
    {
        var prefab = GerarPrefab();
        ColocarNaCena(prefab);
    }

    static GameObject GerarPrefab()
    {
        var raiz = new GameObject("HealthHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        var canvas = raiz.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = raiz.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        var arredondado = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        const float lado = 40f, espaco = 6f;
        float largura = Coracoes * lado + (Coracoes - 1) * espaco;

        var grupo = Vazio(raiz.transform, "Grupo", new Vector2(1, 1), new Vector2(-28, -22), new Vector2(largura, 62));
        var opacidade = grupo.gameObject.AddComponent<CanvasGroup>();

        var fundos = new Image[Coracoes];
        var frentes = new Image[Coracoes];

        for (int i = 0; i < Coracoes; i++)
        {
            float x = -(Coracoes - 1 - i) * (lado + espaco);
            var slot = Vazio(grupo, $"Coracao {i}", new Vector2(1, 1), new Vector2(x, 0), new Vector2(lado, lado));
            fundos[i] = Grafico(slot, "Fundo", null, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(lado, lado));
            frentes[i] = Grafico(slot, "Frente", null, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(lado, lado));
        }

        var trilho = Grafico(grupo, "Trilho", arredondado, Trilho, new Vector2(1, 1), new Vector2(0, -50), new Vector2(largura, 8));
        trilho.type = Image.Type.Sliced;

        var barra = Grafico(trilho.rectTransform, "Barra", arredondado, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(largura, 8));
        barra.type = Image.Type.Filled;
        barra.fillMethod = Image.FillMethod.Horizontal;
        barra.fillOrigin = (int)Image.OriginHorizontal.Left;
        barra.fillAmount = 1f;

        var hud = raiz.AddComponent<HealthHud>();
        raiz.AddComponent<HealthHudBinder>();
        raiz.AddComponent<TesteDeDano>();

        var so = new SerializedObject(hud);
        so.FindProperty("grupo").objectReferenceValue = grupo;
        so.FindProperty("opacidade").objectReferenceValue = opacidade;
        so.FindProperty("barra").objectReferenceValue = barra;
        PreencherArray(so, "fundos", fundos);
        PreencherArray(so, "frentes", frentes);
        so.ApplyModifiedPropertiesWithoutUndo();

        System.IO.Directory.CreateDirectory(PastaUI);
        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, CaminhoPrefab);
        Object.DestroyImmediate(raiz);

        Debug.Log($"[hud] prefab salvo em {CaminhoPrefab}");
        return prefab;
    }

    static void ColocarNaCena(GameObject prefab)
    {
        var cena = EditorSceneManager.OpenScene(CaminhoCena, OpenSceneMode.Single);

        // se ja existe um HUD na cena, substitui em vez de duplicar
        foreach (var raiz in cena.GetRootGameObjects())
            if (raiz.name == "HealthHUD") Object.DestroyImmediate(raiz);

        var instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instancia.name = "HealthHUD";

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log($"[hud] instancia colocada em {CaminhoCena}");
    }

    static void PreencherArray(SerializedObject so, string campo, Object[] valores)
    {
        var p = so.FindProperty(campo);
        p.arraySize = valores.Length;
        for (int i = 0; i < valores.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
    }

    static RectTransform Posicionar(Transform pai, GameObject go, Vector2 ancora, Vector2 pos, Vector2 tam)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = rt.anchorMax = ancora;
        rt.pivot = ancora;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
        return rt;
    }

    static RectTransform Vazio(Transform pai, string nome, Vector2 ancora, Vector2 pos, Vector2 tam)
        => Posicionar(pai, new GameObject(nome, typeof(RectTransform)), ancora, pos, tam);

    static Image Grafico(Transform pai, string nome, Sprite sprite, Color cor, Vector2 ancora, Vector2 pos, Vector2 tam)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Posicionar(pai, go, ancora, pos, tam);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = cor;
        img.raycastTarget = false;
        return img;
    }
}
