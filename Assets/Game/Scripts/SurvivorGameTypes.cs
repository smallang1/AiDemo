using UnityEngine;

namespace Nightfall
{
    public enum SurvivorGameState
    {
        Title,
        Running,
        LevelUp,
        Paused,
        GameOver,
        Victory
    }

    public enum EnemyArchetype
    {
        Bat,
        Ghoul,
        Brute,
        Reaper
    }

    public enum UpgradeKind
    {
        Damage,
        FireRate,
        MaxHealth,
        MoveSpeed,
        Pierce,
        ProjectileCount,
        Magnet,
        Regeneration
    }

    public struct UpgradeOption
    {
        public UpgradeKind Kind;
        public string Title;
        public string Description;
        public string Tag;
        public Color Accent;

        public UpgradeOption(
            UpgradeKind kind,
            string title,
            string description,
            string tag,
            Color accent)
        {
            Kind = kind;
            Title = title;
            Description = description;
            Tag = tag;
            Accent = accent;
        }
    }

    public static class SurvivorPalette
    {
        public static readonly Color Ink = new Color(0.035f, 0.045f, 0.065f, 1f);
        public static readonly Color Panel = new Color(0.075f, 0.09f, 0.12f, 0.96f);
        public static readonly Color PanelLight = new Color(0.13f, 0.15f, 0.19f, 0.98f);
        public static readonly Color Bone = new Color(0.91f, 0.88f, 0.78f, 1f);
        public static readonly Color Muted = new Color(0.64f, 0.66f, 0.68f, 1f);
        public static readonly Color Teal = new Color(0.25f, 0.95f, 0.72f, 1f);
        public static readonly Color Cyan = new Color(0.35f, 0.78f, 1f, 1f);
        public static readonly Color Crimson = new Color(1f, 0.24f, 0.34f, 1f);
        public static readonly Color Gold = new Color(1f, 0.72f, 0.24f, 1f);
        public static readonly Color Violet = new Color(0.72f, 0.43f, 1f, 1f);
    }
}
