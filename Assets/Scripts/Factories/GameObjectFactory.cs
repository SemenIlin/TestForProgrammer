using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameObjectFactory : ScriptableObject
{
    private Scene _scene;

    protected T CreateGameObjectInstance<T>(T prefab) where T : Player
    {
        if (!_scene.isLoaded)
        {
            if (Application.isEditor)
            {
                _scene = SceneManager.GetSceneByName(name);
                if (!_scene.isLoaded)
                {
                    _scene = SceneManager.CreateScene(name);
                }
            }
            else
            {
                _scene = SceneManager.CreateScene(name);
            }
        }

        T instance = Instantiate(prefab);

        instance.Transform = instance.GetComponent<Transform>();
        var _vector2 = new Vector2(instance.Transform.position.x, instance.Transform.position.z);
        instance.Magnitude = (_vector2 - Vector2.zero).magnitude;

        SceneManager.MoveGameObjectToScene(instance.gameObject, _scene);
        return instance;
    }
}