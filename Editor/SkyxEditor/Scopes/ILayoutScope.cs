using System;

namespace Rogue.REditor
{
    public interface ILayoutScope : IDisposable
    {
        public bool IsExpanded { get; }
    }
}