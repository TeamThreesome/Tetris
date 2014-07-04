//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2013.6.29
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

//------------------------------------------------------------------------------
//This is class for a Block
public class Block {
	
    //--------------------------------------------------------------------------
    
	public int 				mSize;			//Size of container box of block
	public bool[,]			mBlocks;		//Define the shape of block
	public Vector3 			mPos;			//position
	public int 				mLength;		//This means the number of cube of block
	public GameObject[]		mBlockObjects;	//To reference the GameObject

    //--------------------------------------------------------------------------
	public void Init(Block blockType, GameObject cubePrefab, Vector3 startPosition) {

        mSize 	= blockType.mSize;
		mBlocks = blockType.mBlocks;
		mLength = blockType.mLength;
		mPos 	= startPosition;

		//Random color for block
		Color col = new Color();	
        col.r = Random.Range(0f, 1f);
        col.g = Random.Range(0f, 1f);
        col.b = Random.Range(0f, 1f);
        col.a = 1.0f; //Opacity

		mBlockObjects = new GameObject[mLength];
		for (int i = 0, index = 0; i < mSize; i++) {
			for (int j = 0; j < mSize; j++) {
				if (mBlocks[i,j]) {
					mBlockObjects[index] = (GameObject)Object.Instantiate(cubePrefab);
					mBlockObjects[index].GetComponent<MeshRenderer>().material.SetColor("_Color", col);
					mBlockObjects[index].transform.position = new Vector3(mPos.x + j, mPos.y - i, 0);
					index++;
				}
			}
		}
	}
	
    //--------------------------------------------------------------------------
	//Update the position of block
	public void UpdateBlock() {

		//Move the blocks
		for (int i = 0; i < mLength; i++)
            mBlockObjects[i].transform.position += Vector3.down;

		mPos += Vector3.down;
	}
	
    //--------------------------------------------------------------------------
	//TODO : make the rotate nicer
	public void Rotate() {

        bool[,] newBlocks = new bool[mSize, mSize];

        //Rotate the array
        for (int i = 0; i < mSize; i++) {
            for (int j = 0; j < mSize; j++) {
                newBlocks[mSize - 1 - j, i] = mBlocks[i, j];
            }
        }
        mBlocks = newBlocks;

		//Update position of GameObjects after rotation
        for (int i = 0, index = 0; i < mSize; i++) {
			for (int j = 0; j < mSize; j++) {
				if (mBlocks[i,j]) {
					mBlockObjects[index].transform.position = new Vector3(mPos.x + j, mPos.y - i, 0);
					index++;
				}
			}
		}
	}
    //--------------------------------------------------------------------------
}