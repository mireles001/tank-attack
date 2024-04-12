using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shibidubi.TankAttack
{
    [DisallowMultipleComponent]
    public class LevelManager : MonoBehaviour
    {
        #region SINGLETON

        public static LevelManager Instance
        {
            get
            {
                return _instance;
            }
        }
        private static LevelManager _instance;

        #endregion

        private enum ExitUnlockType
        {
            AlwaysUnlocked,
            UseKey,
            ClearEnemies,
            UseKeyAndClearEnemies
        }

        public bool ActiveGameplay
        {
            get
            {
                return _activeGameplay;
            }
        }
        public bool PlayerHasKey
        {
            get
            {
                return _hasKey;
            }
        }
        public LevelSettings Settings
        {
            get
            {
                return _settings;
            }
        }

        public event Action LevelStart;
        public event Action LevelEnd;
        public event Action PlayerDefeat;

        [Header("Conditions of victory")]
        [SerializeField] private bool _clearAllEnemies;
        [SerializeField] private LevelExitController _levelExit;
        [SerializeField] private ExitUnlockType _levelExitUnlockType;
        [Header("Global settings")]
        [SerializeField] private LevelSettings _settings;

        private readonly float VICTORY_CONDITION_CHECK_TIC = 0.75f;

        private bool _activeGameplay;
        private bool _hasKey;
        private int _enemies;
        private Coroutine _levelManagerCoroutine;

        #region UNITY_LIFECYCLE

        private void OnDisable()
        {
            KillCoroutine();
        }

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            if (Settings == null)
            {
                return;
            }

            _levelManagerCoroutine = StartCoroutine(LevelStartWait());
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public void End()
        {
            Debug.Log("NEXT LEVEL");
            _activeGameplay = false;
            LevelEnd?.Invoke();

            KillCoroutine();
            _levelManagerCoroutine = StartCoroutine(LevelEndWait());
        }

        public void Defeat()
        {
            Debug.Log("RETRY LEVEL");
            _activeGameplay = false;
            PlayerDefeat?.Invoke();

            KillCoroutine();
            _levelManagerCoroutine = StartCoroutine(LevelRetryWait());
        }

        public void AddEnemy()
        {
            _enemies++;
            EnqueueStatusCheck();
        }

        public void RemoveEnemy()
        {
            _enemies--;
            _enemies = Mathf.Max(0, _enemies);
            EnqueueStatusCheck();
        }

        public void PickUpKey()
        {
            _hasKey = true;
            EnqueueStatusCheck();
        }

        #endregion

        private void EnqueueStatusCheck()
        {
            if (!_activeGameplay)
            {
                return;
            }

            KillCoroutine();
            _levelManagerCoroutine = StartCoroutine(StatusCheckWait());
        }

        private void StatusCheck()
        {
            if (_levelExit == null)
            {
                if (!_clearAllEnemies || _enemies == 0)
                {
                    End();
                }
            }
            else if (_levelExit.IsLocked)
            {
                _clearAllEnemies = _levelExitUnlockType == ExitUnlockType.ClearEnemies || _levelExitUnlockType == ExitUnlockType.UseKeyAndClearEnemies;

                if (_levelExitUnlockType == ExitUnlockType.AlwaysUnlocked)
                {
                    _levelExit.Unlock();
                }
                else if (_levelExitUnlockType == ExitUnlockType.UseKey && PlayerHasKey)
                {
                    _levelExit.Unlock();
                }
                else if (_levelExitUnlockType == ExitUnlockType.ClearEnemies && _enemies == 0)
                {
                    _levelExit.Unlock();
                }
                else if (_levelExitUnlockType == ExitUnlockType.UseKeyAndClearEnemies && PlayerHasKey && _enemies == 0)
                {
                    _levelExit.Unlock();
                }
            }
        }

        private IEnumerator StatusCheckWait()
        {
            yield return new WaitForSeconds(VICTORY_CONDITION_CHECK_TIC);

            StatusCheck();
        }

        private IEnumerator LevelStartWait()
        {
            yield return new WaitForSeconds(Settings.StartWaitDuration);

            _activeGameplay = true;
            LevelStart?.Invoke();

            EnqueueStatusCheck();
        }

        private IEnumerator LevelEndWait()
        {
            yield return new WaitForSeconds(Settings.EndWaitDuration);

            // TODO: Apply going to next level
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator LevelRetryWait()
        {
            yield return new WaitForSeconds(Settings.RetryWaitDuration);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void KillCoroutine()
        {
            if (_levelManagerCoroutine == null)
            {
                return;
            }

            StopCoroutine(_levelManagerCoroutine);
            _levelManagerCoroutine = null;
        }
    }
}