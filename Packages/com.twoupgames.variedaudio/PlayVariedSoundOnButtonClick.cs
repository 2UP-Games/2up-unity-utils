using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets the Button OnClick event at runtime, to trigger a VariedSound Play()
/// Why this way? If we set up the OnClick in the inspector on a prefab, Unity's prefab system misunderstands the intent.
/// Prefab variants with their own OnClick events are seen as "overriding" the OnClick that SHOULD have triggered the sound.
/// Setting at runtime resolves this behaviour.
/// If you DON'T want sound on a button click for a prefab variant, simple disable or delete this component.
/// </summary>
public class PlayVariedSoundOnButtonClick : MonoBehaviour
{
    [SerializeField]
    VariedSound _sound;
    
    [SerializeField]
    Button _button;

    void Reset()
    {
        _sound = GetComponent<VariedSound>();
        _button = GetComponent<Button>();
    }

    void OnEnable()
    {
        _button.onClick.AddListener(_sound.Play);
    }

    void OnDisable()
    {
        _button.onClick.RemoveListener(_sound.Play);
    }
}
