Option Strict On
Option Explicit On

Imports System.Runtime.CompilerServices
Imports System.Text

''' <summary>トークンの評価を行うモジュールです。</summary>
Public Module TomlEvaluation

    ''' <summary>指定したキーでテーブル関連を走査して取得します。</summary>
    ''' <param name="current">現在位置のテーブル。</param>
    ''' <param name="keys">走査するテーブル名リスト。</param>
    ''' <param name="isKeyAndValue">最後の名前を除くならば真。</param>
    ''' <returns>名前で決定したテーブル。</returns>
    <Extension>
    Function TraverseTable(current As TomlTable, keys As TomlToken(), Optional isKeyAndValue As Boolean = True) As TomlTable
        For i As Integer = 0 To keys.Length + If(isKeyAndValue, -2, -1)
            Dim knm = keys(i).GetKeyString()
            If current.Children.ContainsKey(knm) Then
                If TypeOf current.Children(knm) Is TomlTable Then
                    current = DirectCast(current.Children(knm), TomlTable)
                ElseIf TypeOf current.Children(knm) Is TomlTableArray AndAlso Not isKeyAndValue Then
                    current = DirectCast(current.Children(knm), TomlTableArray).GetCurrent()
                Else
                    Throw New TomlSyntaxException($"既にテーブル以外で定義されています:{knm}")
                End If
            Else
                Dim tbl As New TomlTable()
                current.Children.Add(knm, tbl)
                current = tbl
            End If
        Next
        Return current
    End Function

    ''' <summary>トークンから要素を作成します。</summary>
    ''' <param name="tkn">トークン。</param>
    ''' <returns>要素。</returns>
    <Extension>
    Public Function CreateTomlValue(tkn As TomlToken) As ITomlElement
        Select Case tkn.TokenType
            Case TomlToken.TokenTypeEnum.Comment
                Return Nothing
            Case TomlToken.TokenTypeEnum.NumberLiteral
                Return New TomlValue(Of Long)(tkn.Range, tkn.GetLongInteger())
            Case TomlToken.TokenTypeEnum.NumberHexLiteral
                Return New TomlValue(Of Long)(tkn.Range, tkn.GetLongIntegerHexadecimal())
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
                Return tkn.GetArray()
            Case TomlToken.TokenTypeEnum.InfLiteral
                Return New TomlValue(Of Double)(tkn.Range, tkn.GetInfReal())
            Case TomlToken.TokenTypeEnum.NanLiteral
                Return New TomlValue(Of Double)(tkn.Range, tkn.GetNanReal())
            Case TomlToken.TokenTypeEnum.Inline
                Return tkn.GetInlineTable()
            Case Else
                Throw New TomlSyntaxException($"不明なトークンです:{tkn}")
        End Select
    End Function

#Region "文字列"

    ''' <summary>16進数文字を数値に変換します。</summary>
    ''' <param name="c">文字。</param>
    ''' <returns>数値。</returns>
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

    ''' <summary>16進数文字（バイト）を数値に変換します。</summary>
    ''' <param name="c">文字。</param>
    ''' <returns>数値。</returns>
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

    ''' <summary>キー文字列を取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>キー文字列。</returns>
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

    ''' <summary>ダブルクォーテーション、シングルクォーテーションで囲まれた文字列を取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>文字列。</returns>
    <Extension>
    Private Function GetString(token As TomlToken) As String
        Try
            Dim rng = token.Range
            If rng.Length > 6 AndAlso
               rng(0) = ByteDoubleQuot AndAlso rng(1) = ByteDoubleQuot AndAlso rng(2) = ByteDoubleQuot AndAlso
               rng(rng.Length - 3) = ByteDoubleQuot AndAlso rng(rng.Length - 2) = ByteDoubleQuot AndAlso rng(rng.Length - 1) = ByteDoubleQuot Then
                ' """で囲まれている場合、"""を取り除く
                Dim str = rng.ToString()
                Dim tmp As New StringBuilder(str.Length)
                Dim stSkip = str.SkipCrLfPosition()
                Dim notesc = False
                For i As Integer = stSkip To str.Length - 4
                    Dim c = str(i)
                    If c = "\"c AndAlso Not notesc Then
                        i += 1
                        Dim j = i
                        Do While str(j) = " "c OrElse str(j) = vbTab
                            j += 1
                        Loop
                        If str(j) = ChrW(ByteCR) OrElse str(j) = ChrW(ByteLF) Then
                            i = j
                        End If

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
                            Case "e"c
                                notesc = Not notesc
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
                                tmp.Append("\"c)
                            Case "u"c
                                Dim cd1 = str.GetUnicodeNumber(i, 4)
                                i += 4
                                tmp.Append(ChrW(cd1))
                            Case "U"c
                                Dim cd2 = str.GetUnicodeNumber(i, 8)
                                i += 8
                                tmp.Append(Char.ConvertFromUtf32(cd2))
                            Case Else
                                Throw New TomlSyntaxException($"不明なエスケープシーケンスです:{token}")
                        End Select
                    ElseIf c = "\"c AndAlso str(i + 1) = "e"c Then
                        notesc = True
                        i += 2
                    Else
                        tmp.Append(c)
                    End If
                Next
                Return tmp.ToString()

            ElseIf rng(0) = ByteDoubleQuot AndAlso rng(rng.Length - 1) = ByteDoubleQuot Then
                ' "で囲まれている場合、"を取り除く
                Dim str = rng.ToString()
                Dim tmp As New StringBuilder(str.Length)
                Dim notesc = False
                For i As Integer = 1 To str.Length - 2
                    Dim c = str(i)
                    If c = "\"c AndAlso Not notesc Then
                        i += 1
                        Select Case str(i)
                            Case "b"c
                                tmp.Append(ChrW(&H8))
                            Case "e"c
                                notesc = Not notesc
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
                                tmp.Append("\"c)
                            Case "u"c
                                Dim cd1 = str.GetUnicodeNumber(i, 4)
                                i += 4
                                tmp.Append(ChrW(cd1))
                            Case "U"c
                                Dim cd2 = str.GetUnicodeNumber(i, 8)
                                i += 8
                                tmp.Append(Char.ConvertFromUtf32(cd2))
                            Case Else
                                Throw New TomlSyntaxException($"不明なエスケープシーケンスです:{token}")
                        End Select
                    ElseIf c = "\"c AndAlso str(i + 1) = "e"c Then
                        notesc = True
                        i += 2
                    Else
                        tmp.Append(c)
                    End If
                Next
                Return tmp.ToString()

            ElseIf rng.Length > 6 AndAlso
                   rng(0) = ByteSingleQuot AndAlso rng(1) = ByteSingleQuot AndAlso rng(2) = ByteSingleQuot AndAlso
                   rng(rng.Length - 3) = ByteSingleQuot AndAlso rng(rng.Length - 2) = ByteSingleQuot AndAlso rng(rng.Length - 1) = ByteSingleQuot Then
                ' '''で囲まれている場合、'''を取り除く
                Dim str = rng.ToString()
                Dim stSkip = str.SkipCrLfPosition()
                Return str.Substring(stSkip, str.Length - 3 - stSkip)

            ElseIf rng(0) = ByteSingleQuot AndAlso rng(rng.Length - 1) = ByteSingleQuot Then
                ' 'で囲まれている場合、'を取り除く
                Dim str = rng.ToString()
                Return str.Substring(1, str.Length - 2)

            Else
                ' "、'で囲まれていない場合、エラー
                Throw New TomlSyntaxException($"文字列が取得できませんでした:{token}")
            End If

        Catch ex As Exception
            Throw New TomlSyntaxException($"文字列が取得できませんでした:{token}", ex)
        End Try
    End Function

    ''' <summary>Unicode番号を取得します。</summary>
    ''' <param name="str">対象文字列。</param>
    ''' <param name="start">開始位置。</param>
    ''' <param name="count">番号数。</param>
    ''' <returns>Unicode番号。</returns>
    <Extension>
    Private Function GetUnicodeNumber(str As String, start As Integer, count As Integer) As Integer
        Dim cd As Integer = 0
        For i As Integer = start + 1 To start + count
            cd = (cd << 4) + ConvertHexToByte(str(i))
        Next
        Return cd
    End Function

    ''' <summary>直後の改行コードをスキップして次の位置を返します。</summary>
    ''' <param name="str">対象文字列。</param>
    ''' <returns>スキップ位置。</returns>
    <Extension>
    Private Function SkipCrLfPosition(str As String) As Integer
        If str(3) = ChrW(ByteCR) AndAlso str(4) = ChrW(ByteLF) Then
            Return 5
        ElseIf str(3) = ChrW(ByteLF) Then
            Return 4
        Else
            Return 3
        End If
    End Function

#End Region

#Region "数値取得"

    ''' <summary>整数を取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>整数。</returns>
    <Extension>
    Private Function GetLongInteger(token As TomlToken) As Long
        Try
            ' 正負の符号を取得、符号があれば次の文字から、なければ開始位置から
            Dim ans As ULong = 0
            Dim pt As Integer = 0
            Dim msign As Boolean = False
            If token(0) = BytePlus Then
                pt = 1
            ElseIf token(0) = ByteMinus Then
                pt = 1
                msign = True
            End If

            ' 数値部分を取得
            For i As Integer = pt To token.Length - 1
                If token(i) <> ByteUnderBar Then
                    Dim a1 = ans << 3
                    Dim a2 = ans << 1
                    ans = a1 + a2 + (token(i) - ByteCh0)
                End If
            Next

            ' 結果を返却
            Return If(msign, CLng(-ans), CLng(ans))

        Catch ex As Exception
            Throw New TomlSyntaxException($"整数が取得できませんでした:{token}", ex)
        End Try
    End Function

    ''' <summary>2, 8, 16進数の整数を取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>整数。</returns>
    <Extension>
    Private Function GetLongIntegerHexadecimal(token As TomlToken) As Long
        Try
            Dim uans As ULong = 0

            Select Case token(1)
                Case ByteLowX, ByteUpX
                    ' 16進数
                    For i As Integer = 2 To token.Length - 1
                        If token(i) <> ByteUnderBar Then
                            uans = CULng((uans << 4) + ConvertHexToByte(token(i)))
                        End If
                    Next
                    Return BitConverter.ToInt64(BitConverter.GetBytes(uans), 0)

                Case ByteLowO, ByteUpO
                    ' 8進数
                    For i As Integer = 2 To token.Length - 1
                        If token(i) <> ByteUnderBar Then
                            uans = CULng((uans << 3) + (token(i) - ByteCh0))
                        End If
                    Next
                    Return BitConverter.ToInt64(BitConverter.GetBytes(uans), 0)

                Case ByteLowB, ByteUpB
                    ' 2進数
                    For i As Integer = 2 To token.Length - 1
                        If token(i) <> ByteUnderBar Then
                            uans = CULng((uans << 1) + (token(i) - ByteCh0))
                        End If
                    Next
                    Return BitConverter.ToInt64(BitConverter.GetBytes(uans), 0)
            End Select
            Return Long.Parse(token.ToString().Substring(2), Globalization.NumberStyles.HexNumber)

        Catch ex As Exception
            Throw New TomlSyntaxException($"整数が取得できませんでした:{token}", ex)
        End Try
    End Function

    ''' <summary>実数を取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>実数。</returns>
    <Extension>
    Private Function GetReal(token As TomlToken) As Double
        Return GetReal(token, token.Length)
    End Function

    ''' <summary>実数を取得します（指数表記）</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>実数。</returns>
    <Extension>
    Private Function GetExpReal(token As TomlToken) As Double
        Dim subTkn = TryCast(token, TomlHasSubToken)

        ' 仮数部を取得
        Dim num = GetReal(subTkn.SubTokens(0), subTkn.SubTokens(0).Length)

        ' 指数部を取得
        Dim epb = GetReal(subTkn.SubTokens(1), subTkn.SubTokens(1).Length)

        ' 結果を返却
        Return num * Math.Pow(10, epb)
    End Function

    ''' <summary>実数を取得します。</summary>
    ''' <param name="token">トークン。</param>
    ''' <param name="start">開始位置。</param>
    ''' <param name="len">長さ。</param>
    ''' <returns>実数。</returns>
    Private Function GetReal(token As TomlToken, len As Integer) As Double
        Try
            ' 正負の符号を取得、符号があれば次の文字から、なければ開始位置から
            Dim pt As Integer = 0
            Dim msign As Boolean = False
            If token(0) = BytePlus Then
                pt = 1
            ElseIf token(0) = ByteMinus Then
                pt = 1
                msign = True
            End If

            ' 数値部分を取得、小数点があれば小数点の位置を記録
            Dim ans As Long = 0
            Dim dec As Long = -1
            For i As Integer = pt To len - 1
                If token(i) >= ByteCh0 AndAlso token(i) <= ByteCh9 Then
                    ans = ans * 10 + (token(i) - ByteCh0)
                    If dec > -1 Then dec += 1
                ElseIf token(i) = ByteDot Then
                    dec = 0
                End If
            Next

            ' 結果を返却
            Return If(msign, -ans, ans) / If(dec >= 0, Math.Pow(10, dec), 1)

        Catch ex As Exception
            Throw New TomlSyntaxException($"実数が取得できませんでした:{token}", ex)
        End Try
    End Function

    ''' <summary>Infを取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>Inf。</returns>
    <Extension>
    Private Function GetInfReal(token As TomlToken) As Double
        Return If(token(0) = ByteMinus, Double.NegativeInfinity, Double.PositiveInfinity)
    End Function

    ''' <summary>NaNを取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>NaN。</returns>
    <Extension>
    Private Function GetNanReal(token As TomlToken) As Double
        Return If(token(0) = ByteMinus, -Double.NaN, Double.NaN)
    End Function

