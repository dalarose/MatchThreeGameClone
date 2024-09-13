using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Custom class to accomodate useful stuff for our shapes array
/// </summary>
public class ShapesArray
{
    private GameObject _backupG1;
    private GameObject _backupG2;
    private GameObject[,] _shapes = new GameObject[Constants.ROWS, Constants.COLUMNS];

    /// <summary>
    /// Indexer
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public GameObject this[int row, int column]
    {
        get
        {
            try
            {
                return _shapes[row, column];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        set
        {
            _shapes[row, column] = value;
        }
    }

    /// <summary>
    /// Swaps the position of two items, also keeping a backup
    /// </summary>
    /// <param name="g1"></param>
    /// <param name="g2"></param>
    public void Swap(GameObject g1, GameObject g2)
    {
        //hold a backup in case no match is produced
        _backupG1 = g1;
        _backupG2 = g2;

        var g1Shape = g1.GetComponent<Shape>();
        var g2Shape = g2.GetComponent<Shape>();

        //get array indexes
        var g1Row = g1Shape.Row;
        var g1Column = g1Shape.Column;
        var g2Row = g2Shape.Row;
        var g2Column = g2Shape.Column;

        //swap them in the array
        var temp = _shapes[g1Row, g1Column];
        _shapes[g1Row, g1Column] = _shapes[g2Row, g2Column];
        _shapes[g2Row, g2Column] = temp;

        //swap their respective properties
        Shape.SwapColumnRow(g1Shape, g2Shape);
    }

    /// <summary>
    /// Undoes the swap
    /// </summary>
    public void UndoSwap()
    {
        if (_backupG1 == null || _backupG2 == null)
        {
            throw new Exception("Backup is null");
        }

        Swap(_backupG1, _backupG2);
    }

    /// <summary>
    /// Returns the matches found for a list of GameObjects
    /// MatchesInfo class is not used as this method is called on subsequent collapses/checks, 
    /// not the one inflicted by user's drag
    /// </summary>
    /// <param name="gos"></param>
    /// <returns></returns>
    public IEnumerable<GameObject> GetMatches(IEnumerable<GameObject> gos)
    {
        var matches = new List<GameObject>();
        foreach (var go in gos)
        {
            matches.AddRange(GetMatches(go).MatchedCandy);
        }

        return matches.Distinct();
    }

    /// <summary>
    /// Returns the matches found for a single GameObject
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public MatchesInfo GetMatches(GameObject go)
    {
        var matchesInfo = new MatchesInfo();

        var horizontalMatches = GetMatchesHorizontally(go);
        if (ContainsDestroyRowColumnBonus(horizontalMatches))
        {
            horizontalMatches = GetEntireRow(go);
            if (!BonusTypeUtilities.ContainsDestroyWholeRowColumn(matchesInfo.BonusesContained))
            {
                matchesInfo.BonusesContained |= BonusType.DestroyWholeRowColumn;
            }
        }

        matchesInfo.AddObjectRange(horizontalMatches);

        var verticalMatches = GetMatchesVertically(go);
        if (ContainsDestroyRowColumnBonus(verticalMatches))
        {
            verticalMatches = GetEntireColumn(go);
            if (!BonusTypeUtilities.ContainsDestroyWholeRowColumn(matchesInfo.BonusesContained))
            {
                matchesInfo.BonusesContained |= BonusType.DestroyWholeRowColumn;
            }
        }

        matchesInfo.AddObjectRange(verticalMatches);

        return matchesInfo;
    }

    /// <summary>
    /// Removes (sets as null) an item from the array
    /// </summary>
    /// <param name="item"></param>
    public void Remove(GameObject item)
    {
        _shapes[item.GetComponent<Shape>().Row, item.GetComponent<Shape>().Column] = null;
    }

    /// <summary>
    /// Collapses the array on the specific columns, after checking for empty items on them
    /// </summary>
    /// <param name="columns"></param>
    /// <returns>Info about the GameObjects that were moved</returns>
    public AlteredCandyInfo Collapse(IEnumerable<int> columns)
    {
        var collapseInfo = new AlteredCandyInfo();

        ///search in every column
        foreach (var column in columns)
        {
            //begin from bottom row
            for (var row = 0; row < Constants.ROWS - 1; row++)
            {
                //if you find a null item
                if (_shapes[row, column] == null)
                {
                    //start searching for the first non-null
                    for (var row2 = row + 1; row2 < Constants.ROWS; row2++)
                    {
                        if (_shapes[row2, column] == null)
                        {
                            continue;
                        }

                        //if you find one, bring it down (i.e. replace it with the null you found)
                        _shapes[row, column] = _shapes[row2, column];
                        _shapes[row2, column] = null;

                        //calculate the biggest distance
                        if (row2 - row > collapseInfo.MaxDistance) 
                            collapseInfo.MaxDistance = row2 - row;

                        //assign new row and column (name does not change)
                        _shapes[row, column].GetComponent<Shape>().Row = row;
                        _shapes[row, column].GetComponent<Shape>().Column = column;

                        collapseInfo.AddCandy(_shapes[row, column]);
                        break;
                    }
                }
            }
        }

        return collapseInfo;
    }

    /// <summary>
    /// Searches the specific column and returns info about null items
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public IEnumerable<ShapeInfo> GetEmptyItemsOnColumn(int column)
    {
        var emptyItems = new List<ShapeInfo>();
        for (var row = 0; row < Constants.ROWS; row++)
        {
            if (_shapes[row, column] == null)
            {
                emptyItems.Add(new ShapeInfo() { Row = row, Column = column });
            }
        }

        return emptyItems;
    }

    private bool ContainsDestroyRowColumnBonus(IEnumerable<GameObject> matches)
    {
        if (matches.Count() >= Constants.MINIMUM_MATCHES)
        {
            foreach (var go in matches)
            {
                if (BonusTypeUtilities.ContainsDestroyWholeRowColumn(go.GetComponent<Shape>().Bonus))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private IEnumerable<GameObject> GetEntireRow(GameObject go)
    {
        var matches = new List<GameObject>();
        var row = go.GetComponent<Shape>().Row;
        for (var column = 0; column < Constants.COLUMNS; column++)
        {
            matches.Add(_shapes[row, column]);
        }

        return matches;
    }

    private IEnumerable<GameObject> GetEntireColumn(GameObject go)
    {
        var matches = new List<GameObject>();
        var column = go.GetComponent<Shape>().Column;
        for (var row = 0; row < Constants.ROWS; row++)
        {
            matches.Add(_shapes[row, column]);
        }

        return matches;
    }

    /// <summary>
    /// Searches horizontally for matches
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private IEnumerable<GameObject> GetMatchesHorizontally(GameObject go)
    {
        var matches = new List<GameObject>();
        matches.Add(go);
        var shape = go.GetComponent<Shape>();

        // check left
        if (shape.Column != 0)
            for (var column = shape.Column - 1; column >= 0; column--)
            {
                if (_shapes[shape.Row, column].GetComponent<Shape>().IsSameType(shape))
                {
                    matches.Add(_shapes[shape.Row, column]);
                }
                else
                {
                    break;
                }
            }

        // check right
        if (shape.Column != Constants.COLUMNS - 1)
            for (var column = shape.Column + 1; column < Constants.COLUMNS; column++)
            {
                if (_shapes[shape.Row, column].GetComponent<Shape>().IsSameType(shape))
                {
                    matches.Add(_shapes[shape.Row, column]);
                }
                else
                {
                    break;
                }
            }

        // we want more than three matches
        if (matches.Count < Constants.MINIMUM_MATCHES)
        {
            matches.Clear();
        }

        return matches.Distinct();
    }

    /// <summary>
    /// Searches vertically for matches
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private IEnumerable<GameObject> GetMatchesVertically(GameObject go)
    {
        var matches = new List<GameObject>();
        matches.Add(go);
        var shape = go.GetComponent<Shape>();

        // check bottom
        if (shape.Row != 0)
            for (var row = shape.Row - 1; row >= 0; row--)
            {
                if (_shapes[row, shape.Column] != null &&
                    _shapes[row, shape.Column].GetComponent<Shape>().IsSameType(shape))
                {
                    matches.Add(_shapes[row, shape.Column]);
                }
                else
                {
                    break;
                }
            }

        // check top
        if (shape.Row != Constants.ROWS - 1)
            for (var row = shape.Row + 1; row < Constants.ROWS; row++)
            {
                if (_shapes[row, shape.Column] != null && 
                    _shapes[row, shape.Column].GetComponent<Shape>().IsSameType(shape))
                {
                    matches.Add(_shapes[row, shape.Column]);
                }
                else
                {
                    break;
                }
            }

        if (matches.Count < Constants.MINIMUM_MATCHES)
        {
            matches.Clear();
        }

        return matches.Distinct();
    }
}

