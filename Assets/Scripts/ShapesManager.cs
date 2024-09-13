using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

// candy graphics taken from http://opengameart.org/content/candy-pack-1
public class ShapesManager : MonoBehaviour
{
    public Text DebugText, ScoreText;
    public GameObject[] CandyPrefabs;
    public GameObject[] ExplosionPrefabs;
    public GameObject[] BonusPrefabs;
    public SoundManager soundManager;
    
    public bool ShowDebugInfo = false;
    
    private readonly Vector2 _bottomRight = new Vector2(-2.37f, -4.27f);
    private readonly Vector2 _candySize = new Vector2(0.7f, 0.7f);

    private int _score;
    private ShapesArray _shapes;
    private GameState _gameState = GameState.None;
    private GameObject _hitGo = null;
    private Vector2[] _spawnPositions;

    private IEnumerator _checkPotentialMatchesCoroutine;
    private IEnumerator _animatePotentialMatchesCoroutine;
    private IEnumerable<GameObject> _potentialMatches;

    private void Awake()
    {
        DebugText.enabled = ShowDebugInfo;
    }

    // Use this for initialization
    private void Start()
    {
        InitializeTypesOnPrefabShapesAndBonuses();
        InitializeCandyAndSpawnPositions();
        StartCheckForPotentialMatches();
    }

