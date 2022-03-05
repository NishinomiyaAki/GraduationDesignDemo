namespace EditorUI
{
    public enum XmlScannerState
    {
        Start,
        End,
        Comment1,
        Comment2,
        Identifier,
        String1,
        String2_1,
        String2_2,
        String2_3,
        LeftParen1Divide,
        DivideRightParen1,
    }
}