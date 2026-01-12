using System;

namespace Skyx.RuntimeEditor
{
    public class FieldDrawInfo
    {
        public readonly string fieldName;
        public readonly Type requestedType;
        public readonly EColor color;
        public readonly string hint;

        public FieldDrawInfo(string fieldName, int index)
        {
            this.fieldName = fieldName;
            this.color = (EColor) index;
            this.requestedType = null;
            this.hint = "";
        }

        public FieldDrawInfo(string fieldName, Type requestedType, EColor color, string hint)
        {
            this.fieldName = fieldName;
            this.requestedType = requestedType;
            this.color = color;
            this.hint = hint;
        }
    }
}