#End Region

#Region "配列"

    ''' <summary>配列を取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>配列。</returns>
    <Extension>
    Private Function GetArray(token As TomlToken) As TomlArray
        Try
            Dim arr = TryCast(token, TomlHasSubToken)

            ' 配列を作成、要素を追加
            Dim newarry As New TomlArray(token.Range)
            For Each tkn In arr.SubTokens
                Dim v = CreateTomlValue(tkn)
                If v IsNot Nothing Then
                    newarry.Add(v)
                End If
            Next
            Return newarry

        Catch ex As Exception
            Throw New TomlSyntaxException($"配列が取得できませんでした:{token}", ex)
        End Try
    End Function

#End Region

#Region "インラインテーブル"

    ''' <summary>インラインテーブルを取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>インラインテーブル。</returns>
    <Extension>
    Private Function GetInlineTable(token As TomlToken) As TomlTable
        Try
            Dim srcbl = TryCast(token, TomlHasSubToken)

            ' インラインテーブルを作成、要素を追加
            Dim intbl As New TomlTable()
            For Each tkn In srcbl.SubTokens
                Dim kv = TryCast(tkn, TomlKeyValueToken)
                If kv IsNot Nothing Then
                    'intbl.Children.Add(kv.Keys(0).GetKeyString(), CreateTomlValue(kv.Value))
                    intbl.TraverseTable(kv.Keys).Children.Add(kv.Keys(kv.Keys.Length - 1).GetKeyString(), CreateTomlValue(kv.Value))
                Else
                    Throw New TomlSyntaxException($"インラインテーブルが取得できませんでした:{token}")
                End If
            Next
            Return intbl

        Catch ex As Exception
            Throw New TomlSyntaxException($"インラインテーブルが取得できませんでした:{token}", ex)
        End Try
    End Function

