using UnityEngine;
using utilities.general.attributes;

public class PlacingManager : MonoBehaviour
{
    [SerializeField] GameObject blockPrefab;
    [SerializeField] LayerMask raycastLayers;

    [SerializeField][Layer] int placedLayerObject;
    [SerializeField][Layer] int previewLayerObject;

    [SerializeField] KeyCode placeKey = KeyCode.Mouse0;

    GameObject previewObject = null;
    Vector3 previewExtents = Vector3.one * 0.5f;
    Camera cam;

    Vector3 roundedPos;
    Vector3 offsetedPos;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        PreviewPlaceObject();

        if (Input.GetKeyDown(placeKey) && previewObject != null)
        {
            PlaceBlock();
        }
    }

    private void PlaceBlock()
    {
        Transform transform = previewObject.transform;
        GameObject obj = Instantiate(blockPrefab, transform.position, transform.rotation);
        SetLayerRecursively(obj, placedLayerObject);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void PreviewPlaceObject()
    {
        if (previewObject == null)
        {
            previewObject = Instantiate(blockPrefab);
            previewObject.SetActive(true);
            SetLayerRecursively(previewObject, previewLayerObject);

            if (previewObject.TryGetComponent<BoxCollider>(out BoxCollider collider))
            {
                previewExtents = collider.size / 2f;
            }
        }

        Vector3 newpos = Vector3.zero;
        Vector3 normalHit = Vector3.up;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, raycastLayers))
        {
            normalHit = hit.normal;

            // Main fix: Snap to grid based on hit point and normal direction
            Vector3Int snapped = Vector3Int.RoundToInt(hit.point + hit.normal * 0.5f);
            newpos = snapped;
        }
        else
        {
            // Fallback: Snap to ground grid
            newpos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
            newpos.y = 0f;
            newpos = new Vector3(Mathf.Round(newpos.x), 0f, Mathf.Round(newpos.z));
        }

        roundedPos = newpos;
        offsetedPos = newpos;

        previewObject.transform.position = newpos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(roundedPos, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(offsetedPos, 0.2f);
    }
}
