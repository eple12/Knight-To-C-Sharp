using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoveGen
{
    static readonly int[] directionOffsets = {1, 8, -1, -8, 9, 7, -9, -7};

    // VARIABLES USED IN MOVE GENERATION
    static List<Move> moves = new List<Move>();

    static bool genQuietMoves;

    static int[] position;
    static Board board;
    static bool isWhiteTurn;
    static int friendlyColor;
    static int enemyColor;
    static int friendlyKingSquare;

    static ulong enemyAttackMap;
    static ulong enemyAttackMapNoPawns;
    static ulong enemySlidingAttackMap;
    static ulong enemyKnightAttackMap;
    static ulong enemyPawnAttackMap;

    static bool pinsExistInPosition;
    static ulong pinRayBitmask;

    static ulong checkRayBitmask;
    static bool inCheck;
    static bool inDoubleCheck;

    static bool kingsideCastling;
    static bool queensideCastling;

    // Bitboard Index
    static int enemyBishopIndex;
    static int enemyRookIndex;
    static int enemyQueenIndex;
    

    public static List<Move> GenerateMoves(Board _board, bool genOnlyCaptures = false)
    {
        board = _board;
        genQuietMoves = !genOnlyCaptures;

        Initialize();

        CalculateAttackData();
        
        GenerateKingMoves();

        if (inDoubleCheck)
        {
            return moves;
        }

        GenerateSlidingMoves();
        GenerateKnightMoves();
        
        GeneratePawnMoves();

        return moves;
    }

    static void Initialize()
    {
        moves = new List<Move>();

        position = board.position;
        isWhiteTurn = board.isWhiteTurn;

        friendlyColor = isWhiteTurn ? Piece.White : Piece.Black;
        enemyColor = isWhiteTurn ? Piece.Black : Piece.White;

        enemyBishopIndex = isWhiteTurn ? BitboardIndex.BlackBishop : BitboardIndex.WhiteBishop;
        enemyRookIndex = isWhiteTurn ? BitboardIndex.BlackRook : BitboardIndex.WhiteRook;
        enemyQueenIndex = isWhiteTurn ? BitboardIndex.BlackQueen : BitboardIndex.WhiteQueen;

        enemyAttackMap = 0;
        enemyAttackMapNoPawns = 0;
        enemySlidingAttackMap = 0;
        enemyKnightAttackMap = 0;
        enemyPawnAttackMap = 0;

        friendlyKingSquare = board.pieceSquares[isWhiteTurn ? BitboardIndex.WhiteKing : BitboardIndex.BlackKing].squares[0];

        pinsExistInPosition = false;
        pinRayBitmask = 0;

        checkRayBitmask = 0;

        inCheck = false;
        inDoubleCheck = false;

        kingsideCastling = isWhiteTurn ? board.isWhiteKingsideCastle : board.isBlackKingsideCastle;
        queensideCastling = isWhiteTurn ? board.isWhiteQueensideCastle : board.isBlackQueensideCastle;
    }

    // Note: This will only return correct value only after GenerateMoves() call.
    public static bool InCheck()
    {
        return inCheck;
    }

    public static ulong PawnAttackMap()
    {
        return enemyPawnAttackMap;
    }

    public static ulong AttackMapNoPawn()
    {
        return enemyAttackMapNoPawns;
    }

    static void GenerateSlidingMoves()
    {
        PieceList rooks = board.pieceSquares[isWhiteTurn ? BitboardIndex.WhiteRook : BitboardIndex.BlackRook];
        for (int i = 0; i < rooks.count; i++) {
            GenerateSingleSlider(rooks.squares[i], 0, 4);
        }

        PieceList bishops = board.pieceSquares[isWhiteTurn ? BitboardIndex.WhiteBishop : BitboardIndex.BlackBishop];
        for (int i = 0; i < bishops.count; i++) {
            GenerateSingleSlider(bishops.squares[i], 4, 8);
        }

        PieceList queens = board.pieceSquares[isWhiteTurn ? BitboardIndex.WhiteQueen : BitboardIndex.BlackQueen];
        for (int i = 0; i < queens.count; i++) {
            GenerateSingleSlider(queens.squares[i], 0, 8);
        }
    }

    static void GenerateSingleSlider(in int startSquare, in int startDirIndex, in int endDirIndex)
    {
        bool isPinned = IsPinned(startSquare);

        // If this piece is pinned, and if the king is in check, this piece cannot move
        if (inCheck && isPinned)
        {
            return;
        }
        
        for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++)
        {
            int currentOffset = directionOffsets[dirIndex];

            // If this piece is pinned, it can only move along the pin ray
            if (isPinned && !IsMovingAlongRay(currentOffset, friendlyKingSquare, startSquare))
            {
                continue;
            }

            for (int n = 0; n < PreComputedData.numSquaresToEdge[startSquare, dirIndex]; n++)
            {
                int targetSquare = startSquare + currentOffset * (n + 1);

                // Blocked by a friendly piece; Move on to the next direction.
                if (Piece.IsColor(position[targetSquare], friendlyColor))
                {
                    break;
                }

                bool movePreventsCheck = SquareIsInCheckRay(targetSquare);

                if (movePreventsCheck || !inCheck)
                {
                    // Capturing enemy piece
                    if (Piece.IsColor(position[targetSquare], enemyColor))
                    {
                        moves.Add(new Move(startSquare, targetSquare));
                        break;
                    }
                    
                    if (genQuietMoves)
                    {
                        moves.Add(new Move(startSquare, targetSquare));
                    }
                }
                // Check, but this piece cannot help because it is blocked by an enemy piece
                else if (Piece.IsColor(position[targetSquare], enemyColor))
                {
                    break;
                }

                if (movePreventsCheck)
                {
                    break;
                }
            }
        }
    }
    
    static void GenerateKingMoves()
    {
        for (int index = 0; index < PreComputedData.kingSquares[friendlyKingSquare].Count; index++)
        {
            int targetSquare = PreComputedData.kingSquares[friendlyKingSquare][index];

            if (Piece.IsColor(position[targetSquare], friendlyColor))
            {
                continue;
            }

            if (!Bitboard.Contains(enemyAttackMap, targetSquare))
            {
                if (genQuietMoves)
                {
                    moves.Add(new Move(friendlyKingSquare, targetSquare));
                }
                else
                {
                    if (Piece.IsColor(position[targetSquare], enemyColor))
                    {
                        moves.Add(new Move(friendlyKingSquare, targetSquare));
                    }
                }
            }
        }

        // Kingside Castling
        if (!inCheck && kingsideCastling)
        {
            int targetSquare = friendlyKingSquare + 2;

            if (position[targetSquare - 1] == Piece.None && position[targetSquare] == Piece.None && 
            !Bitboard.Contains(enemyAttackMap, targetSquare - 1) && !Bitboard.Contains(enemyAttackMap, targetSquare))
            {
                moves.Add(new Move(friendlyKingSquare, targetSquare, MoveFlag.Castling));
            }
        }
        // Queenside Castling
        if (!inCheck && queensideCastling)
        {
            int targetSquare = friendlyKingSquare - 2;

            if (position[targetSquare - 1] == Piece.None && position[targetSquare + 1] == Piece.None && 
            position[targetSquare] == Piece.None && 
            !Bitboard.Contains(enemyAttackMap, targetSquare + 1) && 
            !Bitboard.Contains(enemyAttackMap, targetSquare))
            {
                moves.Add(new Move(friendlyKingSquare, targetSquare, MoveFlag.Castling));
            }
        }
    }

    static void GenerateKnightMoves()
    {
        PieceList knights = board.pieceSquares[isWhiteTurn ? BitboardIndex.WhiteKnight : BitboardIndex.BlackKnight];

        for (int i = 0; i < knights.count; i++)
        {
            int startSquare = knights.squares[i];

            // Knight cannot move if it is pinned
            if (IsPinned(startSquare))
            {
                continue;
            }

            for (int index = 0; index < PreComputedData.knightSquares[startSquare].Count; index++)
            {
                int targetSquare = PreComputedData.knightSquares[startSquare][index];

                if (Piece.IsColor(position[targetSquare], friendlyColor) || (inCheck && !SquareIsInCheckRay(targetSquare)))
                {
                    continue;
                }

                if (genQuietMoves)
                {
                    moves.Add(new Move(startSquare, targetSquare));
                }
                else
                {
                    if (Piece.IsColor(position[targetSquare], enemyColor))
                    {
                        moves.Add(new Move(startSquare, targetSquare));
                    }
                }
            }
        }
    }

    static void GeneratePawnMoves()
    {
        PieceList pawns = board.pieceSquares[isWhiteTurn ? BitboardIndex.WhitePawn : BitboardIndex.BlackPawn];

        for (int i = 0; i < pawns.count; i++)
        {
            int startSquare = pawns.squares[i];
            int pushOffset = isWhiteTurn ? 8 : -8;

            int targetSquare = startSquare + pushOffset;

            bool generatePromotionMoves = startSquare / 8 == (isWhiteTurn ? 6 : 1);

            // Forward movements
            if (position[targetSquare] == Piece.None && genQuietMoves)
            {
                // This pawn is not pinned, or it is moving along the pin ray
                if (!IsPinned(startSquare) || IsMovingAlongRay(pushOffset, friendlyKingSquare, startSquare))
                {
                    // The king is not in check, or this pawn is going to block the check ray
                    if (!inCheck || SquareIsInCheckRay(targetSquare))
                    {
                        if (generatePromotionMoves)
                        {
                            GeneratePromotionMoves(startSquare, targetSquare);
                        }
                        else
                        {
                            moves.Add(new Move(startSquare, targetSquare));
                        }
                    }

                    // Two squares forward
                    if ((startSquare / 8 == (isWhiteTurn ? 1 : 6)) && position[targetSquare + pushOffset] == Piece.None)
                    {
                        // The king is not in check, or this pawn is going to block the check ray
                        if (!inCheck || SquareIsInCheckRay(targetSquare + pushOffset))
                        {
                            moves.Add(new Move(startSquare, targetSquare + pushOffset, MoveFlag.PawnTwoForward));
                        }
                    }
                }
            }

            // Capture Right
            if (startSquare % 8 < 7)
            {
                int captureOffset = isWhiteTurn ? 9 : - 7;
                targetSquare = startSquare + captureOffset;

                // This pawn is not pinned, or it is moving along the pin ray
                if (!IsPinned(startSquare) || IsMovingAlongRay(captureOffset, friendlyKingSquare, startSquare))
                {
                    // The king is not in check, or this pawn is going to block the check ray
                    if (!inCheck || SquareIsInCheckRay(targetSquare))
                    {
                        // There is an enemy piece
                        if (Piece.IsColor(position[targetSquare], enemyColor))
                        {
                            if (generatePromotionMoves)
                            {
                                GeneratePromotionMoves(startSquare, targetSquare);
                            }
                            else
                            {
                                moves.Add(new Move(startSquare, targetSquare));
                            }
                        }
                    }

                    // En passant
                    if (targetSquare == Square.EnpassantFileToCaptureSquare(board.enpassantFile, isWhiteTurn) && (!inCheck || SquareIsInCheckRay(targetSquare) || SquareIsInCheckRay(targetSquare + (isWhiteTurn ? - 8 : 8))))
                    {
                        position[targetSquare + (isWhiteTurn ? - 8 : 8)] = Piece.None;

                        if (!IsHorizontalChecked())
                        {
                            moves.Add(new Move(startSquare, targetSquare, MoveFlag.EnpassantCapture));
                        }

                        position[targetSquare + (isWhiteTurn ? - 8 : 8)] = (isWhiteTurn ? Piece.Black : Piece.White) | Piece.Pawn;
                    }
                }
            }
            // Capture Left
            if (startSquare % 8 > 0)
            {
                int captureOffset = isWhiteTurn ? 7 : - 9;
                targetSquare = startSquare + captureOffset;

                // This pawn is not pinned, or it is moving along the pin ray
                if (!IsPinned(startSquare) || IsMovingAlongRay(captureOffset, friendlyKingSquare, startSquare))
                {
                    // The king is not in check, or this pawn is going to block the check ray
                    if (!inCheck || SquareIsInCheckRay(targetSquare))
                    {
                        // There is an enemy piece
                        if (Piece.IsColor(position[targetSquare], enemyColor))
                        {
                            if (generatePromotionMoves)
                            {
                                GeneratePromotionMoves(startSquare, targetSquare);
                            }
                            else
                            {
                                moves.Add(new Move(startSquare, targetSquare));
                            }
                        }
                    }

                    // En passant
                    if (targetSquare == Square.EnpassantFileToCaptureSquare(board.enpassantFile, isWhiteTurn) && (!inCheck || SquareIsInCheckRay(targetSquare) || SquareIsInCheckRay(targetSquare + (isWhiteTurn ? - 8 : 8))))
                    {
                        position[targetSquare + (isWhiteTurn ? - 8 : 8)] = Piece.None;

                        if (!IsHorizontalChecked())
                        {
                            moves.Add(new Move(startSquare, targetSquare, MoveFlag.EnpassantCapture));
                        }

                        position[targetSquare + (isWhiteTurn ? - 8 : 8)] = (isWhiteTurn ? Piece.Black : Piece.White) | Piece.Pawn;
                    }
                }
            }
        }
    }

    static void GeneratePromotionMoves(int startSquare, int targetSquare)
    {
        // Generate promotion moves
        // Promote to: Queen, Rook, Knight, Bishop
        moves.Add(new Move(startSquare, targetSquare, MoveFlag.PromoteToQueen));
        moves.Add(new Move(startSquare, targetSquare, MoveFlag.PromoteToRook));
        moves.Add(new Move(startSquare, targetSquare, MoveFlag.PromoteToKnight));
        moves.Add(new Move(startSquare, targetSquare, MoveFlag.PromoteToBishop));
    }


    static void CalculateAttackData()
    {
        CalculateSlidingAttackMap();

        int startDirIndex = 0;
        int endDirIndex = 8;
        
        // Pins / Checks for enemy sliding pieces
        // Skip directions if there is not a piece left to attack in that direction.
        // Only if there is no queen as a queen moves for all 8 directions.
        if (board.pieceSquares[isWhiteTurn ? BitboardIndex.BlackQueen : BitboardIndex.WhiteQueen].count == 0)
        {
            startDirIndex = (board.pieceSquares[isWhiteTurn ? BitboardIndex.BlackRook : BitboardIndex.WhiteRook].count > 0) ? 0 : 4;
            endDirIndex = 
            (board.pieceSquares[isWhiteTurn ? BitboardIndex.BlackBishop : BitboardIndex.WhiteBishop].count > 0) ? 8 : 4;
        }

        for (int dir = startDirIndex; dir < endDirIndex; dir++) {
            bool isDiagonal = dir > 3;

            int n = PreComputedData.numSquaresToEdge[friendlyKingSquare, dir];
            int directionOffset = directionOffsets[dir];
            bool isFriendlyPieceAlongRay = false;
            ulong rayMask = 0;

            for (int i = 0; i < n; i++) {
                int squareIndex = friendlyKingSquare + directionOffset * (i + 1);
                rayMask |= (ulong) 1 << squareIndex;

                // This square contains a friendly piece
                if (Piece.IsColor(position[squareIndex], friendlyColor))
                {
                    // First friendly piece we have come across in this direction, so it might be pinned
                    if (!isFriendlyPieceAlongRay) {
                        isFriendlyPieceAlongRay = true;
                    }
                    // This is the second friendly piece we've found in this direction, therefore pin is not possible
                    else
                    {
                        break;
                    }
                }
                    
                // This square contains an enemy piece
                else if (Piece.IsColor(position[squareIndex], enemyColor))
                {
                    // Check if piece is in bitmask of pieces able to move in current direction
                    if (isDiagonal && Piece.IsDiagonalPiece(position[squareIndex]) || 
                    !isDiagonal && Piece.IsStraightPiece(position[squareIndex]))
                    {
                        // Friendly piece blocks the check, so this is a pin
                        if (isFriendlyPieceAlongRay)
                        {
                            pinsExistInPosition = true;
                            pinRayBitmask |= rayMask;
                        }
                        // No friendly piece blocking the attack, so this is a check
                        else
                        {
                            checkRayBitmask |= rayMask;
                            inDoubleCheck = inCheck; // If already in check, then this is double check
                            inCheck = true;
                        }
                        break;
                    }
                    else
                    {
                        // This enemy piece is not able to move in the current direction, and so is blocking any checks/pins
                        break;
                    }
                }
            }

            // Stop searching for pins if in double check, as the king is the only piece able to move in that case anyway
            if (inDoubleCheck) {
                break;
            }
        }

        // Knight attacks
        PieceList opponentKnights = board.pieceSquares[isWhiteTurn ? BitboardIndex.BlackKnight : BitboardIndex.WhiteKnight];
        
        bool isKnightCheck = false;

        for (int knightIndex = 0; knightIndex < opponentKnights.count; knightIndex++) {
            int startSquare = opponentKnights.squares[knightIndex];
            enemyKnightAttackMap |= PreComputedData.knightMap[startSquare];

            if (!isKnightCheck && Bitboard.Contains(enemyKnightAttackMap, friendlyKingSquare)) {
                isKnightCheck = true;
                inDoubleCheck = inCheck; // if already in check, then this is double check
                inCheck = true;
                checkRayBitmask |= (ulong) 1 << startSquare;
            }
        }

        // Pawn attacks
        PieceList opponentPawns = board.pieceSquares[isWhiteTurn ? BitboardIndex.BlackPawn : BitboardIndex.WhitePawn];
        
        bool isPawnCheck = false;

        for (int pawnIndex = 0; pawnIndex < opponentPawns.count; pawnIndex++) {
            int pawnSquare = opponentPawns.squares[pawnIndex];
            ulong pawnAttacks = isWhiteTurn ? PreComputedData.blackPawnAttackMap[pawnSquare] : PreComputedData.whitePawnAttackMap[pawnSquare];
            enemyPawnAttackMap |= pawnAttacks;

            if (!isPawnCheck && Bitboard.Contains(pawnAttacks, friendlyKingSquare)) {
                isPawnCheck = true;
                inDoubleCheck = inCheck; // If already in check, then this is double check
                inCheck = true;
                checkRayBitmask |= (ulong) 1 << pawnSquare;
            }
        }

        int enemyKingSquare = board.pieceSquares[isWhiteTurn ? BitboardIndex.BlackKing : BitboardIndex.WhiteKing].squares[0];

        enemyAttackMapNoPawns = enemySlidingAttackMap | enemyKnightAttackMap | PreComputedData.kingMap[enemyKingSquare];
        enemyAttackMap = enemyAttackMapNoPawns | enemyPawnAttackMap;
    }

    static void CalculateSlidingAttackMap()
    {
        PieceList enemyRooks = board.pieceSquares[enemyRookIndex];
        PieceList enemyBishops = board.pieceSquares[enemyBishopIndex];
        PieceList enemyQueens = board.pieceSquares[enemyQueenIndex];

        for (int index = 0; index < enemyRooks.count; index++)
        {
            AddSlidingAttackMap(enemyRooks.squares[index], 0, 4);
        }
        for (int index = 0; index < enemyBishops.count; index++)
        {
            AddSlidingAttackMap(enemyBishops.squares[index], 4, 8);
        }
        for (int index = 0; index < enemyQueens.count; index++)
        {
            AddSlidingAttackMap(enemyQueens.squares[index], 0, 8);
        }
    }

    static void AddSlidingAttackMap(int startSquare, int startDirIndex, int endDirIndex)
    {
        for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++)
        {
            int currentOffset = directionOffsets[dirIndex];

            for (int n = 0; n < PreComputedData.numSquaresToEdge[startSquare, dirIndex]; n++)
            {
                int targetSquare = startSquare + currentOffset * (n + 1);

                // Blocked by a piece; Move on to the next direction.
                // Includes friendly capture, to also calculate protected pieces.
                if (position[targetSquare] != Piece.None)
                {
                    enemySlidingAttackMap |= (ulong) 1 << targetSquare;
                    if (targetSquare == friendlyKingSquare)
                    {
                        continue;
                    }
                    break;
                }

                enemySlidingAttackMap |= (ulong) 1 << targetSquare;
            }
        }
    }

    static bool IsPinned(int square)
    {
        return pinsExistInPosition && (pinRayBitmask & (ulong) 1 << square) != 0;
    }

    static bool SquareIsInCheckRay (int square)
    {
        return inCheck && (checkRayBitmask & (ulong) 1 << square) != 0;
    }

    static bool IsMovingAlongRay(int dirOffset, int startSquare, int targetSquare)
    {
        int moveDir = PreComputedData.directionLookup[targetSquare - startSquare + 63];
        
		return dirOffset == moveDir || -dirOffset == moveDir;
    }

    static bool IsHorizontalChecked()
    {
        for (int n = 0; n < PreComputedData.numSquaresToEdge[friendlyKingSquare, 0]; n++)
        {
            if (position[friendlyKingSquare + n + 1] != Piece.None)
            {
                if (Piece.IsColor(position[friendlyKingSquare + n + 1], enemyColor) && 
                Piece.IsStraightPiece(position[friendlyKingSquare + n + 1]))
                {
                    return true;
                }

                break;
            }
        }

        for (int n = 0; n < PreComputedData.numSquaresToEdge[friendlyKingSquare, 2]; n++)
        {
            if (position[friendlyKingSquare - n - 1] != Piece.None)
            {
                if (Piece.IsColor(position[friendlyKingSquare - n - 1], enemyColor) && 
                Piece.IsStraightPiece(position[friendlyKingSquare - n - 1]))
                {
                    return true;
                }
                
                break;
            }
        }

        return false;
    }
}
