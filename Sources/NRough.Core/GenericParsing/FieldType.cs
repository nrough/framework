using NRough.Doc;

namespace GenericParsing
{
    /// <summary>
    ///   Indicates whether text fields are delimited or fixed width.
    /// </summary>
    [AssemblyTreeVisible(false)]
    public enum FieldType
    {
        /// <summary>
        ///   Indicates that the fields are delimited.
        /// </summary>
        Delimited = 0,
        /// <summary>
        ///   Indicates that the fields are fixed width.
        /// </summary>
        FixedWidth = 1,
    }
}
