using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Menu : MonoBehaviour {
    protected int priority = 50;
    protected GlobalUIManager uimanager;

    public void Initialize(int p) {
        priority = p;
        GetComponent<Canvas>().sortingOrder = p;
        uimanager = GlobalUIManager.instance;
        PopulateMenu();
    }

    protected abstract void PopulateMenu();

    public virtual void DestroyMenu() {
        Destroy(gameObject);
    }

    public string GetMenuName() {
        return GetType().Name;
    }

    public int GetPriority() {
        return priority;
    }

    public virtual void ShowMenu() {
        gameObject.SetActive(true);
    }

    public virtual void HideMenu() {
        gameObject.SetActive(false);
    }
}