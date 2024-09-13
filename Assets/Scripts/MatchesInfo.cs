using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MatchesInfo
{
    public BonusType BonusesContained { get; set; }

    /// <summary>
    /// Returns distinct list of matched candy
    /// </summary>
    public IEnumerable<GameObject> MatchedCandy
    {
        get
        {
            return _matchedCandies.Distinct();
        }
    }

    private List<GameObject> _matchedCandies;
    
    public MatchesInfo()
    {
        _matchedCandies = new List<GameObject>();
        BonusesContained = BonusType.None;
    }

    public void AddObjectRange(IEnumerable<GameObject> gos)
    {
        foreach (var item in gos)
        {
            AddObject(item);
        }
    }

    private void AddObject(GameObject go)
    {
        if (!_matchedCandies.Contains(go))
            _matchedCandies.Add(go);
    }
}

