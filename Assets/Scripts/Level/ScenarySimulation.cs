using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ScenarySimulation : MonoBehaviour {
    public static ScenarySimulation instance;
    private float nearLength;
    private float midLength;
    private float farLength;
    private float mapLength;
    private float nearDist;
    private float midDist;
    private float farDist;
    private float farthestDist;
    private bool init = false;

    private Vector2 mapMin;
    private Vector2 mapMax;
    private GameObject Near;
    private GameObject Mid;
    private GameObject Far;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector2 min, Vector2 max) {
        mapMin = min;
        mapMax = max;
        mapLength = max.x - min.x;
        float camWidth = ((float)Screen.width / (float)Screen.height) * Camera.main.orthographicSize * 2f;

        List<Transform> sorter = new List<Transform>();
        Near = GameObject.FindGameObjectWithTag("BG_Near");
        if(Near.transform.childCount > 0)
        {
            foreach (Transform child in Near.transform)
                sorter.Add(child);
            nearLength = sorter.OrderBy(t => t.position.x).Last().position.x - sorter.OrderBy(t => t.position.x).First().position.x - camWidth;
            sorter.Clear();
        }
        else
        {
            nearLength = 0;
        }
        nearDist = Near.transform.position.z;

        Mid = GameObject.FindGameObjectWithTag("BG_Mid");
        if (Mid.transform.childCount > 0)
        {
            foreach (Transform child in Mid.transform)
                sorter.Add(child);
            midLength = sorter.OrderBy(t => t.position.x).Last().position.x - sorter.OrderBy(t => t.position.x).First().position.x - camWidth;
            sorter.Clear();
        }
        else
        {
            midLength = 0;
        }
        midDist = Mid.transform.position.z;

        Far = GameObject.FindGameObjectWithTag("BG_Far");
        if (Far.transform.childCount > 0)
        {
            foreach (Transform child in Far.transform)
                sorter.Add(child);
            farLength = sorter.OrderBy(t => t.position.x).Last().position.x - sorter.OrderBy(t => t.position.x).First().position.x - camWidth;
            sorter.Clear();
        }
        else
        {
            farLength = 0;
        }
        farDist = Far.transform.position.z;

        farthestDist = GameObject.FindGameObjectWithTag("BG_Farthest").transform.position.z;

        foreach (GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner")) {
            spawner.GetComponent<CharacterSpawner>().Initialize();
        }
        init = true;
    }

    private void LateUpdate() {
        if (!init) return;
        Vector3 camPos = CamController.instance.GetCamPos();
        float distX = camPos.x - mapMin.x;
        float distY = camPos.y - mapMin.y - CamController.instance.GetStandardHeight();

        float a = Mathf.Atan(distY / farthestDist);
        float fartriA = Mathf.PI / 2f - a + Mathf.Atan(distY / farDist);
        float fartriHyp = Mathf.Sqrt(Mathf.Pow(farDist, 2) + Mathf.Pow(distY, 2));
        float midtriA = Mathf.PI / 2f - a + Mathf.Atan(distY / midDist);
        float midtriHyp = Mathf.Sqrt(Mathf.Pow(midDist, 2) + Mathf.Pow(distY, 2));
        float neartriA = Mathf.PI / 2f - a + Mathf.Atan(distY / nearDist);
        float neartriHyp = Mathf.Sqrt(Mathf.Pow(nearDist, 2) + Mathf.Pow(distY, 2));

        Vector3 nearPos = Near.transform.position;
        nearPos.x = camPos.x - distX * nearLength / mapLength - CamController.instance.GetCamSize().x;
        nearPos.y = camPos.y + Mathf.Cos(neartriA) * neartriHyp;
        Near.transform.position = nearPos;

        Vector3 midPos = Mid.transform.localPosition;
        midPos.x = camPos.x - distX * midLength / mapLength - CamController.instance.GetCamSize().x;
        midPos.y = camPos.y + Mathf.Cos(midtriA) * midtriHyp;
        Mid.transform.position = midPos;

        Vector3 farPos = Far.transform.localPosition;
        farPos.x = camPos.x - distX * farLength / mapLength - CamController.instance.GetCamSize().x;
        farPos.y = camPos.y + Mathf.Cos(fartriA) * fartriHyp;
        Far.transform.position = farPos;
    }
}
