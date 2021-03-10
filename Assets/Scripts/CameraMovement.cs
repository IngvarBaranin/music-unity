using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Vector3 originalCameraPos;
    private bool resetting = false;
    
    public float XMoveSpeed = 8f;
    
    void Start()
    {
        originalCameraPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!resetting && Countdown.Instance.gameStarted)
        {
            transform.Translate(new Vector3((Time.deltaTime * XMoveSpeed), 0, 0));
        }
    }

    public void ResetCameraSmooth()
    {
        resetting = true;
        StartCoroutine(LerpPosition(originalCameraPos, 0.5f));
        resetting = false;
    }
    
    IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }
    
}
