namespace HuskyKit.Sql
{
    [Flags]
    public enum ForJsonOptions : short
    {
        AUTO = 0x0,                     // 0000
        PATH = 1 << 0,                  // 0001
        WITHOUT_ARRAY_WRAPPER = 1 << 1,  // 0010
        INCLUDE_NULL_VALUES = 1 << 2    // 0100
    }
}
