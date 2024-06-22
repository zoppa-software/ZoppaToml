Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices.ComTypes
Imports System.Text

Public Module TomlEvaluation

    <Extension>
    Public Function CreateTomlValue(tkn As TomlToken) As ITomlItem
        Select Case tkn.TokenType
            Case TomlToken.TokenTypeEnum.NumberLiteral, TomlToken.TokenTypeEnum.NumberHexLiteral
                Return New TomlValue(Of Long)(tkn.Range, tkn.GetLongInteger())
            Case TomlToken.TokenTypeEnum.LiteralString
                Return New TomlValue(Of String)(tkn.Range, tkn.GetString())
            Case TomlToken.TokenTypeEnum.TrueLiteral
                Return New TomlValue(Of Boolean)(tkn.Range, True)
            Case TomlToken.TokenTypeEnum.FalseLiteral
                Return New TomlValue(Of Boolean)(tkn.Range, False)
            Case TomlToken.TokenTypeEnum.DateLiteral
                Return New TomlValue(Of Date)(tkn.Range, tkn.GetDateTime())
            Case TomlToken.TokenTypeEnum.Array
                Return tkn.GetArray(tkn.Range)

                'Case TomlToken.TokenTypeEnum.LiteralString
                '    Return New TomlValue(Of String)(tkn.Range, tkn.GetString())
                'Case TomlToken.TokenTypeEnum.Integer
                '    Return New TomlValue(Of Long)(tkn.Range, tkn.GetInteger())
                'Case TomlToken.TokenTypeEnum.Float
                '    Return New TomlValue(Of Double)(tkn.Range, tkn.GetFloat())

                'Case TomlToken.TokenTypeEnum.Array
                '    Return New TomlValue(Of TomlArray)(tkn.Range, CreateTomlArray(DirectCast(tkn, TomlArrayToken)))
                'Case TomlToken.TokenTypeEnum.Table
                '    Return New TomlValue(Of TomlTable)(tkn.Range, CreateTomlTable(DirectCast(tkn, TomlTableToken)))
            Case Else
                Throw New TomlSyntaxException($"不明なトークンです:{tkn}")
        End Select
    End Function

    <Extension>
    Public Function GetLongInteger(token As TomlToken) As Long
        Dim raw = token.Raw
        Dim ans As Long = 0
        Select Case token.TokenType
            Case TomlToken.TokenTypeEnum.NumberLiteral
                Dim pt As Integer = 0
                Dim msign As Boolean = False
                If raw(0) = BytePlus Then
                    pt = 1
                ElseIf raw(0) = ByteMinus Then
                    pt = 1
                    msign = True
                End If
                For i As Integer = 0 To raw.Length - 1
                    ans = ans * 10 + (raw(i) - ByteCh0)
                Next
                Return ans

            Case TomlToken.TokenTypeEnum.NumberHexLiteral
                Select Case raw(1)
                    Case ByteLowX

                    Case ByteLowO

                    Case ByteLowB

                End Select
                Return Long.Parse(token.ToString().Substring(2), Globalization.NumberStyles.HexNumber)
            Case Else
                Throw New TomlSyntaxException($"整数が取得できませんでした:{token}")
        End Select
    End Function

    <Extension>
    Public Function GetKeyString(token As TomlToken) As String
        Select Case token.TokenType
            Case TomlToken.TokenTypeEnum.KeyString
                Return token.ToString()
            Case TomlToken.TokenTypeEnum.LiteralString
                Return token.GetString()
            Case Else
                Throw New TomlSyntaxException($"キー文字列が取得できませんでした:{token}")
        End Select
    End Function

    <Extension>
    Public Function GetString(token As TomlToken) As String
        If token.TokenType = TomlToken.TokenTypeEnum.LiteralString Then
            Dim rng = token.Range

            If rng(0) = ByteDoubleQuot AndAlso rng(rng.Length - 1) = ByteDoubleQuot Then
                Dim str = rng.ToString()
                Dim tmp As New StringBuilder(str.Length)
                For i As Integer = 1 To str.Length - 2
                    Dim c = str(i)
                    If c = "\"c Then
                        i += 1
                        Select Case str(i)
                            Case "b"c
                                tmp.Append(ChrW(&H8))
                            Case "t"c
                                tmp.Append(ChrW(&H9))
                            Case "n"c
                                tmp.Append(ChrW(&HA))
                            Case "f"c
                                tmp.Append(ChrW(&HC))
                            Case "r"c
                                tmp.Append(ChrW(&HD))
                            Case """"c
                                tmp.Append(ChrW(&H22))
                            Case "\"c
                                tmp.Append(ChrW(&H5))
                            Case "u"c
                                Dim cd1 As Integer = 0
                                For j As Integer = i + 1 To i + 4
                                    cd1 = (cd1 << 4) + ConvertCharToByte(str(j))
                                Next
                                i += 4
                                tmp.Append(ChrW(cd1))
                            Case "U"c
                                Dim cd2 As Integer = 0
                                For j As Integer = i + 1 To i + 8
                                    cd2 = (cd2 << 4) + ConvertCharToByte(str(j))
                                Next
                                i += 8
                                tmp.Append(Char.ConvertFromUtf32(cd2))
                            Case Else
                                Throw New TomlSyntaxException($"不明なエスケープシーケンスです:{token}")
                        End Select
                    Else
                        tmp.Append(c)
                    End If
                Next
                Return tmp.ToString()
            Else
                Throw New TomlSyntaxException($"文字列が閉じられていません:{token}")
            End If
        Else
            Throw New TomlSyntaxException($"文字列が取得できませんでした:{token}")
        End If
    End Function

    Private Function ConvertCharToByte(c As Char) As Byte
        If c >= "0"c AndAlso c <= "9"c Then
            Return CByte(AscW(c) - AscW("0"c))
        ElseIf c >= "A"c AndAlso c <= "F"c Then
            Return CByte(AscW(c) - AscW("A"c) + 10)
        ElseIf c >= "a"c AndAlso c <= "f"c Then
            Return CByte(AscW(c) - AscW("a"c) + 10)
        Else
            Throw New TomlSyntaxException($"不正な文字です:{c}")
        End If
    End Function

    <Extension>
    Public Function GetDateTime(token As TomlToken) As Date
        If token.TokenType = TomlToken.TokenTypeEnum.DateLiteral Then
            Return DateTime.Parse(token.ToString(), Nothing, System.Globalization.DateTimeStyles.RoundtripKind)
        Else
            Throw New TomlSyntaxException($"日付が取得できませんでした:{token}")
        End If
    End Function

    <Extension>
    Public Function GetArray(token As TomlToken, rng As RawSource.Range) As TomlArray
        If token.TokenType = TomlToken.TokenTypeEnum.Array Then
            Dim arr = TryCast(token, TomlHasSubToken)
            If arr IsNot Nothing Then
                Dim newarry As New TomlArray(rng)
                For Each tkn In arr.SubTokens
                    newarry.Add(CreateTomlValue(tkn))
                Next
                Return newarry
            End If
        End If
        Throw New TomlSyntaxException($"配列が取得できませんでした:{token}")
    End Function

End Module
