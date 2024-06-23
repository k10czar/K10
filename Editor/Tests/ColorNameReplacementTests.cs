using NUnit.Framework;

public class ColorNameReplacementTests 
{ 
    [Test]
    public void ColorReplacementLogUnchanged1()
    {
        Assert.AreEqual( K10Log.ReplaceColorsNames( "asdas<>ureotrn" ), "asdas<>ureotrn" );
    }
    
    [Test]
    public void ColorReplacementLogUnchanged2()
    {
        Assert.AreEqual( K10Log.ReplaceColorsNames( "asdas<>ureotrn<" ), "asdas<>ureotrn<" );
    }
    
    [Test]
    public void ColorReplacementLogUnchanged3()
    {
        Assert.AreEqual( K10Log.ReplaceColorsNames( "asdas<>ureotrn<>" ), "asdas<>ureotrn<>" );
    }
    
    [Test]
    public void ColorNameReplacementDoesNotWorkWithCompleteTagOpen()
    {
        Assert.AreEqual( K10Log.ReplaceColorsNames( "asdas<color=red" ), "asdas<color=red" );
    }
    
    [Test]
    public void CustomColorNameReplacementDoesNotWorkWithCompleteTagOpen()
    {
        Assert.AreEqual( K10Log.ReplaceColorsNames( "asdas<color=ArtichokeGreen" ), "asdas<color=ArtichokeGreen" );
    }
    
    [Test]
    public void CustomColorNameReplacementLogShouldWorkWithCompleteTagOpen()
    {
        Assert.AreEqual( $"asdas<color={Colors.LightSalmon.ToHexRGB()}>", K10Log.ReplaceColorsNames( $"asdas<color={nameof(Colors.LightSalmon)}>" ) );
    }
    
    [Test]
    public void CustomColorNameReplacementLogShouldIgnoreCaseUpper()
    {
        Assert.AreEqual( $"asdas<color={Colors.LightSalmon.ToHexRGB()}>", K10Log.ReplaceColorsNames( $"asdas<color={nameof(Colors.LightSalmon).ToUpper()}>" ) );
    }
    
    [Test]
    public void CustomColorNameReplacementLogShouldIgnoreCaseLower()
    {
        Assert.AreEqual( $"asdas<color={Colors.LightSalmon.ToHexRGB()}>", K10Log.ReplaceColorsNames( $"asdas<color={nameof(Colors.LightSalmon).ToLower()}>" ) );
    }
    
    [Test]
    public void CustomColorNameReplacementLogShouldWorkWithMultipleTags()
    {
        Assert.AreEqual( $"<color={Colors.LightSalmon.ToHexRGB()}>asdas</color><color={Colors.Crimson.ToHexRGB()}>Hello</color>", K10Log.ReplaceColorsNames( $"<color={nameof(Colors.LightSalmon)}>asdas</color><color={nameof(Colors.Crimson)}>Hello</color>" ) );
    }
    
    [Test]
    public void CustomColorNameReplacementLogShouldWorkWithMultipleNestedTags()
    {
        Assert.AreEqual( $"<color={Colors.LightSalmon.ToHexRGB()}>asdas<color={Colors.Crimson.ToHexRGB()}>Hello</color></color>", K10Log.ReplaceColorsNames( $"<color={nameof(Colors.LightSalmon)}>asdas<color={nameof(Colors.Crimson)}>Hello</color></color>" ) );
    }
    
    [Test]
    public void CustomColorNameReplacementLogShouldWorkWithMultipleNestedSubsequentTags()
    {
        Assert.AreEqual( $"<color={Colors.LightSalmon.ToHexRGB()}><color={Colors.Crimson.ToHexRGB()}>asdas</color>Hello</color>", K10Log.ReplaceColorsNames( $"<color={nameof(Colors.LightSalmon)}><color={nameof(Colors.Crimson)}>asdas</color>Hello</color>" ) );
    }

    [Test]
    public void CustomColorNameReplacementLogShouldWorkWithMultipleTagsOnOneLine()
    {
        Assert.AreEqual( $"<color={Colors.LightSalmon.ToHexRGB()}>asdas</color><color={Colors.Crimson.ToHexRGB()}>Hello</color>", K10Log.ReplaceColorsNames( $"<color={nameof(Colors.LightSalmon)}>asdas</color><color={nameof(Colors.Crimson)}>Hello</color>" ) );
    }
    
    [Test]
    public void CustomColorNameReplacementLogShouldWorkWithMultipleTagsOnOneLineWithSpaces()
    {
        Assert.AreEqual( $"<color={Colors.LightSalmon.ToHexRGB()}> asdas </color><color={Colors.Crimson.ToHexRGB()}> Hello </color>", K10Log.ReplaceColorsNames( $"<color={nameof(Colors.LightSalmon)}> asdas </color><color={nameof(Colors.Crimson)}> Hello </color>" ) );
    }
    
    [Test]
    public void CustomColorNameReplacementLogShouldWorkWithMultipleTagsOnOneLineWithSpacesAndNewLine()
    {
        Assert.AreEqual( $"<color={Colors.LightSalmon.ToHexRGB()}> asdas \n</color><color={Colors.Crimson.ToHexRGB()}> Hello </color>", K10Log.ReplaceColorsNames( $"<color={nameof(Colors.LightSalmon)}> asdas \n</color><color={nameof(Colors.Crimson)}> Hello </color>" ) );
    }
}
