using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nightfall.EditorTools
{
    internal static class SurvivorArtBuilder
    {
        internal const string ArtRoot = "Assets/Game/Resources/Art";

        internal static void GenerateAll()
        {
            EnsureFolder("Assets/Game");
            EnsureFolder("Assets/Game/Resources");
            EnsureFolder(ArtRoot);

            CreateGroundTile();
            CreateUiPanel();
            CreateUiFill();
            CreateSquare();
            CreateSpark();
            CreateShadow();
            CreateAura();
            CreateVignette();
            CreatePlayer();
            CreateBat();
            CreateGhoul();
            CreateBrute();
            CreateReaper();
            CreateProjectile();
            CreateGem();
            CreateHeart();
            CreateMoon();
            CreateRock();
            CreateGrass();
        }

        internal static Sprite Load(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + "/" + fileName + ".png");
        }

        private static void CreateGroundTile()
        {
            System.Random random = new System.Random(3107);
            CreateTexture(
                "ground_tile",
                256,
                256,
                16f,
                Vector4.zero,
                TextureWrapMode.Repeat,
                canvas =>
                {
                    Color deep = new Color(0.075f, 0.105f, 0.11f, 1f);
                    Color moss = new Color(0.12f, 0.18f, 0.17f, 1f);
                    Color highlight = new Color(0.17f, 0.24f, 0.21f, 1f);

                    for (int y = 0; y < canvas.Height; y++)
                    {
                        for (int x = 0; x < canvas.Width; x++)
                        {
                            float broad = Mathf.PerlinNoise(x * 0.021f, y * 0.021f);
                            float fine = (float)random.NextDouble();
                            Color color = Color.Lerp(deep, moss, broad * 0.75f + fine * 0.12f);
                            if (fine > 0.94f)
                            {
                                color = Color.Lerp(color, highlight, 0.28f);
                            }

                            canvas.Set(x, y, color);
                        }
                    }

                    Color grid = new Color(0.035f, 0.055f, 0.06f, 0.30f);
                    canvas.Rect(0, 0, 256, 3, grid);
                    canvas.Rect(0, 0, 3, 256, grid);
                    canvas.Rect(124, 0, 2, 256, new Color(grid.r, grid.g, grid.b, 0.16f));
                    canvas.Rect(0, 212, 256, 2, new Color(grid.r, grid.g, grid.b, 0.13f));

                    for (int i = 0; i < 70; i++)
                    {
                        int x = random.Next(8, 248);
                        int y = random.Next(8, 248);
                        float size = random.Next(1, 4);
                        canvas.Ellipse(x, y, size * 1.8f, size, new Color(0.19f, 0.23f, 0.19f, 0.22f));
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        int x = random.Next(10, 235);
                        int y = random.Next(10, 235);
                        canvas.Line(x, y, x + random.Next(-18, 19), y + random.Next(8, 28), 1f, new Color(0.035f, 0.05f, 0.05f, 0.25f));
                    }
                });
        }

        private static void CreateUiPanel()
        {
            CreateTexture(
                "ui_panel",
                64,
                64,
                64f,
                new Vector4(18f, 18f, 18f, 18f),
                TextureWrapMode.Clamp,
                canvas =>
                {
                    canvas.RoundedRect(0, 0, 64, 64, 18f, new Color(0.12f, 0.13f, 0.15f, 0.98f));
                    canvas.RoundedOutline(1, 1, 62, 62, 17f, 1.4f, new Color(0.67f, 0.71f, 0.73f, 0.48f));
                    canvas.RoundedRect(4, 4, 56, 56, 15f, new Color(0.18f, 0.20f, 0.23f, 0.22f));
                });
        }

        private static void CreateUiFill()
        {
            CreateTexture(
                "ui_fill",
                64,
                64,
                64f,
                new Vector4(14f, 14f, 14f, 14f),
                TextureWrapMode.Clamp,
                canvas =>
                {
                    canvas.RoundedRect(0, 0, 64, 64, 14f, Color.white);
                    canvas.RoundedRect(3, 3, 58, 58, 12f, new Color(1f, 1f, 1f, 0.92f));
                });
        }

        private static void CreateSquare()
        {
            CreateTexture(
                "fx_square",
                8,
                8,
                8f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas => canvas.Rect(0, 0, 8, 8, Color.white));
        }

        private static void CreateSpark()
        {
            CreateTexture(
                "fx_spark",
                32,
                32,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    canvas.Ellipse(16f, 16f, 14f, 14f, new Color(1f, 1f, 1f, 0.12f));
                    canvas.Ellipse(16f, 16f, 8f, 8f, new Color(1f, 1f, 1f, 0.55f));
                    canvas.Ellipse(16f, 16f, 3.5f, 3.5f, Color.white);
                });
        }

        private static void CreateShadow()
        {
            CreateTexture(
                "fx_shadow",
                64,
                64,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas => canvas.Ellipse(32f, 32f, 27f, 14f, new Color(0f, 0f, 0f, 0.85f)));
        }

        private static void CreateAura()
        {
            CreateTexture(
                "fx_aura",
                128,
                128,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color edge = new Color(0.25f, 0.95f, 0.72f, 0.40f);
                    Color center = new Color(0.25f, 0.95f, 0.72f, 0.02f);
                    for (int y = 0; y < 128; y++)
                    {
                        for (int x = 0; x < 128; x++)
                        {
                            float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(64f, 64f));
                            float ring = Mathf.Exp(-Mathf.Pow((distance - 53f) / 7f, 2f));
                            float fill = Mathf.Clamp01((61f - distance) / 61f) * 0.10f;
                            Color color = Color.Lerp(center, edge, ring);
                            color.a = Mathf.Max(ring * edge.a, fill);
                            canvas.Blend(x, y, color);
                        }
                    }
                });
        }

        private static void CreateVignette()
        {
            CreateTexture(
                "ui_vignette",
                512,
                512,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    for (int y = 0; y < 512; y++)
                    {
                        for (int x = 0; x < 512; x++)
                        {
                            float nx = Mathf.Abs(x / 255.5f - 1f);
                            float ny = Mathf.Abs(y / 255.5f - 1f);
                            float edge = Mathf.Clamp01((Mathf.Max(nx, ny) - 0.42f) / 0.58f);
                            float alpha = edge * edge * 0.62f;
                            canvas.Blend(x, y, new Color(0.015f, 0.02f, 0.025f, alpha));
                        }
                    }
                });
        }

        private static void CreatePlayer()
        {
            CreateTexture(
                "player_arcanist",
                128,
                128,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color outline = new Color(0.025f, 0.05f, 0.065f, 1f);
                    Color robeDark = new Color(0.035f, 0.18f, 0.20f, 1f);
                    Color robe = new Color(0.06f, 0.43f, 0.46f, 1f);
                    Color robeLight = new Color(0.10f, 0.69f, 0.63f, 1f);
                    Color skin = new Color(0.91f, 0.77f, 0.60f, 1f);
                    Color teal = new Color(0.34f, 1f, 0.82f, 1f);

                    canvas.Ellipse(63f, 45f, 31f, 42f, outline);
                    canvas.Triangle(31f, 19f, 96f, 19f, 64f, 101f, robeDark);
                    canvas.Triangle(37f, 20f, 91f, 20f, 65f, 92f, robe);
                    canvas.Rect(52, 17, 24, 18, outline);
                    canvas.Rect(43, 11, 16, 13, new Color(0.02f, 0.035f, 0.045f, 1f));
                    canvas.Rect(69, 11, 16, 13, new Color(0.02f, 0.035f, 0.045f, 1f));
                    canvas.Line(45f, 28f, 77f, 28f, 3f, robeLight);
                    canvas.Line(58f, 20f, 63f, 54f, 2f, robeLight);

                    canvas.Ellipse(64f, 87f, 29f, 28f, outline);
                    canvas.Ellipse(64f, 85f, 23f, 22f, robeDark);
                    canvas.Ellipse(65f, 83f, 16f, 14f, skin);
                    canvas.Rect(55, 83, 20, 5, new Color(0.05f, 0.09f, 0.10f, 1f));
                    canvas.Ellipse(59f, 81f, 2.2f, 2.2f, teal);
                    canvas.Ellipse(70f, 81f, 2.2f, 2.2f, teal);

                    canvas.Line(87f, 36f, 104f, 111f, 6f, outline);
                    canvas.Line(87f, 36f, 104f, 111f, 3f, new Color(0.45f, 0.29f, 0.15f, 1f));
                    canvas.Ellipse(104f, 111f, 13f, 13f, new Color(teal.r, teal.g, teal.b, 0.14f));
                    canvas.Ellipse(104f, 111f, 8f, 8f, teal);
                    canvas.Ellipse(101f, 114f, 3f, 3f, Color.white);

                    canvas.Line(37f, 69f, 48f, 84f, 7f, outline);
                    canvas.Line(38f, 69f, 50f, 84f, 3f, robe);
                    canvas.Ellipse(32f, 64f, 7f, 7f, skin);
                    canvas.Rect(30, 55, 10, 10, skin);
                });
        }

        private static void CreateBat()
        {
            CreateTexture(
                "enemy_bat",
                128,
                128,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color outline = new Color(0.025f, 0.02f, 0.045f, 1f);
                    Color body = new Color(0.28f, 0.18f, 0.48f, 1f);
                    Color wing = new Color(0.18f, 0.12f, 0.34f, 1f);
                    Color wingLight = new Color(0.49f, 0.32f, 0.76f, 1f);
                    Color eye = new Color(1f, 0.29f, 0.39f, 1f);

                    canvas.Triangle(54f, 65f, 9f, 48f, 21f, 92f, outline);
                    canvas.Triangle(74f, 65f, 119f, 48f, 107f, 92f, outline);
                    canvas.Triangle(53f, 65f, 17f, 52f, 27f, 86f, wing);
                    canvas.Triangle(75f, 65f, 111f, 52f, 101f, 86f, wing);
                    canvas.Line(18f, 55f, 52f, 67f, 2f, wingLight);
                    canvas.Line(110f, 55f, 76f, 67f, 2f, wingLight);
                    canvas.Line(26f, 85f, 52f, 66f, 2f, wingLight);
                    canvas.Line(102f, 85f, 76f, 66f, 2f, wingLight);

                    canvas.Ellipse(64f, 63f, 24f, 31f, outline);
                    canvas.Ellipse(64f, 64f, 19f, 26f, body);
                    canvas.Triangle(47f, 78f, 54f, 104f, 63f, 80f, outline);
                    canvas.Triangle(81f, 78f, 74f, 104f, 65f, 80f, outline);
                    canvas.Triangle(51f, 79f, 55f, 96f, 61f, 81f, wingLight);
                    canvas.Triangle(77f, 79f, 73f, 96f, 67f, 81f, wingLight);
                    canvas.Ellipse(56f, 68f, 3f, 3f, eye);
                    canvas.Ellipse(72f, 68f, 3f, 3f, eye);
                    canvas.Triangle(60f, 53f, 68f, 53f, 64f, 47f, new Color(0.92f, 0.86f, 0.75f, 1f));
                });
        }

        private static void CreateGhoul()
        {
            CreateTexture(
                "enemy_ghoul",
                128,
                128,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color outline = new Color(0.03f, 0.045f, 0.035f, 1f);
                    Color skin = new Color(0.43f, 0.60f, 0.40f, 1f);
                    Color light = new Color(0.65f, 0.79f, 0.54f, 1f);
                    Color rag = new Color(0.28f, 0.25f, 0.20f, 1f);
                    Color eye = new Color(1f, 0.28f, 0.24f, 1f);

                    canvas.Line(51f, 15f, 46f, 59f, 13f, outline);
                    canvas.Line(78f, 15f, 82f, 58f, 13f, outline);
                    canvas.Line(51f, 15f, 47f, 57f, 7f, skin);
                    canvas.Line(78f, 15f, 81f, 57f, 7f, skin);
                    canvas.Ellipse(64f, 62f, 31f, 34f, outline);
                    canvas.Ellipse(64f, 65f, 25f, 29f, rag);
                    canvas.Line(34f, 69f, 17f, 43f, 11f, outline);
                    canvas.Line(94f, 69f, 111f, 43f, 11f, outline);
                    canvas.Line(35f, 69f, 20f, 44f, 6f, skin);
                    canvas.Line(93f, 69f, 108f, 44f, 6f, skin);
                    canvas.Ellipse(17f, 39f, 7f, 8f, light);
                    canvas.Ellipse(111f, 39f, 7f, 8f, light);

                    canvas.Ellipse(64f, 89f, 24f, 28f, outline);
                    canvas.Ellipse(64f, 88f, 18f, 22f, skin);
                    canvas.Ellipse(57f, 92f, 3.2f, 3.2f, eye);
                    canvas.Ellipse(71f, 92f, 3.2f, 3.2f, eye);
                    canvas.Line(57f, 80f, 71f, 80f, 2f, outline);
                    canvas.Rect(61, 76, 6, 4, Color.white);
                    canvas.Line(48f, 56f, 80f, 56f, 2f, new Color(0.16f, 0.14f, 0.11f, 1f));
                });
        }

        private static void CreateBrute()
        {
            CreateTexture(
                "enemy_brute",
                128,
                128,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color outline = new Color(0.055f, 0.025f, 0.03f, 1f);
                    Color armor = new Color(0.55f, 0.20f, 0.22f, 1f);
                    Color armorLight = new Color(0.85f, 0.38f, 0.31f, 1f);
                    Color horn = new Color(0.85f, 0.75f, 0.58f, 1f);
                    Color eye = new Color(1f, 0.78f, 0.24f, 1f);

                    canvas.Ellipse(64f, 56f, 43f, 48f, outline);
                    canvas.Ellipse(64f, 59f, 36f, 40f, armor);
                    canvas.Ellipse(61f, 63f, 28f, 29f, armorLight);
                    canvas.Rect(29, 52, 70, 9, new Color(0.24f, 0.11f, 0.12f, 1f));
                    canvas.Rect(35, 32, 58, 6, new Color(0.20f, 0.09f, 0.10f, 1f));

                    canvas.Ellipse(64f, 92f, 25f, 23f, outline);
                    canvas.Ellipse(64f, 91f, 20f, 18f, armor);
                    canvas.Triangle(45f, 103f, 31f, 119f, 55f, 108f, horn);
                    canvas.Triangle(83f, 103f, 97f, 119f, 73f, 108f, horn);
                    canvas.Ellipse(56f, 93f, 3.5f, 3.5f, eye);
                    canvas.Ellipse(72f, 93f, 3.5f, 3.5f, eye);
                    canvas.Rect(55, 82, 18, 4, outline);

                    canvas.Line(28f, 65f, 10f, 32f, 17f, outline);
                    canvas.Line(100f, 65f, 118f, 32f, 17f, outline);
                    canvas.Ellipse(10f, 27f, 10f, 10f, armorLight);
                    canvas.Ellipse(118f, 27f, 10f, 10f, armorLight);
                });
        }

        private static void CreateReaper()
        {
            CreateTexture(
                "boss_reaper",
                192,
                192,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color outline = new Color(0.035f, 0.018f, 0.028f, 1f);
                    Color cloak = new Color(0.18f, 0.065f, 0.10f, 1f);
                    Color cloth = new Color(0.38f, 0.08f, 0.13f, 1f);
                    Color trim = new Color(0.92f, 0.38f, 0.26f, 1f);
                    Color bone = new Color(0.88f, 0.84f, 0.70f, 1f);
                    Color eye = new Color(1f, 0.20f, 0.25f, 1f);

                    canvas.Triangle(38f, 14f, 154f, 14f, 97f, 148f, outline);
                    canvas.Triangle(48f, 16f, 145f, 16f, 97f, 139f, cloak);
                    canvas.Triangle(64f, 20f, 130f, 20f, 98f, 123f, cloth);
                    canvas.Line(54f, 26f, 140f, 26f, 4f, trim);
                    canvas.Line(72f, 22f, 96f, 74f, 2f, new Color(0.12f, 0.03f, 0.05f, 1f));
                    canvas.Line(123f, 22f, 99f, 74f, 2f, new Color(0.12f, 0.03f, 0.05f, 1f));

                    canvas.Ellipse(97f, 143f, 38f, 34f, outline);
                    canvas.Ellipse(97f, 142f, 30f, 28f, cloak);
                    canvas.Ellipse(97f, 140f, 21f, 18f, new Color(0.015f, 0.01f, 0.018f, 1f));
                    canvas.Ellipse(89f, 143f, 4.2f, 4.2f, eye);
                    canvas.Ellipse(105f, 143f, 4.2f, 4.2f, eye);

                    canvas.Line(146f, 24f, 164f, 168f, 7f, outline);
                    canvas.Line(146f, 24f, 164f, 168f, 3f, new Color(0.38f, 0.24f, 0.13f, 1f));
                    canvas.Line(164f, 158f, 137f, 174f, 6f, outline);
                    canvas.Line(164f, 158f, 137f, 174f, 3f, bone);
                    canvas.Ellipse(125f, 176f, 18f, 8f, outline);
                    canvas.Ellipse(127f, 176f, 15f, 5f, bone);
                    canvas.Line(30f, 67f, 15f, 35f, 8f, outline);
                    canvas.Ellipse(14f, 30f, 6f, 7f, bone);
                });
        }

        private static void CreateProjectile()
        {
            CreateTexture(
                "projectile_bolt",
                32,
                64,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color glow = new Color(0.22f, 0.95f, 0.90f, 0.18f);
                    Color core = new Color(0.42f, 1f, 0.86f, 1f);
                    canvas.Ellipse(16f, 40f, 12f, 22f, glow);
                    canvas.Ellipse(16f, 43f, 6.5f, 14f, core);
                    canvas.Ellipse(16f, 47f, 3.5f, 7f, Color.white);
                    canvas.Triangle(10f, 32f, 22f, 32f, 16f, 4f, new Color(0.18f, 0.64f, 0.74f, 0.36f));
                });
        }

        private static void CreateGem()
        {
            CreateTexture(
                "pickup_gem",
                48,
                48,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color glow = new Color(0.30f, 0.88f, 1f, 0.20f);
                    Color fill = new Color(0.34f, 0.91f, 1f, 1f);
                    Color light = new Color(0.82f, 1f, 1f, 1f);
                    canvas.Ellipse(24f, 24f, 21f, 21f, glow);
                    canvas.Diamond(24f, 24f, 15f, 21f, SurvivorPalette.Cyan);
                    canvas.Diamond(24f, 25f, 11f, 16f, fill);
                    canvas.Line(18f, 29f, 22f, 38f, 2f, light);
                    canvas.Line(22f, 38f, 30f, 27f, 2f, new Color(0.65f, 1f, 1f, 0.65f));
                });
        }

        private static void CreateHeart()
        {
            CreateTexture(
                "pickup_heal",
                48,
                48,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color glow = new Color(1f, 0.20f, 0.32f, 0.18f);
                    Color fill = new Color(0.96f, 0.20f, 0.30f, 1f);
                    canvas.Ellipse(24f, 24f, 22f, 22f, glow);
                    canvas.Ellipse(17f, 29f, 9f, 9f, fill);
                    canvas.Ellipse(31f, 29f, 9f, 9f, fill);
                    canvas.Triangle(9f, 27f, 39f, 27f, 24f, 9f, fill);
                    canvas.Ellipse(17f, 31f, 3f, 3f, new Color(1f, 0.76f, 0.78f, 1f));
                });
        }

        private static void CreateMoon()
        {
            CreateTexture(
                "ui_moon",
                256,
                256,
                64f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    canvas.Ellipse(128f, 128f, 112f, 112f, new Color(1f, 0.22f, 0.28f, 0.10f));
                    canvas.Ellipse(128f, 128f, 92f, 92f, new Color(0.78f, 0.13f, 0.18f, 1f));
                    canvas.Ellipse(111f, 142f, 24f, 22f, new Color(0.57f, 0.08f, 0.12f, 0.75f));
                    canvas.Ellipse(158f, 103f, 18f, 15f, new Color(0.56f, 0.075f, 0.12f, 0.75f));
                    canvas.Ellipse(151f, 159f, 13f, 12f, new Color(0.55f, 0.07f, 0.11f, 0.72f));
                    canvas.Ellipse(95f, 98f, 10f, 8f, new Color(0.55f, 0.07f, 0.11f, 0.72f));
                });
        }

        private static void CreateRock()
        {
            CreateTexture(
                "prop_rock",
                64,
                64,
                32f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color outline = new Color(0.025f, 0.04f, 0.04f, 0.7f);
                    Color rock = new Color(0.26f, 0.31f, 0.30f, 1f);
                    Color light = new Color(0.41f, 0.46f, 0.42f, 1f);
                    canvas.Ellipse(31f, 26f, 27f, 18f, outline);
                    canvas.Triangle(8f, 25f, 43f, 52f, 55f, 21f, rock);
                    canvas.Ellipse(37f, 35f, 16f, 13f, light);
                    canvas.Line(22f, 23f, 39f, 37f, 2f, new Color(0.12f, 0.16f, 0.15f, 0.55f));
                });
        }

        private static void CreateGrass()
        {
            CreateTexture(
                "prop_grass",
                64,
                64,
                32f,
                Vector4.zero,
                TextureWrapMode.Clamp,
                canvas =>
                {
                    Color dark = new Color(0.07f, 0.25f, 0.20f, 1f);
                    Color light = new Color(0.20f, 0.48f, 0.34f, 1f);
                    canvas.Line(23f, 9f, 12f, 44f, 3f, dark);
                    canvas.Line(31f, 8f, 26f, 55f, 3f, light);
                    canvas.Line(38f, 9f, 45f, 47f, 3f, dark);
                    canvas.Line(43f, 9f, 55f, 35f, 2f, light);
                    canvas.Line(19f, 9f, 8f, 31f, 2f, light);
                });
        }

        private static void CreateTexture(
            string fileName,
            int width,
            int height,
            float pixelsPerUnit,
            Vector4 border,
            TextureWrapMode wrapMode,
            Action<ArtCanvas> draw)
        {
            ArtCanvas canvas = new ArtCanvas(width, height);
            draw(canvas);

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            texture.name = fileName;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = wrapMode;
            texture.SetPixels(canvas.Pixels);
            texture.Apply(false, false);

            string path = ArtRoot + "/" + fileName + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError("Unable to configure generated texture: " + path);
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.spriteBorder = border;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = wrapMode;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private sealed class ArtCanvas
        {
            public readonly int Width;
            public readonly int Height;
            public readonly Color[] Pixels;

            public ArtCanvas(int width, int height)
            {
                Width = width;
                Height = height;
                Pixels = new Color[width * height];
                for (int i = 0; i < Pixels.Length; i++)
                {
                    Pixels[i] = Color.clear;
                }
            }

            public void Set(int x, int y, Color color)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                {
                    return;
                }

                Pixels[y * Width + x] = color;
            }

            public void Blend(int x, int y, Color color)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height || color.a <= 0f)
                {
                    return;
                }

                int index = y * Width + x;
                Color destination = Pixels[index];
                float sourceAlpha = Mathf.Clamp01(color.a);
                float outAlpha = sourceAlpha + destination.a * (1f - sourceAlpha);
                if (outAlpha <= 0.0001f)
                {
                    Pixels[index] = Color.clear;
                    return;
                }

                Color result = new Color(
                    (color.r * sourceAlpha + destination.r * destination.a * (1f - sourceAlpha)) / outAlpha,
                    (color.g * sourceAlpha + destination.g * destination.a * (1f - sourceAlpha)) / outAlpha,
                    (color.b * sourceAlpha + destination.b * destination.a * (1f - sourceAlpha)) / outAlpha,
                    outAlpha);
                Pixels[index] = result;
            }

            public void Rect(int x, int y, int width, int height, Color color)
            {
                for (int py = y; py < y + height; py++)
                {
                    for (int px = x; px < x + width; px++)
                    {
                        Blend(px, py, color);
                    }
                }
            }

            public void Ellipse(float cx, float cy, float rx, float ry, Color color)
            {
                int minX = Mathf.FloorToInt(cx - rx - 1f);
                int maxX = Mathf.CeilToInt(cx + rx + 1f);
                int minY = Mathf.FloorToInt(cy - ry - 1f);
                int maxY = Mathf.CeilToInt(cy + ry + 1f);

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        float dx = (x + 0.5f - cx) / rx;
                        float dy = (y + 0.5f - cy) / ry;
                        float distance = dx * dx + dy * dy;
                        if (distance > 1.15f)
                        {
                            continue;
                        }

                        float alpha = Mathf.Clamp01((1.05f - distance) * 8f);
                        Color pixel = color;
                        pixel.a *= alpha;
                        Blend(x, y, pixel);
                    }
                }
            }

            public void Line(float x0, float y0, float x1, float y1, float width, Color color)
            {
                float minX = Mathf.Min(x0, x1) - width;
                float maxX = Mathf.Max(x0, x1) + width;
                float minY = Mathf.Min(y0, y1) - width;
                float maxY = Mathf.Max(y0, y1) + width;
                Vector2 start = new Vector2(x0, y0);
                Vector2 end = new Vector2(x1, y1);
                Vector2 segment = end - start;
                float lengthSqr = Mathf.Max(0.001f, segment.sqrMagnitude);

                for (int y = Mathf.FloorToInt(minY); y <= Mathf.CeilToInt(maxY); y++)
                {
                    for (int x = Mathf.FloorToInt(minX); x <= Mathf.CeilToInt(maxX); x++)
                    {
                        Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                        float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSqr);
                        Vector2 closest = start + segment * t;
                        float distance = Vector2.Distance(point, closest);
                        float alpha = Mathf.Clamp01(width * 0.5f + 0.5f - distance);
                        if (alpha <= 0f)
                        {
                            continue;
                        }

                        Color pixel = color;
                        pixel.a *= alpha;
                        Blend(x, y, pixel);
                    }
                }
            }

            public void Triangle(Vector2 a, Vector2 b, Vector2 c, Color color)
            {
                int minX = Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x)));
                int maxX = Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
                int minY = Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y)));
                int maxY = Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y)));

                float area = Edge(a, b, c);
                if (Mathf.Abs(area) < 0.001f)
                {
                    return;
                }

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                        float w0 = Edge(b, c, point) / area;
                        float w1 = Edge(c, a, point) / area;
                        float w2 = 1f - w0 - w1;
                        if (w0 >= -0.02f && w1 >= -0.02f && w2 >= -0.02f)
                        {
                            Blend(x, y, color);
                        }
                    }
                }
            }

            public void Triangle(
                float ax,
                float ay,
                float bx,
                float by,
                float cx,
                float cy,
                Color color)
            {
                Triangle(new Vector2(ax, ay), new Vector2(bx, by), new Vector2(cx, cy), color);
            }

            public void Diamond(float cx, float cy, float rx, float ry, Color color)
            {
                int minX = Mathf.FloorToInt(cx - rx - 1f);
                int maxX = Mathf.CeilToInt(cx + rx + 1f);
                int minY = Mathf.FloorToInt(cy - ry - 1f);
                int maxY = Mathf.CeilToInt(cy + ry + 1f);
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        float distance = Mathf.Abs(x + 0.5f - cx) / rx + Mathf.Abs(y + 0.5f - cy) / ry;
                        if (distance <= 1f)
                        {
                            Color pixel = color;
                            pixel.a *= Mathf.Clamp01((1.02f - distance) * 12f);
                            Blend(x, y, pixel);
                        }
                    }
                }
            }

            public void RoundedRect(float x, float y, float width, float height, float radius, Color color)
            {
                int ix = Mathf.RoundToInt(x);
                int iy = Mathf.RoundToInt(y);
                int iw = Mathf.RoundToInt(width);
                int ih = Mathf.RoundToInt(height);
                int r = Mathf.RoundToInt(radius);
                Rect(ix + r, iy, iw - r * 2, ih, color);
                Rect(ix, iy + r, iw, ih - r * 2, color);
                Ellipse(ix + r, iy + r, r, r, color);
                Ellipse(ix + iw - r, iy + r, r, r, color);
                Ellipse(ix + r, iy + ih - r, r, r, color);
                Ellipse(ix + iw - r, iy + ih - r, r, r, color);
            }

            public void RoundedOutline(
                int x,
                int y,
                int width,
                int height,
                float radius,
                float thickness,
                Color color)
            {
                Line(x + radius, y, x + width - radius, y, thickness, color);
                Line(x + radius, y + height, x + width - radius, y + height, thickness, color);
                Line(x, y + radius, x, y + height - radius, thickness, color);
                Line(x + width, y + radius, x + width, y + height - radius, thickness, color);
                Arc(x + radius, y + radius, radius, 180f, 270f, thickness, color);
                Arc(x + width - radius, y + radius, radius, 270f, 360f, thickness, color);
                Arc(x + radius, y + height - radius, radius, 90f, 180f, thickness, color);
                Arc(x + width - radius, y + height - radius, radius, 0f, 90f, thickness, color);
            }

            private void Arc(
                float cx,
                float cy,
                float radius,
                float startDegrees,
                float endDegrees,
                float thickness,
                Color color)
            {
                Vector2 previous = PointOnCircle(cx, cy, radius, startDegrees);
                int steps = Mathf.Max(3, Mathf.CeilToInt((endDegrees - startDegrees) / 6f));
                for (int i = 1; i <= steps; i++)
                {
                    float angle = Mathf.Lerp(startDegrees, endDegrees, i / (float)steps);
                    Vector2 current = PointOnCircle(cx, cy, radius, angle);
                    Line(previous.x, previous.y, current.x, current.y, thickness, color);
                    previous = current;
                }
            }

            private static Vector2 PointOnCircle(float cx, float cy, float radius, float degrees)
            {
                float radians = degrees * Mathf.Deg2Rad;
                return new Vector2(cx + Mathf.Cos(radians) * radius, cy + Mathf.Sin(radians) * radius);
            }

            private static float Edge(Vector2 a, Vector2 b, Vector2 point)
            {
                return (point.x - a.x) * (b.y - a.y) - (point.y - a.y) * (b.x - a.x);
            }
        }
    }
}
