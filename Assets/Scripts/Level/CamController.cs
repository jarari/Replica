using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour {
    public static CamController instance;
    public bool InScriptedScene { get; set; }
    private Transform target;
    //private Vector3 lastTargetPos;
    private Vector3 camTargetPos;
    private Vector3 camPos;
    private Vector3 shakeCamVec;
    private Coroutine zoomCoroutine;
    private float tilesize = 32f;
    //private int deadzoneWidth = 200;
    //private int deadzoneHeight = 300;
    private float standardAspect;
    private int lastScreenWidth;
    private int lastScreenHeight;
    private float marginWidth = 0;
    private float marginHeight = 0;
    private int camUp = 50;

	//private int camRight = 25;
	//private float smoothValMin = 30f;
	//private float smoothValMax = 50f;
	//private float smoothDiv = 10f;
	 
	private float zoomed = 1f;
    private bool shaking = false;
    private bool zooming = false;

	//asd
	private Rigidbody2D rigb2D;
	private Vector3 lastTargetVel;
	private Vector3 targetAcceleration_lerp;
	private Vector3 targetVelocity;
	private Vector3 targetVelocity_lerp;

	private float smoothVal = 1f;
	private float smoothValAcc = 3f;
	private float smoothValVel = 4f / 3f;
	private float bobbing = 0.3f;
	//asd

	private float ClampCamX(float x) {
		return Mathf.Clamp(x, LevelManager.instance.GetMapMin().x + GetCamSize().x + tilesize / 2f, LevelManager.instance.GetMapMax().x - GetCamSize().x - tilesize / 2f);
	}
	private void FixedUpdate() {
        if (target == null || LevelManager.instance == null)
            return;

        /*
		Vector3 deltapos = target.position - lastTargetPos;
        lastTargetPos = target.position;

        if (Mathf.Abs(target.position.x - camTargetPos.x) > deadzoneWidth / 2f)
            camTargetPos.x = target.position.x + deltapos.x - deadzoneWidth / 2f * Mathf.Sign(target.position.x - camTargetPos.x);
        if (Mathf.Abs(deltapos.x) / Time.deltaTime < 1)
            camTargetPos.x = target.position.x;
        if (Mathf.Abs(target.position.y - camTargetPos.y) > deadzoneHeight / 2f)
            camTargetPos.y = target.position.y + deltapos.y - deadzoneHeight / 2f * Mathf.Sign(target.position.y - camTargetPos.y);
        if (Mathf.Abs(deltapos.y) / Time.deltaTime < 1)
            camTargetPos.y = target.position.y + camUp;

        camTargetPos.x = ClampCamX(camTargetPos.x) + camRight * Mathf.Clamp(deltapos.x / smoothDiv, -1, 1);

        Vector3 deltacam = (camTargetPos - camPos) * 100.0f * Time.deltaTime / smoothVal;
        //if(deltacam.magnitude > 0.025f) {
        //    deltacam /= Mathf.Clamp(smoothValMax - deltacam.magnitude / smoothDiv, smoothValMin, smoothValMax);
        //}
        camPos += deltacam;
		*/

        //asd
        
        if (!InScriptedScene) {
            camTargetPos = target.position;
            camTargetPos.x = ClampCamX(camTargetPos.x);
            camTargetPos.y = camTargetPos.y + camUp;

            targetVelocity = rigb2D.velocity;
            Vector3 targetAcceleration = Vector3.zero;

            if (lastTargetVel.magnitude > 0) {
                targetAcceleration = (targetVelocity - lastTargetVel) / Time.fixedDeltaTime;
            }
            lastTargetVel = targetVelocity;

            targetAcceleration_lerp += (targetAcceleration - targetAcceleration_lerp) * Time.fixedDeltaTime / smoothValAcc;
            targetAcceleration_lerp.y = targetAcceleration_lerp.y * 0.1f;

            targetVelocity_lerp = Vector3.Lerp(targetVelocity_lerp, targetVelocity * 2f, Time.fixedDeltaTime / smoothValVel);
            targetVelocity_lerp.y = targetVelocity_lerp.y * 0.6f * bobbing;

            camPos += ((camTargetPos + targetVelocity_lerp * 0.25f + targetVelocity * (0.1f + 0.1f * bobbing)) + targetAcceleration_lerp * 0.25f * bobbing - camPos) * 10.0f * Time.fixedDeltaTime / smoothVal;
        }
        else {
            camPos += (camTargetPos - camPos) * 10.0f * Time.fixedDeltaTime / smoothVal;
        }

		//camPos += (camTargetPos - camPos) * Time.deltaTime / smoothVal;
		//camPos += (camTargetPos + targetAcceleration_lerp * (0.2f * bobbing) - camPos) * 2.0f * Time.deltaTime / smoothValAcc;
		//camPos += (camTargetPos + targetVelocity * (0.4f + 0.2f * bobbing) - camPos) * Time.deltaTime / smoothValVel;
		//asd
	}

    private void LateUpdate() {
        if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
            SetupCam(Screen.width, Screen.height);

        Vector3 temptarget = camPos;
		//temptarget.x = ClampCamX(temptarget.x);
		if(shaking && !MenuManager.instance.IsPaused())
            temptarget += shakeCamVec;
        Vector3 roundedPos = new Vector3(0, 0, -10) {
            x = (Mathf.Round(temptarget.x * Helper.PixelsPerUnit) / Helper.PixelsPerUnit),
            y = (Mathf.Round(temptarget.y * Helper.PixelsPerUnit) / Helper.PixelsPerUnit)
        };
        transform.position = roundedPos;
	}

	private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        camPos = transform.position;
        SetupCam(Screen.width, Screen.height);
    }

    public void AttachCam(Transform p) {
        SetupCam(Screen.width, Screen.height);
        zoomed = 1f;
        SetCamTarget(p);
        camPos = camTargetPos;

		//asd
		rigb2D = p.GetComponent<Rigidbody2D>();
		//asd
    }

    public void SetCamTarget(Transform p) {
        target = p;
        camTargetPos = p.position + new Vector3(0, camUp, -10);
        camTargetPos.x = ClampCamX(camTargetPos.x);
    }

    public void SetCamTargetPos(Vector3 pos) {
        camTargetPos = pos;
    }
    
    public void SetupCam(int width, int height) {
        lastScreenWidth = width;
        lastScreenHeight = height;
        foreach (Camera cam in Camera.allCameras) {
            cam.orthographicSize = (GlobalUIManager.standardHeight / (1f * Helper.PixelsPerUnit)) * 0.25f;
        }
    }

    public Vector3 GetCamPos() {
        return transform.position;
    }

    public float GetCamUp() {
        return camUp;
    }

    public float GetStandardHeight() {
        return (GlobalUIManager.standardHeight / (1f * Helper.PixelsPerUnit)) * 0.25f;
    }

    public Vector2 GetCamSize() {
        return new Vector2(standardAspect * Camera.main.orthographicSize, Camera.main.orthographicSize);
    }

    public void ShakeCam(float magnitude, float duration) {
        StartCoroutine(Shake(magnitude, duration));
    }

    IEnumerator Shake(float magnitude, float duration) {

        float elapsed = 0.0f;
        shaking = true;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            float percentComplete = elapsed / duration;
            float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

            // map value to [-1, 1]
            float x = Random.value * 2.0f - 1.0f;
            float y = Random.value * 2.0f - 1.0f;
            x *= magnitude * damper;
            y *= magnitude * damper;

            shakeCamVec = new Vector3(x, y, transform.position.z);

            yield return null;
        }
        shaking = false;
    }

    public void ZoomCam(Vector3 target, float amount, float time) {
        zooming = false;
        if(zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(Zoom(target, amount, time));
    }

    public void ZoomCam(float amount, float time) {
        zooming = false;
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(Zoom(amount, time));
    }

    IEnumerator Zoom(float amount, float time) {
        yield return new WaitForEndOfFrame();
        float t = 0;
        float a = zoomed;
        float lastRun = Time.time;
        zooming = true;
        while (zoomed != amount && zooming) {
            t += (Time.time - lastRun) / time;
            zoomed = Mathf.Sin(t * Mathf.PI / 2f) * (amount - a) + a;
            if (t >= 1)
                zoomed = amount;
            float size = Mathf.Round((GlobalUIManager.standardHeight / (1f * Helper.PixelsPerUnit)) * 0.25f / zoomed);
            foreach (Camera cam in Camera.allCameras) {
                cam.orthographicSize = size;
            }
            lastRun = Time.time;
            yield return new WaitForEndOfFrame();
        }
        zooming = false;
    }

    IEnumerator Zoom(Vector3 target, float amount, float time) {
        yield return new WaitForEndOfFrame();
        Vector3 origin = transform.position;
        float t = 0;
        float a = zoomed;
        float lastRun = Time.time;
        zooming = true;
        while (zoomed != amount && zooming) {
            t += (Time.time - lastRun) / time;
            zoomed = Mathf.Sin(t * Mathf.PI / 2f) * (amount - a) + a;
            camPos = Mathf.Sin(t * Mathf.PI / 2f) * (target - origin) + origin;
            camTargetPos = camPos;
            if (t >= 1)
                zoomed = amount;
            float size = Mathf.Round((GlobalUIManager.standardHeight / (1f * Helper.PixelsPerUnit)) * 0.25f / zoomed);
            foreach (Camera cam in Camera.allCameras) {
                cam.orthographicSize = size;
            }
            lastRun = Time.time;
            yield return new WaitForEndOfFrame();
        }
        zooming = false;
    }

    public void RevertZoom(float time) {
        zooming = false;
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(Zoom(1.0f, time));
    }
}
