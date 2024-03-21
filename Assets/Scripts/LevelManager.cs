using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public event Action LevelStart;
    public event Action LevelFinish;
    public event Action LevelDefeat;
    public bool IsPlayerInputLocked { private set; get; }

    private enum ExitUnlockType
    {
        AlwaysUnlocked,
        UseKey,
        ClearEnemies,
        UseKeyAndClearEnemies
    }

    [Header("Conditions of victory")]
    [SerializeField] private bool _clearAllEnemies;
    [SerializeField] private LevelExitController _levelExit;
    [SerializeField] private ExitUnlockType _levelExitUnlockType;

    [Header("Timers")]
    [SerializeField] private float _initialWaitDuration;
    [SerializeField] private float _retryWaitDuration;
    [SerializeField] private float _nextLevelWaitDuration;

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
        _levelManagerCoroutine = StartCoroutine(InitialWait());
    }

    public void Finish()
    {
        IsPlayerInputLocked = true;
        LevelFinish?.Invoke();

        KillCoroutine();
        _levelManagerCoroutine = StartCoroutine(NextLevelWait());
    }

    public void Defeat()
    {
        IsPlayerInputLocked = true;
        LevelDefeat?.Invoke();

        KillCoroutine();
        _levelManagerCoroutine = StartCoroutine(RetryWait());
    }

    private IEnumerator InitialWait()
    {
        yield return new WaitForSeconds(_initialWaitDuration);

        IsPlayerInputLocked = true;
        LevelStart?.Invoke();
    }

    private IEnumerator NextLevelWait()
    {
        yield return new WaitForSeconds(_nextLevelWaitDuration);

        Debug.Log("Go to next level");
    }

    private IEnumerator RetryWait()
    {
        yield return new WaitForSeconds(_retryWaitDuration);

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
