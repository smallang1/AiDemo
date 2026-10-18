using System.Collections.Generic;
using UnityEngine;

namespace Nightfall
{
    public sealed class SurvivorGameManager : MonoBehaviour
    {
        public static SurvivorGameManager Instance { get; private set; }

        public SurvivorGameState State { get; private set; }
        public float RunTime { get; private set; }
        public int Kills { get; private set; }
        public int Level { get; private set; }
        public int Wave { get; private set; }
        public int CurrentXp { get; private set; }
        public int XpToNext { get; private set; }
        public int PendingLevelUps { get; private set; }
        public SurvivorPlayer Player { get; private set; }
        public SurvivorHud Hud { get; private set; }
        public SurvivorCameraFollow CameraFollow { get; private set; }
        public IReadOnlyList<UpgradeOption> OfferedUpgrades => offeredUpgrades;

        public const float ArenaHalfWidth = 29f;
        public const float ArenaHalfHeight = 16.5f;
        public const float FinalBossTime = 150f;
        public const float RunLimit = 210f;

        [SerializeField] private Transform gameplayRoot;

        private readonly List<UpgradeOption> offeredUpgrades = new List<UpgradeOption>();
        private readonly List<UpgradeOption> upgradePool = new List<UpgradeOption>();
        private readonly List<SurvivorEnemy> tempEnemies = new List<SurvivorEnemy>();

        private SurvivorEnemy enemyPrefab;
        private SurvivorProjectile projectilePrefab;
        private SurvivorPickup pickupPrefab;
        private Sprite sparkSprite;
        private Sprite batSprite;
        private Sprite ghoulSprite;
        private Sprite bruteSprite;
        private Sprite reaperSprite;
        private float spawnTimer;
        private float toastCooldown;
        private int lastWave;
        private bool bossSpawned;
        private bool bossDefeated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadResources();
        }

        private void Start()
        {
            Player = FindObjectOfType<SurvivorPlayer>();
            Hud = FindObjectOfType<SurvivorHud>(true);
            CameraFollow = FindObjectOfType<SurvivorCameraFollow>();

            if (Player == null || Hud == null || CameraFollow == null)
            {
                Debug.LogError("暮夜幸存者配置不完整，请通过“工具/暮夜/重建可玩场景”重新生成场景。");
                enabled = false;
                return;
            }

            Hud.Bind(this);
            CameraFollow.SetTarget(Player.transform);
            EnterTitle();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Hud == null || Player == null)
            {
                return;
            }

            HandleGlobalInput();

            switch (State)
            {
                case SurvivorGameState.Running:
                    UpdateRun();
                    break;
                case SurvivorGameState.LevelUp:
                    HandleUpgradeInput();
                    break;
            }

