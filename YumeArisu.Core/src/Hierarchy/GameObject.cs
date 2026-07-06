namespace YumeArisu.Core.Hierarchy;

public sealed class GameObject : IDisposable
{
    public Scene Owner => _owner;
    public uint ID => _id;
    public string Name { get; set; }
    public bool ActiveSelf
    { 
        get => _active;
        set
        {
            if(_active == value) 
                return;
    
            _active = value;
            OnActiveSelfChanged?.Invoke(_active);
        } 
    }
    public IReadOnlyList<Component> Components => _components;

    public event Action<bool> OnActiveSelfChanged;
    
    private Scene _owner;
    private uint _id;
    private bool _active;
    private List<Component> _components;
    private static uint _nextID = 0;

    public GameObject(Scene owner, string name = "")
    {
        _owner = owner;
        _id = _nextID++;

        if (string.IsNullOrEmpty(name))
            name = $"GameObject[{_id}]";
            
        Name = name;
        ActiveSelf = true;
        _components = new();
    }

    public T AddComponent<T>() where T : Component, new()
    {
        if(typeof(T) == typeof(Component))
            throw new InvalidOperationException("아리스는 이해할 수 없었습니다... 선샌니가 무엇을 집어넣으려고 했던걸까요?");

        foreach(var comp in _components)
        {
            if (comp is T)
                throw new InvalidOperationException("같은 타입의 컴포넌트가 이미 존재합니다.");
        }

        T instance = new() { Owner = this };

        _components.Add(instance);
        return instance;
    }

    public T GetComponent<T>() where T : Component
    {
        if(typeof(T) == typeof(Component))
            throw new InvalidOperationException("선샌니 찾으시는 게 있으시면 타입을 정확히 입력해주세요.");

        foreach(var comp in _components)
        {
            if (comp is T match)
                return match;
        }

        return null;
    }

    public List<T> GetComponents<T>() where T : Component
    {
        if(typeof(T) == typeof(Component))
            throw new InvalidOperationException("아리스는 이해할 수 없습니다! 어째서 Copmonents 프로퍼티를 사용하지 않는건가요? 선샌니.");

        List<T> matches = new();
        foreach(var comp in _components)
        {
            if(comp is T match)
                matches.Add(match);
        }

        return matches;
    }

    public void RemoveComponent<T>() where T : Component
    {
        if(typeof(T) == typeof(Component))
            throw new InvalidOperationException("끄앙 이해할 수 없습니다! 무엇을 제거하려는 겁니까? 선샌니!");

        Component findComp = null;
        foreach(var comp in _components)
        {
            if (comp is T)
            {
                findComp = comp;
                break;
            }
        }

        if(findComp != null)
        {
            _components.Remove(findComp);
            findComp.Dispose();
        }
        else
            throw new InvalidOperationException("제거할 컴포넌트가 존재하지 않습니다.");
    }

    public void RemoveComponent(Component component)
    {
        if (!_components.Remove(component))
            throw new InvalidOperationException("제거할 컴포넌트가 존재하지 않습니다.");

        component.Dispose();
    }

    public void Dispose()
    {
        foreach(var comp in _components)
            comp.Dispose();
        _components.Clear();
    }

    public override int GetHashCode()
    {
        return _id.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is GameObject other && _id == other._id;
    }
}