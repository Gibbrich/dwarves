using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutHole : MonoBehaviour
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
            if (Physics.Raycast(ray, out hit, 1000f) && hit.transform.GetComponent<CutHole>() == this)
            {
                int hitTri = hit.triangleIndex;

                //get neighbour
                int[] triangles = GetComponent<MeshFilter>().mesh.triangles;
                Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;

                Vector3 p0 = vertices[triangles[hitTri * 3 + 0]];
                Vector3 p1 = vertices[triangles[hitTri * 3 + 1]];
                Vector3 p2 = vertices[triangles[hitTri * 3 + 2]];

                float edge1 = Vector3.Distance(p0, p1);
                float edge2 = Vector3.Distance(p0, p2);
                float edge3 = Vector3.Distance(p1, p2);

                Vector3 shared1;
                Vector3 shared2;

                if (edge1 > edge2 && edge1 > edge3)
                {
                    shared1 = p0;
                    shared2 = p1;
                }
                else if (edge2 > edge1 && edge2 > edge3)
                {
                    shared1 = p0;
                    shared2 = p2;
                }
                else
                {
                    shared1 = p1;
                    shared2 = p2;
                }

                // find hypotenuse vertices indices
                int v1 = FindVertex(shared1);
                int v2 = FindVertex(shared2);

                int neighbourTriangle = FindTriangle(vertices[v1], vertices[v2], hitTri);
                DeleteSquare(hitTri, neighbourTriangle);
                
                // add new block
                Vector3 hypoVertex1 = vertices[v1] + transform.position;
                Vector3 hypoVertex2 = vertices[v2] + transform.position;
                Vector3 squareCenter = (hypoVertex1 + hypoVertex2) / 2.0f;
                
                // squareCenter always moved from the cube center on 0.5 units
                // magnitude of hit.normal is always equals 1
                // as cube size can be differ than 1, for correct discovering new block center position
                // need subtract squareCenter distance from cube scale and multiply by hit.normal
                float multiplier = transform.localScale.x - 0.5f;
                Vector3 blockPosition = squareCenter + hit.normal * multiplier;

                GameObject block = Instantiate(blockPrefab, blockPosition, Quaternion.identity);
                block.transform.parent = transform;
//                Combine(block);
                
                // delete
            }
        }
    }

    private void DeleteSquare(int index1, int index2)
    {
        Destroy(GetComponent<MeshCollider>());
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        int[] oldTriangles = mesh.triangles;
        int[] newTriangles = new int[oldTriangles.Length - 6];

        int i = 0;
        int j = 0;
        while (j < mesh.triangles.Length)
        {
            if (j != index1 * 3 && j != index2 * 3)
            {
                newTriangles[i++] = oldTriangles[j++];
                newTriangles[i++] = oldTriangles[j++];
                newTriangles[i++] = oldTriangles[j++];
            }
            else
            {
                j += 3;
            }
        }
        GetComponent<MeshFilter>().mesh.triangles = newTriangles;
        gameObject.AddComponent<MeshCollider>();
    }

    private int FindTriangle(Vector3 v1, Vector3 v2, int notTriIndex)
    {
        int[] triangles = GetComponent<MeshFilter>().mesh.triangles;
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;

        int j = 0;

        while (j < triangles.Length)
        {
            if (j / 3 != notTriIndex)
            {
                if (vertices[triangles[j]] == v1 && (vertices[triangles[j + 1]] == v2 || vertices[triangles[j + 2]] == v2))
                {
                    return j / 3;
                }
                else if (vertices[triangles[j]] == v2 && (vertices[triangles[j+1]] == v1 || vertices[triangles[j + 2]] == v1))
                {
                    return j / 3;
                }
                else if (vertices[triangles[j+1]] == v2 && (vertices[triangles[j]] == v1 || vertices[triangles[j+2]] == v1))
                {
                    return j / 3;
                }
                else if (vertices[triangles[j+1]] == v1 && (vertices[triangles[j]] == v2 || vertices[triangles[j+2]] == v2))
                {
                    return j / 3;
                }
            }

            j += 3;
        }

        return -1;
    }

    private int FindVertex(Vector3 v)
    {
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i] == v)
            {
                return i;
            }
        }

        return -1;
    }
    
    private void Combine(GameObject block)
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Destroy(gameObject.GetComponent<MeshCollider>());

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        gameObject.GetComponent<MeshFilter>().mesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);
        gameObject.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        gameObject.AddComponent<MeshCollider>();
        gameObject.SetActive(true);
        
        Destroy(block);
    }
}