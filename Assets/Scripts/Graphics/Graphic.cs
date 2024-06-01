using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Graphic
{
    // GameObjects
    public static GameObject[] piecePrefabs;
    public static Sprite[] pieceSprites;
    public static GameObject legalMoveHighlight;
    public static GameObject legalMoveHighlightPrefab;

    public static GameObject moveHighlightPrefab;

    // Promotion GameObjects
    public static GameObject promotionFade;
    public static GameObject whitePromotionUI;
    public static GameObject blackPromotionUI;

    // Variables
    public static bool isPromoting;
    public static bool isInMatch;
    public static Move promotionMove;
    public static int currentPromotionSquare;
    public static List<PieceObject> pieceObjects;
    public static PieceObject grabbedPieceObject;

    public static GameObject startSquareMoveHighlight;
    public static GameObject targetSquareMoveHighlight;
    public static GameObject grabSquareHighlight;

    public static Board board;

    public static void Initialize()
    {
        board = Main.mainBoard;

        isPromoting = false;
        promotionMove = Move.NullMove;
        isInMatch = true;
        currentPromotionSquare = 64; // Invalid Index;
        pieceObjects = new List<PieceObject>();
        grabbedPieceObject = null;
    }

    public static void AfterLoadingPosition()
    {
        AfterMakingMove();

        board.AfterLoadingPosition();
    }

    // Checks for GameOver MateState
    public static void HandleCurrentMateState()
    {
        MateChecker.MateState mateState = MateChecker.GetPositionState(board, board.currentLegalMoves);

        if (mateState != MateChecker.MateState.None)
        {
            isInMatch = false;
            MateChecker.PrintMateState(mateState);
        }
    }

    public static void Update()
    {

    }

    public static void AfterMakingMove()
    {
        board.currentLegalMoves = MoveGen.GenerateMoves(board);

        HandleCurrentMateState();
    }


    public static void ShowPromotionUI(in int square, in bool isWhitePawn)
    {
        promotionFade.SetActive(true);

        if (isWhitePawn)
        {
            whitePromotionUI.transform.position = Square.SquareIndexToWorld(square);
            whitePromotionUI.SetActive(true);
        }
        else
        {
            blackPromotionUI.transform.position = Square.SquareIndexToWorld(square);
            blackPromotionUI.SetActive(true);
        }
    }
    
    public static void PromotePawn(in int square, in int piece)
    {
        // board.position[square] = piece;
        
        foreach (PieceObject pieceObject in pieceObjects)
        {
            if (pieceObject.SquareIndex == square)
            {
                pieceObject.GetComponent<SpriteRenderer>().sprite = pieceSprites[Piece.GetBitboardIndex(piece)];
                pieceObject.PieceValue = piece;
                
                // board.isWhiteTurn = !board.isWhiteTurn;
                // board.currentZobristKey = Zobrist.GetZobristKey(board); // OPTIMIZE

                // board.pieceSquares[Piece.GetBitboardIndex(piece)].AddPieceAtSquare(square);

                promotionMove.flag = MoveFlag.GetPromotionFlag(piece);
                board.MakeMove(promotionMove);
                promotionMove = Move.NullMove;

                AfterMakingMove();
                
                isPromoting = false;
                currentPromotionSquare = 64; // Invalid Index;
                promotionFade.SetActive(false);
                whitePromotionUI.SetActive(false);
                blackPromotionUI.SetActive(false);
                return;
            }
        }
    }

    
}
