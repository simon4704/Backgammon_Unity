using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardMesh : MonoBehaviour
{
    float triangleHeight;
    float triangleWidth; 

    // Start is called before the first frame update
    void Start()
    {
        triangleHeight = 5f;
        triangleWidth = 1f;

        GenerateMesh();
    }

    void GenerateMesh()
    {
        Mesh mesh;
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        
        Vector3[] vertices = new Vector3[52];
        Vector3 initialXPosition = new Vector3(-6.5f * triangleWidth, 0, 0);

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    for (int l = 0; l < 2; l++)
                    {
                        int currentIndex = l + 2 * k + j * 13 + i * 26;
                        // print(currentIndex);
                        vertices[currentIndex] = initialXPosition +
                                                 new Vector3((k + j * 7 + 0.5f * l) * triangleWidth,
                                                             0f,
                                                             (float)(i - 0.5f) * 2f * (0.2f + (-1) * (l - 1)) * triangleHeight);
                                                             // horrible fking indexing holy shit
                        vertices[12] = new Vector3(-0.5f * triangleWidth, 0, -1.2f * triangleHeight);
                        vertices[25] = new Vector3((0.5f + 6)*triangleWidth, 0, -1.2f * triangleHeight);
                        vertices[38] = new Vector3(-0.5f * triangleWidth, 0, 1.2f * triangleHeight);
                        vertices[51] = new Vector3((0.5f + 6) * triangleWidth, 0, 1.2f * triangleHeight);
                    }
                }
            }
        }

        int[] triangles = new int[72];

        for (int i = 0; i < 12; i++)
        {
            triangles[i * 3] = i * 2 + i / 6;
            triangles[i * 3 + 1] = i * 2 + 1 + i / 6;
            triangles[i * 3 + 2] = (i + 1) * 2 + i / 6;
        }

        for (int i = 12; i < 24; i++)
        {
            triangles[i * 3] = i * 2 + i / 6;
            triangles[i * 3 + 1] = (i + 1) * 2 + i / 6;
            triangles[i * 3 + 2] = i * 2 + 1 + i / 6;

            // print((i * 2 + i / 6) + " " + ((i + 1) * 2 + i / 6) + " " + (i * 2 + 1 + i / 6));
        }


        // might fix colors later


        mesh.vertices = vertices;
        mesh.triangles = triangles;

    }


}
