using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UIElements;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;

    List<Vector3> laserPoints = new List<Vector3>();
    Vector3 rayDirection;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();

        laserPoints.Clear();
        rayDirection = barrelEnd.forward;
        laserPoints.Add(barrelEnd.position);

        if (Physics.Raycast(barrelEnd.position, barrelEnd.forward,out RaycastHit hitInfo, 1000f, targetLayer))
        {
            //laserPoints.Add(hitInfo.point);
            AttemptReflection();
        } 

        line.positionCount = laserPoints.Count;
        for(int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }
    }
    void AttemptReflection()
    {
        Vector3 collisionNormal;
        Vector3 StartPosition = laserPoints[laserPoints.Count -1];
        if (Physics.Raycast(StartPosition, rayDirection, out RaycastHit hitInfo, 200f, targetLayer))
        {
            collisionNormal = hitInfo.normal;
            laserPoints.Add(hitInfo.point);
            ReflectionCalculation(rayDirection, StartPosition, collisionNormal);
            //print(StartPosition);
            Debug.Log("How Big List: " + laserPoints.Count);

        }
        else
        {
            Vector3 endLaser = rayDirection * 200f;
            laserPoints.Add(endLaser);
            print("Mmmm...");
        }
    }
    void ReflectionCalculation(Vector3 v, Vector3 s, Vector3 norm)
    {
        float proj = Vector3.Dot(v, norm);
        Vector3 i = v - s - (2 * (proj * v));
        Vector3 g = i - s;
        rayDirection = g.normalized;
        //Debug.DrawRay(laserPoints[laserPoints.Count - 1], rayDirection, Color.red,0.1f);
        if (Physics.Raycast(laserPoints[laserPoints.Count - 1], rayDirection, 1000f, targetLayer))
        {
            AttemptReflection();
        }
      
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer ))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, directionToTarget.y, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }
}
