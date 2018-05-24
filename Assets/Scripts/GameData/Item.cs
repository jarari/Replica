using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Item {
    protected string className;
    protected ItemTypes type;
    protected bool stackable;
    protected Sprite image;
    public Item() {
        className = "";
        type = ItemTypes.Consumable;
        stackable = false;
        image = null;
    }
    public Item(string classname) {
        className = classname;
        type = (ItemTypes)Convert.ToSingle(GameDataManager.instance.GetData("Data", classname, "Type"));
        stackable = Convert.ToBoolean(GameDataManager.instance.GetData("Data", classname, "Stackable"));
        image = Helper.GetSprite((string)GameDataManager.instance.GetData("Data", classname, "SpritePath"), (string)GameDataManager.instance.GetData("Data", classname, "SpriteName"));
    }

    public virtual void Use() {

    }

    public Sprite GetImage() {
        return image;
    }

    public string GetName() {
        return className;
    }

    public ItemTypes GetItemType() {
        return type;
    }

    public bool IsStackable() {
        return stackable;
    }
}

public static class CachedItem {
    private static Dictionary<string, Func<Item>> InstanceCreateCache = new Dictionary<string, Func<Item>>();

    public static Item GetItemClass(string className) {
        if (!InstanceCreateCache.ContainsKey(className)) {
            Type type = Type.GetType(className, false, true);
            MethodInfo mi = type.GetMethod("Initialize");
            var createInstanceDelegate = (Func<Item>)Delegate.CreateDelegate(typeof(Func<Item>), mi);
            InstanceCreateCache.Add(className, createInstanceDelegate);
        }

        return InstanceCreateCache[className].Invoke();

    }
}

public enum ItemTypes {
    Weapon,
    Consumable,
    InfiniteUse
}