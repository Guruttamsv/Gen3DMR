using UnityEngine;
using TMPro;
using GLTFast;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using Oculus.Interaction;  // For Oculus SDK
using Oculus.Interaction.HandGrab;


public class TextInputHandler : MonoBehaviour
{
    public InputField inputField;
    public TMP_Text feedbackText;
    private string serverURL;
    private string gistRawURL = "https://gist.githubusercontent.com/Guruttamsv/c6e25b48715d6174e013d336bff37de3/raw";

    public Transform spawnPoint;
    public Material defaultMaterial;

    public GameObject interactionPrefab; // Drag and drop prefab in the Inspector

    public float spiralOutwardSpeed = 1f;
    public float initialRevolveSpeed = 50f;
    public float finalOrbitSpeed = 30f;
    public float minOrbitRadius = 3f;
    public float maxOrbitRadius = 7f;
    public float domeHeightFactor = 2f;
    public float waitTimeBeforeMoving = 1f;

    private List<OrbitingObject> orbitingObjects = new List<OrbitingObject>();
    private GameObject assignedModel;

    private void Start()
    {
        StartCoroutine(FetchNgrokURL());
        if (inputField)
        {
            inputField.onEndEdit.AddListener(ProcessText);
        }

    }

    private IEnumerator FetchNgrokURL()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(gistRawURL + "?" + Random.value))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string url = request.downloadHandler.text.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    serverURL = url + "/generate"; // Append "/generate" to match the API endpoint
                    yield return StartCoroutine(CheckServerConnection()); // Ping server after fetching the URL
                }
                else
                {
                    feedbackText.text = "Gist file is empty!";
                }
            }
            else
            {
                feedbackText.text = "Failed to fetch ngrok URL: " + request.error;
            }
        }
    }


    // ✅ Function to check if the server is online
    private IEnumerator CheckServerConnection()
    {
        string jsonData = "{\"prompt\": \"Checking Connection\"}"; // JSON payload
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest pingRequest = new UnityWebRequest(serverURL, "POST"))
        {
            pingRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
            pingRequest.downloadHandler = new DownloadHandlerBuffer();
            pingRequest.SetRequestHeader("Content-Type", "application/json");
            pingRequest.timeout = 5; // Timeout after 5 seconds

            yield return pingRequest.SendWebRequest();

            if (pingRequest.result == UnityWebRequest.Result.Success)
            {
                // Directly display response received from the server
                feedbackText.text = pingRequest.downloadHandler.text + "\nType Prompt & Generate"; // Display the response from the server
            }
            else
            {
                feedbackText.text = "⚠️ Server Not Connected!";
            }
        }
    }




    private void Update()
    {
        foreach (var obj in orbitingObjects)
        {
            obj.UpdateOrbit();
        }
    }

    public void ProcessText(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            feedbackText.text = "Please enter\na valid prompt!";
            return;
        }

        feedbackText.text = $"Processing: {prompt}";
        // Clear the input field
        inputField.text = "";
        StartCoroutine(GenerateAndLoadModel(prompt));
    }

    private IEnumerator GenerateAndLoadModel(string prompt)
    {
        feedbackText.text = "Sending prompt\nto server...";

        using (UnityWebRequest request = CreateRequest(prompt))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                feedbackText.text = "Error generating\nmodel!";
                yield break;
            }

            string filePath = Path.Combine(Application.persistentDataPath, $"{prompt.Replace(" ", "_")}_model.glb");
            File.WriteAllBytes(filePath, request.downloadHandler.data);

            feedbackText.text = "Loading GLB\nmodel...";
            yield return StartCoroutine(LoadModelCoroutine(filePath));
        }
    }

    private UnityWebRequest CreateRequest(string prompt)
    {
        string jsonData = $"{{\"prompt\": \"{prompt}\"}}";
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(serverURL, "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonBytes),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }

    private IEnumerator LoadModelCoroutine(string filePath)
    {
        Task<bool> loadTask = LoadGLBAsync(filePath);
        yield return new WaitUntil(() => loadTask.IsCompleted);

        feedbackText.text = loadTask.Result ? "Model Loaded\nSuccessfully!" : "Failed to\nload model!";
    }

    private async Task<bool> LoadGLBAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        var gltf = new GltfImport();
        if (!await gltf.Load(filePath))
        {
            return false;
        }

        GameObject modelParent = new GameObject("SpawnedModel");
        if (spawnPoint)
        {
            modelParent.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            modelParent.transform.localScale = spawnPoint.localScale;
        }

        await gltf.InstantiateMainSceneAsync(modelParent.transform);

        // Apply scale to all children
        float scaleFactor = 0.2f;
        foreach (Transform child in modelParent.transform)
        {
            child.localScale *= scaleFactor; // Scales each child instead of the parent
        }


        ApplyDefaultMaterial(modelParent);
        AssignNewObject(modelParent);

        return true;
    }

    private void ApplyDefaultMaterial(GameObject model)
    {
        if (defaultMaterial == null)
        {
            defaultMaterial = new Material(Shader.Find("Standard"));
        }

        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
        {
            if (renderer.sharedMaterial == null || renderer.sharedMaterial.name == "Default-Material")
            {
                renderer.sharedMaterial = defaultMaterial;
            }
        }
    }

    private void AssignNewObject(GameObject newObj)
    {
        if (newObj == null) return;

        // Destroy previous model first
        if (assignedModel != null)
        {
            Debug.Log("Destroying existing model: " + assignedModel.name);
            Destroy(assignedModel);
        }

        // Instantiate a new prefab instance
        GameObject interactionInstance = Instantiate(interactionPrefab);

        // Parent the new model inside the prefab instance
        newObj.transform.SetParent(interactionInstance.transform, false);

        // Reset transforms
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localRotation = Quaternion.identity;
        newObj.transform.localScale = Vector3.one;

        // Center the model
        CenterModel(newObj);

        // Assign new interaction prefab as the main object
        assignedModel = interactionInstance;

        StartCoroutine(StartOrbitAfterDelay());
    }

    private void CenterModel(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0) return; // No renderers found, skip centering

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // Calculate the offset
        Vector3 centerOffset = bounds.center - model.transform.position;

        // Apply offset to reposition model
        model.transform.localPosition -= centerOffset;
    }

    private IEnumerator StartOrbitAfterDelay()
    {
        yield return new WaitForSeconds(waitTimeBeforeMoving);
        SpawnOrbitingObject();
    }

    private void SpawnOrbitingObject()
    {
        if (assignedModel == null || spawnPoint == null) return;

        GameObject newObj = assignedModel;
        assignedModel = null;

        float orbitHeight = spawnPoint.position.y + Random.Range(0f, domeHeightFactor);
        float orbitRadius = Random.Range(minOrbitRadius, maxOrbitRadius);

        orbitingObjects.Add(new OrbitingObject(newObj, spawnPoint.position, orbitRadius, orbitHeight,
                                               initialRevolveSpeed, finalOrbitSpeed, spiralOutwardSpeed, waitTimeBeforeMoving));
    }

    private class OrbitingObject
    {
        private readonly GameObject obj;
        private readonly float targetOrbitRadius;
        private readonly float finalOrbitSpeed;
        private readonly float spiralOutwardSpeed;
        private float orbitAngle;
        private bool hasReachedOrbit = false;
        private float waitTime;
        private float currentRadius;
        private readonly Vector3 initialSpawnPosition;
        private readonly float startHeight;
        private readonly float targetHeight;
        private float heightProgress = 0f;

        public OrbitingObject(GameObject obj, Vector3 spawnPosition, float targetOrbitRadius, float targetHeight,
                              float initialRevolveSpeed, float finalOrbitSpeed, float spiralOutwardSpeed, float waitTime)
        {
            this.obj = obj;
            initialSpawnPosition = spawnPosition;
            this.targetOrbitRadius = targetOrbitRadius;
            this.finalOrbitSpeed = finalOrbitSpeed;
            this.spiralOutwardSpeed = spiralOutwardSpeed;
            this.orbitAngle = Random.Range(0f, 360f);
            this.waitTime = waitTime;
            currentRadius = 0f;
            startHeight = spawnPosition.y;
            this.targetHeight = targetHeight;
        }

        public void UpdateOrbit()
        {
            if (obj == null) return;

            if (waitTime > 0)
            {
                waitTime = Mathf.Max(0, waitTime - Time.deltaTime);
                return;
            }

            if (!hasReachedOrbit)
            {
                currentRadius = Mathf.Min(targetOrbitRadius, currentRadius + spiralOutwardSpeed * Time.deltaTime);
                heightProgress = Mathf.Clamp(heightProgress + (Time.deltaTime * spiralOutwardSpeed / targetOrbitRadius), 0f, 1f);
                float smoothHeight = Mathf.Lerp(startHeight, targetHeight, heightProgress);

                orbitAngle += (finalOrbitSpeed * (currentRadius / targetOrbitRadius)) * Time.deltaTime;

                if (currentRadius >= targetOrbitRadius)
                {
                    currentRadius = targetOrbitRadius;
                    hasReachedOrbit = true;
                }

                SetPosition(smoothHeight, currentRadius);
            }
            else
            {
                orbitAngle += finalOrbitSpeed * Time.deltaTime;
                SetPosition(targetHeight, targetOrbitRadius);
            }
        }

        private void SetPosition(float height, float radius)
        {
            obj.transform.position = new Vector3(
                initialSpawnPosition.x + Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * radius,
                height,
                initialSpawnPosition.z + Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * radius
            );
        }
    }
}
