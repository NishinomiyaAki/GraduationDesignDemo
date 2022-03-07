using System;
using System.Text;

namespace EditorUI
{
    public class XmlScript
    {
        public const int LINE_BUFFER_LENGTH = 100000;

        private string _Filename;
        private string _Source;
        private int _SourceLength;
        private int _LineNumber;
        private int _SourceIndex;
        private XmlTokenType _TokenType;
        private StringBuilder _StringBuilder;
        private Record _RootRecord;
        private Record _CurrentRecord;
        private bool _bShowFirstErrorOnly;
        private bool _bFirstErrorShowed;

        public XmlScript()
        {
            _Filename = "";
            _Source = "";
            _SourceLength = 0;
            _LineNumber = 0;
            _SourceIndex = 0;
            _TokenType = XmlTokenType.Unknown;
            _StringBuilder = new StringBuilder(LINE_BUFFER_LENGTH);
            _RootRecord = null;
            _CurrentRecord = null;
            _bShowFirstErrorOnly = true;
            _bFirstErrorShowed = false;
            _RootRecord = new Record();
            _RootRecord.SetTypeString("Root");
            _CurrentRecord = _RootRecord;
        }

        public Record Open(string InFilename)
        {
            _Filename = InFilename;
            OpenSource(_Filename);
            if (_Source.Length == 0)
            {
                return _RootRecord;
            }
            ResetSource();
            ParseXMLScript();
            CloseSource();
            return _RootRecord;
        }

        public Record Parse(string Script)
        {
            OpenSource_Script(Script);
            if (_Source.Length == 0)
            {
                return _RootRecord;
            }
            ResetSource();
            ParseXMLScript();
            CloseSource();
            return _RootRecord;
        }

        public bool Save(string Filename)
        {
            string StringContent = _RootRecord.FormatString();
            return FileHelper.WriteTextFile(Filename, StringContent);
        }

        public Record GetRootRecord()
        {
            return _RootRecord;
        }

        public void OpenSource(string Filename)
        {
            CloseSource();
            ResetSource();
            _Source = FileHelper.ReadTextFile(Filename);
            if (_Source == "")
            {
                Console.WriteLine("Failed to load xml file: {0}\n", Filename);
                DebugHelper.Assert(false);
            }
            _SourceLength = _Source.Length;
        }

        public void OpenSource_Script(string InScript)
        {
            CloseSource();
            ResetSource();
            _Source = InScript;
            _SourceLength = _Source.Length;
        }

        private void CloseSource()
        {
            _Source = "";
        }

        private void ResetSource()
        {
            _SourceIndex = 0;
        }

        private char GetChar()
        {
            if (_SourceIndex < _SourceLength)
            {
                char Char = _Source[_SourceIndex];
                _SourceIndex++;
                if (Char == '\n')
                {
                    _LineNumber++;
                }
                return Char;
            }
            else
            {
                return '\0';
            }
        }

        private void UnGetChar()
        {
            if (_SourceIndex > 0)
            {
                _SourceIndex--;
            }
        }

        private XmlTokenType GetCurrentToken()
        {
            return _TokenType;
        }

        private string GetCurrentTokenString()
        {
            return string.Intern(_StringBuilder.ToString());
        }

