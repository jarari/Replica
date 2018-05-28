using UnityEngine;
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
        List<Transform> sorter = new List<Transform>();
        foreach(GameObject parent in BGParents) {
            if (parent.transform.childCount > 0) {
                foreach (Transform child in parent.transform)
                    sorter.Add(child);
                bgLength.Add(sorter.OrderBy(t => t.position.x).Last().position.x - sorter.OrderBy(t => t.position.x).First().position.x - camWidth);
                sorter.Clear();
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
        float distX = camPos.x - mapMin.x;
        float distY = camPos.y - mapMin.y - CamController.instance.GetCamSize().y;

        float a = Mathf.Atan(distY / farthestDist);

        for(int i = 0; i < BGParents.Count; i++) {
            float triA = Mathf.PI / 2f - a + Mathf.Atan(distY / bgDist[i]);
            float triHyp = Mathf.Sqrt(Mathf.Pow(bgDist[i], 2) + Mathf.Pow(distY, 2));
            Vector3 pos = BGParents[i].transform.position;
            pos.x = camPos.x - distX * bgLength[i] / mapLength - CamController.instance.GetCamSize().x;
            pos.y = camPos.y + Mathf.Cos(triA) * triHyp;
            BGParents[i].transform.position = pos;
        }
    }
}
