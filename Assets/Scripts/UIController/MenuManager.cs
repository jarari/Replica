﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    private bool paused = false;
    private Stack<Menu> menus = new Stack<Menu>();
    private Dictionary<string, string> lastFocus = new Dictionary<string, string>();
    private GlobalUIManager uimanager;
    public static MenuManager instance;
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        uimanager = GlobalUIManager.instance;
    }

    public void PauseToggle() {
        paused = !paused;
        if (paused)
            TimeManager.instance.Pause();
        HandleUI();
    }

    public bool IsPaused() {
        return paused;
    }

    private void HandleUI() {
        if (paused) {
            ShowMenu("MainMenu");
            Cursor.visible = true;
            PlayerHUD.HideHUD();
        }
        else {
            foreach(Menu menu in menus) {
                menu.DestroyMenu();
            }
            Cursor.visible = false;
            PlayerHUD.ShowHUD();
        }
    }

    private void CreateMenu(string classname, int priority) {
        if (uimanager == null) return;
        GameObject menu_obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Menu"), new Vector3(), new Quaternion());
        Menu menu = (Menu)menu_obj.AddComponent(Type.GetType(classname));
        menu.Initialize(priority);
        menus.Push(menu);
    }

    private IEnumerator RetrieveFocus(string menu) {
        if (uimanager == null) yield break;
        yield return new WaitForEndOfFrame();
        if (lastFocus.ContainsKey(menu) && lastFocus[menu] != null) {
            uimanager.FocusObject(uimanager.GetUIObject(lastFocus[menu]));
        }
    }

    public void ShowMenu(string menu) {
        if (uimanager == null) return;
        int priority = 50;
        if(menus.Count > 0) {
            Menu current = menus.Peek();
            if (current != null) {
                current.HideMenu();
                string currentName = current.GetMenuName();
                if (!lastFocus.ContainsKey(currentName))
                    lastFocus.Add(currentName, uimanager.GetNameFromUIObject(uimanager.GetCurrentFocus()));
                else
                    lastFocus[currentName] = uimanager.GetNameFromUIObject(uimanager.GetCurrentFocus());
                priority = current.GetPriority() + 1;
            }
        }
        CreateMenu(menu, priority);
    }

    public void GoBack() {
        if (menus.Count > 0) {
            menus.Pop().DestroyMenu();
            if (menus.Count > 0) {
                menus.Peek().ShowMenu();
                uimanager.FocusObject(null);
                StartCoroutine(RetrieveFocus(menus.Peek().GetMenuName()));
            }
        }
        if (menus.Count == 0)
            PauseToggle();
    }
}