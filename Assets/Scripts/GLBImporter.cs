using System;
using UnityEngine;
using Siccity.GLTFUtility;
using UnityEngine.XR.Interaction.Toolkit; // Required for XR Grab Interactable

public class GLBImporter
{
    public Camera userCamera; // Assign your main camera or XR camera here in the inspector
    public float distanceInFrontOfUser = 2.0f; // Define how far in front of the user the object should appear


    public void ImportGLTF(string filepath, Vector3 spawnPosition)
    {
        GameObject result = Importer.LoadFromFile(filepath);

        result.transform.position = spawnPosition;

        // Add Rigidbody component
        if (result.GetComponent<Rigidbody>() == null)
        {
            result.AddComponent<Rigidbody>();
        }

        // Compute and Add BoxCollider component
        BoxCollider computedCollider = ComputeAndAddBoxCollider(result);


        // Add XR Grab Interactable component
        if (result.GetComponent<XRGrabInteractable>() == null)
        {
            XRGrabInteractable grabInteractable = result.AddComponent<XRGrabInteractable>();
            grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        }
        
        // Load and assign new material
        Material material = Resources.Load<Material>("Materials/DefaultImportMaterial");
        Transform child = result.transform.GetChild(0);
        Renderer renderer = child.GetComponent<Renderer>();
        renderer.material = material;
    }

    private BoxCollider ComputeAndAddBoxCollider(GameObject obj)
    {
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0) return null; // No meshes to compute bounds for

        Bounds bounds = meshFilters[0].sharedMesh.bounds;
        for (int i = 1; i < meshFilters.Length; i++)
        {
            bounds.Encapsulate(meshFilters[i].sharedMesh.bounds);
        }

        BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;

        return boxCollider;
    }
}