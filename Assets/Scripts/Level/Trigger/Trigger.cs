using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour {
    public string action = "";
    public string[] arguments = { };
    public GameObject[] targets;

    public void Initialize() {
        switch (action) {
            case "LoadStage":
                Trigger_LoadStage trig_ls = gameObject.AddComponent<Trigger_LoadStage>();
                trig_ls.Initialize(arguments[0]);
                Destroy(this);
                break;
            case "ActivateSpawner":
                Trigger_ActivateSpawner trig_as = gameObject.AddComponent<Trigger_ActivateSpawner>();
                trig_as.Initialize(targets);
                Destroy(this);
                break;
            case "DeactivateSpawner":
                Trigger_DeactivateSpawner trig_ds = gameObject.AddComponent<Trigger_DeactivateSpawner>();
                trig_ds.Initialize(targets);
                Destroy(this);
                break;
            case "Talk":
                Trigger_Talk trig_talk = gameObject.AddComponent<Trigger_Talk>();
                trig_talk.Initialize(arguments);
                Destroy(this);
                break;
        }
    }
}
