using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws the 2UP logo on each Project view folder representing a 'com.twoupgames' Package
/// Heavily inspired by https://github.com/WooshiiDev/Unity-Folder-Icons/blob/main/FolderIcons/Editor/FolderIcons.cs
/// Thanks Wooshii!!
/// </summary>

/// <remarks>
/// To adapt this for your own icon and packages:
///     1) Replace Icon.png with your own
///     2) Change "Packages/com.twoupgames." below to match your own package identifier scheme.
/// </remarks>
[InitializeOnLoad]
public static class FolderIcon
{
    const string _iconPath = "Packages/com.twoupgames.projectfoldericons/icon.png";

    static Texture _icon;

    static FolderIcon()
    {
        EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
    }

    static void ProjectWindowItemOnGUI(string guid, Rect selectionRect)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);

        if (path.StartsWith("Packages/com.twoupgames.") == false)
        { 
            return;
        }

        if (path.Count(x => x == '/') != 1)
        {
            return;
        }

        //Debug.Log($"path: '{path}'");

        var icon = GetIcon();

        selectionRect.x += 12;
        selectionRect.width = 12;

        GUI.DrawTexture(selectionRect, icon, ScaleMode.ScaleToFit);
    }

    static Texture GetIcon()
    {
        if (_icon != null)
        {
            return _icon;
        }

        _icon = AssetDatabase.LoadAssetAtPath<Texture>(_iconPath);

        return _icon;
    }
}
