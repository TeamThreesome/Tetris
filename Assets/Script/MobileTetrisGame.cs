//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2014.9.16
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

//------------------------------------------------------------------------------
// Class for implementation of touch control Tetris game
// Especially for running on mobile platform
public class MobileTetrisGame : TetrisGame {
	//--------------------------------------------------------------------------
    // Mobile Mode : The action to try to move the all blocks left once
    public override bool MoveRight() {

        // Check if it can move right
        for (int i = 0; i < mActiveBlock.mLength; i++) {
            Vector3 pos = mActiveBlock.mBlockObjects[i].transform.position;
            if (mActiveBlock.mBlockObjects[i].transform.position.x >= MaxBlocksWidth - 1
                    || mTetrisState[((int)pos.x + 1),(int)pos.y]) {
                return false;
            }
        }

        for (int h = 0; h < MaxBlocksHeight; ++h) {
            bool firstState = mTetrisState[0, h];
            for (int w = 0; w < MaxBlocksWidth - 1; ++w) {
                mTetrisState[w, h] = mTetrisState[w+1, h];
            }
            mTetrisState[MaxBlocksWidth-1, h] = firstState;
        }

        for (int h = 0; h < MaxBlocksHeight; ++h) {
            GameObject firstObject = mGameBlockObjects[0, h];
            for (int w = 0; w < MaxBlocksWidth - 1; ++w) {
                mGameBlockObjects[w, h] = mGameBlockObjects[w + 1, h];
                if (mGameBlockObjects[w, h] != null) {
                    Vector3 pos = mGameBlockObjects[w, h].transform.position;
                    mGameBlockObjects[w, h].transform.position = new Vector3(pos.x - 1, pos.y, pos.z);
                }
            }
            mGameBlockObjects[MaxBlocksWidth - 1, h] = firstObject;
            if (mGameBlockObjects[MaxBlocksWidth - 1, h] != null) {
                Vector3 pos = mGameBlockObjects[MaxBlocksWidth - 1, h].transform.position;
                mGameBlockObjects[MaxBlocksWidth - 1, h].transform.position = new Vector3(pos.x - 1 + MaxBlocksWidth, pos.y, pos.z);
            }
        }

        return true;
    }
    
    //--------------------------------------------------------------------------
    // Mobile Mode : The action to try to move the all blocks right once
    public override bool MoveLeft() {

        // Check if it can move left
        for (int i = 0; i < mActiveBlock.mLength; i++) {
            Vector3 blockPos = mActiveBlock.mBlockObjects[i].transform.position;
            if (mActiveBlock.mBlockObjects[i].transform.position.x <= 0
                    || mTetrisState[((int)blockPos.x - 1), (int)blockPos.y]) {
                return false;
            }
        }

        for (int h = 0; h < MaxBlocksHeight; ++h) {
            bool lastState = mTetrisState[MaxBlocksWidth - 1, h];
            for (int w = MaxBlocksWidth - 1; w > 0; w--) {
                mTetrisState[w, h] = mTetrisState[w - 1, h];
            }
            mTetrisState[0, h] = lastState;
        }

        for (int h = 0; h < MaxBlocksHeight; ++h) {
            GameObject lastObject = mGameBlockObjects[MaxBlocksWidth - 1, h];
            for (int w = MaxBlocksWidth - 1; w > 0; w--) {
                mGameBlockObjects[w, h] = mGameBlockObjects[w - 1, h];
                if (mGameBlockObjects[w, h] != null) {
                    Vector3 pos = mGameBlockObjects[w, h].transform.position;
                    mGameBlockObjects[w, h].transform.position = new Vector3(pos.x + 1, pos.y, pos.z);
                }
            }
            mGameBlockObjects[0, h] = lastObject;
            if (mGameBlockObjects[0, h] != null) {
                Vector3 pos = mGameBlockObjects[0, h].transform.position;
                mGameBlockObjects[0, h].transform.position = new Vector3(pos.x + 1 - MaxBlocksWidth, pos.y, pos.z);
            }
        }

        return true;
    }
}

