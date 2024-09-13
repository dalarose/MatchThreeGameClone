using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class AlteredCandyInfo
{
    public int MaxDistance { get; set; }

    private List<GameObject> _newCandy { get; set; }

    /// <summary>
    /// Returns distinct list of altered candy
    /// </summary>
    public IEnumerable<GameObject> AlteredCandy => _newCandy.Distinct();

    public AlteredCandyInfo()
    {
        _newCandy = new List<GameObject>();
    }

    public void AddCandy(GameObject go)
    {
        if (!_newCandy.Contains(go))
        {
            _newCandy.Add(go);
        }
    }
}
