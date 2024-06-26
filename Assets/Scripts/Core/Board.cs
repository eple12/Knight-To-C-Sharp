using System;
using UnityEngine;
using System.Collections.Generic;

public class Board
{
    public int[] position;
    public bool isWhiteTurn;

    public static readonly string initialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public string loadFen;

    public List<Move> currentLegalMoves;
    public ulong currentZobristKey;


    // Piece Square Recognization
    public PieceList[] pieceSquares;

    public Stack<uint> gameStateStack;

    public static readonly uint castlingMask = 0b_0000_0000_0000_0000_0000_0000_0000_1111;
    public static readonly uint capturedPieceMask = 0b_0000_0000_0000_0000_0000_0001_1111_0000;
    public static readonly uint enpassantFileMask = 0b_0000_0000_0000_0000_0001_1110_0000_0000;
    public static readonly uint fiftyCounterMask = 0b_1111_1111_1111_1111_1110_0000_0000_0000;

    /* 
        Bit 0: White Kingside
        Bit 1: White Queenside
        Bit 2: Black Kingside
        Bit 3: Black Queenside
    */
    public byte castlingData;
    

    public bool isWhiteKingsideCastle
    {
        get
        {
            return (castlingData & 0b00000001) != 0;
        }
        set
        {
            if (value)
            {
                castlingData |= 1;
            }
            else
            {
                castlingData &= byte.MaxValue ^ 1;
            }
        }
    }
    public bool isWhiteQueensideCastle
    {
        get
        {
            return (castlingData & 0b00000010) != 0;
        }
        set
        {
            if (value)
            {
                castlingData |= 1 << 1;
            }
            else
            {
                castlingData &= byte.MaxValue ^ (1 << 1);
            }
        }
    }
    public bool isBlackKingsideCastle
    {
        get
        {
            return (castlingData & 0b00000100) != 0;
        }
        set
        {
            if (value)
            {
                castlingData |= 1 << 2;
            }
            else
            {
                castlingData &= byte.MaxValue ^ (1 << 2);
            }
        }
    }
    public bool isBlackQueensideCastle
    {
        get
        {
            return (castlingData & 0b00001000) != 0;
        }
        set
        {
            if (value)
            {
                castlingData |= 1 << 3;
            }
            else
            {
                castlingData &= byte.MaxValue ^ (1 << 3);
            }
        }
    }

    // En passant file; a ~ h (0 ~ 7), 8 => No en passant available.
    public int enpassantFile;

    // If 100, draw by 50-move rule. (Since it counts half-move, after 1.Nf3 Nf6 it's 2)
    public int fiftyRuleHalfClock;

    // For Threefold detection
    public Dictionary<ulong, int> positionHistory = new Dictionary<ulong, int>();

    public Board()
    {
        position = new int[64];
        isWhiteTurn = true;

        currentLegalMoves = new List<Move>();
        currentZobristKey = 0;

        // EQUAL POSITION
        // loadFen = "r2q1rk1/pppb1ppp/1bn5/1B1pP3/3Pn3/5N1P/PP3PP1/RNBQ1RK1 w - - 1 11";

        loadFen = initialFen;

        // Puzzle
        // loadFen = "2k5/8/8/5p2/8/4N3/3K4/8 w - - 0 1";

        pieceSquares = new PieceList[12];

        gameStateStack = new Stack<uint>();

        castlingData = 0;
        enpassantFile = 8;
        fiftyRuleHalfClock = 0;
    }

    public void AfterLoadingPosition()
    {
        currentLegalMoves = MoveGen.GenerateMoves(this);
    }

    public void Reset()
    {
        position = new int[64];
        pieceSquares = new PieceList[12];
        gameStateStack = new Stack<uint>();

        isWhiteTurn = true;
        currentLegalMoves = new List<Move>();
        currentZobristKey = 0;
        castlingData = 0;
        enpassantFile = 8;
        fiftyRuleHalfClock = 0;
        positionHistory.Clear();
    }

