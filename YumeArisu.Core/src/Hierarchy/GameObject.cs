namespace YumeArisu.Core.Hierarchy;

public sealed class GameObject
{
    public Scene Scene { get; private set; }
    public uint ID { get; private set; }
    public uint Layer { get; set; }
    public string Tag { get; set; }
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
    public bool IsDestroyed { get; private set; }
    public event Action<bool> OnActiveSelfChanged;

    private bool _active;
    private List<Component> _components;
    private static uint _nextID = 0;

    public GameObject(Scene owner, string name = "")
    {
        Layer = uint.MaxValue;
        Tag = "";
        Scene = owner;
        ID = _nextID++;

        if (string.IsNullOrEmpty(name))
            name = $"GameObject[{ID}]";
            
        Name = name;
        ActiveSelf = false;
        _components = new();
    }

    public T AddComponent<T>() where T : Component, new()
    {
        if(GetComponent<T>() != null)
            throw new InvalidOperationException("같은 타입의 컴포넌트가 이미 존재합니다.");

        T instance = new() { GameObject = this };
        instance.OnAttached();

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

        var found = GetComponent<T>();
        if (found is null)
            throw new InvalidOperationException("제거할 컴포넌트가 존재하지 않습니다.");

        RemoveComponent(found);
    }

    public void RemoveComponent(Component component)
    {
        if (!_components.Remove(component))
            throw new InvalidOperationException("제거할 컴포넌트가 존재하지 않습니다.");

        component.OnDetached();
    }

    internal void Destroy()
    {
        if(IsDestroyed)
            return;

        IsDestroyed = true;

        foreach(var comp in _components)
            comp.OnDetached();

        _components.Clear();
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is GameObject other && ID == other.ID;
    }
}