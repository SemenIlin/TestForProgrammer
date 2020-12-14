using EpPathFinding.cs;
using UnityEngine;
public class GameField : MonoBehaviour
{
    [SerializeField] private Vector2Int _sizeBoard;
    [SerializeField] private GameObject _elementOfBoardPrefab;

    private Transform _sizeElementOfBoard;

    private GameObject[,] _elementOfBoards;
    public static GameField Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }
    }

    public BaseGrid BaseGrid { get; set; }
    public Vector2Int SizeBoard => _sizeBoard;
    private void Start()
    {
        _sizeElementOfBoard = _elementOfBoardPrefab.GetComponent<Transform>();
        _elementOfBoards = new GameObject[_sizeBoard.x, _sizeBoard.y];

        CreatedBoard();
    }
    private void CreatedBoard()
    {
        BaseGrid = new StaticGrid(_sizeBoard.x, _sizeBoard.y);

        for (int x = 0; x < _sizeBoard.x; ++x)
        {
            for (int y = 0; y < _sizeBoard.y; ++y)
            {
                _elementOfBoards[x, y] = Instantiate(_elementOfBoardPrefab);
                _elementOfBoards[x, y].transform.SetParent(transform, false);
                _elementOfBoards[x, y].transform.localPosition = new Vector3(x * _sizeElementOfBoard.localScale.x,
                                                                             0f, y * _sizeElementOfBoard.localScale.z);

                BaseGrid.SetWalkableAt(x, y, true);
            }
        }
    }
}
