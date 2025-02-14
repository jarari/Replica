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
            case "Stage1Scene1":
                Trigger_Stage1Scene1 trig_s1s1 = gameObject.AddComponent<Trigger_Stage1Scene1>();
                trig_s1s1.Initialize(targets);
                Destroy(this);
                break;
            case "Stage2Scene1":
                Trigger_Stage2Scene1 trig_s2s1 = gameObject.AddComponent<Trigger_Stage2Scene1>();
                trig_s2s1.Initialize(targets);
                Destroy(this);
                break;
            case "Stage2Scene2":
                Trigger_Stage2Scene2 trig_s2s2 = gameObject.AddComponent<Trigger_Stage2Scene2>();
                trig_s2s2.Initialize(targets);
                Destroy(this);
                break;
        }
    }
}
