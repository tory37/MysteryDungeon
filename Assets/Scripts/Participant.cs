using UnityEngine;
using System.Collections;

public class Participant : MonoBehaviour
{
    #region Editor Interface

    [SerializeField] private int speed;

    #endregion

    #region Public Interface

    public int DetermineInitiative()
    {
        return Random.Range(1, 20) + Mathf.FloorToInt(speed / 10);
    }

    #endregion
}
