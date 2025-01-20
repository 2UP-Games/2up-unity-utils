using NaughtyAttributes;
using System;
using UnityEngine;

public abstract class AtomicValue : ScriptableObject
{
    protected virtual void OnEnable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
        UnityEditor.EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
#endif
    }

#if UNITY_EDITOR
    void HandlePlayModeStateChange(UnityEditor.PlayModeStateChange state)
    {
        switch (state) 
        {
            // BEFORE any GO is initialized
            case UnityEditor.PlayModeStateChange.ExitingEditMode:
                Awake();
                break;

            // within/end of the first frame
            case UnityEditor.PlayModeStateChange.EnteredPlayMode:
                break;
        }
    }
#endif

    protected virtual void Awake() { }
}

public abstract class AtomicValue<T> : AtomicValue
{
    [SerializeField]
    protected T _defaultValue;

    [SerializeField]
    protected T _currentValue;

    public T CurrentValue => _currentValue;

    public Action<T> OnValueChanged = delegate { };

    public void SetValue(T newValue)
    {
        if (_currentValue == null && newValue == null)
        {
            return;
        }

        if (_currentValue != null && _currentValue.Equals(newValue))
        {
            return;
        }

        OnValueChanged.Invoke(newValue);
        _currentValue = newValue;
    }

    protected override void Awake()
    {
        _currentValue = _defaultValue;
    }
}