    public void MakeMove(Move move)
    {
        int startSquare = move.startSquare;
        int targetSquare = move.targetSquare;

        int movingPiece = position[startSquare];
        int capturedPiece = position[targetSquare];

        int movingPieceBitboardIndex = Piece.GetBitboardIndex(movingPiece);

        gameStateStack.Push((uint) (castlingData | capturedPiece << 4 | enpassantFile << 9 | fiftyRuleHalfClock << 13));

        fiftyRuleHalfClock++;

        // ZOBRIST UPDATE: REMOVE PREVIOUS ENP.
        currentZobristKey ^= Zobrist.enpassantArray[enpassantFile];

        // ZOBRIST UPDATE: CASTLING RIGHTS
        currentZobristKey ^= Zobrist.castlingArray[castlingData];

        // Resets fifty-move clock if a pawn moves
        if (Piece.GetType(movingPiece) == Piece.Pawn)
        {
            fiftyRuleHalfClock = 0;
        }

        // If the move is a capturing move;
        if (capturedPiece != Piece.None)
        {
            fiftyRuleHalfClock = 0;

            // ZOBRIST UPDATE
            currentZobristKey ^= Zobrist.pieceArray[Piece.GetBitboardIndex(capturedPiece), targetSquare];

            // Rook Captured -> Disable Castling;
            if (capturedPiece == (Piece.White | Piece.Rook))
            {
                if (isWhiteKingsideCastle && targetSquare == 7)
                {
                    isWhiteKingsideCastle = false;
                }
                if (isWhiteQueensideCastle && targetSquare == 0)
                {
                    isWhiteQueensideCastle = false;
                }
            }
            else if (capturedPiece == (Piece.Black | Piece.Rook))
            {
                if (isBlackKingsideCastle && targetSquare == 63)
                {
                    isBlackKingsideCastle = false;
                }
                if (isBlackQueensideCastle && targetSquare == 56)
                {
                    isBlackQueensideCastle = false;
                }
            }
        
            // PIECE SQUARE UPDATE
            pieceSquares[Piece.GetBitboardIndex(capturedPiece)].RemovePieceAtSquare(targetSquare);
        }
        else // Checks if the move is enp.
        {
            if (move.flag == MoveFlag.EnpassantCapture) // En passant
            {
                int capturedPawnSquare = Square.EnpassantFileToPawnSquare(enpassantFile, isWhiteTurn);

                position[capturedPawnSquare] = Piece.None;

                // ZOBRIST UPDATE
                currentZobristKey ^= Zobrist.pieceArray[isWhiteTurn ? BitboardIndex.BlackPawn : BitboardIndex.WhitePawn, capturedPawnSquare];

                // PIECE SQUARE UPDATE
                pieceSquares[isWhiteTurn ? BitboardIndex.BlackPawn : BitboardIndex.WhitePawn].RemovePieceAtSquare(capturedPawnSquare);
            }
        }

        enpassantFile = 8;

        if (move.flag == MoveFlag.PawnTwoForward && Square.IsValidEnpassantFile(targetSquare % 8, this)) // Enp. Square Calculation;
        {
            enpassantFile = targetSquare % 8;
        }
        
        // CASTLING
        if (move.flag == MoveFlag.Castling)
        {
            if (Piece.IsWhitePiece(movingPiece))
            {
                if (targetSquare == startSquare + 2)
                {
                    position[targetSquare + 1] = Piece.None;
                    position[targetSquare - 1] = Piece.White | Piece.Rook;

                    // ZOBRIST UPDATE
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.WhiteRook, targetSquare + 1];
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.WhiteRook, targetSquare - 1];

                    // PIECE SQUARE UPDATE
                    pieceSquares[BitboardIndex.WhiteRook].RemovePieceAtSquare(targetSquare + 1);
                    pieceSquares[BitboardIndex.WhiteRook].AddPieceAtSquare(targetSquare - 1);
                }
                else if (targetSquare == startSquare - 2)
                {
                    position[targetSquare - 2] = Piece.None;
                    position[targetSquare + 1] = Piece.White | Piece.Rook;

                    // ZOBRIST UPDATE
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.WhiteRook, targetSquare - 2];
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.WhiteRook, targetSquare + 1];

