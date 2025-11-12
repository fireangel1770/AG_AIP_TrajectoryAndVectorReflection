using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 1;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle;

    List<Vector3> trajectoryPoints = new List<Vector3>();
    Ray theRayThatWillDefinedTheDirectionOfTheRayCastForTheDrawLine; 
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();

        if (Input.GetButtonDown("Fire1"))
            Fire();

        LineDrawCalculation();
        DrawLine();
    }
 


    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        projectile.GetComponent<Rigidbody>().linearVelocity = projectileSpeed * barrelEnd.transform.forward;
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
            //Debug.Log("hit ground");
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);
        if (angle != null)
            gun.transform.localEulerAngles = new Vector3(360f - (float)angle, 0, 0);
    }

    void LineDrawCalculation()
    {
        Vector3 v = projectileSpeed * barrelEnd.transform.forward;
        float g = gravity.y;

        Vector3 point = Vector3.zero;

        trajectoryPoints.Clear();
        trajectoryPoints.Add(barrelEnd.position);
        int iCount = 0;
        for (float t = 0; t < 10; t += Time.deltaTime)
        {
            Vector3 rayStart = trajectoryPoints[iCount];
            var endPoint = KinematicEquation(v, gravity, t);
            if (!Physics.Raycast(rayStart, endPoint - rayStart, out RaycastHit hitInfo, 0.15f, targetLayer))
            {
                trajectoryPoints.Add(KinematicEquation(v, gravity, t));
            }
            else
            {
                trajectoryPoints.Add(hitInfo.point);
                t = 10;
                return;
            }
            iCount++;
        }

    }
    void DrawLine()
    {

        line.positionCount = trajectoryPoints.Count;
        for (int i = 0; i < line.positionCount; i++)
        {

            line.SetPosition(i, trajectoryPoints[i]);
        }
    }
    Vector3 KinematicEquation(Vector3 vi, Vector3 a, float t)
    {
        var xInitial = barrelEnd.position;

        var dx = vi.x * t + 0.5f * a.x * Mathf.Pow(t,2) + xInitial.x;
        var dy = vi.y * t + 0.5f * -a.y * Mathf.Pow(t,2) + xInitial.y;
        var dz = vi.z * t + 0.5f * a.z * Mathf.Pow(t,2) + xInitial.z;
        Vector3 point = new Vector3(dx, dy, dz);
        return point;
    }
    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;
        
        float y = targetDir.y;
        targetDir.y = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);
        float g = gravity.y;
        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            if (useLow)
                return (Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg);
            else
                return (Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg);
        }
        else
            return null;

    }
}
