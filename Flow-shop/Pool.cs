public class Pool<T> where T : class, new() {

    T[] stack;
    int top = 0;

    public Pool(int capacity)
    {
        stack = new T[capacity];
    }

    public void Push(T item)
    {
        if (top < stack.Length) {
            stack[top++] = item;
        }
    }

    public T Pop()
    {
        if (top > 0) return stack[--top];
        else return new T ();
    }

    public T TryPop()
    {
        if (top > 0) return stack[--top];
        else return null;
    }
}
