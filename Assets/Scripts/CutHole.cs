﻿using System;
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
            if (Physics.Raycast(ray, out hit, 1000f))
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

                int v1 = FindVertex(shared1);
                int v2 = FindVertex(shared2);

                int neighbourTriangle = FindTriangle(vertices[v1], vertices[v2], hitTri);
                DeleteSquare(hitTri, neighbourTriangle);
                
                // add new block
                Vector3 blockPosition = hit.point + hit.normal / 2.0f;
                blockPosition.x = (float) Math.Round(blockPosition.x, MidpointRounding.AwayFromZero);
                blockPosition.y = (float) Math.Round(blockPosition.y, MidpointRounding.AwayFromZero);
                blockPosition.z = (float) Math.Round(blockPosition.z, MidpointRounding.AwayFromZero);

                Instantiate(blockPrefab, blockPosition, Quaternion.identity);
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

    private void DeleteTri(int index)
    {
        Destroy(GetComponent<MeshCollider>());
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        int[] oldTriangles = mesh.triangles;
        int[] newTriangles = new int[oldTriangles.Length - 3];

        int i = 0;
        int j = 0;
        while (j < mesh.triangles.Length)
        {
            if (j != index * 3)
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
}