                    // PIECE SQUARE UPDATE
                    pieceSquares[BitboardIndex.WhiteRook].RemovePieceAtSquare(targetSquare - 2);
                    pieceSquares[BitboardIndex.WhiteRook].AddPieceAtSquare(targetSquare + 1);
                }
            }
            else
            {
                if (targetSquare == startSquare + 2)
                {
                    position[targetSquare + 1] = Piece.None;
                    position[targetSquare - 1] = Piece.Black | Piece.Rook;

                    // ZOBRIST UPDATE
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.BlackRook, targetSquare + 1];
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.BlackRook, targetSquare - 1];

                    // PIECE SQUARE UPDATE
                    pieceSquares[BitboardIndex.BlackRook].RemovePieceAtSquare(targetSquare + 1);
                    pieceSquares[BitboardIndex.BlackRook].AddPieceAtSquare(targetSquare - 1);
                }
                else if (targetSquare == startSquare - 2)
                {
                    position[targetSquare - 2] = Piece.None;
                    position[targetSquare + 1] = Piece.Black | Piece.Rook;

                    // ZOBRIST UPDATE
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.BlackRook, targetSquare - 2];
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.BlackRook, targetSquare + 1];

                    // PIECE SQUARE UPDATE
                    pieceSquares[BitboardIndex.BlackRook].RemovePieceAtSquare(targetSquare - 2);
                    pieceSquares[BitboardIndex.BlackRook].AddPieceAtSquare(targetSquare + 1);
                }
            }
        }

        // Castling rights
        if (movingPiece == (Piece.White | Piece.Rook))
        {
            if (startSquare == 0)
            {
                isWhiteQueensideCastle = false;
            }
            else if (startSquare == 7)
            {
                isWhiteKingsideCastle = false;
            }
        }
        if (movingPiece == (Piece.Black | Piece.Rook))
        {
            if (startSquare == 56)
            {
                isBlackQueensideCastle = false;
            }
            else if (startSquare == 63)
            {
                isBlackKingsideCastle = false;
            }
        }

        if (movingPiece == (Piece.White | Piece.King))
        {
            isWhiteKingsideCastle = false;
            isWhiteQueensideCastle = false;
        }
        if (movingPiece == (Piece.Black | Piece.King))
        {
            isBlackKingsideCastle = false;
            isBlackQueensideCastle = false;
        }

        // Move the piece
        position[targetSquare] = movingPiece;
        position[startSquare] = Piece.None;

        // ZOBRIST UPDATE
        currentZobristKey ^= Zobrist.pieceArray[movingPieceBitboardIndex, startSquare];
        currentZobristKey ^= Zobrist.pieceArray[movingPieceBitboardIndex, targetSquare];

        // PIECE SQUARE UPDATE
        pieceSquares[movingPieceBitboardIndex].RemovePieceAtSquare(startSquare);
        pieceSquares[movingPieceBitboardIndex].AddPieceAtSquare(targetSquare);
        
        // Promotion
        if (MoveFlag.IsPromotion(move.flag))
        {
            int promotionPiece = MoveFlag.GetPromotionPiece(move.flag, isWhiteTurn);
            int promotionBitboardIndex = Piece.GetBitboardIndex(promotionPiece);

            position[targetSquare] = promotionPiece;

            // ZOBRIST UPDATE: RE-CALCULATE PIECE KEY
            currentZobristKey ^= Zobrist.pieceArray[movingPieceBitboardIndex, targetSquare];
            currentZobristKey ^= Zobrist.pieceArray[promotionBitboardIndex, targetSquare];

            // PIECE SQUARE UPDATE
            pieceSquares[promotionBitboardIndex].AddPieceAtSquare(targetSquare);
            pieceSquares[movingPieceBitboardIndex].RemovePieceAtSquare(targetSquare);
        }


        // ZOBRIST UPDATE: ENP SQUARE
        currentZobristKey ^= Zobrist.enpassantArray[enpassantFile];

        // ZOBRIST UPDATE: CASTLING RIGHTS
        currentZobristKey ^= Zobrist.castlingArray[castlingData];

        // ZOBRIST TURN
        currentZobristKey ^= Zobrist.sideToMove;

        isWhiteTurn = !isWhiteTurn;
        
        StorePosition();
    }

    public void UnmakeMove(Move move)
    {
        positionHistory[currentZobristKey]--;
        
        isWhiteTurn = !isWhiteTurn;

        // ZOBRIST TURN
        currentZobristKey ^= Zobrist.sideToMove;

        // ZOBRIST REMOVE CASTLING
        currentZobristKey ^= Zobrist.castlingArray[castlingData];

        // ZOBRIST REMOVE ENP.
        currentZobristKey ^= Zobrist.enpassantArray[enpassantFile];

        int startSquare = move.startSquare;
        int targetSquare = move.targetSquare;

        int movingPiece = position[targetSquare];
        int movingPieceBitboardIndex = Piece.GetBitboardIndex(movingPiece);

        position[startSquare] = movingPiece;
        position[targetSquare] = Piece.None;

        // ZOBRIST PIECE
        currentZobristKey ^= Zobrist.pieceArray[movingPieceBitboardIndex, targetSquare];
        currentZobristKey ^= Zobrist.pieceArray[movingPieceBitboardIndex, startSquare];
        
        // PIECE SQUARE UPDATE
        pieceSquares[movingPieceBitboardIndex].RemovePieceAtSquare(targetSquare);
        pieceSquares[movingPieceBitboardIndex].AddPieceAtSquare(startSquare);

        uint previousGameState = gameStateStack.Pop();

        // Restore Enp. Square
        enpassantFile = (int) (previousGameState & enpassantFileMask) >> 9;

        // ZOBRIST ENP.
        currentZobristKey ^= Zobrist.enpassantArray[enpassantFile];

        // Restore Fifty-Clock
        fiftyRuleHalfClock = (int) (previousGameState & fiftyCounterMask) >> 13;

        // Restore Castling Rights
        castlingData = (byte) (previousGameState & castlingMask);
        
        // ZOBRIST CASTLING
        currentZobristKey ^= Zobrist.castlingArray[castlingData];

        int capturedPiece = (int) (previousGameState & capturedPieceMask) >> 4;

        // If capture
        if (capturedPiece != Piece.None)
        {
            position[targetSquare] = capturedPiece;

            int capturedPieceBitboardIndex = Piece.GetBitboardIndex(capturedPiece);

            // PIECE SQUARE UPDATE
            pieceSquares[capturedPieceBitboardIndex].AddPieceAtSquare(targetSquare);

            // ZOBRIST PIECE
            currentZobristKey ^= Zobrist.pieceArray[capturedPieceBitboardIndex, targetSquare];
        }

        // If En-passant
        if (move.flag == MoveFlag.EnpassantCapture)
        {
            int enpassantPawnSquare = Square.EnpassantFileToPawnSquare(enpassantFile, isWhiteTurn);
            position[enpassantPawnSquare] = (isWhiteTurn ? Piece.Black : Piece.White) | Piece.Pawn;

            int enemyPawnBitboardIndex = isWhiteTurn ? BitboardIndex.BlackPawn : BitboardIndex.WhitePawn;

            // PIECE SQUARE UPDATE
            pieceSquares[enemyPawnBitboardIndex].AddPieceAtSquare(enpassantPawnSquare);

            // ZOBRIST ENP. CAPTURE
            currentZobristKey ^= Zobrist.pieceArray[enemyPawnBitboardIndex, enpassantPawnSquare];
        }

        // If Castling
        if (move.flag == MoveFlag.Castling)
        {
            if (isWhiteTurn)
            {
                if (targetSquare == startSquare + 2)
                {
                    position[targetSquare - 1] = Piece.None;
                    position[targetSquare + 1] = Piece.White | Piece.Rook;

                    // ZOBRIST
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.WhiteRook, targetSquare - 1];
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.WhiteRook, targetSquare + 1];

                    // PIECE SQUARE UPDATE
                    pieceSquares[BitboardIndex.WhiteRook].RemovePieceAtSquare(targetSquare - 1);
                    pieceSquares[BitboardIndex.WhiteRook].AddPieceAtSquare(targetSquare + 1);
                }
                else
                {
                    position[targetSquare + 1] = Piece.None;
                    position[targetSquare - 2] = Piece.White | Piece.Rook;

                    // ZOBRIST
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.WhiteRook, targetSquare + 1];
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.WhiteRook, targetSquare - 2];

                    // PIECE SQUARE UPDATE
                    pieceSquares[BitboardIndex.WhiteRook].RemovePieceAtSquare(targetSquare + 1);
                    pieceSquares[BitboardIndex.WhiteRook].AddPieceAtSquare(targetSquare - 2);
                }
            }
            else
            {
                if (targetSquare == startSquare + 2)
                {
                    position[targetSquare - 1] = Piece.None;
                    position[targetSquare + 1] = Piece.Black | Piece.Rook;

                    // ZOBRIST
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.BlackRook, targetSquare - 1];
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.BlackRook, targetSquare + 1];

                    // PIECE SQUARE UPDATE
                    pieceSquares[BitboardIndex.BlackRook].RemovePieceAtSquare(targetSquare - 1);
                    pieceSquares[BitboardIndex.BlackRook].AddPieceAtSquare(targetSquare + 1);
                }
                else
                {
                    position[targetSquare + 1] = Piece.None;
                    position[targetSquare - 2] = Piece.Black | Piece.Rook;

                    // ZOBRIST
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.BlackRook, targetSquare + 1];
                    currentZobristKey ^= Zobrist.pieceArray[BitboardIndex.BlackRook, targetSquare - 2];

                    // PIECE SQUARE UPDATE
                    pieceSquares[BitboardIndex.BlackRook].RemovePieceAtSquare(targetSquare + 1);
                    pieceSquares[BitboardIndex.BlackRook].AddPieceAtSquare(targetSquare - 2);
                }
            }
        }

        if (MoveFlag.IsPromotion(move.flag)) // If Promotion
        {
            position[startSquare] = (isWhiteTurn ? Piece.White : Piece.Black) | Piece.Pawn;

            // ZOBRIST
            currentZobristKey ^= Zobrist.pieceArray[movingPieceBitboardIndex, startSquare];
            currentZobristKey ^= Zobrist.pieceArray[isWhiteTurn ? BitboardIndex.WhitePawn : BitboardIndex.BlackPawn, startSquare];

            // PIECE SQUARE UPDATE
            pieceSquares[movingPieceBitboardIndex].RemovePieceAtSquare(startSquare);
            pieceSquares[isWhiteTurn ? BitboardIndex.WhitePawn : BitboardIndex.BlackPawn].AddPieceAtSquare(startSquare);
        }
    }
    
    public ulong Perft(int depth)
    {
        if (depth == 0)
        {
            return 1;
        }

        ulong nodes = 0;
        List<Move> legalMoves = MoveGen.GenerateMoves(this);
        
        if (depth == 1)
        {
            return (ulong) legalMoves.Count;
        }

        foreach (Move move in legalMoves)
        {
            MakeMove(move);

            nodes += Perft(depth - 1);

            UnmakeMove(move);
        }

        return nodes;
    }

    void StorePosition()
    {
        if (positionHistory.ContainsKey(currentZobristKey))
        {
            positionHistory[currentZobristKey]++;
        }
        else
        {
            positionHistory.Add(currentZobristKey, 1);
        }
    }



}