using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMaker : MonoBehaviour
{
    Board board;

    GameObject startSquareMoveHighlight;
    GameObject targetSquareMoveHighlight;

    List<PieceObject> pieceObjects;

    public void Initialize()
    {
        board = Graphic.board;
        startSquareMoveHighlight = Graphic.startSquareMoveHighlight;
        targetSquareMoveHighlight = Graphic.targetSquareMoveHighlight;
        pieceObjects = Graphic.pieceObjects;
    }

    public void MakeGraphicalMove(Move move, bool engineMove = false)
    {
        int startSquare = move.startSquare;
        int targetSquare = move.targetSquare;

        // No grabbed piece
        if (Graphic.grabbedPieceObject == null)
        {
            return;
        }
        // Capturing itself
        if (Graphic.grabbedPieceObject.SquareIndex == targetSquare)
        {
            Graphic.grabbedPieceObject.transform.position = Square.SquareIndexToWorld(Graphic.grabbedPieceObject.SquareIndex);
            return;
        }

        // Move Highlight;
        startSquareMoveHighlight.SetActive(true);
        targetSquareMoveHighlight.SetActive(true);

        startSquareMoveHighlight.transform.position = Square.SquareIndexToWorld(move.startSquare);
        targetSquareMoveHighlight.transform.position = Square.SquareIndexToWorld(move.targetSquare);

        int capturedPiece = Piece.None;

        // Captured a piece
        if (board.position[targetSquare] != Piece.None)
        {
            capturedPiece = CapturePiece(targetSquare);
        }

        // Enpassant Capture;
        if (move.flag == MoveFlag.EnpassantCapture)
        {
            CapturePiece(Square.EnpassantFileToPawnSquare(board.enpassantFile, board.isWhiteTurn));
        }
        
        MovePiece(Graphic.grabbedPieceObject, targetSquare);

        // Castling Rook Move;
        if (move.flag == MoveFlag.Castling)
        {
            if (board.isWhiteTurn)
            {
                if (targetSquare == Square.SquareNameToIndex("g1") && board.isWhiteKingsideCastle)
                {
                    MovePiece(FindPieceObject(targetSquare + 1), targetSquare - 1);
                }
                else if (targetSquare == Square.SquareNameToIndex("c1") && board.isWhiteQueensideCastle)
                {
                    MovePiece(FindPieceObject(targetSquare - 2), targetSquare + 1);
                }
            }
            else
            {
                if (targetSquare == Square.SquareNameToIndex("g8") && board.isBlackKingsideCastle)
                {
                    MovePiece(FindPieceObject(targetSquare + 1), targetSquare - 1);
                }
                else if (targetSquare == Square.SquareNameToIndex("c8") && board.isBlackQueensideCastle)
                {
                    MovePiece(FindPieceObject(targetSquare - 2), targetSquare + 1);
                }
            }
        }

        // Promotion
        if (MoveFlag.IsPromotion(move.flag))
        {
            if (engineMove)
            {
                int promotionPiece = MoveFlag.GetPromotionPiece(move.flag, board.isWhiteTurn);

                Graphic.grabbedPieceObject.GetComponent<SpriteRenderer>().sprite = Graphic.pieceSprites[Piece.GetBitboardIndex(promotionPiece)];
                Graphic.grabbedPieceObject.PieceValue = promotionPiece;
            }
            else
            {
                Graphic.ShowPromotionUI(targetSquare, board.isWhiteTurn);

                // board.position[startSquare] = Piece.None;

                // board.pieceSquares[board.isWhiteTurn ? BitboardIndex.WhitePawn : BitboardIndex.BlackPawn].RemovePieceAtSquare(startSquare);
                
                // if (capturedPiece != Piece.None) // Remove captured piece from bitboards
                // {
                //     board.pieceSquares[Piece.GetBitboardIndex(capturedPiece)].RemovePieceAtSquare(targetSquare);
                // }
                Graphic.promotionMove = move;
                
                Graphic.grabbedPieceObject = null;
                Graphic.isPromoting = true;
                Graphic.currentPromotionSquare = targetSquare;

                return;
            }
            
        }

        board.MakeMove(move);

        Graphic.grabbedPieceObject = null;
        Graphic.AfterMakingMove();
    }

    void MovePiece(PieceObject pieceObject, int targetSquare)
    {
        pieceObject.SquareIndex = targetSquare;
        pieceObject.transform.position = Square.SquareIndexToWorld(targetSquare);
    }

    int CapturePiece(int targetSquare)
    {
        PieceObject capturedPieceObject = FindPieceObject(targetSquare);
        Destroy(capturedPieceObject.gameObject);
        pieceObjects.Remove(capturedPieceObject);

        return capturedPieceObject.PieceValue;
    }

    public PieceObject FindPieceObject(int square)
    {
        foreach (PieceObject pieceObject in pieceObjects)
        {
            if (pieceObject.SquareIndex == square)
            {
                return pieceObject;
            }
        }

        // FailSafe
        return new PieceObject();
    }

}
