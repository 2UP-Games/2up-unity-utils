using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;

public class WalkAroundGraph : MonoBehaviour
{
    [SerializeField]
    TrailRenderer _objectToWalk;

    [SerializeField]
    [Range(0.1f, 100f)]
    float _walkSpeed = 10;

    [SerializeField]
    UnityEvent _onGraphWalkComplete;
    public UnityEvent OnGraphWalkComplete => _onGraphWalkComplete;

    [SerializeField]
    bool _walkDirFlipsObject = true;

    [SerializeField]
    bool _normalizeWalkSpeed = true;

    Vector3 _nextDestination;
    public Vector3 NextDestination => _nextDestination;

    float _activeWalkSpeed;
    float _walkSpeedNormalizationMultiplier = 1;

    public Coroutine BeginWalk(Vector3[] points)
    {
        // Move object to starting position, being careful to disable it during transit so any attached components such as trail renderers don't leave a skidmark :) 
        _objectToWalk.gameObject.SetActive(false);
        _objectToWalk.transform.position = points[0];
        _objectToWalk.gameObject.SetActive(true);

        _objectToWalk.Clear();

        return StartCoroutine(DoWalk(points));
    }

    public void Pause()
    {
        _activeWalkSpeed = 0;
    }

    public void Resume()
    {
        _activeWalkSpeed = _walkSpeed;
    }

    IEnumerator DoWalk(Vector3[] points)
    {
        Debug.Log("Starting Walk");

        Resume();

        for (int i = 1; i < points.Length; i++)
        {
            float t = 0;

            Vector3 a = points[i-1];
            Vector3 b = points[i  ];
            _nextDestination = b;
            RecalculateObjectFlip();

            _objectToWalk.transform.position = a;
            RecalculateWalkSpeedNormalization();

            while (t < 1)
            {
                var pos = Vector3.Lerp(a, b, t);

                _objectToWalk.transform.position = pos;

                t += Time.deltaTime * _activeWalkSpeed * _walkSpeedNormalizationMultiplier;

                yield return null;
            }

            _objectToWalk.transform.position = b;
        }

        Debug.Log("Walk Finished");

        _onGraphWalkComplete.Invoke();
    }

    /// <summary>
    /// If '_walkDirFlipsObject' flag is raised, switch the _walkObject's X scale between 1 and -1 depending on the next point's X pos being lesser or greater than the _walkObject's X pos.
    /// </summary>
    void RecalculateObjectFlip()
    {
        if (_walkDirFlipsObject == false)
        {
            return;
        }

        if (_nextDestination.x > _objectToWalk.transform.position.x)
        {
            _objectToWalk.transform.localScale = new(-1, 1);
        }
        else
        {
            _objectToWalk.transform.localScale = new(1, 1);
        }
    }

    void RecalculateWalkSpeedNormalization()
    {
        if (_normalizeWalkSpeed == false)
        {
            _walkSpeedNormalizationMultiplier = 1;
            return;
        }

        float distance = Vector2.Distance(_objectToWalk.transform.position, _nextDestination);

        _walkSpeedNormalizationMultiplier = 1 / distance;
    }
}
