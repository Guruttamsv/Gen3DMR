using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    public Transform spawnPoint; // Assign in Unity Inspector
    public float spiralOutwardSpeed = 1f;
    public float initialRevolveSpeed = 50f;
    public float finalOrbitSpeed = 30f;
    public float minOrbitRadius = 3f;
    public float maxOrbitRadius = 7f;
    public float domeHeightFactor = 2f;
    public float waitTimeBeforeMoving = 1f; // Delay before movement

    private List<OrbitingObject> orbitingObjects = new List<OrbitingObject>();
    private GameObject assignedModel; // Holds the loaded 3D model

    private void Update()
    {
        foreach (var obj in orbitingObjects)
        {
            obj.UpdateOrbit();
        }
    }

    // Called from TextInputHandler when a new model is loaded
    public void AssignNewObject(GameObject newObj)
    {
        if (newObj == null)
        {
            Debug.LogWarning("Assigned model is null!");
            return;
        }

        if (assignedModel != null) // Destroy old model before replacing
        {
            Destroy(assignedModel);
        }

        assignedModel = Instantiate(newObj, spawnPoint.position, Quaternion.identity); // Clone model
        StartCoroutine(StartOrbitAfterDelay());
    }

    private IEnumerator StartOrbitAfterDelay()
    {
        yield return new WaitForSeconds(waitTimeBeforeMoving);
        SpawnOrbitingObject();
    }

    private void SpawnOrbitingObject()
    {
        if (assignedModel == null || spawnPoint == null)
        {
            Debug.LogWarning("No assigned model or spawn point!");
            return;
        }

        GameObject newObj = Instantiate(assignedModel, spawnPoint.position, Quaternion.identity); // Clone assigned model
        float orbitHeight = spawnPoint.position.y + Random.Range(0f, domeHeightFactor);
        float orbitRadius = Random.Range(minOrbitRadius, maxOrbitRadius);

        orbitingObjects.Add(new OrbitingObject(newObj, spawnPoint.position, orbitRadius, orbitHeight, initialRevolveSpeed, finalOrbitSpeed, spiralOutwardSpeed, waitTimeBeforeMoving));
    }

    private class OrbitingObject
    {
        public GameObject obj;
        private float targetOrbitRadius;
        private float revolveSpeed;
        private float finalOrbitSpeed;
        private float spiralOutwardSpeed;
        private float orbitAngle;
        private bool hasReachedOrbit = false;
        private float waitTime;
        private float currentRadius;
        private Vector3 initialSpawnPosition;
        private float startHeight;
        private float targetHeight;
        private float heightProgress = 0f;

        public OrbitingObject(GameObject obj, Vector3 spawnPosition, float targetOrbitRadius, float targetHeight, float revolveSpeed, float finalOrbitSpeed, float spiralOutwardSpeed, float waitTime)
        {
            this.obj = obj;
            this.initialSpawnPosition = spawnPosition;
            this.targetOrbitRadius = targetOrbitRadius;
            this.revolveSpeed = revolveSpeed;
            this.finalOrbitSpeed = finalOrbitSpeed;
            this.spiralOutwardSpeed = spiralOutwardSpeed;
            this.orbitAngle = Random.Range(0f, 360f);
            this.waitTime = waitTime;
            this.currentRadius = 0f;
            this.startHeight = spawnPosition.y;
            this.targetHeight = targetHeight;
        }

        public void UpdateOrbit()
        {
            if (obj == null) return;

            if (waitTime > 0)
            {
                waitTime -= Time.deltaTime;
                return;
            }

            if (!hasReachedOrbit)
            {
                float newRadius = Mathf.Min(currentRadius + spiralOutwardSpeed * Time.deltaTime, targetOrbitRadius);
                orbitAngle += revolveSpeed * Time.deltaTime;

                heightProgress += Time.deltaTime * spiralOutwardSpeed / targetOrbitRadius;
                float smoothHeight = Mathf.Lerp(startHeight, targetHeight, heightProgress);

                if (newRadius >= targetOrbitRadius)
                {
                    newRadius = targetOrbitRadius;
                    hasReachedOrbit = true;
                }

                Vector3 orbitPosition = new Vector3(
                    initialSpawnPosition.x + Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * newRadius,
                    smoothHeight,
                    initialSpawnPosition.z + Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * newRadius
                );

                obj.transform.position = orbitPosition;
                currentRadius = newRadius;
            }
            else
            {
                orbitAngle += finalOrbitSpeed * Time.deltaTime;

                Vector3 orbitPosition = new Vector3(
                    initialSpawnPosition.x + Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * targetOrbitRadius,
                    targetHeight,
                    initialSpawnPosition.z + Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * targetOrbitRadius
                );

                obj.transform.position = orbitPosition;
            }
        }
    }
}
