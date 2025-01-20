using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class VariedSound : MonoBehaviour
{
    const float SafetyBuffer = 0.3f;
 
    // When you have at least 3 clips, it sounds better if we never re-hear the same exact last clip.
    // (From ANY source, hence doing it globally, i.e., as a static.)
    static AudioClip _globalLastClip;
    
    [SerializeField]
    AudioClip[] _clips;

    [SerializeField, Range(0f, 1f)]
    float _volume = 1.0f;
    
    [SerializeField]
    float _minPitch = 0.9f;
    
    [SerializeField]
    float _maxPitch = 1.1f;
    
    [SerializeField]
    float _preDelay = 0.0f;

    [SerializeField]
    bool _playOnAwake = false;
    
    void Awake()
    {
        if (_playOnAwake)
        {
            Play();
        }
    }
    
    [Button]
    public void Play()
    {
        if (_clips == null || _clips.Length == 0)
        {
            Debug.LogWarning("No audio clips were supplied. Please add one or more audio clips.");
            return;
        }
        
        IList<AudioClip> clips;
        // If the list has only 2 sound items, it *doesn't* sound better to exclude any.
        // (In that case you just get an awkward-sounding ping-ponging back and forth of the two sounds.)
        // And obviously if the collection is only 1 thing, just play that 1 thing.
        if (_globalLastClip == null || _clips.Length <= 2)
        {
            clips = _clips;
        }
        else
        {
            clips = new List<AudioClip>(_clips);
            // _globalLastClip won't necessarily be in *this* list, if it came from another source.
            // That's fine; the method just returns false with no error and leaves the list alone, which is what we want.
            clips.Remove(_globalLastClip);
        }

        var clip = clips[Random.Range(0, clips.Count)];
        float pitch = Random.Range(_minPitch, _maxPitch);
        
        // Create separate game object that will survive a scene transition, and live until sound has finished.
        var go = new GameObject
        {
            name = $"[Audio: {clip.name}]"
        };
        DontDestroyOnLoad(go);
        
        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = _volume;
        source.pitch = pitch;
        source.spatialBlend = 0.0f; // 2D only for now
        source.playOnAwake = false;

        source.PlayDelayed(_preDelay);

        Destroy(go, _preDelay + (clip.length / pitch) + SafetyBuffer);

        _globalLastClip = clip;
    }
}
