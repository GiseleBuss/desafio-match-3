using System.Collections.Generic;
using UnityEngine;

public class GameController
{
    private List<List<Tile>> _boardTiles;
    private List<int> _tilesTypes;
    private int _tileCount;
    private Score score = new Score(0);//Gisele - dá pra colocar apenas um int. quan fica melhor? fica bem escalavem só com int?

    public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
    {
        _tilesTypes = new List<int> { 0, 1, 2, 3 };
        _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);
        return _boardTiles;
    }

    public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        Tile switchedTile = newBoard[fromY][fromX];
        newBoard[fromY][fromX] = newBoard[toY][toX];
        newBoard[toY][toX] = switchedTile;

        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                if (FindMatchInLine(newBoard, x, y) || FindMatchInColumn(newBoard, x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        Tile switchedTile = newBoard[fromY][fromX];
        newBoard[fromY][fromX] = newBoard[toY][toX];
        newBoard[toY][toX] = switchedTile;

        MovedTileInfo moveDone = new MovedTileInfo
        {
            from = new Vector2Int(fromX, fromY),
            to = new Vector2Int(toX, toY)
        };

        List<BoardSequence> boardSequences = new List<BoardSequence>();
        List<List<bool>> matchedTiles;

        while (HasMatch(matchedTiles = FindMatches(newBoard, moveDone)))
        {
            List<List<Tile>> boardBeforeCleanup = CopyBoard(newBoard);
            List<Vector2Int> matchedPosition = CleaningMatched(newBoard, matchedTiles);
            List<MovedTileInfo> movedTilesList = DroppingTiles(newBoard, matchedPosition);           
            List<AddedTileInfo> addedTiles = FillingBoard(newBoard);

            BoardSequence sequence = new BoardSequence
            {
                matchedPosition = matchedPosition,
                movedTiles = movedTilesList,
                addedTiles = addedTiles,
                tilesBeforeCleanup = boardBeforeCleanup
            };
            boardSequences.Add(sequence);
        }

        _boardTiles = newBoard;
        return boardSequences;
        //return _boardTiles;
    }

    private List<AddedTileInfo> FillingBoard(List<List<Tile>> newBoard)
    {
        // Filling the board
        List<AddedTileInfo> addedTiles = new List<AddedTileInfo>();
        for (int y = newBoard.Count - 1; y > -1; y--)
        {
            for (int x = newBoard[y].Count - 1; x > -1; x--)
            {
                if (newBoard[y][x].type == -1)
                {
                    int tileType = Random.Range(0, _tilesTypes.Count);
                    Tile tile = newBoard[y][x];
                    tile.id = _tileCount++;
                    tile.type = _tilesTypes[tileType];
                    addedTiles.Add(new AddedTileInfo
                    {
                        position = new Vector2Int(x, y),
                        type = tile.type
                    });
                }
            }
        }

        return addedTiles;
    }

    private List<AddedTileInfo> AddingSpacialTiles(List<List<Tile>> newBoard)
    {
        List<AddedTileInfo> addedSpecialTiles = new List<AddedTileInfo>();



        return addedSpecialTiles;
    }

    private static List<MovedTileInfo> DroppingTiles(List<List<Tile>> newBoard, List<Vector2Int> matchedPosition)
    {

        // Dropping the tiles
        Dictionary<int, MovedTileInfo> movedTiles = new Dictionary<int, MovedTileInfo>();
        List<MovedTileInfo> movedTilesList = new List<MovedTileInfo>();
        for (int i = 0; i < matchedPosition.Count; i++)
        {
            int x = matchedPosition[i].x;
            int y = matchedPosition[i].y;
            if (y > 0)
            {
                for (int j = y; j > 0; j--)
                {
                    Tile movedTile = newBoard[j - 1][x];
                    newBoard[j][x] = movedTile;
                    if (movedTile.type > -1)
                    {
                        if (movedTiles.ContainsKey(movedTile.id))
                        {
                            movedTiles[movedTile.id].to = new Vector2Int(x, j);
                        }
                        else
                        {
                            MovedTileInfo movedTileInfo = new MovedTileInfo
                            {
                                from = new Vector2Int(x, j - 1),
                                to = new Vector2Int(x, j)
                            };
                            movedTiles.Add(movedTile.id, movedTileInfo);
                            movedTilesList.Add(movedTileInfo);
                        }
                    }
                }

                newBoard[0][x] = new Tile
                {
                    id = -1,
                    type = -1
                };
            }
        }

        return movedTilesList;
    }

    private List<Vector2Int> CleaningMatched(List<List<Tile>> newBoard, List<List<bool>> matchedTiles)
    {
        //Cleaning the matched tiles
        List<Vector2Int> matchedPosition = new List<Vector2Int>();
        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                if (matchedTiles[y][x])
                {
                    matchedPosition.Add(new Vector2Int(x, y));
                    score.increaseScore(10);
                    newBoard[y][x] = new Tile { id = -1, type = -1 };
                }
            }
        }

        return matchedPosition;
    }

    public Score GetScore()
    {
        return score;
    }

    public List<BoardSequence> UseSpecialTile(int fromX, int fromY, int toX, int toY, int type)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        MovedTileInfo moveDone = new MovedTileInfo
        {
            from = new Vector2Int(fromX, fromY),
            to = new Vector2Int(fromX, fromX)
        };

        List<BoardSequence> boardSequences = new List<BoardSequence>();
        List<List<bool>> matchedTiles;

        matchedTiles = CreateMatchedTiles(newBoard);

        if (type == 4)
        {
            for (int i = 0; i <= 9; i++)
            {
                matchedTiles[fromY][i] = true;
            }
            score.increaseScore(50);
        }
        else if (type == 5)
        {
            // Tem que arrumar para quando a bomba está na lateral
            for (int y = fromY - 1; (y <= fromY + 1 && y <= 9); y++)
            {
                if (y == -1)
                {
                    continue;
                }
                for (int x = fromX - 1; (x <= fromX + 1 && x <= 9); x++)
                {
                    if (x == -1)
                    {
                        continue;
                    }
                    matchedTiles[y][x] = true;
                }
            }


            score.increaseScore(100);

        }
        else if (type == 6)
        {
            int secondType = 6;
            if(newBoard[toY][toX].type == 6)
            {
                secondType = newBoard[fromY][fromX].type;
                matchedTiles[toY][toX] = true;
            }
            else
            {
                secondType = newBoard[toY][toX].type;
                matchedTiles[fromY][fromX] = true;
            }
            

            

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    //precisa confirmar se o tipo 
                    if (newBoard[y][x].type == secondType)
                    {
                        matchedTiles[y][x] = true;
                    }
                }
            }
        }
        if (type == 7)
        {
            for (int i = 0; i <= 9; i++)
            {
                matchedTiles[i][fromX] = true;
            }
            score.increaseScore(50);
        }

        List<List<Tile>> boardBeforeClean = CopyBoard(newBoard);
        List<Vector2Int> matchedPosition = CleaningMatched(newBoard, matchedTiles);
        List<AddedTileInfo> addedSpecialTiles = AddingSpacialTiles(newBoard);
        List<MovedTileInfo> movedTilesList = DroppingTiles(newBoard, matchedPosition);
        List<AddedTileInfo> addedTiles = FillingBoard(newBoard);

        BoardSequence sequence = new BoardSequence
        {
            matchedPosition = matchedPosition,
            movedTiles = movedTilesList,
            addedTiles = addedTiles,
            tilesBeforeCleanup = boardBeforeClean

        };
        boardSequences.Add(sequence);
        while (HasMatch(matchedTiles = FindMatches(newBoard, moveDone)))
        {
            boardBeforeClean = CopyBoard(newBoard);
            matchedPosition = CleaningMatched(newBoard, matchedTiles);
            addedSpecialTiles = AddingSpacialTiles(newBoard);
            movedTilesList = DroppingTiles(newBoard, matchedPosition);
            addedTiles = FillingBoard(newBoard);

            BoardSequence matchsequence = new BoardSequence
            {
                matchedPosition = matchedPosition,
                movedTiles = movedTilesList,
                addedTiles = addedTiles,
                tilesBeforeCleanup = boardBeforeClean

            };
            boardSequences.Add(matchsequence);
        }


        _boardTiles = newBoard;
        return boardSequences;
    }

    public int GetTileType(int x, int y)
    {
        return _boardTiles[y][x].type;
    }

    private static bool HasMatch(List<List<bool>> list)
    {
        for (int y = 0; y < list.Count; y++)
            for (int x = 0; x < list[y].Count; x++)
                if (list[y][x])
                    return true;
        return false;
    }

    private List<List<bool>> FindMatches(List<List<Tile>> newBoard, MovedTileInfo moveDone)
    {
        List<List<bool>> matchedTiles = CreateMatchedTiles(newBoard);

        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {

                List<Vector2Int> checkCleanerColorResult = CheckCleanerColor(newBoard, matchedTiles, y, x);
                List<Vector2Int> checkLineCleanerResult = CheckLineCleaner(newBoard, matchedTiles, y, x);
                List<Vector2Int> checkColumnResult = CheckColumnCleaner(newBoard, matchedTiles, y, x);
                List<Vector2Int> checkExplosionResult = CheckExplosion(newBoard, matchedTiles, y, x);


                if (checkCleanerColorResult.Count > 0)
                {
                    score.increaseScore(60);
                    SetEspecialTile(newBoard, moveDone, matchedTiles, checkCleanerColorResult, 6);
                }
                else if (checkExplosionResult.Count > 0)
                {
                    score.increaseScore(60);
                    SetEspecialTile(newBoard, moveDone, matchedTiles, checkExplosionResult, 5);
                }
                else if (checkLineCleanerResult.Count > 0)
                {
                    score.increaseScore(60);
                    SetEspecialTile(newBoard, moveDone, matchedTiles, checkLineCleanerResult, 7);
                }
                else if (checkColumnResult.Count > 0)
                {
                    score.increaseScore(60);
                    SetEspecialTile(newBoard, moveDone, matchedTiles, checkColumnResult, 4);
                }

                if (FindMatchInLine(newBoard, x, y))
                {
                    matchedTiles[y][x] = true;
                    matchedTiles[y][x - 1] = true;
                    matchedTiles[y][x - 2] = true;
                }
                if (FindMatchInColumn(newBoard, x, y))
                {
                    matchedTiles[y][x] = true;
                    matchedTiles[y - 1][x] = true;
                    matchedTiles[y - 2][x] = true;
                }
            }
        }

        return matchedTiles;
    }

    private static void SetEspecialTile(List<List<Tile>> newBoard, MovedTileInfo moveDone, List<List<bool>> matchedTiles, List<Vector2Int> checkResult, int type)
    {
        int x;
        int y;

        if (checkResult.Contains(new Vector2Int(moveDone.to.x, moveDone.to.y)))
        {
            y = moveDone.to.y;
            x = moveDone.to.x;
        }
        else if (checkResult.Contains(new Vector2Int(moveDone.from.x, moveDone.from.y)))
        {
            y = moveDone.from.y;
            x = moveDone.from.x;
        }
        else if (type == 6)
        {
            y = checkResult[1].y;
            x = checkResult[1].x;
        }
        else
        {
            y = checkResult[0].y;
            x = checkResult[0].x;
        }

        newBoard[y][x].type = type;
        matchedTiles[y][x] = false;
    }
    private static List<Vector2Int> CheckExplosion(List<List<Tile>> newBoard, List<List<bool>> matchedTiles, int column, int line)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        int typeBase = newBoard[line][column].type;

        //check for horizontal match 3
        if (column > 1 &&
            typeBase == newBoard[line][column - 1].type &&
            typeBase == newBoard[line][column - 2].type)
        {
            //check for matches with one above and one below the line
            if (line > 0 && line < 9)
            {
                /*
                 * #
                 * ###
                 * #
                 */
                if (typeBase == newBoard[line - 1][column - 2].type &&
                    typeBase == newBoard[line + 1][column - 2].type)
                {
                    SetMatchThreeToMatchedTiles(matchedTiles, column, line);
                    AddMatchThreeToResultList(column, line, result);

                    matchedTiles[line - 1][column - 2] = true;
                    matchedTiles[line + 1][column - 2] = true;

                    result.Add(new Vector2Int(column - 2, line - 1));
                    result.Add(new Vector2Int(column - 2, line + 1));
                    Debug.Log("Gisele - explosão um pra cima e um pra baixo para direita");
                }

                /*
                 *   #
                 * ###
                 *   #
                 */
                if (typeBase == newBoard[line - 1][column].type &&
                    typeBase == newBoard[line + 1][column].type)
                {
                    SetMatchThreeToMatchedTiles(matchedTiles, column, line);
                    AddMatchThreeToResultList(column, line, result);

                    matchedTiles[line - 1][column] = true;
                    matchedTiles[line + 1][column] = true;

                    result.Add(new Vector2Int(column - 2, line - 1));
                    result.Add(new Vector2Int(column - 2, line + 1));
                    Debug.Log("Gisele - explosão um pra cima e um pra baixo para esquerda");
                }
            }
            if (line < 8)
            {
                /*
                 * ###
                 * #
                 * #
                 */
                if (typeBase == newBoard[line + 1][column - 2].type &&
                    typeBase == newBoard[line + 2][column - 2].type)
                {
                    SetMatchThreeToMatchedTiles(matchedTiles, column, line);
                    AddMatchThreeToResultList(column, line, result);

                    matchedTiles[line + 1][column - 2] = true;
                    matchedTiles[line + 2][column - 2] = true;

                    result.Add(new Vector2Int(column - 2, line + 1));
                    result.Add(new Vector2Int(column - 2, line + 2));
                    Debug.Log("Gisele - explosão dois para baixo pela esquerda");
                }

                /*
                 * ###
                 *  #
                 *  #
                 */
                if (typeBase == newBoard[line + 1][column - 1].type &&
                    typeBase == newBoard[line + 2][column - 1].type)
                {
                    SetMatchThreeToMatchedTiles(matchedTiles, column, line);
                    AddMatchThreeToResultList(column, line, result);

                    matchedTiles[line + 1][column - 1] = true;
                    matchedTiles[line + 2][column - 1] = true;

                    result.Add(new Vector2Int(column - 1, line + 1));
                    result.Add(new Vector2Int(column - 1, line + 2));
                    Debug.Log("Gisele - explosão dois para baixo pelo meio");
                }

                /*
                 * ###
                 *   #
                 *   #
                 */
                if (typeBase == newBoard[line + 1][column].type &&
                    typeBase == newBoard[line + 2][column].type)
                {
                    SetMatchThreeToMatchedTiles(matchedTiles, column, line);
                    AddMatchThreeToResultList(column, line, result);

                    matchedTiles[line + 1][column] = true;
                    matchedTiles[line + 2][column] = true;

                    result.Add(new Vector2Int(column, line + 1));
                    result.Add(new Vector2Int(column, line + 2));
                    Debug.Log("Gisele - explosão dois para baixo pela direita");
                }
            }
            if (line > 2)
            {
                /*
                 * #
                 * #
                 * ###
                 */
                if (typeBase == newBoard[line - 1][column - 2].type &&
                    typeBase == newBoard[line - 2][column - 2].type)
                {
                    SetMatchThreeToMatchedTiles(matchedTiles, column, line);
                    AddMatchThreeToResultList(column, line, result);

                    matchedTiles[line - 1][column - 2] = true;
                    matchedTiles[line - 2][column - 2] = true;

                    result.Add(new Vector2Int(column - 2, line - 1));
                    result.Add(new Vector2Int(column - 2, line - 2));
                    Debug.Log("Gisele - explosão dois para cima pela esquerda");
                }

                /*
                 *  #
                 *  #
                 * ###
                 */
                if (typeBase == newBoard[line - 1][column - 1].type &&
                    typeBase == newBoard[line - 2][column - 1].type)
                {
                    SetMatchThreeToMatchedTiles(matchedTiles, column, line);
                    AddMatchThreeToResultList(column, line, result);

                    matchedTiles[line - 1][column - 1] = true;
                    matchedTiles[line - 2][column - 1] = true;

                    result.Add(new Vector2Int(column - 1, line - 1));
                    result.Add(new Vector2Int(column - 1, line - 2));
                    Debug.Log("Gisele - explosão dois para cima pelo meio");
                }

                /*
                 *   #
                 *   #
                 * ###
                 */
                if (typeBase == newBoard[line - 1][column].type &&
                    typeBase == newBoard[line - 2][column].type)
                {
                    SetMatchThreeToMatchedTiles(matchedTiles, column, line);
                    AddMatchThreeToResultList(column, line, result);

                    matchedTiles[line - 1][column] = true;
                    matchedTiles[line - 2][column] = true;

                    result.Add(new Vector2Int(column, line - 1));
                    result.Add(new Vector2Int(column, line - 2));
                    Debug.Log("Gisele - explosão dois para cima pela direita");
                }
            }
        }

        return result;
    }

    private static void AddMatchThreeToResultList(int column, int line, List<Vector2Int> result)
    {
        result.Add(new Vector2Int(column, line));
        result.Add(new Vector2Int(column - 1, line));
        result.Add(new Vector2Int(column - 2, line));
    }

    private static void SetMatchThreeToMatchedTiles(List<List<bool>> matchedTiles, int column, int line)
    {
        matchedTiles[line][column] = true;
        matchedTiles[line][column - 1] = true;
        matchedTiles[line][column - 2] = true;
    }


    private static List<Vector2Int> CheckLineCleaner(List<List<Tile>> newBoard, List<List<bool>> matchedTiles, int line, int column)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        if (column > 2
            && newBoard[line][column].type == newBoard[line][column - 1].type
            && newBoard[line][column].type == newBoard[line][column - 2].type
            && newBoard[line][column].type == newBoard[line][column - 3].type)
        {
            Debug.Log("Gisele - match 4 line");

            matchedTiles[line][column] = true;
            matchedTiles[line][column - 1] = true;
            matchedTiles[line][column - 2] = true;
            matchedTiles[line][column - 3] = true;

            result.Add(new Vector2Int(column, line));
            result.Add(new Vector2Int(column - 1, line));
            result.Add(new Vector2Int(column - 2, line));
            result.Add(new Vector2Int(column - 3, line));
        }
        return result;
    }

    private static List<Vector2Int> CheckColumnCleaner(List<List<Tile>> newBoard, List<List<bool>> matchedTiles, int line, int column)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        if (line > 2
            && newBoard[line][column].type == newBoard[line - 1][column].type
            && newBoard[line][column].type == newBoard[line - 2][column].type
            && newBoard[line][column].type == newBoard[line - 3][column].type)
        {
            Debug.Log("Gisele - match 4 Column");

            matchedTiles[line][column] = true;
            matchedTiles[line - 1][column] = true;
            matchedTiles[line - 2][column] = true;
            matchedTiles[line - 3][column] = true;

            result.Add(new Vector2Int(column, line));
            result.Add(new Vector2Int(column, line - 1));
            result.Add(new Vector2Int(column, line - 2));
            result.Add(new Vector2Int(column, line - 3));
        }
        return result;
    }

    private static List<Vector2Int> CheckCleanerColor(List<List<Tile>> newBoard, List<List<bool>> matchedTiles, int column, int line)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        if (line > 1 && line < 8
           && newBoard[column][line].type == newBoard[column][line - 1].type
           && newBoard[column][line].type == newBoard[column][line - 2].type
           && newBoard[column][line].type == newBoard[column][line + 1].type
           && newBoard[column][line].type == newBoard[column][line + 2].type)
        {
            Debug.Log("Gisele - match 5 line"); // aqui com ctz vai estar no meio
            // aqui precisa trocar o role do board

            for (int i = column; i <= column; i++)
            {
                for (int x = line - 2; x <= line + 2; x++)
                {
                    matchedTiles[i][x] = true;

                    result.Add(new Vector2Int(x, i));
                }
            }

        }

        if (column > 1 && column < 8
            && newBoard[column][line].type == newBoard[column - 1][line].type
            && newBoard[column][line].type == newBoard[column - 2][line].type
            && newBoard[column][line].type == newBoard[column + 1][line].type
            && newBoard[column][line].type == newBoard[column + 2][line].type)
        {
            Debug.Log("Gisele - match 5 Column"); // aqui com ctz vai estar no meio

            for (int i = column - 2; i <= column + 2; i++)
            {
                for (int x = line; x <= line; x++)
                {
                    matchedTiles[i][x] = true;

                    result.Add(new Vector2Int(x, i));
                }
            }

        }

        return result;
    }

    private static List<List<bool>> CreateMatchedTiles(List<List<Tile>> newBoard)
    {
        List<List<bool>> matchedTiles = new List<List<bool>>();
        for (int y = 0; y < newBoard.Count; y++)
        {
            matchedTiles.Add(new List<bool>(newBoard[y].Count));
            for (int x = 0; x < newBoard.Count; x++)
            {
                matchedTiles[y].Add(false);
            }
        }

        return matchedTiles;
    }

    private static bool FindMatchInLine(List<List<Tile>> newBoard, int x, int y)
    {
        return x > 1
               && newBoard[y][x].type == newBoard[y][x - 1].type
               && newBoard[y][x - 1].type == newBoard[y][x - 2].type;
    }

    private static bool FindMatchInColumn(List<List<Tile>> newBoard, int x, int y)
    {
        return y > 1
               && newBoard[y][x].type == newBoard[y - 1][x].type
               && newBoard[y - 1][x].type == newBoard[y - 2][x].type;
    }

    private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
    {
        List<List<Tile>> newBoard = new List<List<Tile>>(boardToCopy.Count);
        for (int y = 0; y < boardToCopy.Count; y++)
        {
            newBoard.Add(new List<Tile>(boardToCopy[y].Count));
            for (int x = 0; x < boardToCopy[y].Count; x++)
            {
                Tile tile = boardToCopy[y][x];
                newBoard[y].Add(new Tile { id = tile.id, type = tile.type });
            }
        }

        return newBoard;
    }

    private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
    {
        List<List<Tile>> board = new List<List<Tile>>(height);
        _tileCount = 0;
        for (int y = 0; y < height; y++)
        {
            board.Add(new List<Tile>(width));
            for (int x = 0; x < width; x++)
            {
                board[y].Add(new Tile { id = -1, type = -1 });
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                List<int> noMatchTypes = new List<int>(tileTypes.Count);
                for (int i = 0; i < tileTypes.Count; i++)
                {
                    noMatchTypes.Add(_tilesTypes[i]);
                }

                if (x > 1
                    && board[y][x - 1].type == board[y][x - 2].type)
                {
                    noMatchTypes.Remove(board[y][x - 1].type);
                }
                if (y > 1
                    && board[y - 1][x].type == board[y - 2][x].type)
                {
                    noMatchTypes.Remove(board[y - 1][x].type);
                }

                board[y][x].id = _tileCount++;
                board[y][x].type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
            }
        }

        return board;
    }
}
