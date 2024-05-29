using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inspector : MonoBehaviour
{
    // Serialize Field
    [SerializeField] private GameObject[] piecePrefabs;
    [SerializeField] private Sprite[] pieceSprites;
    [SerializeField] private GameObject legalMoveHighlight;
    [SerializeField] private GameObject legalMoveHighlightPrefab;

    [SerializeField] private GameObject moveHighlightPrefab;

    [SerializeField] private GameObject promotionFade;
    [SerializeField] private GameObject whitePromotionUI;
    [SerializeField] private GameObject blackPromotionUI;

    PositionLoader positionLoader;
    Highlight highlight;
    Mouse mouse;
    MoveMaker moveMaker;

    void Start()
    {
        // Main Initialization
        Main.Initialize();

        // Graphic Initialization
        Graphic.piecePrefabs = piecePrefabs;
        Graphic.pieceSprites = pieceSprites;

        Graphic.legalMoveHighlight = legalMoveHighlight;
        Graphic.legalMoveHighlightPrefab = legalMoveHighlightPrefab;

        Graphic.moveHighlightPrefab = moveHighlightPrefab;

        Graphic.promotionFade = promotionFade;
        Graphic.whitePromotionUI = whitePromotionUI;
        Graphic.blackPromotionUI = blackPromotionUI;

        Graphic.Initialize();

        // Components
        positionLoader = gameObject.GetComponent<PositionLoader>();
        highlight = gameObject.GetComponent<Highlight>();
        mouse = gameObject.GetComponent<Mouse>();
        moveMaker = gameObject.GetComponent<MoveMaker>();

        // Components Initialization
        positionLoader.Initialize();
        highlight.Initialize();
        mouse.Initialize();
        moveMaker.Initialize();

        Main.moveMaker = moveMaker;
        EnginePlayer.moveMaker = moveMaker;

        mouse.highlight = highlight;
        mouse.moveMaker = moveMaker;

        Graphic.AfterLoadingPosition();
    }

    void Update()
    {
        Main.Update();

        mouse.HandleMouseEvents();

        Graphic.Update();
    }
}
