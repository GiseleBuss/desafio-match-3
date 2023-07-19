using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private GameController gameController;

    [SerializeField] public int boardWidth = 10;

    [SerializeField] public int boardHeight = 10;

    [SerializeField] public BoardView boardView;

    public TMP_Text scoreValue;

    private void Awake()
    {
        gameController = new GameController();
        boardView.onTileClick += OnTileClick;
    }

    private void Start()
    {
        List<List<Tile>> board = gameController.StartGame(boardWidth, boardHeight);
        boardView.CreateBoard(board);
    }

    private int selectedX, selectedY = -1;

    private bool isAnimating;

    private void OnTileClick(int x, int y)
    {
        if (isAnimating) return;

        if (selectedX > -1 && selectedY > -1)
        {
            int type = gameController.GetTileType(x, y);
            int typeAnt = gameController.GetTileType(selectedX, selectedY);


            // esse typeAnt impede que o limpa linha e a bomba sejam feitas por combinacao de outra
            // cor, mas impede que o explosão de cor funcione

            if ((type > 3 && x == selectedX && y == selectedY) || (type == 6 || typeAnt == 6))            
            {
                if (type == 0)
                {
                    type = 5;
                }

                if (typeAnt == 6)
                {
                    type = typeAnt;
                }

                {
                    List<BoardSequence> useSpecialTileResult = gameController.UseSpecialTile(x, y, selectedX, selectedY, type);
                    scoreValue.text = gameController.GetScore().value.ToString();
                    AnimateBoard(useSpecialTileResult, 0, () => isAnimating = false);
                    selectedX = -1;
                    selectedY = -1;
                }
            }
            else if (Mathf.Abs(selectedX - x) + Mathf.Abs(selectedY - y) > 1)
            {
                selectedX = -1;
                selectedY = -1;
            }
            else
            {
                isAnimating = true;
                boardView.SwapTiles(selectedX, selectedY, x, y).onComplete += () =>
                {
                    bool isValid = gameController.IsValidMovement(selectedX, selectedY, x, y);
                    if (!isValid)
                    {
                        boardView.SwapTiles(x, y, selectedX, selectedY)
                        .onComplete += () => isAnimating = false;
                    }
                    else
                    {
                        List<BoardSequence> swapResult = gameController.SwapTile(selectedX, selectedY, x, y);
                        scoreValue.text = gameController.GetScore().value.ToString();
                        AnimateBoard(swapResult, 0, () => isAnimating = false);
                    }

                    selectedX = -1;
                    selectedY = -1;
                };
            }
        }
        else
        {
            selectedX = x;
            selectedY = y;
        }
    }

    private void AnimateBoard(List<BoardSequence> boardSequences, int i, Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        BoardSequence boardSequence = boardSequences[i];
        sequence.Append(boardView.UpdateTiles(boardSequence.tilesBeforeCleanup));
        sequence.Append(boardView.DestroyTiles(boardSequence.matchedPosition));       
        sequence.Append(boardView.MoveTiles(boardSequence.movedTiles));
        sequence.Append(boardView.CreateTile(boardSequence.addedTiles));

        i++;
        if (i < boardSequences.Count)
        {
            sequence.onComplete += () => AnimateBoard(boardSequences, i, onComplete);
        }
        else
        {
            sequence.onComplete += () => onComplete();
        }
    }
}
