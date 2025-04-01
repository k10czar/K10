using System;
using System.Linq;
using UnityEngine;

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

public class SingleLineDrawer : PropertyAttribute
{
    public readonly FieldDrawInfo[] drawInfos;

    public SingleLineDrawer(int infoCount, params object[] infos)
    {
        var totalInfo = infos.Length / infoCount;
        drawInfos = new FieldDrawInfo[totalInfo];

        var insertIndex = 0;
        for (var i = 0; i < infos.Length; i += infoCount, insertIndex++)
        {
            var name = (string) infos[i];
            Type type = null;
            EColor color = (EColor) insertIndex;
            string hint = "";

            var info1 = infos[i + 1];
            if (info1 is string hint1) hint = hint1;
            else if (info1 is Type type1) type = type1;
            else if (info1 is EColor color1) color = color1;

            if (infoCount > 2)
            {
                var info2 = infos[i + 2];
                if (info2 is string hint2) hint = hint2;
                else if (info2 is Type type2) type = type2;
                else if (info2 is EColor color2) color = color2;
            }

            if (infoCount > 3)
            {
                var info3 = infos[i + 3];
                if (info3 is string hint3) hint = hint3;
                else if (info3 is Type type3) type = type3;
                else if (info3 is EColor color3) color = color3;
            }

            drawInfos[insertIndex] = new FieldDrawInfo(name, type, color, hint);
        }
    }

    public SingleLineDrawer(params string[] fieldNames)
    {
        drawInfos = fieldNames.Select((entry, index) => new FieldDrawInfo(entry, index)).ToArray();
    }
}