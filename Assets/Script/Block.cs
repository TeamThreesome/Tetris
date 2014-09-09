//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2013.6.29
//------------------------------------------------------------------------------
using UnityEngine;
// using System.Collections;

//------------------------------------------------------------------------------
// This is class for a Block
public class Block {

    //--------------------------------------------------------------------------

    public int              mSize;          // Side length of container box of block
    public bool[,]          mBlocks;        // Define the shape of block
    public Vector3          mPos;           // position
    public int              mLength;        // This means the number of cube of block
    public GameObject[]     mBlockObjects;  // To reference the GameObject

    //--------------------------------------------------------------------------
    public void Init(Block blockType, GameObject cubePrefab, Vector3 startPosition, Color col) {

        mSize   = blockType.mSize;
        mBlocks = blockType.mBlocks;
        mLength = blockType.mLength;
        mPos    = startPosition;

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

    public void Destroy() {
        for (int i = 0; i < mLength; i++) {
            if (mBlockObjects[i] != null)
                Object.Destroy(mBlockObjects[i]);
        }
    }
    
    //--------------------------------------------------------------------------
    // Update the position of block
    public void UpdateBlock() {

        // Move the blocks
        for (int i = 0; i < mLength; i++)
            mBlockObjects[i].transform.position += Vector3.down;

        // Mark the position down 1
        mPos += Vector3.down;
    }
    
    //--------------------------------------------------------------------------
    //TODO : make the rotate nicer
    public void Rotate(bool IsClockwise) {

        bool[,] newBlocks = new bool[mSize, mSize];

        // Rotate the array
        for (int i = 0; i < mSize; i++) {
            for (int j = 0; j < mSize; j++) {
                if (IsClockwise)
                    newBlocks[i, j] = mBlocks[mSize - 1 - j, i];
                else
                    newBlocks[mSize - 1 - j, i] = mBlocks[i, j];
            }
        }
        mBlocks = newBlocks;

        // Update position of GameObjects after rotation
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