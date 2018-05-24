using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour {
    public string action = "";
    public string[] arguments = { };
    public void Initialize() {
        switch (action) {
            case "LoadStage":
                Trigger_LoadStage trigger = gameObject.AddComponent<Trigger_LoadStage>();
                trigger.Initialize(arguments[0]);
                Destroy(this);
                break;
        }
    }
}
