﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ScenarySimulation : MonoBehaviour {
    public static ScenarySimulation instance;
    private List<float> bgLength = new List<float>();
    private List<float> bgDist = new List<float>();
    private float mapLength;
    private float farthestDist;
    private bool init = false;

    private Vector2 mapMin;
    private Vector2 mapMax;
    private List<GameObject> BGParents;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector2 min, Vector2 max, List<GameObject> BGp) {
        mapMin = min;
        mapMax = max;
        mapLength = max.x - min.x;
        float camWidth = ((float)Screen.width / (float)Screen.height) * Camera.main.orthographicSize * 2f;

        BGParents = BGp;
        bgLength.Clear();
        bgDist.Clear();
        for(int i = 0; i < BGParents.Count; i++) {
            GameObject parent = BGParents[i];
            if (parent.transform.childCount > 0) {
                float xmax = -Mathf.Infinity;
                float xmin = Mathf.Infinity;
                foreach (Transform child in parent.transform) {
                    xmax = Mathf.Max(xmax, child.position.x);
                    xmin = Mathf.Min(xmin, child.position.x);
                }
                bgLength.Add(xmax - xmin);
            }
            else {
                bgLength.Add(0);
            }
            bgDist.Add(parent.transform.position.z);
        }

        farthestDist = GameObject.FindGameObjectWithTag("BG_Farthest").transform.position.z;
        init = true;
    }

    private void LateUpdate() {
        if (!init) return;
        Vector3 camPos = CamController.instance.GetCamPos();
        Vector2 camSize = CamController.instance.GetCamSize();
        float mapCompleted = (camPos.x - mapMin.x - camSize.x) / (mapLength - camSize.x * 2f);
        float distY = camPos.y - mapMin.y - camSize.y;
        float a = Mathf.Atan(distY / farthestDist);

        for(int i = 0; i < BGParents.Count; i++) {
            float triA = Mathf.PI / 2f - a + Mathf.Atan(distY / bgDist[i]);
            float triHyp = Mathf.Sqrt(Mathf.Pow(bgDist[i], 2) + Mathf.Pow(distY, 2));
            Vector3 pos = BGParents[i].transform.position;
            pos.x = mapMin.x + (mapLength - camSize.x * 2f) * mapCompleted - bgLength[i] * mapCompleted + camSize.x * mapCompleted * 2f;
            pos.y = camPos.y + Mathf.Cos(triA) * triHyp;
            BGParents[i].transform.position = pos;
        }
    }
}