        private XmlTokenType GetToken()
        {
            XmlScannerState ScannerState = XmlScannerState.Start;
            _TokenType = XmlTokenType.Unknown;
            _StringBuilder.Clear();
            while (ScannerState != XmlScannerState.End)
            {
                char Char = GetChar();
                bool bSaveChar = false;
                switch (ScannerState)
                {
                    case XmlScannerState.Start:
                        if (Char == ' ' || Char == '\t' || Char == '\r' || Char == '\n')
                        {
                            break;
                        }
                        switch (Char)
                        {
                            case '<':
                                ScannerState = XmlScannerState.LeftParen1Divide;
                                break;

                            case '>':
                                _TokenType = XmlTokenType.RightParen1;
                                ScannerState = XmlScannerState.End;
                                break;

                            case '/':
                                ScannerState = XmlScannerState.DivideRightParen1;
                                break;

                            case '-':
                                ScannerState = XmlScannerState.Comment1;
                                break;

                            case '=':
                                _TokenType = XmlTokenType.Equal;
                                ScannerState = XmlScannerState.End;
                                break;

                            case '"':
                                _TokenType = XmlTokenType.String;
                                ScannerState = XmlScannerState.String1;
                                break;

                            case '#':
                                _TokenType = XmlTokenType.String;
                                ScannerState = XmlScannerState.String2_1;
                                break;

                            case '\0':
                                _TokenType = XmlTokenType.Eof;
                                ScannerState = XmlScannerState.End;
                                break;
                        }
                        if (CharHelper.IsAlpha_Fast(Char) || CharHelper.IsUnicodeChar(Char))
                        {
                            bSaveChar = true;
                            _TokenType = XmlTokenType.Identifier;
                            ScannerState = XmlScannerState.Identifier;
                        }
                        break;

                    case XmlScannerState.Comment1:
                        if (Char == '-')
                        {
                            ScannerState = XmlScannerState.Comment2;
                        }
                        else
                        {
                            _TokenType = XmlTokenType.Unknown;
                            ScannerState = XmlScannerState.End;
                        }
                        break;

                    case XmlScannerState.Comment2:
                        if (Char == '\n')
                        {
                            ScannerState = XmlScannerState.Start;
                        }
                        if (Char == '\0')
                        {
                            _TokenType = XmlTokenType.Eof;
                            ScannerState = XmlScannerState.End;
                        }
                        break;

                    case XmlScannerState.Identifier:
                        if (CharHelper.IsIdentifier_Fast(Char) || CharHelper.IsUnicodeChar(Char))
                        {
                            bSaveChar = true;
                            ScannerState = XmlScannerState.Identifier;
                        }
                        else
                        {
                            UnGetChar();
                            ScannerState = XmlScannerState.End;
                        }
                        break;

                    case XmlScannerState.String1:
                        if (Char != '"')
                        {
                            bSaveChar = true;
                            ScannerState = XmlScannerState.String1;
                        }
                        else
                        {
                            ScannerState = XmlScannerState.End;
                        }
                        break;

                    case XmlScannerState.String2_1:
                        if (Char == '#')
                        {
                            ScannerState = XmlScannerState.String2_2;
                        }
                        else
                        {
                            DebugHelper.Assert(false);
                        }
                        break;

                    case XmlScannerState.String2_2:
                        if (Char == '#')
                        {
                            ScannerState = XmlScannerState.String2_3;
                        }
                        else
                        {
                            bSaveChar = true;
                            ScannerState = XmlScannerState.String2_2;
                        }
                        break;

                    case XmlScannerState.String2_3:
                        if (Char == '#')
                        {
                            ScannerState = XmlScannerState.End;
                        }
                        else
                        {
                            bSaveChar = true;
                            ScannerState = XmlScannerState.String2_2;
                        }
                        break;

                    case XmlScannerState.LeftParen1Divide:
                        if (Char == '/')
                        {
                            _TokenType = XmlTokenType.LeftParen1Divide;
                            ScannerState = XmlScannerState.End;
                        }
                        else
                        {
                            UnGetChar();
                            _TokenType = XmlTokenType.LeftParen1;
                            ScannerState = XmlScannerState.End;
                        }
                        break;

                    case XmlScannerState.DivideRightParen1:
                        if (Char == '>')
                        {
                            _TokenType = XmlTokenType.DivideRightParen1;
                            ScannerState = XmlScannerState.End;
                        }
                        else if (Char == '/')
                        {
                            ScannerState = XmlScannerState.Comment2;
                        }
                        else
                        {
                            UnGetChar();
                            _TokenType = XmlTokenType.Unknown;
                            ScannerState = XmlScannerState.End;
                        }
                        break;

                    case XmlScannerState.End:
                        DebugHelper.Assert(false);
                        break;
                }
                if (bSaveChar)
                {
                    _StringBuilder.Append(Char);
                }
            }
            return _TokenType;
        }

        private void MatchToken(XmlTokenType ExpectedTokenType)
        {
            if (ExpectedTokenType != GetCurrentToken())
            {
                if (_bShowFirstErrorOnly)
                {
                    if (_bFirstErrorShowed)
                    {
                        return;
                    }
                }
                Console.WriteLine("Error {0}({1}): Unexpected token type:{2}, token string:{3}, expected token type:{4}",
                    _Filename, _LineNumber + 1, GetCurrentToken(), GetCurrentTokenString(), ExpectedTokenType);
                GetToken();
                _bFirstErrorShowed = true;
            }

            GetToken();
        }

        private void ParseXMLScript()
        {
            ResetSource();
            GetToken();
            ParseXMLElementList();
        }

        private void ParseXMLElementList()
        {
            while (_TokenType != XmlTokenType.Eof)
            {
                ParseXMLElement();
            }
        }

        private void ParseXMLElementList1()
        {
            while (_TokenType != XmlTokenType.LeftParen1Divide && _TokenType != XmlTokenType.Eof)
            {
                ParseXMLElement();
            }
        }

        private void ParseXMLElement()
        {
            Record SavedCurrentRecord = _CurrentRecord;
            MatchToken(XmlTokenType.LeftParen1);
            if (_TokenType == XmlTokenType.Identifier)
            {
                Record Record = _CurrentRecord.AddChild();
                Record.SetTypeString(GetCurrentTokenString());
                _CurrentRecord = Record;
            }
            MatchToken(XmlTokenType.Identifier);
            ParseXMLAttributeList();
            if (_TokenType == XmlTokenType.DivideRightParen1)
            {
                MatchToken(XmlTokenType.DivideRightParen1);
            }
            else if (_TokenType == XmlTokenType.RightParen1)
            {
                MatchToken(XmlTokenType.RightParen1);
                ParseXMLElementList1();
                MatchToken(XmlTokenType.LeftParen1Divide);
                MatchToken(XmlTokenType.Identifier);
                MatchToken(XmlTokenType.RightParen1);
            }
            _CurrentRecord = SavedCurrentRecord;
        }

        private void ParseXMLAttributeList()
        {
            while (_TokenType != XmlTokenType.DivideRightParen1 && _TokenType != XmlTokenType.RightParen1 && _TokenType != XmlTokenType.Eof)
            {
                ParseXMLAttribute();
            }
        }

        private void ParseXMLAttribute()
        {
            string Name = "";
            string Value = "";
            if (_TokenType == XmlTokenType.Identifier)
            {
                Name = GetCurrentTokenString();
            }
            MatchToken(XmlTokenType.Identifier);
            MatchToken(XmlTokenType.Equal);
            if (_TokenType == XmlTokenType.String)
            {
                Value = GetCurrentTokenString();
            }
            MatchToken(XmlTokenType.String);
            _CurrentRecord.SetString(Name, Value);
        }
    }
}