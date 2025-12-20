using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skyx.RuntimeEditor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EnumFilterAttribute : PropertyAttribute
    {
        public IEnumerable<object> list;
        public bool listIsValid;

        public EnumFilterAttribute(params object[] list)
        {
            this.list = list;
            listIsValid = true;
        }

        public EnumFilterAttribute(bool listIsValid, params object[] list)
        {
            this.list = list;
            this.listIsValid = listIsValid;
        }
    }
}