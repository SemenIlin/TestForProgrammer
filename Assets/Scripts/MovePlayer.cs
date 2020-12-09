
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    private Transform _transform;
    private Vector3 _rightDirection;
    private Vector3 _upDirection;
    private void Start()
    {
        _transform = GetComponent<Transform>();
        _rightDirection = new Vector3(0.3f, 0, 0);
        _upDirection = new Vector3(0, 0, 0.3f);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.RightArrow) && _transform.position.x < 79)
        {
            _transform.position += _rightDirection;
            Debug.Log(_transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && _transform.position.x > 0.001)
        {
            _transform.position -= _rightDirection;
            Debug.Log(_transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && _transform.position.z < 79)
        {
            _transform.position += _upDirection;
            Debug.Log(_transform.position);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _transform.position.z > 0.001)
        {
            _transform.position -= _upDirection;
            Debug.Log(_transform.position);
        }
    }
}
