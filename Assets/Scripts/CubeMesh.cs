using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeData {
	public Vector3 position;
	public Color[] colors = new Color[6];
}

public class CubeMesh : MonoBehaviour {

	public CubeMesh symmetry;
	public List<CubeData> cubeList = new List<CubeData> ();

	public Vector3 minPos, maxPos;

	MeshFilter mFil;
	MeshCollider mCol;
	
	bool[,,] cubeTable;
	
	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Color> colors = new List<Color> ();

	void Start () {

	}

	void Update () {
	
	}

	public void AddCube (Vector3 pos, Color color) {
		CubeData newCube = new CubeData ();
		newCube.position = pos;
		newCube.colors = new Color[6];
		for (int i=0; i<6; i++) newCube.colors [i] = color;
		cubeList.Add (newCube);
	}

	public void RemoveCube (Vector3 pos) {
		foreach (CubeData cube in cubeList) {
			if(cube.position == pos) {
				cubeList.Remove(cube);
				return;
			}
		}
	}

	public void SetColor (Vector3 pos, Vector3 dir, Color color) {
		foreach (CubeData cube in cubeList) {
			if(cube.position == pos) {
				if (dir==Vector3.right) cube.colors[0]=color;
				else if (dir==Vector3.left) cube.colors[1]=color;
				else if (dir==Vector3.up) cube.colors[2]=color;
				else if (dir==Vector3.down) cube.colors[3]=color;
				else if (dir==Vector3.forward) cube.colors[4]=color;
				else if (dir==Vector3.back) cube.colors[5]=color;
				return;
			}
		}
	}

	public bool HasCube (Vector3 pos) {
		if (pos.x < minPos.x || pos.x > maxPos.x || pos.y < minPos.y || pos.y > maxPos.y || pos.z < minPos.z || pos.z > maxPos.z)
			return false;
		return cubeTable [(int)(pos.x - minPos.x), (int)(pos.y - minPos.y), (int)(pos.z - minPos.z)];
	}

	public void UpdateMesh () {
		vertices.Clear ();
		triangles.Clear ();
		colors.Clear ();

		UpdateBounds ();
		UpdateTable ();

		foreach (CubeData cube in cubeList) 
			CubeToMesh(cube);

		mFil = GetComponent<MeshFilter> ();
		mCol = GetComponent<MeshCollider> ();
		mFil.mesh.Clear();
		mFil.mesh.vertices = vertices.ToArray();
		mFil.mesh.colors = colors.ToArray();
		mFil.mesh.triangles = triangles.ToArray();
		mFil.mesh.RecalculateNormals ();
		mCol.sharedMesh = new Mesh ();
		mCol.sharedMesh = mFil.mesh;
	}

	public int GetCubeCount () {
		return cubeList.Count;
	}

	public void LoadFromMeshData (MeshData meshData) {
		foreach (SCubeData scd in meshData.cubeList) {
			CubeData cd = new CubeData();
			cd.position = scd.position.ToVector3();
			for (int i=0; i<6; i++) cd.colors[i] = scd.colors[i].ToColor();
			cubeList.Add(cd);
		}
		minPos = meshData.minPos.ToVector3();
		maxPos = meshData.maxPos.ToVector3();
		UpdateMesh ();
	}

	void UpdateBounds () {
		minPos = new Vector3 (int.MaxValue, int.MaxValue, int.MaxValue);
		maxPos = new Vector3 (int.MinValue, int.MinValue, int.MinValue);

		foreach (CubeData cube in cubeList) {
			Vector3 pos = cube.position;
			if (pos.x < minPos.x) minPos.x = pos.x;
			if (pos.y < minPos.y) minPos.y = pos.y;
			if (pos.z < minPos.z) minPos.z = pos.z;
			if (pos.x > maxPos.x) maxPos.x = pos.x;
			if (pos.y > maxPos.y) maxPos.y = pos.y;
			if (pos.z > maxPos.z) maxPos.z = pos.z;
		}
	}

	void UpdateTable () {
		cubeTable = new bool[(int)(maxPos.x - minPos.x) + 1, (int)(maxPos.y - minPos.y) + 1, (int)(maxPos.z - minPos.z) + 1];
		foreach (CubeData cube in cubeList) {
			Vector3 pos = cube.position;
			cubeTable[(int)(pos.x-minPos.x),(int)(pos.y-minPos.y),(int)(pos.z-minPos.z)] = true;
		}
	}

	void CubeToMesh (CubeData cube) {
		float x = cube.position.x, y = cube.position.y, z = cube.position.z;
		if (!HasCube (new Vector3 (x + 1, y, z))) {
			vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
			for(int i=0;i<4;i++) colors.Add(cube.colors[0]);
			AddQuadTriangles ();
		}
		if (!HasCube (new Vector3 (x - 1, y, z))) {
			vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
			vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
			vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
			vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
			for(int i=0;i<4;i++) colors.Add(cube.colors[1]);
			AddQuadTriangles ();
		}
		if (!HasCube (new Vector3 (x, y + 1, z))) {
			vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
			vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
			for(int i=0;i<4;i++) colors.Add(cube.colors[2]);
			AddQuadTriangles ();
		}
		if (!HasCube (new Vector3 (x, y - 1, z))) {
			vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
			vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
			for(int i=0;i<4;i++) colors.Add(cube.colors[3]);
			AddQuadTriangles ();
		}
		if (!HasCube (new Vector3 (x, y, z + 1))) {
			vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
			vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
			vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
			for(int i=0;i<4;i++) colors.Add(cube.colors[4]);
			AddQuadTriangles ();
		}
		if (!HasCube (new Vector3 (x, y, z - 1))) {
			vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
			vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
			vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
			for(int i=0;i<4;i++) colors.Add(cube.colors[5]);
			AddQuadTriangles ();
		}
	}

	void AddQuadTriangles () {
		triangles.Add(vertices.Count - 4);
		triangles.Add(vertices.Count - 3);
		triangles.Add(vertices.Count - 2);
		
		triangles.Add(vertices.Count - 4);
		triangles.Add(vertices.Count - 2);
		triangles.Add(vertices.Count - 1);
	}
}