    private void Update()
    {
        if (ShowDebugInfo)
        {
            DebugText.text = DebugUtilities.GetArrayContents(_shapes);
        }

        if (_gameState == GameState.None)
        {
            // user has clicked or touched
            if (Input.GetMouseButtonDown(0))
            {
                // get the hit position
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    // we have a hit!!!
                    _hitGo = hit.collider.gameObject;
                    _gameState = GameState.SelectionStarted;
                }
            }
        }
        else if (_gameState == GameState.SelectionStarted)
        {
            // user dragged
            if (Input.GetMouseButton(0))
            {
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && _hitGo != hit.collider.gameObject)
                {
                    // user did a hit, no need to show him hints 
                    StopCheckForPotentialMatches();

                    // if the two shapes are diagonally aligned (different row and column), just return
                    if (!Utilities.AreVerticalOrHorizontalNeighbors(
                            _hitGo.GetComponent<Shape>(),
                            hit.collider.gameObject.GetComponent<Shape>())
                        )
                    {
                        _gameState = GameState.None;
                    }
                    else
                    {
                        _gameState = GameState.Animating;
                        FixSortingLayer(_hitGo, hit.collider.gameObject);
                        StartCoroutine(FindMatchesAndCollapse(hit));
                    }
                }
            }
        }
    }

    public void InitializeCandyAndSpawnPositionsFromPremadeLevel()
    {
        InitializeVariables();

        var premadeLevel = DebugUtilities.FillShapesArrayFromResourcesData();

        if (_shapes != null)
        {
            DestroyAllCandy();
        }

        _shapes = new ShapesArray();
        _spawnPositions = new Vector2[Constants.COLUMNS];

        for (var row = 0; row < Constants.ROWS; row++)
        {
            for (var column = 0; column < Constants.COLUMNS; column++)
            {
                var newCandy = GetSpecificCandyOrBonusForPremadeLevel(premadeLevel[row, column]);
                InstantiateAndPlaceNewCandy(row, column, newCandy);
            }
        }

        SetupSpawnPositions();
    }

    public void InitializeCandyAndSpawnPositions()
    {
        InitializeVariables();

        if (_shapes != null)
        {
            DestroyAllCandy();
        }

        _shapes = new ShapesArray();
        _spawnPositions = new Vector2[Constants.COLUMNS];

        for (var row = 0; row < Constants.ROWS; row++)
        {
            for (var column = 0; column < Constants.COLUMNS; column++)
            {
                var newCandy = GetRandomCandy();

                // check if two previous horizontal are of the same type
                while (
                    column >= 2 &&
                    _shapes[row, column - 1].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()) &&
                    _shapes[row, column - 2].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }

                // check if two previous vertical are of the same type
                while (
                    row >= 2 &&
                    _shapes[row - 1, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()) &&
                    _shapes[row - 2, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }

                InstantiateAndPlaceNewCandy(row, column, newCandy);
            }
        }

        SetupSpawnPositions();
    }

    
    /// <summary>
    /// Initialize shapes
    /// </summary>
    private void InitializeTypesOnPrefabShapesAndBonuses()
    {
        // just assign the name of the prefab
        foreach (var item in CandyPrefabs)
        {
            item.GetComponent<Shape>().Type = item.name;
        }

        // assign the name of the respective "normal" candy as the type of the Bonus
        foreach (var item in BonusPrefabs)
        {
            item.GetComponent<Shape>().Type = 
                CandyPrefabs
                    .Where(
                        x => 
                            x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim()))
                    .Single()
                    .name;
        }
    }

    private void InstantiateAndPlaceNewCandy(int row, int column, GameObject newCandy)
    {
        var go = Instantiate(
            newCandy,
            _bottomRight + new Vector2(column * _candySize.x, row * _candySize.y),
            Quaternion.identity);

        // assign the specific properties
        go.GetComponent<Shape>().Assign(newCandy.GetComponent<Shape>().Type, row, column);
        _shapes[row, column] = go;
    }

    private void SetupSpawnPositions()
    {
        // create the spawn positions for the new shapes (will pop from the 'ceiling')
        for (var column = 0; column < Constants.COLUMNS; column++)
        {
            _spawnPositions[column] =
                _bottomRight + new Vector2(column * _candySize.x, Constants.ROWS * _candySize.y);
        }
    }

    /// <summary>
    /// Destroy all candy gameobjects
    /// </summary>
    private void DestroyAllCandy()
    {
        for (var row = 0; row < Constants.ROWS; row++)
        {
            for (var column = 0; column < Constants.COLUMNS; column++)
            {
                Destroy(_shapes[row, column]);
            }
        }
    }

    /// <summary>
    /// Modifies sorting layers for better appearance when dragging/animating
    /// </summary>
    /// <param name="hitGo"></param>
    /// <param name="hitGo2"></param>
    private void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
    {
        var sp1 = hitGo.GetComponent<SpriteRenderer>();
        var sp2 = hitGo2.GetComponent<SpriteRenderer>();
        if (sp1.sortingOrder <= sp2.sortingOrder)
        {
            sp1.sortingOrder = 1;
            sp2.sortingOrder = 0;
        }
    }

    private IEnumerator FindMatchesAndCollapse(RaycastHit2D hit2)
    {
        // get the second item that was part of the swipe
        var hitGo2 = hit2.collider.gameObject;
        _shapes.Swap(_hitGo, hitGo2);

        // move the swapped ones
        _hitGo.transform.DOMove(hitGo2.transform.position, Constants.ANIMATION_DURATION);
        hitGo2.transform.DOMove(_hitGo.transform.position, Constants.ANIMATION_DURATION);
        yield return new WaitForSeconds(Constants.ANIMATION_DURATION);

        // get the matches via the helper methods
        var hitGomatchesInfo = _shapes.GetMatches(_hitGo);
        var hitGo2matchesInfo = _shapes.GetMatches(hitGo2);

        var totalMatches = hitGomatchesInfo.MatchedCandy
            .Union(hitGo2matchesInfo.MatchedCandy).Distinct();

        // if user's swap didn't create at least a 3-match, undo their swap
        if (totalMatches.Count() < Constants.MINIMUM_MATCHES)
        {
            _hitGo.transform.DOMove(hitGo2.transform.position, Constants.ANIMATION_DURATION);
            hitGo2.transform.DOMove(_hitGo.transform.position, Constants.ANIMATION_DURATION);
            yield return new WaitForSeconds(Constants.ANIMATION_DURATION);

            _shapes.UndoSwap();
        }

        // if more than 3 matches and no Bonus is contained in the line, we will award a new Bonus
        var addBonus = totalMatches.Count() >= Constants.MINIMUM_MATCHES_FOR_BONUS &&
            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGomatchesInfo.BonusesContained) &&
            !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGo2matchesInfo.BonusesContained);

        Shape hitGoCache = null;
        if (addBonus)
        {
            // get the game object that was of the same type
            var sameTypeGo = hitGomatchesInfo.MatchedCandy.Count() > 0 ? _hitGo : hitGo2;
            hitGoCache = sameTypeGo.GetComponent<Shape>();
        }

        var timesRun = 1;
        while (totalMatches.Count() >= Constants.MINIMUM_MATCHES)
        {
            // increase score
            IncreaseScore((totalMatches.Count() - 2) * Constants.MATCH_THREE_SCORE);

            if (timesRun >= 2)
            {
                IncreaseScore(Constants.SUBSEQUENT_MATCH_SCORE);
            }

            soundManager.PlayCrinkleSfx();

            foreach (var item in totalMatches)
            {
                _shapes.Remove(item);
                RemoveFromScene(item);
            }

            // check and instantiate Bonus if needed
            if (addBonus)
            {
                CreateBonus(hitGoCache);
            }

            addBonus = false;

            // get the columns that we had a collapse
            var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

            // the order the 2 methods below get called is important!!!
            // collapse the ones gone
            var collapsedCandyInfo = _shapes.Collapse(columns);
            // create new ones
            var newCandyInfo = CreateNewCandyInSpecificColumns(columns);
            var maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

            MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
            MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);

            //will wait for both of the above animations
            yield return new WaitForSeconds(Constants.MOVE_ANIMATION_MIN_DURATION * maxDistance);

            //search if there are matches with the new/collapsed items
            totalMatches = _shapes
                .GetMatches(collapsedCandyInfo.AlteredCandy)
                .Union(_shapes.GetMatches(newCandyInfo.AlteredCandy))
                .Distinct();

            timesRun++;
        }

        _gameState = GameState.None;
        StartCheckForPotentialMatches();
    }

    /// <summary>
    /// Creates a new Bonus based on the shape parameter
    /// </summary>
    /// <param name="hitGoCache"></param>
    private void CreateBonus(Shape hitGoCache)
    {
        var bonus = Instantiate(GetBonusFromType(
                    hitGoCache.Type),
                _bottomRight + new Vector2(hitGoCache.Column * _candySize.x, hitGoCache.Row * _candySize.y),
                Quaternion.identity);
        _shapes[hitGoCache.Row, hitGoCache.Column] = bonus;
        var bonusShape = bonus.GetComponent<Shape>();
        // will have the same type as the "normal" candy
        bonusShape.Assign(hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
        // add the proper Bonus type
        bonusShape.Bonus |= BonusType.DestroyWholeRowColumn;
    }

    /// <summary>
    /// Spawns new candy in columns that have missing ones
    /// </summary>
    /// <param name="columnsWithMissingCandy"></param>
    /// <returns>Info about new candies created</returns>
    private AlteredCandyInfo CreateNewCandyInSpecificColumns(IEnumerable<int> columnsWithMissingCandy)
    {
        var newCandyInfo = new AlteredCandyInfo();

        // find how many null values the column has
        foreach (var column in columnsWithMissingCandy)
        {
            var emptyItems = _shapes.GetEmptyItemsOnColumn(column);
            foreach (var item in emptyItems)
            {
                var go = GetRandomCandy();
                var newCandy = Instantiate(go, _spawnPositions[column], Quaternion.identity);

                newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);

                if (Constants.ROWS - item.Row > newCandyInfo.MaxDistance)
                {
                    newCandyInfo.MaxDistance = Constants.ROWS - item.Row;
                }

                _shapes[item.Row, item.Column] = newCandy;
                newCandyInfo.AddCandy(newCandy);
            }
        }
        return newCandyInfo;
    }

    /// <summary>
    /// Animates gameobjects to their new position
    /// </summary>
    /// <param name="movedGameObjects"></param>
    private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects, int distance)
    {
        foreach (var item in movedGameObjects)
        {
            item.transform
                .DOMove(
                _bottomRight + new Vector2(item.GetComponent<Shape>().Column * _candySize.x,
                    item.GetComponent<Shape>().Row * _candySize.y),
                Constants.MOVE_ANIMATION_MIN_DURATION * distance
            );
        }
    }

    /// <summary>
    /// Destroys the item from the scene and instantiates a new explosion gameobject
    /// </summary>
    /// <param name="item"></param>
    private void RemoveFromScene(GameObject item)
    {
        var explosion = GetRandomExplosion();
        var newExplosion = Instantiate(explosion, item.transform.position, Quaternion.identity);
        Destroy(newExplosion, Constants.EXPLOSION_DURATION);
        Destroy(item);
    }

    /// <summary>
    /// Get a random candy
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomCandy()
    {
        return CandyPrefabs[Random.Range(0, CandyPrefabs.Length)];
    }

    private void InitializeVariables()
    {
        _score = 0;
        ShowScore();
    }

    private void IncreaseScore(int amount)
    {
        _score += amount;
        ShowScore();
    }

    private void ShowScore()
    {
        ScoreText.text = $"Score: {_score}";
    }

    /// <summary>
    /// Get a random explosion
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomExplosion()
    {
        return ExplosionPrefabs[Random.Range(0, ExplosionPrefabs.Length)];
    }

    /// <summary>
    /// Gets the specified Bonus for the specific type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private GameObject GetBonusFromType(string type)
    {
        var color = type.Split('_')[1].Trim();
        foreach (var item in BonusPrefabs)
        {
            if (item.GetComponent<Shape>().Type.Contains(color))
            {
                return item;
            }
        }
        throw new System.Exception("Wrong type");
    }

    /// <summary>
    /// Starts the coroutines, keeping a reference to stop later
    /// </summary>
    private void StartCheckForPotentialMatches()
    {
        StopCheckForPotentialMatches();
        // get a reference to stop it later
        _checkPotentialMatchesCoroutine = CheckPotentialMatches();
        StartCoroutine(_checkPotentialMatchesCoroutine);
    }

    /// <summary>
    /// Stops the coroutines
    /// </summary>
    private void StopCheckForPotentialMatches()
    {
        if (_animatePotentialMatchesCoroutine != null)
        {
            StopCoroutine(_animatePotentialMatchesCoroutine);
        }

        if (_checkPotentialMatchesCoroutine != null)
        {
            StopCoroutine(_checkPotentialMatchesCoroutine);
        }

        ResetOpacityOnPotentialMatches();
    }

    /// <summary>
    /// Resets the opacity on potential matches (probably user dragged something?)
    /// </summary>
    private void ResetOpacityOnPotentialMatches()
    {
        if (_potentialMatches != null)
        {
            foreach (var item in _potentialMatches)
            {
                if (item == null)
                {
                    break;
                }

                var c = item.GetComponent<SpriteRenderer>().color;
                c.a = 1.0f;
                item.GetComponent<SpriteRenderer>().color = c;
            }
        }
    }

    /// <summary>
    /// Finds potential matches
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckPotentialMatches()
    {
        yield return new WaitForSeconds(Constants.WAIT_BEFORE_POTENTIAL_MATCHES_CHECK);
        _potentialMatches = Utilities.GetPotentialMatches(_shapes);
        if (_potentialMatches != null)
        {
            while (true)
            {
                _animatePotentialMatchesCoroutine = Utilities.AnimatePotentialMatches(_potentialMatches);
                StartCoroutine(_animatePotentialMatchesCoroutine);
                yield return new WaitForSeconds(Constants.WAIT_BEFORE_POTENTIAL_MATCHES_CHECK);
            }
        }
    }

    /// <summary>
    /// Gets a specific candy or Bonus based on the premade level information.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private GameObject GetSpecificCandyOrBonusForPremadeLevel(string info)
    {
        var tokens = info.Split('_');

        if (tokens.Count() == 1)
        {
            foreach (var item in CandyPrefabs)
            {
                if (item.GetComponent<Shape>().Type.Contains(tokens[0].Trim()))
                {
                    return item;
                }
            }

        }
        else if (tokens.Count() == 2 && tokens[1].Trim() == "B")
        {
            foreach (var item in BonusPrefabs)
            {
                if (item.name.Contains(tokens[0].Trim()))
                {
                    return item;
                }
            }
        }

        throw new System.Exception("Wrong type, check your premade level");
    }
}
