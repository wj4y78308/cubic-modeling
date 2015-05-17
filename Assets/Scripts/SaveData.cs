using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {
	public string name;
	public List<MeshData> meshList;

	public SaveData (string str, CubeMesh[] cubeMeshes) {
		name = str;
		meshList = new List<MeshData> ();
		foreach (CubeMesh cubeMesh in cubeMeshes) {
			MeshData meshData = new MeshData(cubeMesh);
			if (cubeMesh.symmetry!=null)
				for (int i=0; i<cubeMeshes.Length; i++ )
					if (cubeMesh.symmetry == cubeMeshes[i])
						meshData.symmetry = i;
			meshList.Add(meshData);
		}
	}
}

[Serializable]
public class MeshData {
	public SVector3 position;
	public List<SCubeData> cubeList;
	public int symmetry = -1;
	public SVector3 minPos,maxPos;

	public MeshData (CubeMesh cubeMesh) {
		position = new SVector3(cubeMesh.transform.position);
		cubeList = new List<SCubeData> ();
		foreach (CubeData cd in cubeMesh.cubeList)
			cubeList.Add (new SCubeData (cd));
		minPos = new SVector3(cubeMesh.minPos);
		maxPos = new SVector3(cubeMesh.maxPos);
	}
}

[Serializable]
public class SVector3 {
	public float x,y,z;

	public SVector3 (Vector3 v) {
		x = v.x;
		y = v.y;
		z = v.z;
	}

	public Vector3 ToVector3 () {
		return new Vector3 (x, y, z);
	}
}

[Serializable]
public class SColor {
	public float r,g,b;
	
	public SColor (Color c) {
		r = c.r;
		g = c.g;
		b = c.b;
	}

	public Color ToColor () {
		return new Color (r,g,b);
	}
}

[Serializable]
public class SCubeData {
	public SVector3 position;
	public SColor[] colors;

	public SCubeData (CubeData cd) {
		position = new SVector3 (cd.position);
		colors = new SColor[6];
		for (int i=0; i<6; i++)
			colors [i] = new SColor (cd.colors [i]);
	}
}