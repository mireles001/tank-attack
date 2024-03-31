using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance
    {
        get
        {
            return _instance;
        }
    }
    private static LevelManager _instance;

    private enum ExitUnlockType
    {
        AlwaysUnlocked,
        UseKey,
        ClearEnemies,
        UseKeyAndClearEnemies
    }

    public event Action LevelStart;
    public event Action LevelEnd;
    public event Action PlayerDefeat;

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

        _levelManagerCoroutine = StartCoroutine(StartWait());
    }

    private void StatusCheck()
    {
        if (!_activeGameplay)
        {
            return;
        }

        KillCoroutine();
        _levelManagerCoroutine = StartCoroutine(StatusCheckWait());
    }

    private void RunStatusCheck()
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

    public void End()
    {
        Debug.Log("NEXT LEVEL");
        _activeGameplay = false;
        LevelEnd?.Invoke();

        KillCoroutine();
        _levelManagerCoroutine = StartCoroutine(EndWait());
    }

    public void Defeat()
    {
        Debug.Log("RETRY LEVEL");
        _activeGameplay = false;
        PlayerDefeat?.Invoke();

        KillCoroutine();
        _levelManagerCoroutine = StartCoroutine(RetryWait());
    }

    public void AddEnemy()
    {
        _enemies++;
        StatusCheck();
    }

    public void RemoveEnemy()
    {
        _enemies--;
        _enemies = Mathf.Max(0, _enemies);
        StatusCheck();
    }

    public void PickUpKey()
    {
        _hasKey = true;
        StatusCheck();
    }

    private IEnumerator StatusCheckWait()
    {
        yield return new WaitForSeconds(VICTORY_CONDITION_CHECK_TIC);

        RunStatusCheck();
    }

    private IEnumerator StartWait()
    {
        yield return new WaitForSeconds(Settings.StartWaitDuration);

        _activeGameplay = true;
        LevelStart?.Invoke();

        StatusCheck();
    }

    private IEnumerator EndWait()
    {
        yield return new WaitForSeconds(Settings.EndWaitDuration);

        // TODO: Apply going to next level
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator RetryWait()
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
