using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float ZOOM_COEFFICIENT = 0.01f;

    [SerializeField] private float speed;
    [SerializeField] private float minSize;
    [SerializeField] private float maxSize;
    [SerializeField] private Vector2 minCameraPosition;
    [SerializeField] private Vector2 maxCameraPosition;

    private Vector3 _startPosition;
    private Vector3 _direction;
    private Camera _camera;
    private Transform _transform;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        _transform = GetComponent<Transform>();
    }
    private void Update()
    {
        if (Input.touchCount == 2)
        {
            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            var touchZeroLastPosition = touchZero.position - touchZero.deltaPosition;
            var touchOneLastPosition = touchOne.position - touchOne.deltaPosition;

            var distanceTouch = (touchZeroLastPosition - touchOneLastPosition).magnitude;
            var currentDistanceTouch = (touchZero.position - touchOne.position).magnitude;
            var difference = currentDistanceTouch - distanceTouch;
            Zoom(difference * ZOOM_COEFFICIENT);
        }

        if (Input.GetMouseButtonDown(0)) 
        { 
            _startPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            _direction = _startPosition - _camera.ScreenToWorldPoint(Input.mousePosition);
            _transform.position = new Vector3(Mathf.Clamp(_transform.position.x +_direction.x , minCameraPosition.x, maxCameraPosition.x),
                                         _transform.position.y,
                                         Mathf.Clamp(_transform.position.z + _direction.z, minCameraPosition.y, maxCameraPosition.y));
        }
    }

    private void Zoom(float increment)
    {
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - increment, minSize, maxSize);
    }
}
