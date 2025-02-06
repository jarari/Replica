using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[Serializable()]
public class PlayerData
{
    public float Health;
    public string WeaponClass;
    public SlotData[] slots;
}

[Serializable()]
public class SlotData {
    public string itemName;
    public int itemCount;
    public SlotData(string n, int c) {
        itemName = n;
        itemCount = c;
    }
}

public static class Save
{
    private static Character player;
    private static BinaryFormatter bf;
    private static FileStream file;

    public static void CreateNewData() {
        bf = new BinaryFormatter();
        file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.OpenOrCreate);
        PlayerData data = new PlayerData();

        data.Health = GameDataManager.instance.GetCharacterStat("character_echo", CharacterStats.Health);
        //data.WeaponClass = player.GetWeapon().GetClass();

        List<SlotData> tempSlots = new List<SlotData>();
        tempSlots.Add(new SlotData("item_grenade", 10));
        tempSlots.Add(new SlotData("item_bullet", 30));
        data.slots = tempSlots.ToArray();

        bf.Serialize(file, data);
        file.Close();
    }

    public static void SaveData() {
        player = CharacterManager.GetPlayer();
        bf = new BinaryFormatter();
        file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.OpenOrCreate);
        PlayerData data = new PlayerData();

        data.Health = player.GetCurrentStat(CharacterStats.Health);
        //data.WeaponClass = player.GetWeapon().GetClass();

        List<SlotData> tempSlots = new List<SlotData>();
        foreach(InventorySlot inven in player.GetInventory().GetList()) {
            tempSlots.Add(new SlotData(inven.item.GetClassName(), inven.count));
        }
        data.slots = tempSlots.ToArray();

        bf.Serialize(file, data);
        file.Close();
    }

    public static void LoadData() {
        //string stage = "map_stage01";
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
            bf = new BinaryFormatter();
            player = CharacterManager.GetPlayer();

            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            player.SetCurrentStat(CharacterStats.Health, data.Health);
            //player.GiveWeapon(data.WeaponClass);

            foreach(SlotData sd in data.slots) {
                for(int i = 0; i < sd.itemCount; i++)
                    player.GetInventory().AddItem(sd.itemName);
            }
        }
        //LevelManager.instance.LoadMap(stage);
    }
}
