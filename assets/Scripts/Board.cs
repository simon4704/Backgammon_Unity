using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Board
{

    // To fix:

    // Make AI :)


    public List<int> movesLeft;
    public int selectedCheckerIndex;
    public int[] numberOfCheckersAtPositionIndex;
    public Checker[] checkerClasses = new Checker[30];

    public List<int> lastMoveMade = new List<int>(); // resets on dice roll!


    // Actual spawning happens from loop in 'Player' script. This is just here because 'Checker' cannot be created from 'Player'.
    public void SpawnChecker(int color, int spawnPositionIndex, int rowPosition, Vector3 spawnPositionInSpace, int index)
    {
        Checker checkerClass = new Checker(color, spawnPositionIndex, rowPosition, spawnPositionInSpace, index);
        checkerClasses[index] = checkerClass;
    }





    public int FindCheckerIndexFromPositionIndex(int positionIndex)
    {
        int checkerIndex = -1;
        for (int i = 0; i < checkerClasses.Length; i++)
        {
            if (checkerClasses[i].positionOnBoardIndex == positionIndex)
            {
                checkerIndex = i;
            }
        }
        return checkerIndex;
    }



    

    public List<Vector3> FindHighlightCheckerPositions(int selectedCheckerPositionIndex)
    {
        List<Vector3> positionsToMoveTo = new List<Vector3>();
        Checker selectedChecker = checkerClasses[selectedCheckerPositionIndex];

        positionsToMoveTo.Add(selectedChecker.positionInSpace + Vector3.up * 0.1f);

        if (selectedChecker.possibleMoves.Count > 2)
        {
            positionsToMoveTo.Add(NeatFunctions.CalculateCheckerPosition(numberOfCheckersAtPositionIndex, selectedChecker.positionOnBoardIndex + selectedChecker.possibleMoves[0]));
        }
        else
        {
            foreach (int move in selectedChecker.possibleMoves)
            {
                positionsToMoveTo.Add(NeatFunctions.CalculateCheckerPosition(numberOfCheckersAtPositionIndex, selectedChecker.positionOnBoardIndex + move));
            }
        }

        return positionsToMoveTo;
    }



    public List<int> MoveChecker(int moveToUseIndex)
    {
        List<int> indexOfCheckerObjectsToMove = new List<int>();

        Checker checkerToMove = checkerClasses[selectedCheckerIndex];
        int moveUsed;
        int newPositionOnBoardIndex;
        
        moveUsed = checkerToMove.possibleMoves[moveToUseIndex];
        

        newPositionOnBoardIndex = checkerToMove.positionOnBoardIndex + moveUsed;


        lastMoveMade.Add(selectedCheckerIndex);
        lastMoveMade.Add(checkerClasses[selectedCheckerIndex].positionOnBoardIndex);
        lastMoveMade.Add(newPositionOnBoardIndex);


        if (numberOfCheckersAtPositionIndex[newPositionOnBoardIndex] == 1)
        {
            int indexOfCheckerAlreadyInPosition = FindCheckerIndexFromPositionIndex(newPositionOnBoardIndex);
            Checker checkerAlreadyInPosition = checkerClasses[indexOfCheckerAlreadyInPosition];

            if (checkerToMove.checkerColor != checkerAlreadyInPosition.checkerColor)
            {
                numberOfCheckersAtPositionIndex[checkerAlreadyInPosition.positionOnBoardIndex]--;
                checkerAlreadyInPosition.positionOnBoardIndex = NeatFunctions.beatIndices[checkerAlreadyInPosition.checkerColor];
                checkerAlreadyInPosition.positionOnRowIndex = numberOfCheckersAtPositionIndex[checkerAlreadyInPosition.positionOnBoardIndex] + 1;
                checkerAlreadyInPosition.positionInSpace = NeatFunctions.CalculateCheckerPosition(numberOfCheckersAtPositionIndex, checkerAlreadyInPosition.positionOnBoardIndex);
                numberOfCheckersAtPositionIndex[checkerAlreadyInPosition.positionOnBoardIndex]++;

                indexOfCheckerObjectsToMove.Add(indexOfCheckerAlreadyInPosition);
            }
        }


        numberOfCheckersAtPositionIndex[checkerToMove.positionOnBoardIndex]--;
        checkerToMove.positionOnBoardIndex = newPositionOnBoardIndex;
        checkerToMove.positionOnRowIndex = numberOfCheckersAtPositionIndex[checkerToMove.positionOnBoardIndex] + 1;
        checkerToMove.positionInSpace = NeatFunctions.CalculateCheckerPosition(numberOfCheckersAtPositionIndex, checkerToMove.positionOnBoardIndex);
        numberOfCheckersAtPositionIndex[checkerToMove.positionOnBoardIndex]++;

        indexOfCheckerObjectsToMove.Insert(0, selectedCheckerIndex);


        if (newPositionOnBoardIndex == 27 || newPositionOnBoardIndex == 0)
        {
            if (checkerToMove.checkerColor == 0)
            {
                moveUsed += 1;
            }
            else
            {
                moveUsed -= 1;
            }
        }



        
        if (NeatFunctions.rolledPair)
        {
            movesLeft.RemoveAt(movesLeft.Count - 1);
        }
        else
        {
            if (movesLeft.Count == 2)
            {
                movesLeft.Remove(moveUsed);
            }
            else
            {
                movesLeft.RemoveAt(0);
            }
        }

        NeatFunctions.checkerSelected = false;

        return indexOfCheckerObjectsToMove;

    }




    public bool CheckIfSpotIsAvailable(Checker checkerToMove, int newPositionOnBoardIndex)
    {
        bool isAvailable = true;
        if (numberOfCheckersAtPositionIndex[newPositionOnBoardIndex] > 1)
        {
            int checkerColorAtNewPosition = checkerClasses[FindCheckerIndexFromPositionIndex(newPositionOnBoardIndex)].checkerColor;
            if (checkerColorAtNewPosition != checkerToMove.checkerColor)
            {
                isAvailable = false;
            }
        }
        return isAvailable;
    }





    // All the shit that has to do with moveability
    public bool CheckMoveability(Checker checker)
    {
        bool canMove = false;

        bool isTurn = false;
        if (checker.checkerColor == NeatFunctions.whoseTurnIsIt){
            isTurn = true;
        }

        bool allCheckersHaveExited = false;
        if (numberOfCheckersAtPositionIndex[NeatFunctions.finishedIndices[NeatFunctions.whoseTurnIsIt]] == 15)
        {
            allCheckersHaveExited = true;
        }


        bool noCheckersBeat = CheckIfNoCheckersBeat();
        bool checkerIsBeat = CheckIfCheckerIsBeat(checker);
        bool isOnTop = CheckIfOnTopOfRow(checker);
        List<int> possibleMovesOnBoard = CheckIfPossibleMovesOnBoard(checker);
        bool allCheckersInLastCell = CheckIfAllCheckersInLastCell();
        bool anyCanExitExact = CheckIfAnyCanExitExact();
        List<int> possibleExitExactMove = CheckIfCanExitExact(checker);
        bool isInBack = CheckIfInBack(checker);
        List<int> possibleExitMoves = CheckIfCanExit(checker);


        //Debug.Log("Reach first check shit");
        if (isTurn)
        {
            //Debug.Log("Is turn");
            if (!allCheckersHaveExited)
            {
                // Debug.Log("Still checkers on board");
                if (noCheckersBeat)
                {
                    //Debug.Log("No checkers beat");
                    //Debug.Log("position: " + checker.positionOnBoardIndex + " n of check at pos: " + numberOfCheckersAtPositionIndex[checker.positionOnBoardIndex]);
                    if (isOnTop)
                    {
                        //Debug.Log("Is on top");
                        if (allCheckersInLastCell)
                        {
                            // Debug.Log("All in last cell");
                            if (anyCanExitExact)
                            {
                                // Debug.Log("Any can exit exact");
                                if (possibleExitExactMove.Count > 0)
                                {
                                    canMove = true;
                                    checker.SetPossibleMoves(possibleExitExactMove);
                                }
                            }
                            else
                            {
                                if (isInBack)
                                {
                                    // Debug.Log("is in back");
                                    // List<int> allPossibleMoves = new List<int>();

                                    if (possibleExitMoves.Count > 0)
                                    {
                                        List<int> possibleExitMove = new List<int>();
                                        possibleExitMove.Add(possibleExitMoves.Min());
                                        canMove = true;
                                        checker.SetPossibleMoves(possibleExitMove);
                                    }

                                    else if (possibleMovesOnBoard.Count > 0)
                                    {
                                        canMove = true;
                                        checker.SetPossibleMoves(possibleMovesOnBoard);
                                    }
                                    
                                    //if (allPossibleMoves.Count > 0)
                                    //{
                                    //    checker.SetPossibleMoves(allPossibleMoves);
                                    //}
                                }
                            }
                        }
                        else
                        {
                            //Debug.Log("N of moves left: " + movesLeft.Count + " n of moves on board: " + possibleMovesOnBoard.Count);
                            //Debug.Log("Reach this shit");
                            if (possibleMovesOnBoard.Count > 0)
                            {
                                canMove = true;
                                checker.SetPossibleMoves(possibleMovesOnBoard);
                            }
                        }

                    }
                }
                else
                {
                    if (checkerIsBeat)
                    {
                        if (isOnTop)
                        {
                            if (possibleMovesOnBoard.Count > 0)
                            {
                                canMove = true;
                                checker.SetPossibleMoves(possibleMovesOnBoard);
                            }
                        }
                    }
                }
            }            
        }
        
        return canMove;
    }

    

    public bool CheckIfNoCheckersBeat()
    {
        bool noCheckersBeat = true;
        int beatCheckerPosition = NeatFunctions.beatIndices[NeatFunctions.whoseTurnIsIt];

        if (numberOfCheckersAtPositionIndex[beatCheckerPosition] > 0)
        {
            noCheckersBeat = false;
        }

        return noCheckersBeat;
    }


    public bool CheckIfCheckerIsBeat(Checker checker)
    {
        bool checkerIsBeat = false;
        if (checker.positionOnBoardIndex == 1 || checker.positionOnBoardIndex == 26)
        {
            checkerIsBeat = true;
        }

        return checkerIsBeat;
    }


    public List<int> CheckIfPossibleMovesOnBoard(Checker checker)
    {
        List<int> possibleMovesOnBoard = new List<int>();
        //Debug.Log("(From CheckPossibleMoves)  N of moves left: " + movesLeft.Count);
        //Debug.Log("Checker position: " + checker.positionOnBoardIndex);
        foreach (int move in movesLeft)
        {
            int resultingPosition = checker.positionOnBoardIndex + move;
            //Debug.Log("Move to be made: " + move + " Resulting position: " + resultingPosition);
            if (resultingPosition > 1 && resultingPosition < 26)
            {
                //Debug.Log("Resulting position on board");
                if (numberOfCheckersAtPositionIndex[resultingPosition] > 1)
                {
                    //Debug.Log("Spot is occupied");
                    if (CheckIfSpotIsAvailable(checker, resultingPosition))
                    {
                        //Debug.Log("  -by same color");
                        possibleMovesOnBoard.Add(move);
                    }
                }
                else
                {
                    //Debug.Log("Spot is not occupied");
                    possibleMovesOnBoard.Add(move);
                }
            }
        }
        //Debug.Log("N of possible moves from function: " + possibleMovesOnBoard.Count);
        return possibleMovesOnBoard;
    }


    public bool CheckIfAnyMovesPossible()
    {
        bool movesPossible = false;
        foreach (Checker checker in checkerClasses)
        {
            if (CheckMoveability(checker))
            {
                movesPossible = true;
            }
        }
        return movesPossible;
    }


    public bool CheckIfAllCheckersInLastCell()
    {
        // print("Checking exit");
        bool allCheckersInLastCell = true;
        foreach (Checker checker in checkerClasses)
        {
            if (NeatFunctions.whoseTurnIsIt == 0)
            {
                if (checker.checkerColor == 0)
                {
                    if (checker.positionOnBoardIndex > 7)
                    {
                        allCheckersInLastCell = false;
                    }
                }
            }
            else
            {
                if (checker.checkerColor == 1)
                {
                    if (checker.positionOnBoardIndex < 20)
                    {
                        allCheckersInLastCell = false;
                    }
                }
            }
        }
        return allCheckersInLastCell;
    }


    public List<int> CheckIfCanExit(Checker checker)
    {
        List<int> canExitMoves = new List<int>();
        if (checker.checkerColor == 0)
        {
            foreach (int move in movesLeft)
            {
                if (checker.positionOnBoardIndex + move < 1)
                {
                    canExitMoves.Add(-checker.positionOnBoardIndex);
                }
            }
        }
        else
        {
            foreach (int move in movesLeft)
            {
                if (checker.positionOnBoardIndex + move > 26)
                {
                    canExitMoves.Add(27 - checker.positionOnBoardIndex);
                }
            }
        }
        return canExitMoves;
    }


    public bool CheckIfAnyCanExitExact()
    {
        bool anyCanExicExact = false;
        foreach (Checker checker in checkerClasses)
        {
            if (CheckIfCanExitExact(checker).Count > 0)
            {
                anyCanExicExact = true;
            }
        }
        return anyCanExicExact;
    }


    public List<int> CheckIfCanExitExact(Checker checker)
    {
        List<int> possibleExitExactMove = new List<int>();
        foreach (int move in movesLeft)
        {
            if (checker.checkerColor == NeatFunctions.whoseTurnIsIt)
            {
                if (checker.positionOnBoardIndex + move == NeatFunctions.exitIndices[checker.checkerColor]) // !OBS! has to be 'beatIndices' as 'exitIndices' would be too large
                {
                    // Debug.Log("Index of checker that can move: " + checker.checkerIndex);
                    int actualMove = move;
                    if (checker.checkerColor == 0)
                    {
                        actualMove -= 1;
                    }
                    else
                    {
                        actualMove += 1;
                    }
                    possibleExitExactMove.Add(actualMove);
                }
            }
        }

        return possibleExitExactMove;
    }


    public bool CheckIfOnTopOfRow(Checker checker)
    {
        if (checker.positionOnRowIndex == numberOfCheckersAtPositionIndex[checker.positionOnBoardIndex])
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public bool CheckIfInBack(Checker checker)
    {
        bool isInBack = true;
        
        if (checker.checkerColor == 0)
        {
            foreach (Checker checkerInArray in checkerClasses)
            {
                if (checkerInArray.checkerColor == 0)
                {
                    if (checkerInArray.positionOnBoardIndex > checker.positionOnBoardIndex)
                    {
                        isInBack = false;
                    }
                }
            }
        }
        else
        {
            foreach (Checker checkerInArray in checkerClasses)
            {
                if (checkerInArray.checkerColor == 1)
                {
                    if (checkerInArray.positionOnBoardIndex < checker.positionOnBoardIndex)
                    {
                        isInBack = false;
                    }
                }
            }
        }
        return isInBack;
    }
    // End of all the shit that has to do with moveability





    



    







    public class Checker
    {
        public int checkerColor; // 0 = black, 1 = white
        public int positionOnBoardIndex;
        public int positionOnRowIndex;
        public Vector3 positionInSpace;
        public int checkerIndex;

        public Checker(int checkerColorToAdd, int positionOnBoardToAdd, int positionOnRowIndexToAdd, Vector3 positionInSpaceToAdd, int checkerIndexToAdd)
        {
            checkerColor = checkerColorToAdd;
            positionOnBoardIndex = positionOnBoardToAdd;
            positionOnRowIndex = positionOnRowIndexToAdd;
            positionInSpace = positionInSpaceToAdd;
            checkerIndex = checkerIndexToAdd;
        }


        

        public bool canMove;
        public List<int> possibleMoves = new List<int>();
        public void SetPossibleMoves(List<int> moves)
        {
            if (moves.Count > 0)
            {
                canMove = true;

                possibleMoves.Clear();
                foreach (int move in moves)
                {
                    possibleMoves.Add(move);
                }
            }
            else
            {
                canMove = false;
            }
            
        }

    }


}