#End Region

#Region "時間"

    ''' <summary>日付を取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>日付。</returns>
    <Extension>
    Private Function GetDateTime(token As TomlToken) As (Integer, Date, DateTimeOffset)
        Try
            ' 日付部分を取得
            Dim year = (token(0) - ByteCh0) * 1000 + (token(1) - ByteCh0) * 100 + (token(2) - ByteCh0) * 10 + (token(3) - ByteCh0)
            Dim moth = (token(5) - ByteCh0) * 10 + (token(6) - ByteCh0)
            Dim dayd = (token(8) - ByteCh0) * 10 + (token(9) - ByteCh0)
            If token.Length < 11 Then
                Return (0, New Date(year, moth, dayd), Nothing)
            End If

            ' 時間部分を取得
            Dim hour = (token(11) - ByteCh0) * 10 + (token(12) - ByteCh0)
            Dim mint = (token(14) - ByteCh0) * 10 + (token(15) - ByteCh0)

            Dim secd = 0
            Dim dtSplit As Integer = 19
            If token.Length > 16 AndAlso token(16) = ByteColon Then
                secd = (token(17) - ByteCh0) * 10 + (token(18) - ByteCh0)
                dtSplit = 19
            Else
                dtSplit = 16
            End If

            Dim mill As Integer = 0
            If token.Length > dtSplit - 1 AndAlso token(dtSplit) = ByteDot Then
                For i As Integer = dtSplit + 1 To Math.Min(token.Length - 1, dtSplit + 3)
                    If token(i) >= ByteCh0 AndAlso token(i) <= ByteCh9 Then
                        mill = mill * 10 + (token(i) - ByteCh0)
                    Else
                        dtSplit = i
                        Exit For
                    End If
                Next
            End If

            ' タイムゾーン部分を取得
            For i As Integer = dtSplit To token.Length - 1
                If token(i) = ByteUpZ OrElse token(i) = ByteLowZ Then
                    ' UTC
                    Return (1, Nothing, New DateTimeOffset(year, moth, dayd, hour, mint, secd, mill, TimeSpan.Zero))

                ElseIf token(i) = BytePlus OrElse token(i) = ByteMinus Then
                    ' オフセット
                    Dim ohour = (token(i + 1) - ByteCh0) * 10 + (token(i + 2) - ByteCh0)
                    If ohour < 0 OrElse ohour > 23 Then
                        Throw New TomlSyntaxException($"時間が不正です:{token}")
                    End If
                    If token(i) = ByteMinus Then ohour = -ohour
                    Dim omint = (token(i + 4) - ByteCh0) * 10 + (token(i + 5) - ByteCh0)
                    If omint < 0 OrElse omint > 59 Then
                        Throw New TomlSyntaxException($"分が不正です:{token}")
                    End If
                    Return (1, Nothing, New DateTimeOffset(year, moth, dayd, hour, mint, secd, mill, New TimeSpan(ohour, omint, 0)))
                End If
            Next
            Return (0, New Date(year, moth, dayd, hour, mint, secd, mill), Nothing)

        Catch ex As Exception
            Throw New TomlSyntaxException($"日付が取得できませんでした:{token}")
        End Try
    End Function

    ''' <summary>時間を取得します。</summary>
    ''' <param name="token">判定するトークン。</param>
    ''' <returns>時間。</returns>
    <Extension>
    Private Function GetTime(token As TomlToken) As TimeSpan
        Try
            ' 時間部分を取得
            Dim hour = (token(0) - ByteCh0) * 10 + (token(1) - ByteCh0)
            Dim mint = (token(3) - ByteCh0) * 10 + (token(4) - ByteCh0)
            Dim secd = If(token.Length > 5, (token(6) - ByteCh0) * 10 + (token(7) - ByteCh0), 0)
            Dim mill As Integer = 0
            For i As Integer = 9 To Math.Min(token.Length - 1, 11)
                mill = mill * 10 + (token(i) - ByteCh0)
            Next

            ' 結果を返却
            Return New TimeSpan(0, hour, mint, secd, mill)

        Catch ex As Exception
            Throw New TomlSyntaxException($"時間が取得できませんでした:{token}", ex)
        End Try
    End Function

#End Region

End Module
