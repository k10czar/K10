using System;

namespace Skyx.SkyxEditor
{
    public interface ILayoutScope : IDisposable
    {
        public bool IsExpanded { get; }
    }
}