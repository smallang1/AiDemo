using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace Nightfall.EditorTools
{
    public static class SurvivorDemoBuilder
    {
        private const string GameRoot = "Assets/Game";
        private const string ResourcesRoot = GameRoot + "/Resources";
        private const string PrefabRoot = ResourcesRoot + "/Prefabs";
        private const string UiPrefabRoot = GameRoot + "/Prefabs";
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string HudPrefabPath = UiPrefabRoot + "/SurvivorHud.prefab";
        private const string CjkFontSourcePath = GameRoot + "/Fonts/NotoSansCJKsc-Regular.otf";
        private const string CjkFontAssetPath = GameRoot + "/Fonts/NightfallCJK-SDF.asset";

        [MenuItem("工具/暮夜/重建可玩场景")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "暮夜幸存者",
                    "请先退出播放模式，再重建可玩场景。",
                    "确定");
                return;
            }

            EnsureFolder(GameRoot);
            EnsureFolder(ResourcesRoot);
            EnsureFolder(PrefabRoot);
            EnsureFolder(UiPrefabRoot);
            EnsureFolder(GameRoot + "/Fonts");
            EnsureFolder("Assets/Scenes");

            SurvivorArtBuilder.GenerateAll();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            CreatePlayerPrefab();
            CreateEnemyPrefab();
            CreateProjectilePrefab();
            CreatePickupPrefab();
            CreateHudPrefab();
            BuildScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("暮夜幸存者可玩场景已重建：" + ScenePath);
        }

        private static void CreatePlayerPrefab()
        {
            GameObject root = new GameObject("SurvivorPlayer");
            root.transform.position = Vector3.zero;

            SpriteRenderer shadow = CreateSpriteChild(
                "Shadow",
                root.transform,
                SurvivorArtBuilder.Load("fx_shadow"),
                new Color(0f, 0f, 0f, 0.34f),
                8,
                new Vector3(0f, -0.42f, 0f),
                new Vector3(1.65f, 0.66f, 1f));

            GameObject auraObject = new GameObject("Aura");
            auraObject.transform.SetParent(root.transform, false);
            SpriteRenderer aura = auraObject.AddComponent<SpriteRenderer>();
            aura.sprite = SurvivorArtBuilder.Load("fx_aura");
            aura.color = new Color(1f, 1f, 1f, 0.24f);
            aura.sortingOrder = 9;
            aura.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            aura.transform.localScale = Vector3.one * 1.25f;

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer body = CreateSpriteChild(
                "Body",
                visual.transform,
                SurvivorArtBuilder.Load("player_arcanist"),
                Color.white,
                20,
                Vector3.zero,
                Vector3.one);

            SurvivorPlayer player = root.AddComponent<SurvivorPlayer>();
            SerializedObject serialized = new SerializedObject(player);
            SetObject(serialized, "bodyRenderer", body);
            SetObject(serialized, "visualRoot", visual.transform);
            SetObject(serialized, "auraRenderer", aura);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, PrefabRoot + "/SurvivorPlayer.prefab");
        }

        private static void CreateEnemyPrefab()
        {
            GameObject root = new GameObject("SurvivorEnemy");
            SpriteRenderer shadow = CreateSpriteChild(
                "Shadow",
                root.transform,
                SurvivorArtBuilder.Load("fx_shadow"),
                new Color(0f, 0f, 0f, 0.3f),
                10,
                new Vector3(0f, -0.3f, 0f),
                new Vector3(1.1f, 0.44f, 1f));

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer body = CreateSpriteChild(
                "Body",
                visual.transform,
                SurvivorArtBuilder.Load("enemy_bat"),
                Color.white,
                30,
                Vector3.zero,
                Vector3.one);

            GameObject healthRoot = new GameObject("HealthBar");
            healthRoot.transform.SetParent(root.transform, false);
            healthRoot.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            SpriteRenderer healthBack = CreateSpriteChild(
                "Back",
                healthRoot.transform,
                SurvivorArtBuilder.Load("fx_square"),
                new Color(0.02f, 0.025f, 0.03f, 0.9f),
                40,
                Vector3.zero,
                new Vector3(1.25f, 0.13f, 1f));
            SpriteRenderer healthFill = CreateSpriteChild(
                "Fill",
                healthRoot.transform,
                SurvivorArtBuilder.Load("fx_square"),
                new Color(0.94f, 0.24f, 0.32f, 1f),
                41,
                new Vector3(0f, 0f, 0.01f),
                new Vector3(1.17f, 0.075f, 1f));

            SurvivorEnemy enemy = root.AddComponent<SurvivorEnemy>();
            SerializedObject serialized = new SerializedObject(enemy);
            SetObject(serialized, "bodyRenderer", body);
            SetObject(serialized, "shadowRenderer", shadow);
            SetObject(serialized, "visualRoot", visual.transform);
            SetObject(serialized, "healthRoot", healthRoot.transform);
            SetObject(serialized, "healthFill", healthFill);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            healthRoot.SetActive(false);
            SavePrefab(root, PrefabRoot + "/SurvivorEnemy.prefab");
        }

        private static void CreateProjectilePrefab()
        {
            GameObject root = new GameObject("SurvivorProjectile");
            SpriteRenderer body = CreateSpriteChild(
                "Body",
                root.transform,
                SurvivorArtBuilder.Load("projectile_bolt"),
                Color.white,
                55,
                Vector3.zero,
                Vector3.one);

            TrailRenderer trail = root.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.minVertexDistance = 0.04f;
            trail.widthMultiplier = 1f;
            trail.widthCurve = AnimationCurve.Linear(0f, 0.16f, 1f, 0f);
            trail.alignment = LineAlignment.View;
            trail.textureMode = LineTextureMode.Stretch;
            trail.sortingOrder = 54;
            trail.material = GetOrCreateSpriteMaterial();
            trail.startColor = new Color(0.25f, 0.95f, 0.72f, 0.8f);
            trail.endColor = new Color(0.25f, 0.95f, 0.72f, 0f);

            SurvivorProjectile projectile = root.AddComponent<SurvivorProjectile>();
            SerializedObject serialized = new SerializedObject(projectile);
            SetObject(serialized, "bodyRenderer", body);
            SetObject(serialized, "trail", trail);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, PrefabRoot + "/SurvivorProjectile.prefab");
        }

        private static void CreatePickupPrefab()
        {
            GameObject root = new GameObject("SurvivorPickup");
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer body = CreateSpriteChild(
                "Body",
                visual.transform,
                SurvivorArtBuilder.Load("pickup_gem"),
                Color.white,
                25,
                Vector3.zero,
                Vector3.one);

            SurvivorPickup pickup = root.AddComponent<SurvivorPickup>();
            SerializedObject serialized = new SerializedObject(pickup);
            SetObject(serialized, "bodyRenderer", body);
            SetObject(serialized, "visualRoot", visual.transform);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, PrefabRoot + "/SurvivorPickup.prefab");
        }

        private static void CreateHudPrefab()
        {
            Sprite panelSprite = SurvivorArtBuilder.Load("ui_panel");
            Sprite fillSprite = SurvivorArtBuilder.Load("ui_fill");
            Sprite moonSprite = SurvivorArtBuilder.Load("ui_moon");
            Sprite vignetteSprite = SurvivorArtBuilder.Load("ui_vignette");
            TMP_FontAsset font = GetOrCreateCjkFontAsset();
            if (font == null)
            {
                Debug.LogError("中文字体资产创建失败，请检查 Noto Sans CJK SC 字体文件后重试。");
                return;
            }

            GameObject canvasObject = new GameObject(
                "SurvivorHUD",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(SurvivorHud));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1920f, 1080f);

            HudRefs refs = new HudRefs();
            Image vignette = CreateImage(
                "Vignette",
                canvasRect,
                vignetteSprite,
                Color.white,
                false,
                panelSprite != null ? Image.Type.Simple : Image.Type.Simple);
            Stretch(vignette.rectTransform);

            RectTransform topBar = CreateRect(
                "TopBar",
                canvasRect,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, 112f),
                Vector2.zero);
            Image topBarImage = topBar.gameObject.AddComponent<Image>();
            topBarImage.sprite = panelSprite;
            topBarImage.type = Image.Type.Sliced;
            topBarImage.color = new Color(0.035f, 0.045f, 0.065f, 0.94f);
            topBarImage.raycastTarget = false;

            RectTransform healthCluster = CreateRect(
                "HealthCluster",
                topBar,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(388f, 82f),
                new Vector2(30f, -16f));
            TextMeshProUGUI healthHeader = CreateText(
                "HealthHeader",
                healthCluster,
                "生命",
                new Vector2(260f, 24f),
                new Vector2(0f, -4f),
                16f,
                SurvivorPalette.Teal,
                TextAlignmentOptions.Left,
                FontStyles.Bold,
                font);
            SetAnchors(healthHeader.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            healthHeader.rectTransform.anchoredPosition = new Vector2(4f, -3f);

            Image healthBack = CreateImage(
                "HealthBack",
                healthCluster,
                fillSprite,
                new Color(0.015f, 0.02f, 0.025f, 0.92f),
                false,
                Image.Type.Sliced);
            healthBack.rectTransform.anchorMin = new Vector2(0f, 0f);
            healthBack.rectTransform.anchorMax = new Vector2(0f, 0f);
            healthBack.rectTransform.pivot = new Vector2(0f, 0f);
            healthBack.rectTransform.sizeDelta = new Vector2(366f, 31f);
            healthBack.rectTransform.anchoredPosition = new Vector2(0f, 4f);

            refs.HealthFill = CreateImage(
                "HealthFill",
                healthBack.rectTransform,
                fillSprite,
                SurvivorPalette.Crimson,
                false,
                Image.Type.Filled);
            refs.HealthFill.fillMethod = Image.FillMethod.Horizontal;
            refs.HealthFill.fillOrigin = 0;
            refs.HealthFill.fillAmount = 1f;
            Stretch(refs.HealthFill.rectTransform, 3f, 3f, 3f, 3f);

            refs.HealthText = CreateText(
                "HealthText",
                healthBack.rectTransform,
                "100 / 100",
                new Vector2(260f, 26f),
                Vector2.zero,
                17f,
                SurvivorPalette.Bone,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            Stretch(refs.HealthText.rectTransform);

            refs.LevelText = CreateText(
                "LevelText",
                healthCluster,
                "等级 1",
                new Vector2(118f, 22f),
                new Vector2(274f, -7f),
                15f,
                SurvivorPalette.Gold,
                TextAlignmentOptions.Right,
                FontStyles.Bold,
                font);

            refs.TimerText = CreateText(
                "Timer",
                topBar,
                "00:00",
                new Vector2(250f, 66f),
                new Vector2(0f, -52f),
                38f,
                SurvivorPalette.Bone,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            refs.TimerText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            refs.TimerText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            refs.TimerText.rectTransform.pivot = new Vector2(0.5f, 1f);

            refs.KillsText = CreateText(
                "Kills",
                topBar,
                "魂晶  0",
                new Vector2(250f, 34f),
                new Vector2(-32f, -56f),
                18f,
                SurvivorPalette.Muted,
                TextAlignmentOptions.Right,
                FontStyles.Bold,
                font);
            refs.KillsText.rectTransform.anchorMin = new Vector2(1f, 1f);
            refs.KillsText.rectTransform.anchorMax = new Vector2(1f, 1f);
            refs.KillsText.rectTransform.pivot = new Vector2(1f, 1f);

            Image xpBack = CreateImage(
                "XpBack",
                topBar,
                fillSprite,
                new Color(0.01f, 0.015f, 0.02f, 0.96f),
                false,
                Image.Type.Sliced);
            xpBack.rectTransform.anchorMin = new Vector2(0f, 0f);
            xpBack.rectTransform.anchorMax = new Vector2(1f, 0f);
            xpBack.rectTransform.pivot = new Vector2(0.5f, 0f);
            xpBack.rectTransform.sizeDelta = new Vector2(-36f, 12f);
            xpBack.rectTransform.anchoredPosition = new Vector2(0f, 4f);

            refs.XpFill = CreateImage(
                "XpFill",
                xpBack.rectTransform,
                fillSprite,
                SurvivorPalette.Cyan,
                false,
                Image.Type.Filled);
            refs.XpFill.fillMethod = Image.FillMethod.Horizontal;
            refs.XpFill.fillOrigin = 0;
            Stretch(refs.XpFill.rectTransform, 2f, 2f, 2f, 2f);

            refs.XpText = CreateText(
                "XpText",
                topBar,
                "0 / 10",
                new Vector2(180f, 20f),
                new Vector2(-172f, 15f),
                13f,
                SurvivorPalette.Cyan,
                TextAlignmentOptions.Right,
                FontStyles.Bold,
                font);
            refs.XpText.rectTransform.anchorMin = new Vector2(1f, 1f);
            refs.XpText.rectTransform.anchorMax = new Vector2(1f, 1f);

            refs.WaveText = CreateText(
                "Wave",
                canvasRect,
                "第 1 波",
                new Vector2(160f, 34f),
                new Vector2(38f, 34f),
                18f,
                SurvivorPalette.Teal,
                TextAlignmentOptions.Left,
                FontStyles.Bold,
                font);
            refs.WaveText.rectTransform.anchorMin = Vector2.zero;
            refs.WaveText.rectTransform.anchorMax = Vector2.zero;
            refs.WaveText.rectTransform.pivot = Vector2.zero;

            TMP_Text controls = CreateText(
                "Controls",
                canvasRect,
                "方向键或 WASD 移动     自动攻击     Esc 暂停",
                new Vector2(760f, 32f),
                new Vector2(0f, 26f),
                14f,
                new Color(0.64f, 0.67f, 0.69f, 0.85f),
                TextAlignmentOptions.Center,
                FontStyles.Normal,
                font);
            controls.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            controls.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            controls.rectTransform.pivot = new Vector2(0.5f, 0f);

            refs.DamageFlash = CreateImage(
                "DamageFlash",
                canvasRect,
                null,
                new Color(1f, 0.1f, 0.2f, 0f),
                false,
                Image.Type.Simple);
            Stretch(refs.DamageFlash.rectTransform);

            CreateTitleOverlay(canvasRect, panelSprite, moonSprite, font, refs);
            CreateUpgradeOverlay(canvasRect, panelSprite, fillSprite, font, refs);
            CreatePauseOverlay(canvasRect, panelSprite, font, refs);
            CreateResultOverlay(canvasRect, panelSprite, font, refs, false);
            CreateResultOverlay(canvasRect, panelSprite, font, refs, true);
            CreateToast(canvasRect, panelSprite, font, refs);

            refs.TitlePanel.SetActive(true);
            refs.UpgradePanel.SetActive(false);
            refs.PausePanel.SetActive(false);
            refs.GameOverPanel.SetActive(false);
            refs.VictoryPanel.SetActive(false);
            refs.ToastGroup.alpha = 0f;

            SurvivorHud hud = canvasObject.GetComponent<SurvivorHud>();
            SerializedObject serialized = new SerializedObject(hud);
            SetObject(serialized, "titlePanel", refs.TitlePanel);
            SetObject(serialized, "upgradePanel", refs.UpgradePanel);
            SetObject(serialized, "pausePanel", refs.PausePanel);
            SetObject(serialized, "gameOverPanel", refs.GameOverPanel);
            SetObject(serialized, "victoryPanel", refs.VictoryPanel);
            SetObject(serialized, "timerText", refs.TimerText);
            SetObject(serialized, "killsText", refs.KillsText);
            SetObject(serialized, "levelText", refs.LevelText);
            SetObject(serialized, "waveText", refs.WaveText);
            SetObject(serialized, "healthText", refs.HealthText);
            SetObject(serialized, "xpText", refs.XpText);
            SetObject(serialized, "healthFill", refs.HealthFill);
            SetObject(serialized, "xpFill", refs.XpFill);
            SetObject(serialized, "damageFlash", refs.DamageFlash);
            SetObject(serialized, "upgradeLevelText", refs.UpgradeLevelText);
            SetObject(serialized, "startButton", refs.StartButton);
            SetObject(serialized, "resumeButton", refs.ResumeButton);
            SetObject(serialized, "restartButton", refs.RestartButton);
            SetObject(serialized, "gameOverRestartButton", refs.GameOverRestartButton);
            SetObject(serialized, "gameOverTitleButton", refs.GameOverTitleButton);
            SetObject(serialized, "victoryTitleButton", refs.VictoryTitleButton);
            SetObject(serialized, "pauseTitleButton", refs.PauseTitleButton);
            SetObject(serialized, "pauseStatsText", refs.PauseStatsText);
            SetObject(serialized, "gameOverStatsText", refs.GameOverStatsText);
            SetObject(serialized, "victoryStatsText", refs.VictoryStatsText);
            SetObject(serialized, "toastText", refs.ToastText);
            SetObject(serialized, "toastGroup", refs.ToastGroup);
            SetObjectArray(serialized, "upgradeButtons", refs.UpgradeButtons);
            SetObjectArray(serialized, "upgradeNumbers", refs.UpgradeNumbers);
            SetObjectArray(serialized, "upgradeTags", refs.UpgradeTags);
            SetObjectArray(serialized, "upgradeTitles", refs.UpgradeTitles);
            SetObjectArray(serialized, "upgradeDescriptions", refs.UpgradeDescriptions);
            SetObjectArray(serialized, "upgradeStrips", refs.UpgradeStrips);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(canvasObject, HudPrefabPath);
        }

        private static void CreateTitleOverlay(
            RectTransform canvas,
            Sprite panelSprite,
            Sprite moonSprite,
            TMP_FontAsset font,
            HudRefs refs)
        {
            RectTransform overlay = CreateOverlay("TitlePanel", canvas, 0.88f);
            refs.TitlePanel = overlay.gameObject;

            RectTransform card = CreatePanel("TitleCard", overlay, panelSprite, new Vector2(920f, 680f), Vector2.zero);
            Image moon = CreateImage(
                "BloodMoon",
                card,
                moonSprite,
                new Color(1f, 1f, 1f, 0.92f),
                false,
                Image.Type.Simple);
            moon.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            moon.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            moon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            moon.rectTransform.sizeDelta = new Vector2(190f, 190f);
            moon.rectTransform.anchoredPosition = new Vector2(0f, -142f);

            TMP_Text eyebrow = CreateText(
                "Eyebrow",
                card,
                "一场三分钟的噩梦",
                new Vector2(520f, 28f),
                new Vector2(0f, 98f),
                17f,
                SurvivorPalette.Crimson,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            eyebrow.characterSpacing = 0f;

            TMP_Text title = CreateText(
                "Title",
                card,
                "暮夜幸存者",
                new Vector2(850f, 104f),
                new Vector2(0f, 38f),
                60f,
                SurvivorPalette.Bone,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            title.characterSpacing = 0f;

            TMP_Text subtitle = CreateText(
                "Subtitle",
                card,
                "在兽潮中存活，收集魂晶，击败猩红守望者。",
                new Vector2(760f, 70f),
                new Vector2(0f, -35f),
                22f,
                SurvivorPalette.Muted,
                TextAlignmentOptions.Center,
                FontStyles.Normal,
                font,
                true);

            TMP_Text hint = CreateText(
                "Hint",
                card,
                "使用方向键或 WASD 移动\n攻击与索敌全自动",
                new Vector2(620f, 70f),
                new Vector2(0f, -117f),
                17f,
                new Color(0.68f, 0.72f, 0.72f, 1f),
                TextAlignmentOptions.Center,
                FontStyles.Normal,
                font,
                true);
            hint.lineSpacing = 8f;

            refs.StartButton = CreateButton(
                "BeginButton",
                card,
                "开始狩猎",
                new Vector2(360f, 76f),
                new Vector2(0f, -228f),
                new Color(0.09f, 0.36f, 0.32f, 1f),
                SurvivorPalette.Teal,
                font,
                panel: true,
                sprite: panelSprite,
                24f);
        }

        private static void CreateUpgradeOverlay(
            RectTransform canvas,
            Sprite panelSprite,
            Sprite fillSprite,
            TMP_FontAsset font,
            HudRefs refs)
        {
            RectTransform overlay = CreateOverlay("UpgradePanel", canvas, 0.82f);
            refs.UpgradePanel = overlay.gameObject;

            TMP_Text header = CreateText(
                "Header",
                overlay,
                "选择强化",
                new Vector2(900f, 72f),
                new Vector2(0f, 330f),
                46f,
                SurvivorPalette.Bone,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            header.characterSpacing = 0f;

            refs.UpgradeLevelText = CreateText(
                "Level",
                overlay,
                "已达到第 2 级",
                new Vector2(500f, 32f),
                new Vector2(0f, 277f),
                18f,
                SurvivorPalette.Gold,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);

            refs.UpgradeButtons = new Button[3];
            refs.UpgradeNumbers = new TMP_Text[3];
            refs.UpgradeTags = new TMP_Text[3];
            refs.UpgradeTitles = new TMP_Text[3];
            refs.UpgradeDescriptions = new TMP_Text[3];
            refs.UpgradeStrips = new Image[3];

            float[] xPositions = { -432f, 0f, 432f };
            for (int i = 0; i < 3; i++)
            {
                Button button = CreateButton(
                    "UpgradeCard" + (i + 1),
                    overlay,
                    string.Empty,
                    new Vector2(390f, 350f),
                    new Vector2(xPositions[i], -15f),
                    new Color(0.075f, 0.09f, 0.12f, 0.98f),
                    Color.white,
                    font,
                    panel: true,
                    sprite: panelSprite,
                    24f);
                refs.UpgradeButtons[i] = button;

                Image strip = CreateImage(
                    "Accent",
                    button.transform,
                    fillSprite,
                    SurvivorPalette.Teal,
                    false,
                    Image.Type.Sliced);
                strip.rectTransform.anchorMin = new Vector2(0f, 1f);
                strip.rectTransform.anchorMax = new Vector2(1f, 1f);
                strip.rectTransform.pivot = new Vector2(0.5f, 1f);
                strip.rectTransform.sizeDelta = new Vector2(-10f, 7f);
                strip.rectTransform.anchoredPosition = new Vector2(0f, -5f);
                refs.UpgradeStrips[i] = strip;

                refs.UpgradeNumbers[i] = CreateText(
                    "Number",
                    button.transform,
                    (i + 1).ToString("00"),
                    new Vector2(86f, 42f),
                    new Vector2(124f, 118f),
                    29f,
                    new Color(0.55f, 0.58f, 0.61f, 0.8f),
                    TextAlignmentOptions.Left,
                    FontStyles.Bold,
                    font);

                refs.UpgradeTags[i] = CreateText(
                    "Tag",
                    button.transform,
                    "进攻",
                    new Vector2(250f, 26f),
                    new Vector2(0f, 81f),
                    15f,
                    SurvivorPalette.Teal,
                    TextAlignmentOptions.Center,
                    FontStyles.Bold,
                    font);
                refs.UpgradeTags[i].characterSpacing = 0f;

                refs.UpgradeTitles[i] = CreateText(
                    "CardTitle",
                    button.transform,
                    "猎手印记",
                    new Vector2(330f, 48f),
                    new Vector2(0f, 37f),
                    27f,
                    SurvivorPalette.Bone,
                    TextAlignmentOptions.Center,
                    FontStyles.Bold,
                    font);

                refs.UpgradeDescriptions[i] = CreateText(
                    "Description",
                    button.transform,
                    "奥术伤害提高。",
                    new Vector2(310f, 98f),
                    new Vector2(0f, -50f),
                    18f,
                    SurvivorPalette.Muted,
                    TextAlignmentOptions.Center,
                    FontStyles.Normal,
                    font,
                    true);
                refs.UpgradeDescriptions[i].lineSpacing = 5f;
            }

            TMP_Text hotkeyHint = CreateText(
                "HotkeyHint",
                overlay,
                "按 1 / 2 / 3 选择",
                new Vector2(500f, 32f),
                new Vector2(0f, -276f),
                17f,
                new Color(0.66f, 0.69f, 0.71f, 1f),
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            hotkeyHint.characterSpacing = 0f;
        }

        private static void CreatePauseOverlay(
            RectTransform canvas,
            Sprite panelSprite,
            TMP_FontAsset font,
            HudRefs refs)
        {
            RectTransform overlay = CreateOverlay("PausePanel", canvas, 0.78f);
            refs.PausePanel = overlay.gameObject;
            RectTransform card = CreatePanel("PauseCard", overlay, panelSprite, new Vector2(580f, 420f), Vector2.zero);

            TMP_Text title = CreateText(
                "Title",
                card,
                "狩猎暂停",
                new Vector2(500f, 64f),
                new Vector2(0f, 125f),
                38f,
                SurvivorPalette.Bone,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            title.characterSpacing = 0f;

            refs.PauseStatsText = CreateText(
                "Stats",
                card,
                "时间  00:00     魂晶  0     等级  1",
                new Vector2(500f, 36f),
                new Vector2(0f, 55f),
                16f,
                SurvivorPalette.Muted,
                TextAlignmentOptions.Center,
                FontStyles.Normal,
                font);

            refs.ResumeButton = CreateButton(
                "Resume",
                card,
                "继续狩猎",
                new Vector2(330f, 66f),
                new Vector2(0f, -48f),
                new Color(0.09f, 0.36f, 0.32f, 1f),
                SurvivorPalette.Teal,
                font,
                true,
                panelSprite,
                21f);

            refs.PauseTitleButton = CreateButton(
                "TitleButton",
                card,
                "放弃本局",
                new Vector2(260f, 52f),
                new Vector2(0f, -126f),
                new Color(0.12f, 0.12f, 0.14f, 1f),
                SurvivorPalette.Muted,
                font,
                true,
                panelSprite,
                17f);
        }

        private static void CreateResultOverlay(
            RectTransform canvas,
            Sprite panelSprite,
            TMP_FontAsset font,
            HudRefs refs,
            bool victory)
        {
            string name = victory ? "VictoryPanel" : "GameOverPanel";
            RectTransform overlay = CreateOverlay(name, canvas, victory ? 0.76f : 0.84f);
            RectTransform card = CreatePanel(
                victory ? "VictoryCard" : "GameOverCard",
                overlay,
                panelSprite,
                new Vector2(680f, 500f),
                Vector2.zero);

            if (victory)
            {
                refs.VictoryPanel = overlay.gameObject;
            }
            else
            {
                refs.GameOverPanel = overlay.gameObject;
            }

            TMP_Text title = CreateText(
                "Title",
                card,
                victory ? "破晓" : "黑夜吞噬了你",
                new Vector2(620f, 76f),
                new Vector2(0f, 158f),
                41f,
                victory ? SurvivorPalette.Gold : SurvivorPalette.Crimson,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            title.characterSpacing = 0f;

            TMP_Text subtitle = CreateText(
                "Subtitle",
                card,
                victory
                    ? "猩红守望者已被击败。"
                    : "兽潮愈发凶猛，尝试另一种强化组合。",
                new Vector2(560f, 44f),
                new Vector2(0f, 97f),
                20f,
                SurvivorPalette.Muted,
                TextAlignmentOptions.Center,
                FontStyles.Normal,
                font);

            TMP_Text stats = CreateText(
                "Stats",
                card,
                "时间  00:00     魂晶  0     等级  1",
                new Vector2(600f, 36f),
                new Vector2(0f, 30f),
                17f,
                SurvivorPalette.Bone,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);

            Button restart = CreateButton(
                "Restart",
                card,
                victory ? "再次狩猎" : "重新挑战",
                new Vector2(320f, 66f),
                new Vector2(0f, -68f),
                victory
                    ? new Color(0.38f, 0.27f, 0.07f, 1f)
                    : new Color(0.37f, 0.08f, 0.12f, 1f),
                victory ? SurvivorPalette.Gold : SurvivorPalette.Crimson,
                font,
                true,
                panelSprite,
                21f);

            Button titleButton = CreateButton(
                "TitleButton",
                card,
                "返回标题",
                new Vector2(260f, 52f),
                new Vector2(0f, -148f),
                new Color(0.12f, 0.12f, 0.14f, 1f),
                SurvivorPalette.Muted,
                font,
                true,
                panelSprite,
                16f);

            if (victory)
            {
                refs.RestartButton = restart;
                refs.VictoryStatsText = stats;
                refs.VictoryTitleButton = titleButton;
            }
            else
            {
                refs.GameOverRestartButton = restart;
                refs.GameOverStatsText = stats;
                refs.GameOverTitleButton = titleButton;
            }
        }

        private static void CreateToast(
            RectTransform canvas,
            Sprite panelSprite,
            TMP_FontAsset font,
            HudRefs refs)
        {
            RectTransform root = CreateRect(
                "Toast",
                canvas,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(620f, 64f),
                new Vector2(0f, 86f));
            refs.ToastGroup = root.gameObject.AddComponent<CanvasGroup>();
            refs.ToastGroup.interactable = false;
            refs.ToastGroup.blocksRaycasts = false;

            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = panelSprite;
            background.type = Image.Type.Sliced;
            background.color = new Color(0.035f, 0.045f, 0.065f, 0.96f);
            background.raycastTarget = false;

            refs.ToastText = CreateText(
                "Label",
                root,
                "第 1 波",
                new Vector2(560f, 38f),
                Vector2.zero,
                20f,
                SurvivorPalette.Bone,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            refs.ToastText.characterSpacing = 0f;
        }

        private static void BuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject world = new GameObject("World");
            GameObject groundObject = new GameObject("Ground");
            groundObject.transform.SetParent(world.transform, false);
            SpriteRenderer ground = groundObject.AddComponent<SpriteRenderer>();
            ground.sprite = SurvivorArtBuilder.Load("ground_tile");
            ground.drawMode = SpriteDrawMode.Tiled;
            ground.tileMode = SpriteTileMode.Continuous;
            ground.size = new Vector2(64f, 38f);
            ground.sortingOrder = -100;

            GameObject decorationRoot = new GameObject("Decorations");
            decorationRoot.transform.SetParent(world.transform, false);
            CreateDecorations(decorationRoot.transform);

            GameObject gameplayRoot = new GameObject("GameplayRoot");
            gameplayRoot.transform.SetParent(world.transform, false);

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/SurvivorPlayer.prefab");
            if (playerPrefab == null)
            {
                Debug.LogError("玩家预制体缺失：" + PrefabRoot + "/SurvivorPlayer.prefab");
                return;
            }

            GameObject playerObject = PrefabUtility.InstantiatePrefab(playerPrefab, scene) as GameObject;
            if (playerObject != null)
            {
                playerObject.name = "Player";
                playerObject.transform.position = Vector3.zero;
            }

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(SurvivorCameraFollow));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = SurvivorPalette.Ink;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;

            SurvivorCameraFollow cameraFollow = cameraObject.GetComponent<SurvivorCameraFollow>();
            SerializedObject cameraSerialized = new SerializedObject(cameraFollow);
            SetObject(cameraSerialized, "target", playerObject.transform);
            SetFloat(cameraSerialized, "cameraSize", 6.6f);
            cameraSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject managerObject = new GameObject("GameManager");
            SurvivorGameManager manager = managerObject.AddComponent<SurvivorGameManager>();
            SerializedObject managerSerialized = new SerializedObject(manager);
            SetObject(managerSerialized, "gameplayRoot", gameplayRoot.transform);
            managerSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            if (hudPrefab == null)
            {
                Debug.LogError("HUD 预制体缺失：" + HudPrefabPath);
                return;
            }

            GameObject hudObject = PrefabUtility.InstantiatePrefab(hudPrefab, scene) as GameObject;
            if (hudObject != null)
            {
                hudObject.name = "SurvivorHUD";
            }

            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystemObject = new GameObject(
                    "EventSystem",
                    typeof(EventSystem),
                    typeof(StandaloneInputModule));
                eventSystemObject.transform.SetParent(world.transform, false);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            Selection.activeGameObject = managerObject;
        }

        private static void CreateDecorations(Transform parent)
        {
            Sprite rock = SurvivorArtBuilder.Load("prop_rock");
            Sprite grass = SurvivorArtBuilder.Load("prop_grass");
            System.Random random = new System.Random(7331);

            for (int i = 0; i < 95; i++)
            {
                bool isRock = i % 4 != 0;
                GameObject decoration = new GameObject(isRock ? "Grass" : "Rock");
                decoration.transform.SetParent(parent, false);
                float x = (float)(random.NextDouble() * 58.0 - 29.0);
                float y = (float)(random.NextDouble() * 33.0 - 16.5);
                decoration.transform.localPosition = new Vector3(x, y, 0f);
                SpriteRenderer renderer = decoration.AddComponent<SpriteRenderer>();
                renderer.sprite = isRock ? grass : rock;
                renderer.sortingOrder = -90;
                float scale = (float)(0.45 + random.NextDouble() * 0.6);
                decoration.transform.localScale = Vector3.one * scale;
                decoration.transform.localRotation = Quaternion.Euler(0f, 0f, (float)(random.NextDouble() * 360.0));
                Color tint = isRock
                    ? new Color(0.74f, 0.82f, 0.77f, 0.62f)
                    : new Color(0.63f, 0.80f, 0.70f, 0.48f);
                renderer.color = tint;
            }
        }

        private static RectTransform CreateOverlay(string name, RectTransform parent, float alpha)
        {
            RectTransform overlay = CreateRect(
                name,
                parent,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero);
            Image scrim = overlay.gameObject.AddComponent<Image>();
            scrim.color = new Color(0.012f, 0.016f, 0.022f, alpha);
            scrim.raycastTarget = true;
            return overlay;
        }

        private static RectTransform CreatePanel(
            string name,
            RectTransform parent,
            Sprite sprite,
            Vector2 size,
            Vector2 position)
        {
            Image image = CreateImage(
                name,
                parent,
                sprite,
                new Color(0.06f, 0.075f, 0.10f, 0.98f),
                false,
                Image.Type.Sliced);
            image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            image.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            image.rectTransform.sizeDelta = size;
            image.rectTransform.anchoredPosition = position;
            return image.rectTransform;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 size,
            Vector2 position,
            Color normalColor,
            Color accentColor,
            TMP_FontAsset font,
            bool panel,
            Sprite sprite,
            float fontSize)
        {
            Image image = CreateImage(
                name,
                parent,
                sprite,
                normalColor,
                true,
                panel ? Image.Type.Sliced : Image.Type.Simple);
            image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            image.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            image.rectTransform.sizeDelta = size;
            image.rectTransform.anchoredPosition = position;

            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.Lerp(accentColor, Color.white, 0.34f);
            colors.pressedColor = Color.Lerp(accentColor, Color.black, 0.28f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.55f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            TMP_Text text = CreateText(
                "Label",
                image.transform,
                label,
                new Vector2(size.x - 28f, size.y - 16f),
                Vector2.zero,
                fontSize,
                SurvivorPalette.Bone,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                font);
            text.characterSpacing = 1f;
            Stretch(text.rectTransform);
            return button;
        }

        private static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            string content,
            Vector2 size,
            Vector2 position,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment,
            FontStyles style,
            TMP_FontAsset font,
            bool wrap = false)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSharedMaterial = font.material;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.enableWordWrapping = wrap;
            text.overflowMode = wrap ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
            text.enableAutoSizing = false;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Sprite sprite,
            Color color,
            bool raycastTarget,
            Image.Type type)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Image image = gameObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycastTarget;
            image.type = type;
            return image;
        }

        private static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 position)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            rect.localScale = Vector3.one;
            return rect;
        }

        private static void SetAnchors(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
        }

        private static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static SpriteRenderer CreateSpriteChild(
            string name,
            Transform parent,
            Sprite sprite,
            Color color,
            int sortingOrder,
            Vector3 localPosition,
            Vector3 localScale)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localScale = localScale;
            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static Material GetOrCreateSpriteMaterial()
        {
            const string path = "Assets/Game/Art/SurvivorTrail.mat";
            EnsureFolder("Assets/Game/Art");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Sprites/Default");
            material = new Material(shader)
            {
                name = "SurvivorTrail"
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static TMP_FontAsset GetOrCreateCjkFontAsset()
        {
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(CjkFontSourcePath);
            if (sourceFont == null)
            {
                Debug.LogError("中文字体文件缺失：" + CjkFontSourcePath);
                return null;
            }

            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CjkFontAssetPath);
            if (existing != null)
            {
                bool valid =
                    existing.material != null &&
                    existing.atlasTextures != null &&
                    existing.atlasTextures.Length > 0 &&
                    existing.atlasTextures.All(texture => texture != null) &&
                    existing.HasCharacter('暮') &&
                    existing.HasCharacter('夜');
                if (valid)
                {
                    return existing;
                }

                AssetDatabase.DeleteAsset(CjkFontAssetPath);
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                false);
            if (fontAsset == null)
            {
                return null;
            }

            fontAsset.name = "NightfallCJK-SDF";
            Texture2D atlasTexture = new Texture2D(2048, 2048, TextureFormat.Alpha8, false);
            atlasTexture.name = "NightfallCJK-SDF Atlas 0";
            Color32[] clearPixels = new Color32[2048 * 2048];
            atlasTexture.SetPixels32(clearPixels);
            atlasTexture.Apply(false, false);
            fontAsset.atlasTextures[0] = atlasTexture;
            fontAsset.material.SetTexture("_MainTex", atlasTexture);

            const string representativeCharacters =
                "暮夜幸存者三分钟噩梦在兽潮中存活收集魂晶击败猩红守望者方向键或移动攻击与索敌全自动开始狩猎生命等级波次暂停选择强化已达成按返回继续放弃本局时间破晓已被黑夜吞噬愈发凶猛尝试另一种组合再次挑战标题重新奥术伤害提高攻速冷却降低最大恢复速度移速额外穿透名敌人发射枚追踪弹拾取范围每秒第苏醒了陨落坚持到到达";
            if (!fontAsset.TryAddCharacters(representativeCharacters, out string missingCharacters))
            {
                Debug.LogWarning("中文字体缺少以下字形：" + missingCharacters);
            }

            fontAsset.ReadFontAssetDefinition();
            AssetDatabase.CreateAsset(fontAsset, CjkFontAssetPath);
            if (fontAsset.atlasTextures != null)
            {
                for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                {
                    Texture2D atlas = fontAsset.atlasTextures[i];
                    if (atlas != null && !AssetDatabase.Contains(atlas))
                    {
                        atlas.name = "NightfallCJK-SDF Atlas " + i;
                        AssetDatabase.AddObjectToAsset(atlas, fontAsset);
                    }
                }
            }

            if (fontAsset.material != null && !AssetDatabase.Contains(fontAsset.material))
            {
                fontAsset.material.name = "NightfallCJK-SDF Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(CjkFontAssetPath, ImportAssetOptions.ForceSynchronousImport);

            TMP_FontAsset persisted = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CjkFontAssetPath);
            if (persisted == null || persisted.material == null ||
                persisted.atlasTextures == null || persisted.atlasTextures.Length == 0)
            {
                Debug.LogError("中文字体资产持久化校验失败：" + CjkFontAssetPath);
                return null;
            }

            return persisted;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        private static void SetObject(SerializedObject serialized, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError("Serialized property not found: " + propertyName);
                return;
            }

            property.objectReferenceValue = value;
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError("Serialized property not found: " + propertyName);
                return;
            }

            property.floatValue = value;
        }

        private static void SetObjectArray<T>(SerializedObject serialized, string propertyName, IReadOnlyList<T> values)
            where T : UnityEngine.Object
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError("Serialized property not found: " + propertyName);
                return;
            }

            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string normalized = path.Replace("\\", "/");
            string parent = Path.GetDirectoryName(normalized)?.Replace("\\", "/");
            string name = Path.GetFileName(normalized);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private sealed class HudRefs
        {
            public GameObject TitlePanel;
            public GameObject UpgradePanel;
            public GameObject PausePanel;
            public GameObject GameOverPanel;
            public GameObject VictoryPanel;
            public TMP_Text TimerText;
            public TMP_Text KillsText;
            public TMP_Text LevelText;
            public TMP_Text WaveText;
            public TMP_Text HealthText;
            public TMP_Text XpText;
            public Image HealthFill;
            public Image XpFill;
            public Image DamageFlash;
            public TMP_Text UpgradeLevelText;
            public Button[] UpgradeButtons;
            public TMP_Text[] UpgradeNumbers;
            public TMP_Text[] UpgradeTags;
            public TMP_Text[] UpgradeTitles;
            public TMP_Text[] UpgradeDescriptions;
            public Image[] UpgradeStrips;
            public Button StartButton;
            public Button ResumeButton;
            public Button RestartButton;
            public Button GameOverRestartButton;
            public Button GameOverTitleButton;
            public Button VictoryTitleButton;
            public Button PauseTitleButton;
            public TMP_Text PauseStatsText;
            public TMP_Text GameOverStatsText;
            public TMP_Text VictoryStatsText;
            public TMP_Text ToastText;
            public CanvasGroup ToastGroup;
        }
    }
}
