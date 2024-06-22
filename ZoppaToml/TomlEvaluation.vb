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
            Case TomlToken.TokenTypeEnum.RealLiteral
                Return New TomlValue(Of Double)(tkn.Range, tkn.GetReal())
            Case TomlToken.TokenTypeEnum.RealExpLiteral
                Return New TomlValue(Of Double)(tkn.Range, tkn.GetExpReal())
            Case TomlToken.TokenTypeEnum.TrueLiteral
                Return New TomlValue(Of Boolean)(tkn.Range, True)
            Case TomlToken.TokenTypeEnum.FalseLiteral
                Return New TomlValue(Of Boolean)(tkn.Range, False)
            Case TomlToken.TokenTypeEnum.DateLiteral
                Dim dans = tkn.GetDateTime()
                If dans.Item1 = 0 Then
                    Return New TomlValue(Of Date)(tkn.Range, dans.Item2)
                Else
                    Return New TomlValue(Of DateTimeOffset)(tkn.Range, dans.Item3)
                End If
            Case TomlToken.TokenTypeEnum.TimeLiteral
                Return New TomlValue(Of TimeSpan)(tkn.Range, tkn.GetTime())
            Case TomlToken.TokenTypeEnum.Array
                Return tkn.GetArray(tkn.Range)
            Case TomlToken.TokenTypeEnum.InfLiteral
                Return New TomlValue(Of Double)(tkn.Range, tkn.GetInfReal())
            Case TomlToken.TokenTypeEnum.NanLiteral
                Return New TomlValue(Of Double)(tkn.Range, tkn.GetNanReal())
            Case Else
                Throw New TomlSyntaxException($"不明なトークンです:{tkn}")
        End Select
    End Function

    <Extension>
    Public Function GetLongInteger(token As TomlToken) As Long
        Dim raw = token.Raw

        Select Case token.TokenType
            Case TomlToken.TokenTypeEnum.NumberLiteral
                Dim ans As Long = 0
                Dim pt As Integer = 0
                Dim msign As Boolean = False
                If raw(0) = BytePlus Then
                    pt = 1
                ElseIf raw(0) = ByteMinus Then
                    pt = 1
                    msign = True
                End If
                For i As Integer = pt To raw.Length - 1
                    If raw(i) <> ByteUnderBar Then
                        ans = ans * 10 + (raw(i) - ByteCh0)
                    End If
                Next
                Return If(msign, -ans, ans)

            Case TomlToken.TokenTypeEnum.NumberHexLiteral
                Dim uans As ULong = 0
                Select Case raw(1)
                    Case ByteLowX
                        For i As Integer = 2 To raw.Length - 1
                            If raw(i) <> ByteUnderBar Then
                                uans = CULng((uans << 4) + ConvertHexToByte(raw(i)))
                            End If
                        Next
                        Return BitConverter.ToInt64(BitConverter.GetBytes(uans), 0)

                    Case ByteLowO
                        For i As Integer = 2 To raw.Length - 1
                            If raw(i) <> ByteUnderBar Then
                                uans = CULng((uans << 3) + (raw(i) - ByteCh0))
                            End If
                        Next
                        Return BitConverter.ToInt64(BitConverter.GetBytes(uans), 0)

                    Case ByteLowB
                        For i As Integer = 2 To raw.Length - 1
                            If raw(i) <> ByteUnderBar Then
                                uans = CULng((uans << 1) + (raw(i) - ByteCh0))
                            End If
                        Next
                        Return BitConverter.ToInt64(BitConverter.GetBytes(uans), 0)
                End Select
                Return Long.Parse(token.ToString().Substring(2), Globalization.NumberStyles.HexNumber)
            Case Else
                Throw New TomlSyntaxException($"整数が取得できませんでした:{token}")
        End Select
    End Function

    Private Function ConvertHexToByte(c As Byte) As Integer
        If c >= ByteCh0 AndAlso c <= ByteCh9 Then
            Return c - ByteCh0
        ElseIf c >= ByteUpA AndAlso c <= ByteUpF Then
            Return c - ByteUpA + 10
        ElseIf c >= ByteLowA AndAlso c <= ByteLowF Then
            Return c - ByteLowA + 10
        Else
            Throw New TomlSyntaxException($"不正な文字です:{c}")
        End If
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
            If rng.Length > 6 AndAlso
               rng(0) = ByteDoubleQuot AndAlso rng(1) = ByteDoubleQuot AndAlso rng(2) = ByteDoubleQuot AndAlso
               rng(rng.Length - 3) = ByteDoubleQuot AndAlso rng(rng.Length - 2) = ByteDoubleQuot AndAlso rng(rng.Length - 1) = ByteDoubleQuot Then
                Dim str = rng.ToString()
                Dim tmp As New StringBuilder(str.Length)

                Dim stSkip As Integer = 3
                If str(3) = ChrW(ByteCR) AndAlso str(4) = ChrW(ByteLF) Then
                    stSkip = 5
                ElseIf str(3) = ChrW(ByteLF) Then
                    stSkip = 4
                End If

                For i As Integer = stSkip To str.Length - 4
                    Dim c = str(i)
                    If c = "\"c Then
                        i += 1
                        Select Case str(i)
                            Case ChrW(ByteCR)
                                If str(i + 1) = ChrW(ByteLF) Then
                                    i += 1
                                    Do While Char.IsWhiteSpace(str(i + 1))
                                        i += 1
                                    Loop
                                End If
                            Case ChrW(ByteLF)
                                Do While Char.IsWhiteSpace(str(i + 1))
                                    i += 1
                                Loop
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
                                    cd1 = (cd1 << 4) + ConvertHexToByte(str(j))
                                Next
                                i += 4
                                tmp.Append(ChrW(cd1))
                            Case "U"c
                                Dim cd2 As Integer = 0
                                For j As Integer = i + 1 To i + 8
                                    cd2 = (cd2 << 4) + ConvertHexToByte(str(j))
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
            ElseIf rng(0) = ByteDoubleQuot AndAlso rng(rng.Length - 1) = ByteDoubleQuot Then
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
                                    cd1 = (cd1 << 4) + ConvertHexToByte(str(j))
                                Next
                                i += 4
                                tmp.Append(ChrW(cd1))
                            Case "U"c
                                Dim cd2 As Integer = 0
                                For j As Integer = i + 1 To i + 8
                                    cd2 = (cd2 << 4) + ConvertHexToByte(str(j))
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
            ElseIf rng.Length > 6 AndAlso
                   rng(0) = ByteSingleQuot AndAlso rng(1) = ByteSingleQuot AndAlso rng(2) = ByteSingleQuot AndAlso
                   rng(rng.Length - 3) = ByteSingleQuot AndAlso rng(rng.Length - 2) = ByteSingleQuot AndAlso rng(rng.Length - 1) = ByteSingleQuot Then
                Dim str = rng.ToString()
                Dim stSkip As Integer = 3
                If str(3) = ChrW(ByteCR) AndAlso str(4) = ChrW(ByteLF) Then
                    stSkip = 5
                ElseIf str(3) = ChrW(ByteLF) Then
                    stSkip = 4
                End If
                Return str.Substring(stSkip, str.Length - 3 - stSkip)

            ElseIf rng(0) = ByteSingleQuot AndAlso rng(rng.Length - 1) = ByteSingleQuot Then
                Dim str = rng.ToString()
                Return str.Substring(1, str.Length - 2)
            End If
        End If
        Throw New TomlSyntaxException($"文字列が取得できませんでした:{token}")
    End Function

    Private Function ConvertHexToByte(c As Char) As Byte
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
    Public Function GetDateTime(token As TomlToken) As (Integer, Date, DateTimeOffset)
        If token.TokenType = TomlToken.TokenTypeEnum.DateLiteral Then
            Dim raw = token.Raw
            Dim year = (raw(0) - ByteCh0) * 1000 + (raw(1) - ByteCh0) * 100 + (raw(2) - ByteCh0) * 10 + (raw(3) - ByteCh0)
            Dim moth = (raw(5) - ByteCh0) * 10 + (raw(6) - ByteCh0)
            Dim dayd = (raw(8) - ByteCh0) * 10 + (raw(9) - ByteCh0)
            If raw.Length < 10 Then
                Return (0, New Date(year, moth, dayd), Nothing)
            End If

            Dim hour = (raw(11) - ByteCh0) * 10 + (raw(12) - ByteCh0)
            Dim mint = (raw(14) - ByteCh0) * 10 + (raw(15) - ByteCh0)
            Dim secd = (raw(17) - ByteCh0) * 10 + (raw(18) - ByteCh0)
            Dim mill As Integer = 0
            If raw(19) = ByteDot Then
                For i As Integer = 20 To Math.Min(raw.Length - 1, 22)
                    mill = mill * 10 + (raw(i) - ByteCh0)
                Next
            End If

            Dim off As Integer = -1
            For i As Integer = 19 To raw.Length - 1
                If raw(i) = ByteUpZ OrElse raw(i) = BytePlus OrElse raw(i) = ByteMinus Then
                    off = i
                    Exit For
                End If
            Next
            If off < 0 Then
                Return (0, New Date(year, moth, dayd, hour, mint, secd, mill), Nothing)
            ElseIf raw(off) = ByteUpZ Then
                Return (1, Nothing, New DateTimeOffset(year, moth, dayd, hour, mint, secd, mill, TimeSpan.Zero))
            Else
                Dim ohour = (raw(off + 1) - ByteCh0) * 10 + (raw(off + 2) - ByteCh0)
                If raw(off) = ByteMinus Then ohour = -ohour
                Dim omint = (raw(off + 4) - ByteCh0) * 10 + (raw(off + 5) - ByteCh0)
                Return (1, Nothing, New DateTimeOffset(year, moth, dayd, hour, mint, secd, mill, New TimeSpan(ohour, omint, 0)))
            End If
        Else
            Throw New TomlSyntaxException($"日付が取得できませんでした:{token}")
        End If
    End Function

    <Extension>
    Public Function GetTime(token As TomlToken) As TimeSpan
        If token.TokenType = TomlToken.TokenTypeEnum.TimeLiteral Then
            Dim raw = token.Raw
            Dim hour = (raw(0) - ByteCh0) * 10 + (raw(1) - ByteCh0)
            Dim mint = (raw(3) - ByteCh0) * 10 + (raw(4) - ByteCh0)
            Dim secd = (raw(6) - ByteCh0) * 10 + (raw(7) - ByteCh0)
            Dim mill As Integer = 0
            For i As Integer = 9 To Math.Min(raw.Length - 1, 11)
                mill = mill * 10 + (raw(i) - ByteCh0)
            Next
            Return New TimeSpan(0, hour, mint, secd, mill)
        End If
        Throw New TomlSyntaxException($"時間が取得できませんでした:{token}")
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

    <Extension>
    Public Function GetReal(token As TomlToken) As Double
        Dim raw = token.Raw
        Return GetReal(raw, 0, raw.Length)
    End Function

    <Extension>
    Public Function GetExpReal(token As TomlToken) As Double
        Dim raw = token.Raw
        Dim exp As Integer
        For i As Integer = 1 To raw.Length - 1
            If raw(i) = ByteLowE OrElse raw(i) = ByteUpE Then
                exp = i
                Exit For
            End If
        Next

        Dim num = GetReal(raw, 0, exp)
        Dim epb = GetReal(raw, exp + 1, raw.Length)
        Return num * Math.Pow(10, epb)
    End Function

    Private Function GetReal(raw As Byte(), start As Integer, len As Integer) As Double
        Dim pt As Integer = start
        Dim msign As Boolean = False
        If raw(start) = BytePlus Then
            pt = start + 1
        ElseIf raw(start) = ByteMinus Then
            pt = start + 1
            msign = True
        End If

        Dim ans As Long = 0
        Dim dec As Long = -1
        For i As Integer = pt To len - 1
            If raw(i) >= ByteCh0 AndAlso raw(i) <= ByteCh9 Then
                ans = ans * 10 + (raw(i) - ByteCh0)
                If dec > -1 Then dec += 1
            ElseIf raw(i) = ByteDot Then
                dec = 0
            End If
        Next
        Return If(msign, -ans, ans) / If(dec >= 0, Math.Pow(10, dec), 1)
    End Function

    <Extension>
    Public Function GetInfReal(token As TomlToken) As Double
        Dim raw = token.Raw
        If raw(0) = ByteMinus Then
            Return Double.NegativeInfinity
        Else
            Return Double.PositiveInfinity
        End If
    End Function

    <Extension>
    Public Function GetNanReal(token As TomlToken) As Double
        Dim raw = token.Raw
        If raw(0) = ByteMinus Then
            Return -Double.NaN
        Else
            Return Double.NaN
        End If
    End Function

End Module
