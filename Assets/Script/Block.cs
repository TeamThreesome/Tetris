//--------------------------------------------
// Jimmy Liu
// OWNSELF
// 2013.6.29
//--------------------------------------------
using UnityEngine;
using System.Collections;

//This is class for Blocks
public class Block {
	
	public int size;	// Size of container box of block
	public bool[,] blocks;	//Define the shape of block
	public Vector3 pos;		//position
	//public int centerX,centerY;
	public int length;		//This means the number of cube of block
	public GameObject[] blockObjects;	//To reference the GameObject
	public GameObject cube;	//Prefab

	void Start () {
	}
	void Update () {
	}
	//Currently the moving is done in GameManager.cs
	public void MoveLeft() {
	}
	public void MoveRight() {
	}
	
	public void SpawnBlock() {
		blockObjects = new GameObject[length];
		Color col = new Color();	//Random color for block
        col.a = 1.0f;
        col.r = Random.Range(0f, 1f);
        col.g = Random.Range(0f, 1f);
        col.b = Random.Range(0f, 1f);
		int index=0;
		for(int i=0;i<size;i++) {
			for(int j=0;j<size;j++) {
				if(blocks[i,j]) {
					blockObjects[index] = (GameObject)Object.Instantiate(cube);
					blockObjects[index].GetComponent<MeshRenderer>().material.SetColor("_Color",col);
					blockObjects[index].transform.position = new Vector3(pos.x+j,pos.y-i,0);
					index++;
				}
			}
		}//for
	}
	
	//Update the position of block
	public void UpdateBlock() {
		for(int i=0;i<length;i++) {
			Vector3 position = blockObjects[i].transform.position;
            float posy = position.y - 1;
            blockObjects[i].transform.position = new Vector3(position.x, posy, position.z);
		}
		pos = new Vector3(pos.x,pos.y-1,0);
	}
	
	//TODO : make the rotate nicer
	public void Rotate() {
        bool[,] newBlocks = new bool[size, size];
        for (int i = 0; i < size;i++ )	//Rotate the array
            for (int j = 0; j < size;j++ )
                newBlocks[size - 1 - j,i] = blocks[i, j];
        blocks = newBlocks;
        int index = 0;
        for(int i=0;i<size;i++) {
			for(int j=0;j<size;j++) {
				if(blocks[i,j]) {	//Update the position of GameObjects
					blockObjects[index].transform.position = new Vector3(pos.x+j,pos.y-i,0);
					index++;
				}
			}
		}
	}
}
