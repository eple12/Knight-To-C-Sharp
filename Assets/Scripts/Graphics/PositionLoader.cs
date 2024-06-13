using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PositionLoader : MonoBehaviour
{
    Board board;
    List<PieceObject> pieceObjects;
    GameObject[] piecePrefabs;

    public GameObject fenTextObject;
    TextMeshProUGUI fenTMP;
    
    public GameObject placeHolderObject;
    TextMeshProUGUI placeHolderTMP;

    public GameObject inputFieldObject;
    TMP_InputField inputField;

    public void Initialize()
    {
        // Initialize Variables
        board = Graphic.board;
        pieceObjects = Graphic.pieceObjects;
        piecePrefabs = Graphic.piecePrefabs;
        fenTMP = fenTextObject.GetComponent<TextMeshProUGUI>();
        placeHolderTMP = placeHolderObject.GetComponent<TextMeshProUGUI>();
        inputField = inputFieldObject.GetComponent<TMP_InputField>();

        Setup();

        LoadPositionFromFen(board.loadFen);
    }

    void Setup()
    {
        // Initialize Graphics
        DestroyAllPieceObjects();
        Graphic.Reset();

        board.Reset();
    }

    public void LoadButton()
    {
        LoadPositionFromFen(placeHolderTMP.enabled ? board.loadFen : fenTMP.text);

        inputField.text = "";
    }

    void PlaceSinglePiece(int piece, int square)
    {
        if (piece == Piece.None)
        {
            return;
        }

        PieceObject createdPiece = Instantiate(piecePrefabs[Piece.GetBitboardIndex(piece)]).GetComponent<PieceObject>();
        
        createdPiece.transform.position = Square.SquareIndexToWorld(square);
        pieceObjects.Add(createdPiece);

        board.position[square] = piece;

        createdPiece.SquareIndex = square;
        createdPiece.PieceValue = piece;

        // Add square; (Piece Squares)
        board.pieceSquares[Piece.GetBitboardIndex(piece)].AddPieceAtSquare(square);
    }

    void LoadPositionFromFen(string fen)
    {
        board.BeforeLoadingPosition();
        Setup();

        for (int i = 0; i < 12; i++)
        {
            if (board.pieceSquares[i] == null)
            {
                board.pieceSquares[i] = new PieceList();
            }
            board.pieceSquares[i].squares = new int[16]; // Resets piece squares to 0;
            board.pieceSquares[i].count = 0;
        }

        Dictionary<char, int> fenCharToIndex = new Dictionary<char, int>()
        {
            {'P', Piece.White | Piece.Pawn}, {'N', Piece.White | Piece.Knight}, {'B', Piece.White | Piece.Bishop}, 
            {'R', Piece.White | Piece.Rook}, {'Q', Piece.White | Piece.Queen}, {'K', Piece.White | Piece.King}, 

            {'p', Piece.Black | Piece.Pawn}, {'n', Piece.Black | Piece.Knight}, {'b', Piece.Black | Piece.Bishop}, 
            {'r', Piece.Black | Piece.Rook}, {'q', Piece.Black | Piece.Queen}, {'k', Piece.Black | Piece.King}
        };

        string[] splitFen = fen.Split(' ');

        string fenboard = splitFen[0];
        int file = 0;
        int rank = 7;

        foreach(char character in fenboard)
        {
            if (character == '/')
            {
                file = 0;
                rank--;
            }
            else
            {
                if (char.IsDigit(character))
                {
                    file += (int)char.GetNumericValue(character);
                }
                else
                {
                    PlaceSinglePiece(fenCharToIndex[character], Square.FileRankToSquareIndex(file, rank));
                    file++;
                }
            }
        }

        // Turn;
        board.isWhiteTurn = splitFen[1] == "w";

        // Castle;
        string castleFen = splitFen[2];
        board.isWhiteKingsideCastle = false;
        board.isWhiteQueensideCastle = false;
        board.isBlackKingsideCastle = false;
        board.isBlackQueensideCastle = false;

        if (castleFen == "-")
        {
            // No CASTLING!
        }
        else
        {
            if (castleFen.Contains('K'))
            {
                board.isWhiteKingsideCastle = true;
            }
            if (castleFen.Contains('Q'))
            {
                board.isWhiteQueensideCastle = true;
            }
            if (castleFen.Contains('k'))
            {
                board.isBlackKingsideCastle = true;
            }
            if (castleFen.Contains('q'))
            {
                board.isBlackQueensideCastle = true;
            }
        }
    
        // En passant square;
        string enpassantFen = splitFen[3];
        board.enpassantFile = 8; // Invalid Index;
        if (enpassantFen == "-")
        {

        }
        else
        {
            board.enpassantFile = Square.SquareNameToIndex(enpassantFen) % 8;
        }

        // Fifty-counter;
        board.fiftyRuleHalfClock = splitFen.Length >= 5 && char.IsDigit(splitFen[4].ToCharArray()[0]) ? Convert.ToInt32(splitFen[4]) : 0;

        board.currentZobristKey = Zobrist.GetZobristKey(board);
        board.positionHistory[board.currentZobristKey] = 1;

        Graphic.AfterLoadingPosition();
    }

    void DestroyAllPieceObjects()
    {
        foreach (PieceObject pieceObject in Graphic.pieceObjects)
        {
            Destroy(pieceObject.gameObject);
        }

        Graphic.pieceObjects.Clear();
    }
}
