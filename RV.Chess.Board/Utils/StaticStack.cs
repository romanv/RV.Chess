namespace RV.Chess.Board.Utils
{
    internal class StaticStack<T>
    {
        private readonly T[] _stack;
        private readonly int _size;
        private int _pos;

        internal StaticStack(int size)
        {
            _stack = new T[size];
            _size = size;
            _pos = -1;
        }

        public int Count => _pos + 1;

        internal void Push(T item)
        {
            _pos++;

            if (_pos > _size - 1)
            {
                throw new StackOverflowException();
            }

            _stack[_pos] = item;
        }

        internal T Pop()
        {
            if (_pos < 0)
            {
                throw new InvalidOperationException("Stack is empty");
            }

            return _stack[_pos--];
        }

        internal T Peek()
        {
            return _stack[_pos];
        }

        internal void Clear()
        {
            _pos = -1;
            Array.Clear(_stack);
        }
    }
}
