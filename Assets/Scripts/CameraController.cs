using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float smoothing;
    private Transform target;
    private Vector3 newCameraPos;
    private Vector3 refZ;
    
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        transform.position = new Vector3(target.position.x, target.position.y, -10);
    }

    // Update is called once per frame
    void Update()
    {
        newCameraPos = new Vector3(target.position.x, target.position.y, -10);
    }

    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, newCameraPos, ref refZ, smoothing);
    }

    public void Shake()
    {
        transform.DOShakePosition(0.05f, 1.5f, 10, 90f, false, true);
    }
}
