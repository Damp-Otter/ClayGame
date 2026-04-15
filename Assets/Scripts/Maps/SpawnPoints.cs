using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPoints : MonoBehaviour
{
    private static SpawnPoints _singleton; public static SpawnPoints singleton { get { return _singleton; } }
    [SerializeField] private Transform[] _points = null;
    private int _lastIndex = 0;

    private void Awake()
    {
        _singleton = this;
    }

    public Transform GetPoint(int index)
    {
        if(_lastIndex < _points.Length)
        {
            return _points[index];
        }
        return null;
    }

    public Transform GetPointInOrder()
    {
        int point = _lastIndex;
        _lastIndex++;
        if (_lastIndex >= _points.Length)
        {
            _lastIndex = 0;
        }
        return GetPoint(point);
    }

}
