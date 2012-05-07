xof 0303txt 0032


template VertexDuplicationIndices { 
 <b8d65549-d7c9-4995-89cf-53a9a8b031e3>
 DWORD nIndices;
 DWORD nOriginalVertices;
 array DWORD indices[nIndices];
}
template XSkinMeshHeader {
 <3cf169ce-ff7c-44ab-93c0-f78f62d172e2>
 WORD nMaxSkinWeightsPerVertex;
 WORD nMaxSkinWeightsPerFace;
 WORD nBones;
}
template SkinWeights {
 <6f0d123b-bad2-4167-a0d0-80224f25fabb>
 STRING transformNodeName;
 DWORD nWeights;
 array DWORD vertexIndices[nWeights];
 array float weights[nWeights];
 Matrix4x4 matrixOffset;
}

Frame RootFrame {

  FrameTransformMatrix {
    1.000000,0.000000,0.000000,0.000000,
    0.000000,1.000000,0.000000,0.000000,
    0.000000,0.000000,-1.000000,0.000000,
    0.000000,0.000000,0.000000,1.000000;;
  }
  Frame Cube {

    FrameTransformMatrix {
      1.000000,0.000000,0.000000,0.000000,
      0.000000,1.000000,0.000000,0.000000,
      0.000000,0.000000,1.000000,0.000000,
      -0.322600,1.448400,0.000000,1.000000;;
    }
Mesh {
112;
-0.213000; 0.309800; -1.000000;,
-0.213000; -0.309800; -1.000000;,
-0.832500; -0.309800; -1.000000;,
-0.832500; 0.309800; -1.000000;,
-0.832500; -0.309800; 1.000000;,
-0.213000; -0.309800; 1.000000;,
-0.213000; 0.309800; 1.000000;,
-0.832500; 0.309800; 1.000000;,
-0.832500; -1.000000; 0.309800;,
-0.832500; -1.000000; -0.309800;,
-0.213000; -1.000000; -0.309800;,
-0.213000; -1.000000; 0.309800;,
-1.522800; 0.309800; 0.309800;,
-1.522800; 0.309800; -0.309800;,
-1.522800; -0.309800; -0.309800;,
-1.522800; -0.309800; 0.309800;,
-0.832500; 1.000000; -0.309800;,
-0.832500; 1.000000; 0.309800;,
-0.213000; 1.000000; 0.309800;,
-0.213000; 1.000000; -0.309800;,
-0.213000; 0.309800; -1.000000;,
0.477200; 0.309800; -0.309800;,
0.477200; -0.309800; -0.309800;,
-0.213000; -0.309800; -1.000000;,
-0.832500; -1.000000; -0.309800;,
-0.832500; -0.309800; -1.000000;,
-0.213000; -0.309800; -1.000000;,
-0.213000; -1.000000; -0.309800;,
-0.213000; 1.000000; -0.309800;,
-0.213000; 0.309800; -1.000000;,
-0.832500; 0.309800; -1.000000;,
-0.832500; 1.000000; -0.309800;,
-1.522800; 0.309800; -0.309800;,
-0.832500; 0.309800; -1.000000;,
-0.832500; -0.309800; -1.000000;,
-1.522800; -0.309800; -0.309800;,
-0.832500; 1.000000; 0.309800;,
-0.832500; 0.309800; 1.000000;,
-0.213000; 0.309800; 1.000000;,
-0.213000; 1.000000; 0.309800;,
-0.832500; 0.309800; 1.000000;,
-1.522800; 0.309800; 0.309800;,
-1.522800; -0.309800; 0.309800;,
-0.832500; -0.309800; 1.000000;,
-0.213000; -0.309800; 1.000000;,
0.477200; -0.309800; 0.309800;,
0.477200; 0.309800; 0.309800;,
-0.213000; 0.309800; 1.000000;,
-0.832500; -0.309800; 1.000000;,
-0.832500; -1.000000; 0.309800;,
-0.213000; -1.000000; 0.309800;,
-0.213000; -0.309800; 1.000000;,
-0.213000; 1.000000; 0.309800;,
0.477200; 0.309800; 0.309800;,
0.477200; 0.309800; -0.309800;,
-0.213000; 1.000000; -0.309800;,
0.477200; -0.309800; 0.309800;,
-0.213000; -1.000000; 0.309800;,
-0.213000; -1.000000; -0.309800;,
0.477200; -0.309800; -0.309800;,
-0.832500; -1.000000; 0.309800;,
-1.522800; -0.309800; 0.309800;,
-1.522800; -0.309800; -0.309800;,
-0.832500; -1.000000; -0.309800;,
-1.522800; 0.309800; 0.309800;,
-0.832500; 1.000000; 0.309800;,
-0.832500; 1.000000; -0.309800;,
-1.522800; 0.309800; -0.309800;,
-0.213000; 0.309800; -1.000000;,
-0.213000; 1.000000; -0.309800;,
0.477200; 0.309800; -0.309800;,
-0.213000; -0.309800; -1.000000;,
0.477200; -0.309800; -0.309800;,
-0.213000; -1.000000; -0.309800;,
-0.832500; -0.309800; -1.000000;,
-0.832500; -1.000000; -0.309800;,
-1.522800; -0.309800; -0.309800;,
-0.832500; 0.309800; -1.000000;,
-1.522800; 0.309800; -0.309800;,
-0.832500; 1.000000; -0.309800;,
-0.213000; 0.309800; 1.000000;,
0.477200; 0.309800; 0.309800;,
-0.213000; 1.000000; 0.309800;,
-0.213000; -0.309800; 1.000000;,
-0.213000; -1.000000; 0.309800;,
0.477200; -0.309800; 0.309800;,
-0.832500; -0.309800; 1.000000;,
-1.522800; -0.309800; 0.309800;,
-0.832500; -1.000000; 0.309800;,
-0.832500; 0.309800; 1.000000;,
-0.832500; 1.000000; 0.309800;,
-1.522800; 0.309800; 0.309800;,
0.477200; -0.309800; 0.309800;,
0.477200; -0.309800; -0.309800;,
1.522800; -0.309800; -0.309800;,
1.522800; -0.309800; 0.309800;,
0.477200; 0.309800; 0.309800;,
0.477200; -0.309800; 0.309800;,
1.522800; -0.309800; 0.309800;,
1.522800; 0.309800; 0.309800;,
0.477200; 0.309800; -0.309800;,
0.477200; 0.309800; 0.309800;,
1.522800; 0.309800; 0.309800;,
1.522800; 0.309800; -0.309800;,
0.477200; -0.309800; -0.309800;,
0.477200; 0.309800; -0.309800;,
1.522800; 0.309800; -0.309800;,
1.522800; -0.309800; -0.309800;,
1.522800; -0.309800; 0.309800;,
1.522800; -0.309800; -0.309800;,
1.522800; 0.309800; -0.309800;,
1.522800; 0.309800; 0.309800;;
30;
4; 0, 3, 2, 1;,
4; 4, 7, 6, 5;,
4; 8, 11, 10, 9;,
4; 12, 15, 14, 13;,
4; 16, 19, 18, 17;,
4; 20, 23, 22, 21;,
4; 24, 27, 26, 25;,
4; 28, 31, 30, 29;,
4; 32, 35, 34, 33;,
4; 36, 39, 38, 37;,
4; 40, 43, 42, 41;,
4; 44, 47, 46, 45;,
4; 48, 51, 50, 49;,
4; 52, 55, 54, 53;,
4; 56, 59, 58, 57;,
4; 60, 63, 62, 61;,
4; 64, 67, 66, 65;,
3; 68, 70, 69;,
3; 71, 73, 72;,
3; 74, 76, 75;,
3; 77, 79, 78;,
3; 80, 82, 81;,
3; 83, 85, 84;,
3; 86, 88, 87;,
3; 89, 91, 90;,
4; 92, 95, 94, 93;,
4; 96, 99, 98, 97;,
4; 100, 103, 102, 101;,
4; 104, 107, 106, 105;,
4; 108, 111, 110, 109;;
  MeshMaterialList {
    1;
    30;
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0;;
  Material Material {
    0.800000; 0.800000; 0.800000;1.0;;
    0.500000;
    1.000000; 1.000000; 1.000000;;
    0.0; 0.0; 0.0;;
  }  //End of Material
    }  //End of MeshMaterialList
  MeshNormals {
112;
    0.366985; 0.366985; -0.854762;,
    0.366985; -0.366985; -0.854762;,
    -0.366985; -0.366985; -0.854762;,
    -0.366985; 0.366985; -0.854762;,
    -0.366985; -0.366985; 0.854762;,
    0.366985; -0.366985; 0.854762;,
    0.366985; 0.366985; 0.854762;,
    -0.366985; 0.366985; 0.854762;,
    -0.366985; -0.854762; 0.366985;,
    -0.366985; -0.854762; -0.366985;,
    0.366985; -0.854762; -0.366985;,
    0.366985; -0.854762; 0.366985;,
    -0.854762; 0.366985; 0.366985;,
    -0.854762; 0.366985; -0.366985;,
    -0.854762; -0.366985; -0.366985;,
    -0.854762; -0.366985; 0.366985;,
    -0.366985; 0.854762; -0.366985;,
    -0.366985; 0.854762; 0.366985;,
    0.366985; 0.854762; 0.366985;,
    0.366985; 0.854762; -0.366985;,
    0.366985; 0.366985; -0.854762;,
    0.524735; 0.601917; -0.601917;,
    0.524735; -0.601917; -0.601917;,
    0.366985; -0.366985; -0.854762;,
    -0.366985; -0.854762; -0.366985;,
    -0.366985; -0.366985; -0.854762;,
    0.366985; -0.366985; -0.854762;,
    0.366985; -0.854762; -0.366985;,
    0.366985; 0.854762; -0.366985;,
    0.366985; 0.366985; -0.854762;,
    -0.366985; 0.366985; -0.854762;,
    -0.366985; 0.854762; -0.366985;,
    -0.854762; 0.366985; -0.366985;,
    -0.366985; 0.366985; -0.854762;,
    -0.366985; -0.366985; -0.854762;,
    -0.854762; -0.366985; -0.366985;,
    -0.366985; 0.854762; 0.366985;,
    -0.366985; 0.366985; 0.854762;,
    0.366985; 0.366985; 0.854762;,
    0.366985; 0.854762; 0.366985;,
    -0.366985; 0.366985; 0.854762;,
    -0.854762; 0.366985; 0.366985;,
    -0.854762; -0.366985; 0.366985;,
    -0.366985; -0.366985; 0.854762;,
    0.366985; -0.366985; 0.854762;,
    0.524735; -0.601917; 0.601917;,
    0.524735; 0.601917; 0.601917;,
    0.366985; 0.366985; 0.854762;,
    -0.366985; -0.366985; 0.854762;,
    -0.366985; -0.854762; 0.366985;,
    0.366985; -0.854762; 0.366985;,
    0.366985; -0.366985; 0.854762;,
    0.366985; 0.854762; 0.366985;,
    0.524735; 0.601917; 0.601917;,
    0.524735; 0.601917; -0.601917;,
    0.366985; 0.854762; -0.366985;,
    0.524735; -0.601917; 0.601917;,
    0.366985; -0.854762; 0.366985;,
    0.366985; -0.854762; -0.366985;,
    0.524735; -0.601917; -0.601917;,
    -0.366985; -0.854762; 0.366985;,
    -0.854762; -0.366985; 0.366985;,
    -0.854762; -0.366985; -0.366985;,
    -0.366985; -0.854762; -0.366985;,
    -0.854762; 0.366985; 0.366985;,
    -0.366985; 0.854762; 0.366985;,
    -0.366985; 0.854762; -0.366985;,
    -0.854762; 0.366985; -0.366985;,
    0.366985; 0.366985; -0.854762;,
    0.366985; 0.854762; -0.366985;,
    0.524735; 0.601917; -0.601917;,
    0.366985; -0.366985; -0.854762;,
    0.524735; -0.601917; -0.601917;,
    0.366985; -0.854762; -0.366985;,
    -0.366985; -0.366985; -0.854762;,
    -0.366985; -0.854762; -0.366985;,
    -0.854762; -0.366985; -0.366985;,
    -0.366985; 0.366985; -0.854762;,
    -0.854762; 0.366985; -0.366985;,
    -0.366985; 0.854762; -0.366985;,
    0.366985; 0.366985; 0.854762;,
    0.524735; 0.601917; 0.601917;,
    0.366985; 0.854762; 0.366985;,
    0.366985; -0.366985; 0.854762;,
    0.366985; -0.854762; 0.366985;,
    0.524735; -0.601917; 0.601917;,
    -0.366985; -0.366985; 0.854762;,
    -0.854762; -0.366985; 0.366985;,
    -0.366985; -0.854762; 0.366985;,
    -0.366985; 0.366985; 0.854762;,
    -0.366985; 0.854762; 0.366985;,
    -0.854762; 0.366985; 0.366985;,
    0.524735; -0.601917; 0.601917;,
    0.524735; -0.601917; -0.601917;,
    0.577349; -0.577349; -0.577349;,
    0.577349; -0.577349; 0.577349;,
    0.524735; 0.601917; 0.601917;,
    0.524735; -0.601917; 0.601917;,
    0.577349; -0.577349; 0.577349;,
    0.577349; 0.577349; 0.577349;,
    0.524735; 0.601917; -0.601917;,
    0.524735; 0.601917; 0.601917;,
    0.577349; 0.577349; 0.577349;,
    0.577349; 0.577349; -0.577349;,
    0.524735; -0.601917; -0.601917;,
    0.524735; 0.601917; -0.601917;,
    0.577349; 0.577349; -0.577349;,
    0.577349; -0.577349; -0.577349;,
    0.577349; -0.577349; 0.577349;,
    0.577349; -0.577349; -0.577349;,
    0.577349; 0.577349; -0.577349;,
    0.577349; 0.577349; 0.577349;;
30;
4; 0, 3, 2, 1;,
4; 4, 7, 6, 5;,
4; 8, 11, 10, 9;,
4; 12, 15, 14, 13;,
4; 16, 19, 18, 17;,
4; 20, 23, 22, 21;,
4; 24, 27, 26, 25;,
4; 28, 31, 30, 29;,
4; 32, 35, 34, 33;,
4; 36, 39, 38, 37;,
4; 40, 43, 42, 41;,
4; 44, 47, 46, 45;,
4; 48, 51, 50, 49;,
4; 52, 55, 54, 53;,
4; 56, 59, 58, 57;,
4; 60, 63, 62, 61;,
4; 64, 67, 66, 65;,
3; 68, 70, 69;,
3; 71, 73, 72;,
3; 74, 76, 75;,
3; 77, 79, 78;,
3; 80, 82, 81;,
3; 83, 85, 84;,
3; 86, 88, 87;,
3; 89, 91, 90;,
4; 92, 95, 94, 93;,
4; 96, 99, 98, 97;,
4; 100, 103, 102, 101;,
4; 104, 107, 106, 105;,
4; 108, 111, 110, 109;;
}  //End of MeshNormals
  }  // End of the Mesh Cube 
  }  // SI End of the Object Cube 
}  // End of the Root Frame