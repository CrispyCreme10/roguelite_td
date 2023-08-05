using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour {

    private VisualElement _root;
    private Button _loadoutBtn;

    private void Awake() {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _loadoutBtn = _root.Q<Button>("LoadoutBtn");
        RegisterLoadoutButton();
    }

    private void RegisterLoadoutButton() {
        _loadoutBtn?.RegisterCallback<PointerUpEvent, string>(DocumentNames.HandleNavigationClick,
            DocumentNames.LOADOUT);
    }
}
