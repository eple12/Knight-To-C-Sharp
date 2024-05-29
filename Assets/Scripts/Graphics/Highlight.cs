using UnityEngine;

public class Highlight : MonoBehaviour
{
    Board board;

    GameObject moveHighlightPrefab;
    GameObject legalMoveHighlightPrefab;
    GameObject legalMoveHighlight;
    GameObject grabSquareHighlight;

    public void Initialize()
    {
        board = Graphic.board;

        moveHighlightPrefab = Graphic.moveHighlightPrefab;
        legalMoveHighlightPrefab = Graphic.legalMoveHighlightPrefab;
        legalMoveHighlight = Graphic.legalMoveHighlight;
        

        CreateAllLegalMoveHighlights();
        CreateAllMoveHighlights();
        
        grabSquareHighlight = Graphic.grabSquareHighlight;
    }

    void CreateAllLegalMoveHighlights()
    {
        // Legal move highlights
        for (int square = 0; square < 64; square++)
        {
            GameObject createdHighlight = Instantiate(legalMoveHighlightPrefab);
            createdHighlight.transform.position = Square.SquareIndexToWorld(square);
            createdHighlight.transform.parent = legalMoveHighlight.transform;
        }
    }

    void CreateAllMoveHighlights()
    {
        Graphic.startSquareMoveHighlight = Instantiate(moveHighlightPrefab);
        Graphic.targetSquareMoveHighlight = Instantiate(moveHighlightPrefab);
        Graphic.grabSquareHighlight = Instantiate(moveHighlightPrefab);
    }

    public void ShowLegalMovesFromSquareIndex(int square)
    {
        for (int i = 0; i < board.currentLegalMoves.Count; i++)
        {
            if (board.currentLegalMoves[i].startSquare == square)
            {
                legalMoveHighlight.transform.GetChild(board.currentLegalMoves[i].targetSquare).gameObject.SetActive(true);
            }
        }
        
        grabSquareHighlight.transform.position = Square.SquareIndexToWorld(square);
        grabSquareHighlight.SetActive(true);
    }

    public void RemoveLegalMoveHighlights()
    {
        for (int i = 0; i < 64; i++)
        {
            legalMoveHighlight.transform.GetChild(i).gameObject.SetActive(false);
        }

        grabSquareHighlight.SetActive(false);
    }
}
