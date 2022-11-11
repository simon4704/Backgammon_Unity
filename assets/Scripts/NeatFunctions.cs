using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NeatFunctions
{
    public static int whoseTurnIsIt = 0; // 0 means black turn, 1 means white turn.
    public static int whoseTurnIsItNot = 1;
    public static bool diceRolled = false;
    public static bool rolledPair;
    public static bool checkerSelected = false;
    public static int selectedCheckerIndex;

    public static int[] spawnPositionIndexLookupTable;
    public static Vector3[] boardPositions;

    public static int[] exitIndices = new int[] { 1, 26 }; // black checker exits at 1 then goes to 0, white exits at 26 then goes to 27
    public static int[] beatIndices = new int[] { 26, 1 }; // black beat checker goes to 26, white goes to 1
    public static int[] finishedIndices = new int[] { 0, 27 };

    public static float triangleHeight;
    public static float triangleWidth;





    public static void InitializeImportantValues()
    {
        spawnPositionIndexLookupTable = new int[] { 25, 25, 14, 14, 14, 14, 14, 9, 9, 9, 7, 7, 7, 7, 7 }; // default starting positions
        // spawnPositionIndexLookupTable = new int[] { 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 6, 6, 7, 7, }; // all checkers in last cell
        // spawnPositionIndexLookupTable = new int[] { 25, 25, 14, 14, 9, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // 2 checkers at start
        // spawnPositionIndexLookupTable = new int[] { 25, 9, 3, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // 2 checkers a bit apart
        // spawnPositionIndexLookupTable = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // all checkers have exited

        triangleHeight = 5f;
        triangleWidth = 1f;

        boardPositions = new Vector3[28];
        for (int i = 0; i < 8; i++)
        {
            boardPositions[i] = new Vector3((float)(8f - (i - 0f)) * triangleWidth, 0, -1.2f * triangleHeight + 0.5f * triangleWidth);
        }
        for (int i = 8; i < 14; i++)
        {
            boardPositions[i] = new Vector3((float)(-1f - (i - 8f)) * triangleWidth, 0, -1.2f * triangleHeight + 0.5f * triangleWidth);
        }
        for (int i = 14; i < 20; i++)
        {
            boardPositions[i] = new Vector3((float)(-6f + (i - 14f)) * triangleWidth, 0, 1.2f * triangleHeight - 0.5f * triangleWidth);
        }
        for (int i = 20; i < 28; i++)
        {
            boardPositions[i] = new Vector3((float)(1f + (i - 20f)) * triangleWidth, 0, 1.2f * triangleHeight - 0.5f * triangleWidth);
        }
    }


    public static int[] SetNumberOfCheckersAtPositionIndex()
    {
        int[] numberOfCheckersAtIndex = new int[28];
        for (int i = 0; i < numberOfCheckersAtIndex.Length; i++)
        {
            numberOfCheckersAtIndex[i] = 0;
        }
        return numberOfCheckersAtIndex;
    }




    public static Vector3 CalculateCheckerPosition(int[] numberOfCheckersAtPositionIndex, int positionOnBoardIndex)
    {
        Vector3 checkerPosition = boardPositions[positionOnBoardIndex];

        if (positionOnBoardIndex < 14)
        {
            checkerPosition += Vector3.forward * numberOfCheckersAtPositionIndex[positionOnBoardIndex] * 0.8f;
        }
        else
        {
            checkerPosition += Vector3.back * numberOfCheckersAtPositionIndex[positionOnBoardIndex] * 0.8f;
        }

        return checkerPosition;
    }


    public static List<int> RollDice(int fixedRoll1, int fixedRoll2)
    {
        List<int> movesLeft;

        int roll1;
        int roll2;

        if (fixedRoll1 == 0 && fixedRoll2 == 0)
        {
            roll1 = Random.Range(1, 7);
            roll2 = Random.Range(1, 7);
        }
        else
        {
            roll1 = fixedRoll1;
            roll2 = fixedRoll2;
        }


        if (whoseTurnIsIt == 0)
        {
            roll1 *= -1;
            roll2 *= -1;
        }

        if (roll1 == roll2)
        {
            movesLeft = new List<int> { roll1, roll1, roll1, roll1 };
            rolledPair = true;
        }
        else
        { // if one of the fixed rolls is 0, that roll should be ignored so only 1 move is left
            if (roll1 == 0)
            {
                movesLeft = new List<int> { roll2 };
            }
            else if (roll2 == 0)
            {
                movesLeft = new List<int> { roll1 };
            }
            else
            {
                movesLeft = new List<int> { Mathf.FloorToInt(Mathf.Min(roll1, roll2)), Mathf.FloorToInt(Mathf.Max(roll1, roll2)) };
                rolledPair = false;
            }
        }

        return movesLeft;
    }





    








}
