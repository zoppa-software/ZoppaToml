Option Strict On
Option Explicit On

Imports System.Runtime.CompilerServices

Public Module TomlLexical

    <Extension>
    Public Function Lexical(raw As RawSource) As List(Of TomlToken)
        Dim res As New List(Of TomlToken)()

        Dim pointer = raw.GetPointer()
        Dim getItem = False
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteSharp
                        res.Add(LexicalComment(raw, pointer))

                    Case ByteLBacket
                        If Not getItem Then
                            If pointer.Peek(1) = ByteLBacket Then
                                res.Add(LoadTableArrayHeader(raw, pointer))
                            Else
                                res.Add(LoadTableHeader(raw, pointer))
                            End If
                            getItem = True
                        Else
                            Throw New TomlSyntaxException($"テーブル定義は新しい行である必要があります。:{pointer.TakeChar(5)}")
                        End If

                    Case ByteCR, ByteLF
                        res.Add(LexicalLineFeed(raw, pointer))
                        getItem = False

                    Case ByteSpace, ByteTab
                        res.Add(LexicalSpaces(raw, pointer))

                    Case Else
                        If Not getItem Then
                            res.Add(LexicalKeyAndValue(raw, pointer))
                            getItem = True
                        Else
                            Throw New TomlSyntaxException($"キー、値は新しい行である必要があります。:{pointer.TakeChar(5)}")
                        End If
                End Select
            Else
                pointer.Next()
            End If
        Loop
        Return res
    End Function

    ''' <summary>空白トークンを作成します。</summary>
    ''' <param name="raw">生値参照。</param>
    ''' <param name="pointer">生値ポインタ。</param>
    ''' <returns>空白トークン。</returns>
    Public Function LexicalSpaces(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 AndAlso (c(0) = ByteSpace OrElse c(0) = ByteTab) Then
                pointer.Next()
            Else
                Exit Do
            End If
        Loop
        Return New TomlToken(TomlToken.TokenTypeEnum.Spaces, raw.GetRange(start, pointer.Index))
    End Function

    ''' <summary>改行トークンを作成します。</summary>
    ''' <param name="raw">生値参照。</param>
    ''' <param name="pointer">生値ポインタ。</param>
    ''' <returns>改行トークン。</returns>
    Public Function LexicalLineFeed(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim c = pointer.Current
        If pointer.Peek(0) = ByteCR AndAlso pointer.Peek(1) = ByteLF Then
            Dim res As New TomlToken(TomlToken.TokenTypeEnum.LineFeed, raw.GetRange(pointer.Index, pointer.Index + 1))
            pointer.Skip(2)
            Return res
        Else
            Dim res As New TomlToken(TomlToken.TokenTypeEnum.LineFeed, raw.GetRange(pointer.Index, pointer.Index))
            pointer.Skip(1)
            Return res
        End If
    End Function

    ''' <summary>コメントトークンを作成します。</summary>
    ''' <param name="raw">生値参照。</param>
    ''' <param name="pointer">生値ポインタ。</param>
    ''' <returns>コメントトークン。</returns>
    Public Function LexicalComment(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 AndAlso (c(0) = ByteCR OrElse c(0) = ByteLF) Then
                Return New TomlToken(TomlToken.TokenTypeEnum.Comment, raw.GetRange(start, pointer.Index - 1))
            Else
                pointer.Next()
            End If
        Loop
        Return New TomlToken(TomlToken.TokenTypeEnum.Comment, raw.GetRange(start, pointer.Index - 1))
    End Function

    ''' <summary>テーブルヘッダトークンを作成します。</summary>
    ''' <param name="raw">生値参照。</param>
    ''' <param name="pointer">生値ポインタ。</param>
    ''' <returns>テーブルヘッダトークン。</returns>
    Public Function LoadTableHeader(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        pointer.Skip(1)

        Dim keys = LexicalKey(raw, pointer)

        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteRBacket
                        pointer.Skip(1)
                        Return New TomlTableHeaderToken(TomlToken.TokenTypeEnum.TableHeader, raw.GetRange(start, pointer.Index), keys.ToArray())
                End Select
                pointer.Skip(1)
            Else
                pointer.Next()
            End If
        Loop

        Throw New TomlSyntaxException($"テーブルが閉じられていません:{raw.GetPointer(start).TakeChar(5)}")
    End Function

    ''' <summary>テーブル配列ヘッダトークンを作成します。</summary>
    ''' <param name="raw">生値参照。</param>
    ''' <param name="pointer">生値ポインタ。</param>
    ''' <returns>テーブル配列ヘッダトークン。</returns>
    Public Function LoadTableArrayHeader(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        pointer.Skip(2)

        Dim keys = LexicalKey(raw, pointer)

        Dim getTkn = False
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteRBacket
                        If pointer.Peek(1) = ByteRBacket Then
                            pointer.Skip(2)
                            Return New TomlTableHeaderToken(TomlToken.TokenTypeEnum.TableArrayHeader, raw.GetRange(start, pointer.Index), keys.ToArray())
                        End If
                        pointer.Skip(1)
                End Select
            Else
                pointer.Next()
            End If
        Loop

        Throw New TomlSyntaxException($"テーブル配列が閉じられていません:{raw.GetPointer(start).TakeChar(5)}")
    End Function

    ''' <summary>キーと値トークンを作成します。</summary>
    ''' <param name="raw">生値参照。</param>
    ''' <param name="pointer">生値ポインタ。</param>
    ''' <returns>キーと値トークン。</returns>
    Public Function LexicalKeyAndValue(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        Dim keys = LexicalKey(raw, pointer)

        SkipChars(raw, pointer, ByteSpace, ByteTab)
        SkipChars(raw, pointer, ByteEqual)
        SkipChars(raw, pointer, ByteSpace, ByteTab)

        Dim value = LexicalValue(raw, pointer)

        Return New TomlKeyValueToken(TomlToken.TokenTypeEnum.KeyAndValue, raw.GetRange(start, pointer.Index - 1), keys.ToArray(), value)
    End Function

    ''' <summary>キートークンを作成します。</summary>
    ''' <param name="raw">生値参照。</param>
    ''' <param name="pointer">生値ポインタ。</param>
    ''' <returns>キートークン。</returns>
    Private Function LexicalKey(raw As RawSource, pointer As RawSource.Pointer) As List(Of TomlToken)
        Dim res As New List(Of TomlToken)()

        Dim start = pointer.Index
        Dim getTkn = False
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteDoubleQuot
                        If Not getTkn Then
                            res.Add(GetStringFromDoubleQuot(raw, pointer))
                            getTkn = True
                        Else
                            Throw New TomlSyntaxException($"キーの解析に失敗しました:{raw.GetPointer(start).TakeChar(5)}")
                        End If

                    Case ByteSingleQuot
                        If Not getTkn Then
                            res.Add(GetStringFromSingleQuot(raw, pointer))
                            getTkn = True
                        Else
                            Throw New TomlSyntaxException($"キーの解析に失敗しました:{raw.GetPointer(start).TakeChar(5)}")
                        End If

                    Case ByteUpA To ByteUpZ, ByteLowA To ByteLowZ, ByteCh0 To ByteCh9, ByteUnderBar, ByteHyphen
                        If Not getTkn Then
                            res.Add(GetKeyChar(raw, pointer))
                            getTkn = True
                        Else
                            Throw New TomlSyntaxException($"キーの解析に失敗しました:{raw.GetPointer(start).TakeChar(5)}")
                        End If

                    Case ByteDot
                        pointer.Skip(1)
                        start = pointer.Index
                        getTkn = False

                    Case ByteSharp, ByteTab
                        pointer.Skip(1)

                    Case Else
                        Return res
                End Select
            Else
                pointer.Next()
            End If
        Loop
        Throw New TomlSyntaxException($"キーの解析に失敗しました:{raw.GetPointer(start).TakeChar(5)}")
    End Function

    Private Function GetStringFromDoubleQuot(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        If pointer.Peek(0) = ByteDoubleQuot AndAlso pointer.Peek(1) = ByteDoubleQuot AndAlso pointer.Peek(2) = ByteDoubleQuot Then
            pointer.Skip(3)

            Do While Not pointer.IsEnd
                Dim c = pointer.Current
                If c.Length = 1 Then
                    If pointer.Peek(0) = ByteBKSlash AndAlso
                       pointer.Peek(1) = ByteDoubleQuot Then
                        pointer.Skip(2)
                    ElseIf pointer.Peek(0) = ByteDoubleQuot AndAlso
                       pointer.Peek(1) = ByteDoubleQuot AndAlso
                       pointer.Peek(2) = ByteDoubleQuot AndAlso
                       pointer.Peek(3) <> ByteDoubleQuot Then
                        pointer.Skip(3)
                        Return New TomlToken(TomlToken.TokenTypeEnum.LiteralString, raw.GetRange(start, pointer.Index - 1))
                    Else
                        pointer.Skip(1)
                    End If
                Else
                    pointer.Next()
                End If
            Loop
            Throw New TomlSyntaxException($"文字列の終端が見つかりません。:{raw.GetPointer(start).TakeChar(5)}")
        End If

        pointer.Skip(1)

        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                If pointer.Peek(0) = ByteBKSlash AndAlso
                       pointer.Peek(1) = ByteDoubleQuot Then
                    pointer.Skip(2)
                ElseIf pointer.Peek(0) = ByteDoubleQuot Then
                    pointer.Skip(1)
                    Return New TomlToken(TomlToken.TokenTypeEnum.LiteralString, raw.GetRange(start, pointer.Index - 1))
                Else
                    pointer.Skip(1)
                End If
            Else
                pointer.Next()
            End If
        Loop
        Throw New TomlSyntaxException($"文字列の終端が見つかりません:{raw.GetPointer(start).TakeChar(5)}")
    End Function

    Private Function GetStringFromSingleQuot(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        If pointer.Peek(0) = ByteSingleQuot AndAlso pointer.Peek(1) = ByteSingleQuot AndAlso pointer.Peek(2) = ByteSingleQuot Then
            pointer.Skip(3)

            Do While Not pointer.IsEnd
                Dim c = pointer.Current
                If c.Length = 1 Then
                    If pointer.Peek(0) = ByteSingleQuot AndAlso
                       pointer.Peek(1) = ByteSingleQuot AndAlso
                       pointer.Peek(2) = ByteSingleQuot AndAlso
                       pointer.Peek(3) <> ByteSingleQuot Then
                        pointer.Skip(3)
                        Return New TomlToken(TomlToken.TokenTypeEnum.LiteralString, raw.GetRange(start, pointer.Index - 1))
                    End If
                    pointer.Skip(1)
                Else
                    pointer.Next()
                End If
            Loop
            Throw New TomlSyntaxException($"文字列の終端が見つかりません。:{raw.GetPointer(start).TakeChar(5)}")
        End If

        pointer.Skip(1)

        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteSingleQuot
                        pointer.Skip(1)
                        Return New TomlToken(TomlToken.TokenTypeEnum.LiteralString, raw.GetRange(start, pointer.Index - 1))
                End Select
                pointer.Skip(1)
            Else
                pointer.Next()
            End If
        Loop
        Throw New TomlSyntaxException($"文字列の終端が見つかりません:{raw.GetPointer(start).TakeChar(5)}")
    End Function

    Private Function GetKeyChar(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteUpA To ByteUpZ, ByteLowA To ByteLowZ, ByteCh0 To ByteCh9, ByteUnderBar, ByteHyphen
                        pointer.Skip(1)
                    Case Else
                        Return New TomlToken(TomlToken.TokenTypeEnum.KeyString, raw.GetRange(start, pointer.Index - 1))
                End Select
            Else
                Return New TomlToken(TomlToken.TokenTypeEnum.KeyString, raw.GetRange(start, pointer.Index - 1))
            End If
        Loop
        Return New TomlToken(TomlToken.TokenTypeEnum.KeyString, raw.GetRange(start, pointer.Index - 1))
    End Function

    Public Sub SkipChars(raw As RawSource, pointer As RawSource.Pointer, ParamArray ch As Byte())
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 AndAlso Array.IndexOf(ch, c(0)) >= 0 Then
                pointer.Skip(1)
            Else
                Return
            End If
        Loop
    End Sub

    Public Function LexicalValue(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteDoubleQuot
                        Return GetStringFromDoubleQuot(raw, pointer)

                    Case ByteSingleQuot
                        Return GetStringFromSingleQuot(raw, pointer)

                    Case ByteLowT
                        Return GetTrueLiteral(raw, pointer)

                    Case ByteLowF
                        Return GetFalseLiteral(raw, pointer)

                    Case Else
                        Exit Do
                End Select
            Else
                pointer.Next()
            End If
        Loop
        Throw New TomlSyntaxException($"値の解析に失敗しました:{raw.GetPointer(start).TakeChar(5)}")
    End Function

    Private Function GetTrueLiteral(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        If pointer.Peek(0) = AscW("t"c) AndAlso
           pointer.Peek(1) = AscW("r"c) AndAlso
           pointer.Peek(2) = AscW("u"c) AndAlso
           pointer.Peek(3) = AscW("e"c) Then
            pointer.Skip(4)
            Return New TomlToken(TomlToken.TokenTypeEnum.TrueLiteral, raw.GetRange(start, pointer.Index - 1))
        End If
        Throw New TomlSyntaxException($"解析できないリテラルです:{raw.GetPointer(start).TakeChar(5)}")
    End Function

    Private Function GetFalseLiteral(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        If pointer.Peek(0) = AscW("f"c) AndAlso
           pointer.Peek(1) = AscW("a"c) AndAlso
           pointer.Peek(2) = AscW("l"c) AndAlso
           pointer.Peek(3) = AscW("s"c) AndAlso
           pointer.Peek(3) = AscW("e"c) Then
            pointer.Skip(4)
            Return New TomlToken(TomlToken.TokenTypeEnum.FalseLiteral, raw.GetRange(start, pointer.Index - 1))
        End If
        Throw New TomlSyntaxException($"解析できないリテラルです:{raw.GetPointer(start).TakeChar(5)}")
    End Function

End Module