            Hud.Refresh();
        }

        private void HandleGlobalInput()
        {
            if (State == SurvivorGameState.Title &&
                (SurvivorInput.GetKeyDown(KeyCode.Return) || SurvivorInput.GetKeyDown(KeyCode.Space)))
            {
                BeginRun();
                return;
            }

            if ((State == SurvivorGameState.GameOver || State == SurvivorGameState.Victory) &&
                (SurvivorInput.GetKeyDown(KeyCode.R) || SurvivorInput.GetKeyDown(KeyCode.Return)))
            {
                BeginRun();
                return;
            }

            if (SurvivorInput.GetKeyDown(KeyCode.Escape) || SurvivorInput.GetKeyDown(KeyCode.P))
            {
                if (State == SurvivorGameState.Running)
                {
                    Pause();
                }
                else if (State == SurvivorGameState.Paused)
                {
                    Resume();
                }
            }
        }

        private void UpdateRun()
        {
            RunTime += Time.deltaTime;
            toastCooldown -= Time.unscaledDeltaTime;

            int nextWave = Mathf.FloorToInt(RunTime / 30f) + 1;
            if (nextWave != lastWave)
            {
                lastWave = nextWave;
                Wave = nextWave;
                Hud.ShowToast("第 " + Wave + " 波");
            }

            if (!bossSpawned && RunTime >= FinalBossTime)
            {
                bossSpawned = true;
                SpawnEnemy(EnemyArchetype.Reaper, true);
                Hud.ShowToast("猩红守望者苏醒了");
                CameraFollow.Shake(0.45f, 1.2f);
            }

            if (bossDefeated)
            {
                TriggerVictory();
                return;
            }

            if (RunTime >= RunLimit && !bossSpawned)
            {
                SpawnEnemy(EnemyArchetype.Reaper, true);
                bossSpawned = true;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
            {
                return;
            }

            float difficulty = Mathf.Min(1f, RunTime / 150f);
            int burst = RunTime > 75f && Random.value < 0.30f ? 2 : 1;
            for (int i = 0; i < burst; i++)
            {
                SpawnEnemy(PickArchetype(difficulty), false);
            }

            spawnTimer = Mathf.Lerp(0.90f, 0.30f, difficulty) * Random.Range(0.88f, 1.12f);
        }

        private EnemyArchetype PickArchetype(float difficulty)
        {
            float roll = Random.value;
            if (RunTime > 45f && roll > 0.80f)
            {
                return EnemyArchetype.Brute;
            }

            if (RunTime > 20f && roll > 0.46f)
            {
                return EnemyArchetype.Ghoul;
            }

            return EnemyArchetype.Bat;
        }

        private void SpawnEnemy(EnemyArchetype archetype, bool boss)
        {
            if (enemyPrefab == null)
            {
                return;
            }

            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector2.right;
            }

            float distance = boss ? 10.5f : Random.Range(10.5f, 13.5f);
            Vector2 spawnPosition = (Vector2)Player.transform.position + direction * distance;
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, -ArenaHalfWidth + 1f, ArenaHalfWidth - 1f);
            spawnPosition.y = Mathf.Clamp(spawnPosition.y, -ArenaHalfHeight + 1f, ArenaHalfHeight - 1f);

            SurvivorEnemy enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, gameplayRoot);
            float difficulty = Mathf.Min(1f, RunTime / 150f);
            enemy.Configure(archetype, difficulty, boss, GetEnemySprite(archetype), this);
        }

        private Sprite GetEnemySprite(EnemyArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyArchetype.Ghoul:
                    return ghoulSprite;
                case EnemyArchetype.Brute:
                    return bruteSprite;
                case EnemyArchetype.Reaper:
                    return reaperSprite;
                default:
                    return batSprite;
            }
        }

        public void BeginRun()
        {
            ClearWorld();
            Time.timeScale = 1f;
            RunTime = 0f;
            Kills = 0;
            Level = 1;
            Wave = 1;
            CurrentXp = 0;
            XpToNext = GetXpRequirement(Level);
            PendingLevelUps = 0;
            spawnTimer = 0.5f;
            lastWave = 1;
            bossSpawned = false;
            bossDefeated = false;
            toastCooldown = 0f;
            Player.ResetForRun();
            Player.transform.position = Vector3.zero;
            CameraFollow.PlaceImmediately();
            State = SurvivorGameState.Running;
            Hud.ShowToast("坚持到猩红守望者陨落");
        }

        public void ReturnToTitle()
        {
            ClearWorld();
            Time.timeScale = 1f;
            Player.ResetForRun();
            Player.transform.position = Vector3.zero;
            CameraFollow.PlaceImmediately();
            EnterTitle();
        }

        private void EnterTitle()
        {
            Time.timeScale = 1f;
            State = SurvivorGameState.Title;
            RunTime = 0f;
            Kills = 0;
            Level = 1;
            Wave = 1;
            CurrentXp = 0;
            XpToNext = GetXpRequirement(Level);
            PendingLevelUps = 0;
        }

        private void Pause()
        {
            State = SurvivorGameState.Paused;
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            if (State != SurvivorGameState.Paused)
            {
                return;
            }

            State = SurvivorGameState.Running;
            Time.timeScale = 1f;
        }

        public void AddXp(int amount)
        {
            if (State != SurvivorGameState.Running)
            {
                return;
            }

            CurrentXp += amount;
            while (CurrentXp >= XpToNext)
            {
                CurrentXp -= XpToNext;
                Level++;
                PendingLevelUps++;
                XpToNext = GetXpRequirement(Level);
            }

            if (PendingLevelUps > 0 && State == SurvivorGameState.Running)
            {
                ShowNextUpgrade();
            }
        }

        private int GetXpRequirement(int level)
        {
            return Mathf.RoundToInt(7f + level * 3.6f + Mathf.Pow(level, 1.16f));
        }

        private void ShowNextUpgrade()
        {
            State = SurvivorGameState.LevelUp;
            Time.timeScale = 0f;
            BuildUpgradeChoices();
            Hud.ShowUpgrade(offeredUpgrades, Level);
        }

        private void BuildUpgradeChoices()
        {
            upgradePool.Clear();
            upgradePool.Add(new UpgradeOption(
                UpgradeKind.Damage,
                "猎手印记",
                "奥术伤害提高 18%。",
                "进攻",
                SurvivorPalette.Crimson));
            upgradePool.Add(new UpgradeOption(
                UpgradeKind.FireRate,
                "急速咏唱",
                "自动攻击冷却降低 12%。",
                "节奏",
                SurvivorPalette.Cyan));
            upgradePool.Add(new UpgradeOption(
                UpgradeKind.MaxHealth,
                "血之誓约",
                "最大生命提高 20，并恢复 20 生命。",
                "生存",
                SurvivorPalette.Crimson));
            upgradePool.Add(new UpgradeOption(
                UpgradeKind.MoveSpeed,
                "夜行步",
                "移动速度提高 8%。",
                "机动",
                SurvivorPalette.Teal));
            upgradePool.Add(new UpgradeOption(
                UpgradeKind.Pierce,
                "裂魂符文",
                "投射物额外穿透 1 名敌人。",
                "穿透",
                SurvivorPalette.Violet));
            upgradePool.Add(new UpgradeOption(
                UpgradeKind.ProjectileCount,
                "双生獠牙",
                "额外发射 1 枚追踪弹。",
                "多重射击",
                SurvivorPalette.Gold));
            upgradePool.Add(new UpgradeOption(
                UpgradeKind.Magnet,
                "墓园磁石",
                "魂晶拾取范围提高 25%。",
                "辅助",
                SurvivorPalette.Teal));
            upgradePool.Add(new UpgradeOption(
                UpgradeKind.Regeneration,
                "不灭脉动",
                "每秒恢复 0.35 生命。",
                "恢复",
                SurvivorPalette.Teal));

            offeredUpgrades.Clear();
            while (offeredUpgrades.Count < 3 && upgradePool.Count > 0)
            {
                int index = Random.Range(0, upgradePool.Count);
                offeredUpgrades.Add(upgradePool[index]);
                upgradePool.RemoveAt(index);
            }
        }

        public void ChooseUpgrade(int index)
        {
            if (State != SurvivorGameState.LevelUp || index < 0 || index >= offeredUpgrades.Count)
            {
                return;
            }

            ApplyUpgrade(offeredUpgrades[index].Kind);
            PendingLevelUps = Mathf.Max(0, PendingLevelUps - 1);

            if (PendingLevelUps > 0)
            {
                BuildUpgradeChoices();
                Hud.ShowUpgrade(offeredUpgrades, Level);
                return;
            }

            State = SurvivorGameState.Running;
            Time.timeScale = 1f;
            Hud.HideUpgrade();
        }

        private void ApplyUpgrade(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage:
                    Player.AddDamage(0.18f);
                    break;
                case UpgradeKind.FireRate:
                    Player.AddFireRate(0.12f);
                    break;
                case UpgradeKind.MaxHealth:
                    Player.AddMaxHealth(20f, true);
                    break;
                case UpgradeKind.MoveSpeed:
                    Player.AddMoveSpeed(0.08f);
                    break;
                case UpgradeKind.Pierce:
                    Player.AddPierce(1);
                    break;
                case UpgradeKind.ProjectileCount:
                    Player.AddProjectile();
                    break;
                case UpgradeKind.Magnet:
                    Player.AddPickupRadius(0.25f);
                    break;
                case UpgradeKind.Regeneration:
                    Player.AddRegeneration(0.35f);
                    break;
            }
        }

        private void HandleUpgradeInput()
        {
            if (SurvivorInput.GetKeyDown(KeyCode.Alpha1) || SurvivorInput.GetKeyDown(KeyCode.Keypad1))
            {
                ChooseUpgrade(0);
            }
            else if (SurvivorInput.GetKeyDown(KeyCode.Alpha2) || SurvivorInput.GetKeyDown(KeyCode.Keypad2))
            {
                ChooseUpgrade(1);
            }
            else if (SurvivorInput.GetKeyDown(KeyCode.Alpha3) || SurvivorInput.GetKeyDown(KeyCode.Keypad3))
            {
                ChooseUpgrade(2);
            }
        }

        public void SpawnProjectile(Vector2 position, Vector2 direction, float damage, float speed, int pierce)
        {
            if (projectilePrefab == null || gameplayRoot == null)
            {
                return;
            }

            SurvivorProjectile projectile = Instantiate(projectilePrefab, position, Quaternion.identity, gameplayRoot);
            projectile.Initialize(this, direction, damage, speed, pierce);
        }

        public void SpawnPickup(Vector2 position, int value, bool heal)
        {
            if (pickupPrefab == null || gameplayRoot == null)
            {
                return;
            }

            SurvivorPickup pickup = Instantiate(pickupPrefab, position, Quaternion.identity, gameplayRoot);
            pickup.Initialize(this, value, heal);
        }

        public void OnEnemyKilled(SurvivorEnemy enemy, Vector2 position)
        {
            Kills++;
            SpawnBurst(position, enemy.IsBoss ? SurvivorPalette.Gold : SurvivorPalette.Crimson, enemy.IsBoss ? 24 : 6);
            CameraFollow.Shake(enemy.IsBoss ? 0.55f : 0.08f, enemy.IsBoss ? 1.2f : 0.15f);

            if (enemy.IsBoss)
            {
                bossDefeated = true;
                Hud.ShowToast("猩红守望者已陨落");
                SpawnPickup(position, 20, true);
            }
            else
            {
                SpawnPickup(position, enemy.XpValue, false);
            }

            if (Kills % 35 == 0)
            {
                SpawnPickup(position + Vector2.up, 18, true);
            }
        }

        public void PlayerDied()
        {
            if (State == SurvivorGameState.GameOver)
            {
                return;
            }

            State = SurvivorGameState.GameOver;
            Time.timeScale = 0f;
            CameraFollow.Shake(0.5f, 1f);
        }

        private void TriggerVictory()
        {
            if (State == SurvivorGameState.Victory)
            {
                return;
            }

            State = SurvivorGameState.Victory;
            Time.timeScale = 0f;
        }

        public void SpawnBurst(Vector2 position, Color color, int count)
        {
            if (sparkSprite == null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                GameObject spark = new GameObject("Spark");
                spark.transform.SetParent(gameplayRoot, false);
                spark.transform.position = position;
                SpriteRenderer renderer = spark.AddComponent<SpriteRenderer>();
                renderer.sprite = sparkSprite;
                renderer.color = color;
                renderer.sortingOrder = 60;
                SurvivorSpark effect = spark.AddComponent<SurvivorSpark>();
                Vector2 direction = Random.insideUnitCircle.normalized;
                effect.Initialize(direction * Random.Range(1.8f, 5.5f), Random.Range(0.18f, 0.42f), Random.Range(0.08f, 0.2f));
            }
        }

        public void ShowToast(string message)
        {
            if (Hud != null)
            {
                Hud.ShowToast(message);
            }
        }

        public void ApplyCameraShake(float intensity, float duration)
        {
            CameraFollow.Shake(intensity, duration);
        }

        public void FlashDamage()
        {
            Hud.FlashDamage();
        }

        private void ClearWorld()
        {
            if (gameplayRoot == null)
            {
                return;
            }

            for (int i = gameplayRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(gameplayRoot.GetChild(i).gameObject);
            }
        }

        private void LoadResources()
        {
            enemyPrefab = Resources.Load<SurvivorEnemy>("Prefabs/SurvivorEnemy");
            projectilePrefab = Resources.Load<SurvivorProjectile>("Prefabs/SurvivorProjectile");
            pickupPrefab = Resources.Load<SurvivorPickup>("Prefabs/SurvivorPickup");
            sparkSprite = Resources.Load<Sprite>("Art/fx_spark");
            batSprite = Resources.Load<Sprite>("Art/enemy_bat");
            ghoulSprite = Resources.Load<Sprite>("Art/enemy_ghoul");
            bruteSprite = Resources.Load<Sprite>("Art/enemy_brute");
            reaperSprite = Resources.Load<Sprite>("Art/boss_reaper");

            if (gameplayRoot == null)
            {
                GameObject root = GameObject.Find("GameplayRoot");
                if (root != null)
                {
                    gameplayRoot = root.transform;
                }
            }
        }

#if UNITY_EDITOR
        private void UpdateEditorShortcuts()
        {
            if (SurvivorInput.GetKeyDown(KeyCode.F8) && State == SurvivorGameState.Running)
            {
                RunTime = Mathf.Max(RunTime, FinalBossTime - 1f);
            }

            if (SurvivorInput.GetKeyDown(KeyCode.F9) && State == SurvivorGameState.Running)
            {
                AddXp(XpToNext);
            }
        }
#endif

        private void LateUpdate()
        {
#if UNITY_EDITOR
            UpdateEditorShortcuts();
#endif
        }
    }
}
