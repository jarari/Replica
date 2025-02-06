using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour {
    public static CamController instance;
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
        if (target == null || LevelManager.instance == null || zoomed != 1f)
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
		
		targetVelocity = rigb2D.velocity;
		Vector3 targetAcceleration = Vector3.zero;

		if(lastTargetVel.magnitude > 0) {
			targetAcceleration = (targetVelocity - lastTargetVel) / Time.fixedDeltaTime;
		}
		lastTargetVel = targetVelocity;

		targetAcceleration_lerp += (targetAcceleration - targetAcceleration_lerp) * Time.fixedDeltaTime / smoothValAcc;
		targetAcceleration_lerp.y = targetAcceleration_lerp.y * 0.1f;

		targetVelocity_lerp = Vector3.Lerp(targetVelocity_lerp, targetVelocity * 2f, Time.fixedDeltaTime / smoothValVel);
		targetVelocity_lerp.y = targetVelocity_lerp.y * 0.6f * bobbing;

		camTargetPos = target.position;
		camTargetPos.x = ClampCamX(camTargetPos.x);
		camTargetPos.y = camTargetPos.y + camUp;

		camPos += ((camTargetPos + targetVelocity_lerp * 0.25f + targetVelocity * (0.1f + 0.1f * bobbing)) + targetAcceleration_lerp * 0.25f * bobbing - camPos) * 10.0f * Time.fixedDeltaTime / smoothVal;
		//camPos += (camTargetPos - camPos) * Time.deltaTime / smoothVal;
		//camPos += (camTargetPos + targetAcceleration_lerp * (0.2f * bobbing) - camPos) * 2.0f * Time.deltaTime / smoothValAcc;
		//camPos += (camTargetPos + targetVelocity * (0.4f + 0.2f * bobbing) - camPos) * Time.deltaTime / smoothValVel;
		//asd
	}

    private void LateUpdate() {
        if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
            SetupCam();

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
        SetupCam();
    }

    public void AttachCam(Transform p) {
        SetupCam();
        zoomed = 1f;
        target = p;
        //lastTargetPos = target.position;
        camTargetPos = p.position + new Vector3(0, camUp, -10);
        camTargetPos.x = ClampCamX(camTargetPos.x);
        camPos = camTargetPos;
		//ShakeCam(10f, 5);

		//asd
		rigb2D = p.GetComponent<Rigidbody2D>();
		//asd
    }
    
    public void SetupCam() {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        Camera.main.orthographicSize = (GlobalUIManager.standardHeight / (1f * Helper.PixelsPerUnit)) * 0.25f;
        Vector2 forcedSize = GetForcedScreenSize();
        marginWidth = (Screen.width - forcedSize.x) / Screen.width;
        marginHeight = (Screen.height - forcedSize.y) / Screen.height;
        Camera.main.rect = new Rect(marginWidth / 2f, marginHeight / 2f, 1.0f - marginWidth, 1.0f - marginHeight);
        PlayerHUD.DrawUI();
    }

    public Vector2 GetForcedScreenSize() {
        standardAspect = GlobalUIManager.standardWidth / (float)GlobalUIManager.standardHeight;
        Vector2 forcedSize = new Vector2(Screen.width, Screen.height);
        if (Screen.width / (float)Screen.height > standardAspect) {
            forcedSize.x = Screen.height * standardAspect;
        }
        else if (Screen.width / (float)Screen.height < standardAspect) {
            forcedSize.y = Screen.width / standardAspect;
        }
        return forcedSize;
    }

    public Vector2 GetMargin() {
        return new Vector2(marginWidth, marginHeight);
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

    IEnumerator Zoom(Vector3 target, float amount, float time) {
        yield return new WaitForEndOfFrame();
        float ticktime = 0.02f;
        Vector3 origin = transform.position;
        float nextTick = Time.realtimeSinceStartup;
        float t = 0;
        float a = zoomed;
        zooming = true;
        while (zoomed != amount && zooming) {
            if (nextTick > Time.realtimeSinceStartup) yield return null;
            nextTick = Time.realtimeSinceStartup + ticktime;
            t += 1 / time * ticktime;
            zoomed = Mathf.Sin(t * Mathf.PI / 2f) * (amount - a) + a;
            camPos = Mathf.Sin(t * Mathf.PI / 2f) * (target - origin) + origin;
            if (t >= 1)
                zoomed = amount;
            Camera.main.orthographicSize = Mathf.Round((GlobalUIManager.standardHeight / (1f * Helper.PixelsPerUnit)) * 0.25f / zoomed);
            yield return null;
        }
        zooming = false;
    }
}
