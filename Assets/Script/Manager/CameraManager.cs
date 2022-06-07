using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraManager : MonoBehaviour
{
    public UniversalManager um;

    

    PlayerScript ps;
    
    public Camera playerCam;
    public List<CamShakePart> camShakePartList = new List<CamShakePart>();
    public float minShakePower;
    public float maxShakePower;

    public Vector3 targetOffset;
    public float synchronizeSpeed;
    public Vector3 curCamShakeOffset;



    void Awake()
    {
        playerCam = Camera.main;

        ps = um.player.GetComponent<PlayerScript>();
    }
    void Update()
    {
        float currentShakePower = 0;
        for(int i =0; i < camShakePartList.Count; i++)
        {
            currentShakePower += camShakePartList[i].curPower;
            camShakePartList[i].curPower -= camShakePartList[i].decreaseSpeed * Time.deltaTime;
            if (camShakePartList[i].curPower <= 0)
            {
                camShakePartList.RemoveAt(i);
                i--;
            }
        }
        if (currentShakePower >= minShakePower)
        {
            targetOffset = new Vector3(Random.Range(-360, 360), Random.Range(-360, 360), Random.Range(-360, 360)) * Mathf.Clamp(currentShakePower, 0, maxShakePower) / 360;
        }
        else
        {
            targetOffset = Vector3.zero;
        }

        curCamShakeOffset = Vector3.Lerp(curCamShakeOffset, targetOffset, synchronizeSpeed * Time.deltaTime);
        
    }
    public void ShakeCamera(float time, float power, bool fadeness, float delay)
    {
        StartCoroutine(ShakeCamera_Cor(time, power, fadeness, delay));
    }
    IEnumerator ShakeCamera_Cor(float time, float power, bool fade, float delay)
    {
        if (time == 0)
            yield break;

        if (delay != 0)
            yield return new WaitForSeconds(delay);

        if (fade)
        {
            camShakePartList.Add(new CamShakePart { curPower = power, decreaseSpeed = power/time });
        }
        else
        {
            CamShakePart csp = new CamShakePart { curPower = power, decreaseSpeed = 0 };
            camShakePartList.Add(csp);
            yield return new WaitForSeconds(time);
            camShakePartList.Remove(csp);
        }

    }
    
}

[System.Serializable]
public class CamShakePart
{
    public float curPower;
    public float decreaseSpeed;
}