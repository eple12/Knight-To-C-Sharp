using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour
{
    Board board;
    // List<PieceObject> pieceObjects;

    public Highlight highlight;
    public MoveMaker moveMaker;


    public void Initialize()
    {
        board = Graphic.board;
        // pieceObjects = Graphic.pieceObjects;
    }

    public void HandleMouseEvents()
    {
        // Not in match, return early
        if (!Graphic.isInMatch)
        {
            return;
        }

        // If in search, return early
        if (EnginePlayer.isSearching)
        {
            return;
        }

        Vector2 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // If the mouse is outside of the board && the player is not grabbing a piece, return early;
        bool isOutOfBounds = (currentMousePosition.x > 4 || currentMousePosition.x < -4 || currentMousePosition.y > 4 || currentMousePosition.y < -4) ? true : false;

        // Convert currentMousePosition to Square Index; (0 ~ 63)
        int currentMouseSquareIndex = Square.FileRankToSquareIndex((int) (currentMousePosition.x + 4), (int) (currentMousePosition.y + 4));
        
        // If a square is clicked:
        if (Input.GetMouseButtonDown(0) && !isOutOfBounds)
        {
            // No piece in the square or clicked an opponent piece
            if (board.position[currentMouseSquareIndex] == Piece.None)
            {   }
            else if (!Graphic.isPromoting) // Grab a piece
            {
                PieceObject pieceObject = moveMaker.FindPieceObject(currentMouseSquareIndex);

                if (Piece.IsColor(board.position[pieceObject.SquareIndex], board.isWhiteTurn ? Piece.White : Piece.Black))
                {
                    // Find the PieceObject
                    Graphic.grabbedPieceObject = pieceObject;
                    highlight.ShowLegalMovesFromSquareIndex(currentMouseSquareIndex);
                }
            }

            // Promotion Click;
            if (Graphic.isPromoting && !isOutOfBounds)
            {
                if (board.isWhiteTurn) // currentTurn => isWhitePawn
                {
                    if (currentMouseSquareIndex == Graphic.currentPromotionSquare)
                    {
                        Graphic.PromotePawn(Graphic.currentPromotionSquare, Piece.White | Piece.Queen);
                    }
                    else if (currentMouseSquareIndex == Graphic.currentPromotionSquare - 8)
                    {
                        Graphic.PromotePawn(Graphic.currentPromotionSquare, Piece.White | Piece.Knight);
                    }
                    else if (currentMouseSquareIndex == Graphic.currentPromotionSquare - 16)
                    {
                        Graphic.PromotePawn(Graphic.currentPromotionSquare, Piece.White | Piece.Rook);
                    }
                    else if (currentMouseSquareIndex == Graphic.currentPromotionSquare - 24)
                    {
                        Graphic.PromotePawn(Graphic.currentPromotionSquare, Piece.White | Piece.Bishop);
                    }
                }
                else
                {
                    if (currentMouseSquareIndex == Graphic.currentPromotionSquare)
                    {
                        Graphic.PromotePawn(Graphic.currentPromotionSquare, Piece.Black | Piece.Queen);
                    }
                    else if (currentMouseSquareIndex == Graphic.currentPromotionSquare + 8)
                    {
                        Graphic.PromotePawn(Graphic.currentPromotionSquare, Piece.Black | Piece.Knight);
                    }
                    else if (currentMouseSquareIndex == Graphic.currentPromotionSquare + 16)
                    {
                        Graphic.PromotePawn(Graphic.currentPromotionSquare, Piece.Black | Piece.Rook);
                    }
                    else if (currentMouseSquareIndex == Graphic.currentPromotionSquare + 24)
                    {
                        Graphic.PromotePawn(Graphic.currentPromotionSquare, Piece.Black | Piece.Bishop);
                    }
                }
                
            }
        }
        if (Input.GetMouseButtonUp(0) && Graphic.grabbedPieceObject != null)
        {
            bool isHoveringLegalMoveSquare = false;
            
            Move move = new Move(64, 64);
            foreach (Move item in board.currentLegalMoves)
            {
                if (item.startSquare == Graphic.grabbedPieceObject.SquareIndex && item.targetSquare == currentMouseSquareIndex)
                {
                    move = item;
                    isHoveringLegalMoveSquare = true;
                    break;
                }
            }

            if (!isOutOfBounds && isHoveringLegalMoveSquare)
            {
                moveMaker.MakeGraphicalMove(move);
            }
            else
            {
                Graphic.grabbedPieceObject.transform.position = Square.SquareIndexToWorld(Graphic.grabbedPieceObject.SquareIndex);
            }
            Graphic.grabbedPieceObject = null;
            highlight.RemoveLegalMoveHighlights();
        }

        if (Graphic.grabbedPieceObject != null)
        {
            Graphic.grabbedPieceObject.transform.position = (Vector3) currentMousePosition + new Vector3(0, 0, -1);
        }
        
    }
}
