using System;
using System.Linq;
using Skyx.RuntimeEditor;
using Skyx.Trees;
using Object = UnityEngine.Object;

namespace Rogue.Explorer
{
    public interface IExplorerSearchFilter<in T> where T : Object
    {
        public bool Invert { get; }

        public bool FitsFilter(T candidate)
        {
            var result = IsValid(candidate);
            return Invert ? !result : result;
        }

        public bool IsValid(T candidate);
    }

    [Serializable]
    public abstract class EnumSourcedFilter<T> : ExplorerFilter where T: Enum
    {
        public T value;
        public override FieldDrawInfo DrawInfo { get; } = new("value", typeof(T), EColor.Secondary, "");
    }

    [Serializable]
    public abstract class DoubleEnumSourcedFilter<T, U> : ExplorerFilter where T: Enum
    {
        public T value;
        public U value2;

        public override FieldDrawInfo DrawInfo { get; } = new("value", typeof(T), EColor.Secondary, "");
        public override FieldDrawInfo SecondaryDrawInfo { get; } = new("value2", typeof(U), EColor.Info, "");
    }

    [Serializable]
    public abstract class EnumMaskSourcedFilter<T> : ExplorerFilter where T: Enum
    {
        public T value;
        public bool mustHaveAll;
        public override FieldDrawInfo DrawInfo { get; } = new("value", typeof(T), EColor.Secondary, "");
    }

    [Serializable]
    public abstract class StringSourcedFilter : ExplorerFilter
    {
        public string value = string.Empty;
        public override FieldDrawInfo DrawInfo { get; } = new("value", typeof(string), EColor.Secondary, "");
    }

    [Serializable]
    public abstract class BoolSourcedFilter : ExplorerFilter
    {
        public bool value = true;
        public override FieldDrawInfo DrawInfo { get; } = new("value", typeof(bool), EColor.Secondary, "");
    }

    [Serializable]
    public abstract class ObjectSourcedFilter<T> : ExplorerFilter where T: Object
    {
        public T value;
        public override FieldDrawInfo DrawInfo { get; } = new("value", typeof(T), EColor.Secondary, "");
    }

    [Serializable]
    public abstract class TypeSourcedFilter : ExplorerFilter
    {
        public abstract Type TargetType { get; }
        public string value;
        public override FieldDrawInfo DrawInfo { get; } = null;
    }

    [Serializable]
    public abstract class ExplorerFilter
    {
        private string name;
        public string Name
        {
            get
            {
                if (name != null) return name;

                var classTree = GetType().GetCustomAttributes(typeof(ClassTreeAttribute), false).FirstOrDefault() as ClassTreeAttribute;
                name = classTree?.displayName ?? "MISSING CLASS TREE";

                return name;
            }
        }

        public virtual FieldDrawInfo DrawInfo => null;
        public virtual FieldDrawInfo SecondaryDrawInfo => null;

        public bool Invert => invert;
        public bool invert;
    }
}