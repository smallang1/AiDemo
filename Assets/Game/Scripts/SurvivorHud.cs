using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nightfall
{
    public sealed class SurvivorHud : MonoBehaviour
    {
        [Header("面板")]
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        [Header("状态栏")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text killsText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image xpFill;
        [SerializeField] private Image damageFlash;

        [Header("升级")]
        [SerializeField] private TMP_Text upgradeLevelText;
        [SerializeField] private Button[] upgradeButtons = new Button[3];
        [SerializeField] private TMP_Text[] upgradeNumbers = new TMP_Text[3];
        [SerializeField] private TMP_Text[] upgradeTags = new TMP_Text[3];
        [SerializeField] private TMP_Text[] upgradeTitles = new TMP_Text[3];
        [SerializeField] private TMP_Text[] upgradeDescriptions = new TMP_Text[3];
        [SerializeField] private Image[] upgradeStrips = new Image[3];

        [Header("操作")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button gameOverTitleButton;
        [SerializeField] private Button victoryTitleButton;
        [SerializeField] private Button pauseTitleButton;

        [Header("结算")]
        [SerializeField] private TMP_Text pauseStatsText;
        [SerializeField] private TMP_Text gameOverStatsText;
        [SerializeField] private TMP_Text victoryStatsText;

        [Header("反馈")]
        [SerializeField] private TMP_Text toastText;
        [SerializeField] private CanvasGroup toastGroup;

        private SurvivorGameManager manager;
        private bool bound;
        private float damageAlpha;
        private float toastTimer;

        public void Bind(SurvivorGameManager gameManager)
        {
            manager = gameManager;
            if (bound)
            {
                return;
            }

            bound = true;
            startButton.onClick.AddListener(OnStart);
            resumeButton.onClick.AddListener(OnResume);
            restartButton.onClick.AddListener(OnRestart);
            gameOverRestartButton.onClick.AddListener(OnRestart);
            gameOverTitleButton.onClick.AddListener(OnReturnToTitle);
            victoryTitleButton.onClick.AddListener(OnReturnToTitle);
            pauseTitleButton.onClick.AddListener(OnReturnToTitle);

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int index = i;
                upgradeButtons[i].onClick.AddListener(() => manager.ChooseUpgrade(index));
            }
        }

        public void Refresh()
        {
            if (manager == null || manager.Player == null)
            {
                return;
            }

            SurvivorPlayer player = manager.Player;
            if (timerText != null)
            {
                timerText.text = FormatTime(manager.RunTime);
            }

            if (killsText != null)
            {
                killsText.text = "魂晶  " + manager.Kills;
            }

            if (levelText != null)
            {
                levelText.text = "等级 " + manager.Level;
            }

            if (waveText != null)
            {
                waveText.text = "第 " + manager.Wave + " 波";
            }

            if (healthText != null)
            {
                healthText.text = Mathf.CeilToInt(player.CurrentHealth) + " / " + Mathf.CeilToInt(player.MaxHealth);
            }

            if (xpText != null)
            {
                xpText.text = manager.CurrentXp + " / " + manager.XpToNext;
            }

            if (healthFill != null)
            {
                healthFill.fillAmount = Mathf.Clamp01(player.CurrentHealth / Mathf.Max(1f, player.MaxHealth));
            }

            if (xpFill != null)
            {
                xpFill.fillAmount = Mathf.Clamp01(manager.CurrentXp / (float)Mathf.Max(1, manager.XpToNext));
            }

            titlePanel.SetActive(manager.State == SurvivorGameState.Title);
            upgradePanel.SetActive(manager.State == SurvivorGameState.LevelUp);
            pausePanel.SetActive(manager.State == SurvivorGameState.Paused);
            gameOverPanel.SetActive(manager.State == SurvivorGameState.GameOver);
            victoryPanel.SetActive(manager.State == SurvivorGameState.Victory);

            if (manager.State == SurvivorGameState.Paused && pauseStatsText != null)
            {
                pauseStatsText.text = BuildStats();
            }

            if (manager.State == SurvivorGameState.GameOver && gameOverStatsText != null)
            {
                gameOverStatsText.text = BuildStats();
            }

            if (manager.State == SurvivorGameState.Victory && victoryStatsText != null)
            {
                victoryStatsText.text = BuildStats();
            }

            UpdateFeedback();
        }

        public void ShowUpgrade(IReadOnlyList<UpgradeOption> options, int level)
        {
            upgradePanel.SetActive(true);
            if (upgradeLevelText != null)
            {
                upgradeLevelText.text = "已达到第 " + level + " 级";
            }

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                bool hasOption = options != null && i < options.Count;
                upgradeButtons[i].gameObject.SetActive(hasOption);
                if (!hasOption)
                {
                    continue;
                }

                UpgradeOption option = options[i];
                upgradeNumbers[i].text = (i + 1).ToString("00");
                upgradeTags[i].text = option.Tag;
                upgradeTitles[i].text = option.Title;
                upgradeDescriptions[i].text = option.Description;
                upgradeTags[i].color = option.Accent;
                upgradeStrips[i].color = option.Accent;

                ColorBlock colors = upgradeButtons[i].colors;
                colors.normalColor = new Color(0.09f, 0.105f, 0.14f, 1f);
                colors.highlightedColor = Color.Lerp(option.Accent, Color.white, 0.16f);
                colors.pressedColor = Color.Lerp(option.Accent, Color.black, 0.24f);
                colors.selectedColor = colors.highlightedColor;
                upgradeButtons[i].colors = colors;
            }
        }

        public void HideUpgrade()
        {
            upgradePanel.SetActive(false);
        }

        public void ShowToast(string message)
        {
            if (toastText == null || toastGroup == null)
            {
                return;
            }

            toastText.text = message;
            toastTimer = 2.1f;
            toastGroup.alpha = 1f;
        }

        public void FlashDamage()
        {
            damageAlpha = 0.38f;
        }

        private void UpdateFeedback()
        {
            if (damageFlash != null)
            {
                damageAlpha = Mathf.Max(0f, damageAlpha - Time.unscaledDeltaTime * 1.9f);
                Color color = SurvivorPalette.Crimson;
                color.a = damageAlpha;
                damageFlash.color = color;
            }

            if (toastGroup != null && toastText != null)
            {
                if (toastTimer > 0f)
                {
                    toastTimer -= Time.unscaledDeltaTime;
                    toastGroup.alpha = Mathf.Clamp01(toastTimer * 3f);
                }
                else
                {
                    toastGroup.alpha = 0f;
                }
            }
        }

        private string BuildStats()
        {
            return "时间  " + FormatTime(manager.RunTime) +
                   "     魂晶  " + manager.Kills +
                   "     等级  " + manager.Level;
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        private void OnStart()
        {
            manager.BeginRun();
        }

        private void OnResume()
        {
            manager.Resume();
        }

        private void OnRestart()
        {
            manager.BeginRun();
        }

        private void OnReturnToTitle()
        {
            manager.ReturnToTitle();
        }
    }
}
