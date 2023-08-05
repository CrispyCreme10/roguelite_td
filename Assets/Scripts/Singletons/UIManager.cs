using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Dictionary<string, UIDocument> _documents;
    private string _activeDocumentKey;
    
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }

        _documents = new Dictionary<string, UIDocument>();
        foreach (var doc in GetComponentsInChildren<UIDocument>()) {
            if (doc.name == DocumentNames.MAIN_MENU) {
                // display flex & set key for default active
                _activeDocumentKey = doc.name;
                ShowDocument(_activeDocumentKey);
                continue;
            }
            _documents[doc.name] = doc;
            HideDocument(doc.name);
        }
    }

    public void TransitionToScreen(string targetName) {
        HideDocument(_activeDocumentKey);
        ShowDocument(targetName);
        SetActiveDocumentKey(targetName);
    }

    private void ShowDocument(string docName) {
        _documents[docName].rootVisualElement.style.display = DisplayStyle.Flex;
    }
    
    private void HideDocument(string docName) {
        _documents[docName].rootVisualElement.style.display = DisplayStyle.None;
    }

    private void SetActiveDocumentKey(string newKey) {
        _activeDocumentKey = newKey;
    }
}

public static class DocumentNames {
    // these strings should be identical to the UI Documents' GameObject's name
    public static readonly string MAIN_MENU = "MainMenu";
    public static readonly string LOADOUT = "PlayerInventory";

    public static void HandleNavigationClick(PointerUpEvent evt, string targetName) {
        UIManager.Instance.TransitionToScreen(targetName);
    }
}