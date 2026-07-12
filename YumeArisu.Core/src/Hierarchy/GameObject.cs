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
    public bool ActiveInHierarchy => ActiveSelf && (Transform.Parent?.GameObject.ActiveInHierarchy ?? true);
    public Transform Transform { get; internal set; }
    public IReadOnlyList<Component> Components => _components;
    public bool IsDestroyed { get; private set; }
    public event Action<bool> OnActiveSelfChanged;

    private bool _active;
    private List<Component> _components;
    private static uint _nextID = 0;

    internal GameObject(Scene owner, string name = "")
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

    /// <summary>
    /// 게임오브젝트에 컴포넌트를 추가합니다.
    /// </summary>
    /// <typeparam name="T">추가할 컴포넌트의 타입</typeparam>
    /// <returns>추가한 컴포넌트</returns>
    public T AddComponent<T>() where T : Component, new()
    {
        T instance = new() { GameObject = this };
        instance.OnAttached();

        _components.Add(instance);
        return instance;
    }

    /// <summary>
    /// 타입으로 컴포넌트를 찾습니다.
    /// 반환된 컴포넌트는 가장 먼저 찾은 컴포넌트입니다. 
    /// </summary>
    /// <typeparam name="T">찾을 컴포넌트의 타입</typeparam>
    /// <returns>가장 먼저 찾은 컴포넌트</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T GetComponent<T>() where T : Component
    {
        if(typeof(T) == typeof(Component))
            throw new InvalidOperationException("선샌니 컴포넌트 타입을 정확히 입력해주세요.");

        foreach(var comp in _components)
        {
            if (comp is T match)
                return match;
        }

        return null;
    }

    /// <summary>
    /// 같은 타입의 여러 컴포넌트들을 찾습니다.
    /// </summary>
    /// <typeparam name="T">찾을 컴포넌트들의 타입</typeparam>
    /// <returns>해당 타입의 컴포넌트 리스트</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public List<T> GetComponents<T>() where T : Component
    {
        if(typeof(T) == typeof(Component))
            throw new InvalidOperationException("아리스는 이해할 수 없습니다! 어째서 Components 프로퍼티를 사용하지 않는건가요? 선샌니.");

        List<T> matches = new();
        foreach(var comp in _components)
        {
            if(comp is T match)
                matches.Add(match);
        }

        return matches;
    }

    /// <summary>
    /// 게임오브젝트에 컴포넌트를 제거합니다.
    /// </summary>
    /// <param name="component">제거할 컴포넌트</param>
    /// <exception cref="InvalidOperationException">제거할 수 없는 컴포넌트 혹은 존재하지 않는 컴포넌트</exception>
    public void RemoveComponent(Component component)
    {
        if (component is Transform)
            throw new InvalidOperationException("Transform은 제거할 수 없습니다.");

        if (!_components.Remove(component))
            throw new InvalidOperationException("제거할 컴포넌트가 존재하지 않습니다.");

        component.OnDetached();
    }

    internal void Destroy()
    {
        if(IsDestroyed)
            return;

        IsDestroyed = true;

        // 자식들도 재귀적으로 먼저 파괴 (자식 목록을 복사해서 순회)
        foreach (var child in Transform.Children.ToList())
            child.GameObject.Destroy();

        // 부모의 자식 목록에서 자기 자신을 제거
        Transform.SetParent(null);

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