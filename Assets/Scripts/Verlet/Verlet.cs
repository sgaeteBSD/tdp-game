using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Verlet : MonoBehaviour
{

    [System.Serializable]
    public struct Point
    {
        public GameObject obj;
        public Vector2 pos;
        public Vector2 prevPos;
        public Vector2 acceleration;
    }

    [SerializeField] private GameObject pointObject;
    public Vector2[] pointLocations;
    private List<Point> points;
    public float Timestep;

    void Start()
    {
        for (int i = 0; i < pointLocations.Length; i++)
        {
            Point newPoint = new Point();
            newPoint.obj = Instantiate(pointObject, transform);
            newPoint.obj.transform.position = newPoint.pos = newPoint.prevPos = pointLocations[i];
            newPoint.acceleration = Vector2.zero;
            points.Add(newPoint);
        }
    }
    
    void FixedUpdate()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        //erm
        // foreach (var point in points)
        // {
        //     Vector2 temp = point.pos;
        //     point.pos += point.pos - point.prevPos + point.acceleration * Timestep * Timestep;
        //     point.prevPos = temp;
        // }
    }
}
