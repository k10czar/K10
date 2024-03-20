using System;

public class ConstLikeAttribute : Attribute { }
public class LazyConstAttribute : ConstLikeAttribute { }
public class FieldPoolAttribute : ConstLikeAttribute { }
public class BlackboardAttribute : ConstLikeAttribute { }
public class ResetedOnLoadAttribute : ConstLikeAttribute { }
 