namespace YumeArisu.Core.Utility;

public readonly struct ReadOnlyListView<T>
{
    private readonly List<T> _list;

    public ReadOnlyListView(List<T> list) => _list = list;

    public int Count => _list?.Count ?? 0;
    public T this[int index] => _list[index];

    // List<T>.Enumerator는 struct라서 foreach가 이걸 그대로 쓰면 boxing 없음
    public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

    public static implicit operator ReadOnlyListView<T>(List<T> list) => new(list);
}