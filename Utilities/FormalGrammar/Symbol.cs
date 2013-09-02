﻿using System.Collections.Generic;

namespace WallpaperGenerator.Utilities.FormalGrammar
{
    public class Symbol<T>
    {
        public string Name { get; private set; }

        public T Value { get; private set; }

        public bool IsTerminal { get; private set; }

        public Symbol(string name)
            : this(name, default(T), false)
        {
        }

        public Symbol(string name, T value)
            : this(name, value, true)
        {
        }

        private Symbol(string name, T value, bool isTerminal)
        {
            Name = name;
            Value = value;
            IsTerminal = isTerminal;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Symbol<T>) obj);
        }

        protected bool Equals(Symbol<T> other)
        {
            return string.Equals(Name, other.Name) && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
            }
        }
    }
}
