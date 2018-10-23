using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Item {
    protected string className;
    protected string name;
    protected ItemTypes type;
    protected bool stackable;
    protected Sprite image;

	private JDictionary itemData;

    public Item() {
        className	= "";
        type		= ItemTypes.Consumable;
        stackable	= false;
        image		= null;
    }
    public Item(string classname) {
        className	= classname;
		itemData	= GameDataManager.instance.RootData[className];

        name		= itemData["Name"].Value<string>();
        type		= (ItemTypes) itemData["Type"].Value<int>();
        stackable	= itemData["Stackable"].Value<bool>();
        image		= Helper.GetSprite(itemData["SpritePath"].Value<string>(), itemData["SpriteName"].Value<string>());
    }

    public static Item Initialize(string classname) {
        var item = new Item(classname);
        return item;
    }

    public virtual void Use() {

    }

    public Sprite GetImage() {
        return image;
    }

    public string GetName() {
        return name;
    }

    public string GetClassName() {
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
    private static Dictionary<string, Func<string, Item>> InstanceCreateCache = new Dictionary<string, Func<string, Item>>();

    public static Item GetItemClass(string className) {
        string classname = "Item";
		JDictionary itemData = GameDataManager.instance.RootData[className];

        if(itemData["ScriptClass"])
            classname = itemData["ScriptClass"].Value<string>();

        if (!InstanceCreateCache.ContainsKey(classname)) {
            Type type = Type.GetType(classname, false, true);
            MethodInfo mi = type.GetMethod("Initialize");
            var createInstanceDelegate = (Func<string, Item>)Delegate.CreateDelegate(typeof(Func<string, Item>), mi);
            InstanceCreateCache.Add(classname, createInstanceDelegate);
        }

        return InstanceCreateCache[classname].Invoke(className);
    }
}

public enum ItemTypes {
    Weapon,
    Ammo,
    Grenade,
    Attachment_Base,
    Attachment_Bullet,
    Attachment_Blade,
    Attachment_Knuckle,
    Consumable,
    InfiniteUse
}