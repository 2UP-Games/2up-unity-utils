# 2UP Games utility packages for Unity

These are internally-written Unity packages that we have found useful at 2UP Games. Each is small and focused.

* Atomic Values: globally accessible variables with OnValueChanged events, backed by ScriptableObjects
* ComfyUI API: Unity-friendly encapsulation of the [ComfyUI](https://github.com/comfyanonymous/ComfyUI) HTTP API
* Folder Icon: demarcate your packages with a helpful icon
* Inspectable Dictionary: serialize a dictionary with a nice Inspector interface
* Varied Audio: simple helpers for gently-varied audio
* Vector Graphics Tools: SVG parser, including a realtime path-walker

The packages are intended to be used as [embedded dependencies](https://docs.unity3d.com/Manual/upm-embed.html#embed-create). Simply copy any of these folders into your project's Packages directory.

We are not able to provide support or accept pull requests, but please feel free to fork and adapt for your own needs. The code is open source under the liberal [3-clause BSD license](https://opensource.org/license/bsd-3-clause).

## Contributors

* Calum Spring, lead programmer
* Weber Li, senior server developer
* Tim Knauf, CTO and co-founder

## Notes

Some of these packages use the (excellent!) [NaughtyAttributes package](https://github.com/dbrizov/NaughtyAttributes).

Atomic Values takes inspiration from [Unity Atoms](https://github.com/unity-atoms/unity-atoms).