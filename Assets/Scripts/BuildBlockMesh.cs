using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildBlockMesh : MonoBehaviour
{
    public GameObject blockPrefab;
    
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                Vector3 blockPosition = hit.point + hit.normal / 2.0f;
                blockPosition.x = (float) Math.Round(blockPosition.x, MidpointRounding.AwayFromZero);
                blockPosition.y = (float) Math.Round(blockPosition.y, MidpointRounding.AwayFromZero);
                blockPosition.z = (float) Math.Round(blockPosition.z, MidpointRounding.AwayFromZero);

                GameObject block = Instantiate(blockPrefab, blockPosition, Quaternion.identity);
                block.transform.parent = transform;
                Combine(block);
            }
        }
    }

    private void Combine(GameObject block)
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Destroy(GetComponent<MeshCollider>());

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine, true);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshCollider>();
        gameObject.SetActive(true);
        
        Destroy(block);
    }
}