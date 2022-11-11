using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    Board actualBoard;


    public GameObject blackCheckerPrefab;
    public GameObject whiteCheckerPrefab;
    public GameObject checkerHightlightPrefab;

    GameObject[] checkerObjects;
    GameObject[] checkerHighlightObjects;

    public enum DiceRollType { Random, Fixed};
    public DiceRollType diceRollType;

    [Range(0, 6)] public int fixedRoll1;
    [Range(0, 6)] public int fixedRoll2;

    public bool printBoardEvaluations;

    public Vector2 soloCheckerValue;
    public Vector2 forwardSoloCheckerMultiplier; // prolly easiest to leave at 1
    public Vector2 beatOpponentCheckerValue;
    public Vector2 frontRearBiasValue; // positive value means forward bias, which prefers to move forward checkers forward
                                       // negative value means rearward bias, which prefers to move rearward checkers forward

    float bestScoreSoFar;
    Board bestBoardSoFar;

    // Start is called before the first frame update
    void Start()
    {
        NeatFunctions.InitializeImportantValues();

        actualBoard = new Board();

        checkerObjects = new GameObject[30];
        checkerHighlightObjects = new GameObject[3];

        actualBoard.numberOfCheckersAtPositionIndex = NeatFunctions.SetNumberOfCheckersAtPositionIndex();
        SpawnCheckers();
    }

    // Update is called once per frame
    void Update()
    {
        CheckGameState();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StupidFix();
        }



        if (Input.GetKeyDown(KeyCode.P))
        {
            for (int i = 0; i < 30; i++)
            {
                print("Checker index: " + i + " position on board: " + actualBoard.checkerClasses[i].positionOnBoardIndex + " position on row: " + actualBoard.checkerClasses[i].positionOnRowIndex);
            }
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            for (int i = 0; i < actualBoard.lastMoveMade.Count / 3; i++)
            {
                print("Checker " + actualBoard.lastMoveMade[i * 3] + " moved from " + actualBoard.lastMoveMade[i * 3 + 1] + " to " + actualBoard.lastMoveMade[i * 3 + 2]);
            }
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            ResetBoard();
        }
    }
    

    void CheckGameState()
    {
        if (NeatFunctions.diceRolled == false)
        {
            if (CheckMouseOverRollDice() || Input.GetKeyDown(KeyCode.R))
            {
                if (diceRollType == DiceRollType.Random)
                {
                    actualBoard.movesLeft = NeatFunctions.RollDice(0, 0);
                    print("Roll: " + Mathf.Abs(actualBoard.movesLeft[0]) + " and " + Mathf.Abs(actualBoard.movesLeft[1]));
                    NeatFunctions.diceRolled = true;
                }
                else
                {
                    actualBoard.movesLeft = NeatFunctions.RollDice(fixedRoll1, fixedRoll2);
                    print("Fixed roll: " + fixedRoll1 + " and " + fixedRoll2);
                    NeatFunctions.diceRolled = true;
                }
                actualBoard.lastMoveMade = new List<int>();
                
            }
        }
        else
        {
            if (actualBoard.CheckIfAnyMovesPossible())
            {
                if (CheckMouseOverAI() == NeatFunctions.whoseTurnIsIt || Input.GetKeyDown(KeyCode.E))
                {
                    bestScoreSoFar = -10000f;
                    Board AIBoard = CopyBoard(actualBoard);
                    //print("AIBoard hash: " + AIBoard.GetHashCode());
                    //print("AIshit");
                    //print("Moves on actual: " + actualBoard.movesLeft.Count + " moves on AI: " + AIBoard.movesLeft.Count);
                    LoopThroughMoves(AIBoard);
                    actualBoard = CopyBoard(bestBoardSoFar);
                    UpdateCheckerPositions(actualBoard);
                }

                else if (NeatFunctions.checkerSelected == false)
                {
                    int hoveredCheckerIndex = FindCheckerIndexAtCursor();

                    if (hoveredCheckerIndex != -1)
                    {
                        if (Input.GetKeyDown(KeyCode.P))
                        {
                            print(actualBoard.checkerClasses[hoveredCheckerIndex].possibleMoves.Count);
                        }
                        if (actualBoard.CheckMoveability(actualBoard.checkerClasses[hoveredCheckerIndex]))
                        {
                            List<Vector3> highlightCheckerPositions = actualBoard.FindHighlightCheckerPositions(hoveredCheckerIndex);
                            MoveHighlightCheckers(highlightCheckerPositions);

                            if (Input.GetMouseButtonDown(0))
                            {
                                actualBoard.selectedCheckerIndex = hoveredCheckerIndex;
                                NeatFunctions.checkerSelected = true;
                            }
                        }
                    }
                    else
                    {
                        ResetHighlightCheckerPositions();
                    }
                }
                else
                {
                    int hoveredHighlightChecker = FindHighlightCheckerAtCursor();

                    if (hoveredHighlightChecker != -1)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            List<int> newCheckerPositionInSpace = actualBoard.MoveChecker(hoveredHighlightChecker - 1);
                            MoveCheckerObjects(newCheckerPositionInSpace);
                            ResetHighlightCheckerPositions();
                        }
                    }
                }
            }
            else
            {
                ChangeTurns();
            }
        }
    }





    // Player related stuff
    int FindCheckerIndexAtCursor()
    {
        int hitCheckerIndex = 0;
        bool checkerHit = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            int hitObjectInstanceID = hit.collider.gameObject.GetInstanceID();

            for (int i = 0; i < checkerObjects.Length; i++)
            {
                if (hitObjectInstanceID == checkerObjects[i].GetInstanceID())
                {
                    checkerHit = true;
                    hitCheckerIndex = i;
                }
            }
        }
        if (checkerHit)
        {
            return hitCheckerIndex;
        }
        else
        {
            return -1;
        }
    }


    int FindHighlightCheckerAtCursor()
    {
        int hitCheckerIndex = -1;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            int hitObjectInstanceID = hit.collider.gameObject.GetInstanceID();

            for (int i = 1; i < checkerHighlightObjects.Length; i++)
            {
                if (hitObjectInstanceID == checkerHighlightObjects[i].GetInstanceID())
                {
                    hitCheckerIndex = i;
                }
            }
        }
        return hitCheckerIndex;
    }


    bool CheckMouseOverRollDice()
    {
        bool rollDiceButtonHit = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.tag == "RollDiceButton")
            {
                Debug.DrawLine(ray.origin, hit.point);
                if (Input.GetMouseButtonDown(0))
                {
                    rollDiceButtonHit = true;
                }
            }
        }
        return rollDiceButtonHit;
    }


    void ResetHighlightCheckerPositions()
    {
        foreach (GameObject checker in checkerHighlightObjects)
        {
            checker.transform.position = Vector3.down;
        }
    }


    void MoveHighlightCheckers(List<Vector3> positions)
    {
        int highlightCheckerIndex = 0;
        foreach (Vector3 position in positions)
        {
            checkerHighlightObjects[highlightCheckerIndex].transform.position = position;
            highlightCheckerIndex++;
        }
    }


    void MoveCheckerObjects(List<int> indices)
    {
        foreach (int index in indices)
        {
            checkerObjects[index].transform.position = actualBoard.checkerClasses[index].positionInSpace;
        }
    }


    void ChangeTurns()
    {
        NeatFunctions.diceRolled = false;

        if (NeatFunctions.whoseTurnIsIt == 0)
        {
            NeatFunctions.whoseTurnIsIt = 1;
            print("White turn");
            NeatFunctions.whoseTurnIsItNot = 0;
        }
        else
        {
            NeatFunctions.whoseTurnIsIt = 0;
            print("Black turn");
            NeatFunctions.whoseTurnIsItNot = 1;
        }
    }



    void SpawnCheckers()
    {
        // Spawn black checkers
        for (int i = 0; i < 15; i++)
        {
            int spawnPositionIndex = NeatFunctions.spawnPositionIndexLookupTable[i];
            Vector3 checkerSpawnPosition = NeatFunctions.CalculateCheckerPosition(actualBoard.numberOfCheckersAtPositionIndex, spawnPositionIndex);

            GameObject checkerObject = (GameObject)Instantiate(blackCheckerPrefab, checkerSpawnPosition, Quaternion.identity);
            checkerObjects[i] = checkerObject;

            actualBoard.SpawnChecker(0, spawnPositionIndex, actualBoard.numberOfCheckersAtPositionIndex[spawnPositionIndex] + 1, checkerSpawnPosition, i);

            actualBoard.numberOfCheckersAtPositionIndex[spawnPositionIndex]++;
        }

        // Spawn white checkers
        for (int i = 15; i < 30; i++)
        {
            int spawnPositionIndex = 27 - NeatFunctions.spawnPositionIndexLookupTable[i - 15];
            Vector3 checkerSpawnPosition = NeatFunctions.CalculateCheckerPosition(actualBoard.numberOfCheckersAtPositionIndex, spawnPositionIndex);

            GameObject checkerObject = (GameObject)Instantiate(whiteCheckerPrefab, checkerSpawnPosition, Quaternion.identity);
            checkerObjects[i] = checkerObject;

            actualBoard.SpawnChecker(1, spawnPositionIndex, actualBoard.numberOfCheckersAtPositionIndex[spawnPositionIndex] + 1, checkerSpawnPosition, i);

            actualBoard.numberOfCheckersAtPositionIndex[spawnPositionIndex]++;
        }



        for (int i = 0; i < 3; i++)
        {
            GameObject checkerHighlightObject = (GameObject)Instantiate(checkerHightlightPrefab, Vector3.down, Quaternion.identity);
            if (i == 0)
            {
                checkerHighlightObject.GetComponent<Collider>().enabled = false;
            }
            checkerHighlightObjects[i] = checkerHighlightObject;
        }

    }


    void ResetBoard()
    {
        actualBoard.numberOfCheckersAtPositionIndex = NeatFunctions.SetNumberOfCheckersAtPositionIndex();

        // Reset black checkers
        for (int i = 0; i < 15; i++)
        {
            int resetPositionIndex = NeatFunctions.spawnPositionIndexLookupTable[i];
            actualBoard.checkerClasses[i].positionOnBoardIndex = resetPositionIndex;
            actualBoard.checkerClasses[i].positionOnRowIndex = actualBoard.numberOfCheckersAtPositionIndex[resetPositionIndex] + 1;
            Vector3 checkerResetPosition = NeatFunctions.CalculateCheckerPosition(actualBoard.numberOfCheckersAtPositionIndex, resetPositionIndex);
            actualBoard.checkerClasses[i].positionInSpace = checkerResetPosition;
            actualBoard.numberOfCheckersAtPositionIndex[resetPositionIndex]++;
        }

        // Reset white checkers
        for (int i = 15; i < 30; i++)
        {
            int resetPositionIndex = 27 - NeatFunctions.spawnPositionIndexLookupTable[i - 15];
            actualBoard.checkerClasses[i].positionOnBoardIndex = resetPositionIndex;
            Vector3 checkerResetPosition = NeatFunctions.CalculateCheckerPosition(actualBoard.numberOfCheckersAtPositionIndex, resetPositionIndex);
            actualBoard.checkerClasses[i].positionInSpace = checkerResetPosition;
            actualBoard.numberOfCheckersAtPositionIndex[resetPositionIndex]++;
        }

        UpdateCheckerPositions(actualBoard);
    }
    






    // AI related stuff

    Board CopyBoard(Board boardToCopy)
    {
        Board newBoard = new Board();
        newBoard.movesLeft = new List<int>();
        int[] numberOfCheckersAtPositionIndexCopy = new int[28];
        newBoard.lastMoveMade = new List<int>();

        // Honestly fuck this haha, worth remembering:
            // If i create a 'Board newBoard = new Board();' then that board gets its own location in memory and its own variables.
            // But if i then set 'newBoard.numberOfCheckersAtPositionIndex = boardToCopy.numberOfCheckersAtPositionIndex'
            // then both boards (despite having different memory locations) point to the same 'numberOfCheckersAtPositionIndex'.
        for (int i = 0; i < boardToCopy.numberOfCheckersAtPositionIndex.Length; i++)
        {
            numberOfCheckersAtPositionIndexCopy[i] = boardToCopy.numberOfCheckersAtPositionIndex[i];
        }
        newBoard.numberOfCheckersAtPositionIndex = numberOfCheckersAtPositionIndexCopy;

        foreach (int move in boardToCopy.movesLeft)
        {
            newBoard.movesLeft.Add(move);
        }

        int checkerIndex = 0;
        foreach (Board.Checker checkerToCopy in boardToCopy.checkerClasses)
        {
            newBoard.checkerClasses[checkerIndex] = new Board.Checker(checkerToCopy.checkerColor, checkerToCopy.positionOnBoardIndex, checkerToCopy.positionOnRowIndex, checkerToCopy.positionInSpace, checkerToCopy.checkerIndex);
            checkerIndex++;
        }

        foreach (int value in boardToCopy.lastMoveMade)
        {
            newBoard.lastMoveMade.Add(value);
        }

        return newBoard;
    }


    // Fun fact, the maximum number of possible combinations of moves is    T = n^m * m!
        // where "T" is the number of combinations
        // "n" is the number of movable checkers
        // "m" is the number of moves left
    // So 2 movable checkers and 2 moves is 8 combinations
    // but 2 movable checkers and 4 moves is 384 combinations.
    // Some of them are duplicates but the code doesnt check for that,
    // so that is why it runs for so long when a pair is rolled :)
    void LoopThroughMoves(Board board)
    {
        if (board.movesLeft.Count > 0)
        {
            for (int checkerIndex = 0; checkerIndex < board.checkerClasses.Length; checkerIndex++)
            {
                //print("Current checker index: " + checkerIndex);
                if (board.CheckMoveability(board.checkerClasses[checkerIndex]))
                {
                    for (int moveIndex = 0; moveIndex < board.checkerClasses[checkerIndex].possibleMoves.Count; moveIndex++)
                    {
                        Board newBoard = CopyBoard(board);
                        newBoard.selectedCheckerIndex = checkerIndex;
                        newBoard.CheckMoveability(newBoard.checkerClasses[newBoard.selectedCheckerIndex]);

                        // print("-----Start of move-----");
                        // print("CheckerIndex: " + checkerIndex + " moveindex: " + moveIndex + " positon: " + newBoard.checkerClasses[newBoard.selectedCheckerIndex].positionOnBoardIndex + " move to make: " + newBoard.checkerClasses[newBoard.selectedCheckerIndex].possibleMoves[moveIndex] + " n of moves on board: " + newBoard.movesLeft.Count);
                        newBoard.MoveChecker(moveIndex);
                        newBoard.CheckMoveability(newBoard.checkerClasses[checkerIndex]);
                        // print("CheckerIndex: " + checkerIndex + " positon: " + newBoard.checkerClasses[newBoard.selectedCheckerIndex].positionOnBoardIndex);
                        // print("-----End of move-----");
                        if (newBoard.movesLeft.Count > 0)
                        {
                            if (newBoard.CheckIfAnyMovesPossible())
                            {
                                LoopThroughMoves(newBoard);
                            }
                            else
                            {
                                EvaluateTurn(newBoard);
                            }
                        }
                        else
                        {
                            EvaluateTurn(newBoard);
                        }
                    }
                }
            }
        }
    }


    void EvaluateTurn(Board board)
    {
        float totalScore = 0;
        // print("Evaluating");

        // Penalty for checkers that are left alone
        for (int positionIndex = 2; positionIndex < board.numberOfCheckersAtPositionIndex.Length - 2; positionIndex++)
        {
            if (board.numberOfCheckersAtPositionIndex[positionIndex] == 1)
            {
                Board.Checker soloCheckerFound = board.checkerClasses[board.FindCheckerIndexFromPositionIndex(positionIndex)];

                if (soloCheckerFound.checkerColor == NeatFunctions.whoseTurnIsIt)
                {
                    // print("Solo checker found at: " + positionIndex);
                    float checkerLerpValue = Mathf.Abs(soloCheckerFound.positionOnBoardIndex - NeatFunctions.beatIndices[NeatFunctions.whoseTurnIsIt]) / 25f;
                    totalScore -= soloCheckerValue[NeatFunctions.whoseTurnIsIt] * (1 + forwardSoloCheckerMultiplier[NeatFunctions.whoseTurnIsIt] * checkerLerpValue);
                }
            }
        }

        // Penalty for not beating opponent checker
        foreach (Board.Checker checker in board.checkerClasses)
        {
            if (checker.positionOnBoardIndex > 1 && checker.positionOnBoardIndex < 26)
            {
                if (checker.checkerColor == NeatFunctions.whoseTurnIsItNot)
                {
                    float checkerLerpValue = Mathf.Abs(checker.positionOnBoardIndex - NeatFunctions.beatIndices[NeatFunctions.whoseTurnIsItNot]) / 25f;
                    totalScore -= beatOpponentCheckerValue[NeatFunctions.whoseTurnIsIt] * (1 + checkerLerpValue);
                }
            }
        }


        // Forward / backward bias
        foreach (Board.Checker checker in board.checkerClasses)
        {
            if (checker.positionOnBoardIndex > 1 && checker.positionOnBoardIndex < 26)
            {
                if (checker.checkerColor == NeatFunctions.whoseTurnIsIt)
                {
                    float checkerLerpValue = (float)Mathf.Abs(checker.positionOnBoardIndex - NeatFunctions.beatIndices[checker.checkerColor]) / 25f;
                    checkerLerpValue = checkerLerpValue * 2f - 1f;
                    // print(checkerLerpValue);
                    totalScore += Mathf.Abs(frontRearBiasValue[NeatFunctions.whoseTurnIsIt]) * Mathf.Sign(frontRearBiasValue[NeatFunctions.whoseTurnIsIt]) * 1 / (-Mathf.Sign(frontRearBiasValue[NeatFunctions.whoseTurnIsIt]) * checkerLerpValue + 1.1f);
                    
                    // totalScore += Mathf.Abs(frontRearBiasValue[NeatFunctions.whoseTurnIsIt]) * 1 / (1 + Mathf.Exp(Mathf.Sign(frontRearBiasValue[NeatFunctions.whoseTurnIsIt] * -1f) * 4f * checkerLerpValue));
                }
            }
        }

        if (printBoardEvaluations)
        {
            print("-----Board evaluation-----");
            for (int i = 0; i < board.lastMoveMade.Count / 3; i++)
            {
                print("Checker " + board.lastMoveMade[i * 3] + " moved from " + board.lastMoveMade[i * 3 + 1] + " to " + board.lastMoveMade[i * 3 + 2]);
            }
            print("Total score: " + totalScore);
        }


        if (totalScore > bestScoreSoFar)
        {
            // print("New best board found");
            bestScoreSoFar = totalScore;
            bestBoardSoFar = CopyBoard(board);
        }
    }



    void UpdateCheckerPositions(Board board)
    {
        for (int i = 0; i < board.checkerClasses.Length; i++)
        {
            checkerObjects[i].transform.position = board.checkerClasses[i].positionInSpace;
        }
    }




    int CheckMouseOverAI()
    {
        int colorHit = -1;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.tag == "BlackAIButton" || hit.collider.tag == "WhiteAIButton")
            {
                Debug.DrawLine(ray.origin, hit.point, Color.blue);
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.collider.tag == "BlackAIButton")
                    {
                        colorHit = 0;
                    }
                    else if (hit.collider.tag == "WhiteAIButton")
                    {
                        colorHit = 1;
                    }
                }
            }
        }
        return colorHit;
    }



    public void StupidFix()
    {
        foreach (GameObject checkerHighLighter in checkerHighlightObjects)
        {
            checkerHighLighter.transform.Translate(0, 0.1f, 0);
        }
    }


}
