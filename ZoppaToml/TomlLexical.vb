﻿Option Strict On
Option Explicit On

Imports System.Runtime.CompilerServices
Imports ZoppaToml.TomlToken

Public Module TomlLexical

    <Extension>
    Public Function Lexical(raw As RawSource) As List(Of TomlToken)
        Dim res As New List(Of TomlToken)()

        Dim pointer = raw.GetPointer()
        Dim getItem = False
        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
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
                            Throw New TomlSyntaxException($"テーブル定義は新しい行である必要があります。:{pointer.TakeChar(10)}")
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
                            Throw New TomlSyntaxException($"キー、値は新しい行である必要があります。:{pointer.TakeChar(10)}")
                        End If
                End Select
            Else
                pointer.Skip(cs.skip)
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
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 AndAlso (cs.curByte = ByteSpace OrElse cs.curByte = ByteTab) Then
                pointer.Skip(1)
            Else
                Exit Do
            End If
        Loop
        Return New TomlToken(TokenTypeEnum.Spaces, raw.GetRange(start, pointer.Index - 1))
    End Function

    ''' <summary>改行トークンを作成します。</summary>
    ''' <param name="raw">生値参照。</param>
    ''' <param name="pointer">生値ポインタ。</param>
    ''' <returns>改行トークン。</returns>
    Public Function LexicalLineFeed(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        If pointer.Peek(0) = ByteCR AndAlso pointer.Peek(1) = ByteLF Then
            Dim res As New TomlToken(TokenTypeEnum.LineFeed, raw.GetRange(pointer.Index, pointer.Index + 1))
            pointer.Skip(2)
            Return res
        Else
            Dim res As New TomlToken(TokenTypeEnum.LineFeed, raw.GetRange(pointer.Index, pointer.Index))
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
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 AndAlso (cs.curByte = ByteCR OrElse cs.curByte = ByteLF) Then
                Return New TomlToken(TokenTypeEnum.Comment, raw.GetRange(start, pointer.Index - 1))
            Else
                pointer.Skip(cs.skip)
            End If
        Loop
        Return New TomlToken(TokenTypeEnum.Comment, raw.GetRange(start, pointer.Index - 1))
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
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteRBacket
                        pointer.Skip(1)
                        Return New TomlHasSubToken(TokenTypeEnum.TableHeader, raw.GetRange(start, pointer.Index), keys.ToArray())
                End Select
                pointer.Skip(1)
            Else
                pointer.Skip(cs.skip)
            End If
        Loop

        Throw New TomlSyntaxException($"テーブルが閉じられていません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
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
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteRBacket
                        If pointer.Peek(1) = ByteRBacket Then
                            pointer.Skip(2)
                            Return New TomlHasSubToken(TokenTypeEnum.TableArrayHeader, raw.GetRange(start, pointer.Index), keys.ToArray())
                        End If
                        pointer.Skip(1)
                End Select
            Else
                pointer.Skip(cs.skip)
            End If
        Loop

        Throw New TomlSyntaxException($"テーブル配列が閉じられていません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    ''' <summary>キーと値トークンを作成します。</summary>
    ''' <param name="raw">生値参照。</param>
    ''' <param name="pointer">生値ポインタ。</param>
    ''' <returns>キーと値トークン。</returns>
    Public Function LexicalKeyAndValue(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        Dim keys = LexicalKey(raw, pointer)
        If keys.Count <= 0 Then
            Throw New TomlSyntaxException($"キーが見つかりません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
        End If

        SkipChars(raw, pointer, ByteSpace, ByteTab)
        SkipChars(raw, pointer, ByteEqual)
        SkipChars(raw, pointer, ByteSpace, ByteTab)

        Dim value = LexicalValue(raw, pointer)

        Return New TomlKeyValueToken(TokenTypeEnum.KeyAndValue, raw.GetRange(start, pointer.Index - 1), keys.ToArray(), value)
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
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteDoubleQuot
                        If Not getTkn Then
                            res.Add(GetStringFromDoubleQuot(raw, pointer))
                            getTkn = True
                        Else
                            Throw New TomlSyntaxException($"キーの解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If

                    Case ByteSingleQuot
                        If Not getTkn Then
                            res.Add(GetStringFromSingleQuot(raw, pointer))
                            getTkn = True
                        Else
                            Throw New TomlSyntaxException($"キーの解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If

                    Case ByteUpA To ByteUpZ, ByteLowA To ByteLowZ, ByteCh0 To ByteCh9, ByteUnderBar, ByteHyphen
                        If Not getTkn Then
                            res.Add(GetKeyChar(raw, pointer))
                            getTkn = True
                        Else
                            Throw New TomlSyntaxException($"キーの解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If

                    Case ByteDot
                        pointer.Skip(1)
                        start = pointer.Index
                        getTkn = False

                    Case ByteSpace, ByteTab
                        pointer.Skip(1)

                    Case Else
                        Return res
                End Select
            Else
                pointer.Skip(cs.skip)
            End If
        Loop
        Throw New TomlSyntaxException($"キーの解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    Private Function GetStringFromDoubleQuot(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        If pointer.Peek(0) = ByteDoubleQuot AndAlso pointer.Peek(1) = ByteDoubleQuot AndAlso pointer.Peek(2) = ByteDoubleQuot Then
            pointer.Skip(3)

            Do While Not pointer.IsEnd
                Dim cs = pointer.GetCurrentByteAndSkip()
                If cs.skip = 1 Then
                    If pointer.Peek(0) = ByteBKSlash AndAlso
                       pointer.Peek(1) = ByteDoubleQuot Then
                        pointer.Skip(2)
                    ElseIf pointer.Peek(0) = ByteDoubleQuot AndAlso
                       pointer.Peek(1) = ByteDoubleQuot AndAlso
                       pointer.Peek(2) = ByteDoubleQuot AndAlso
                       pointer.Peek(3) <> ByteDoubleQuot Then
                        pointer.Skip(3)
                        Return New TomlToken(TokenTypeEnum.LiteralString, raw.GetRange(start, pointer.Index - 1))
                    Else
                        pointer.Skip(1)
                    End If
                Else
                    pointer.Skip(cs.skip)
                End If
            Loop
            Throw New TomlSyntaxException($"文字列の終端が見つかりません。:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
        End If

        pointer.Skip(1)

        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                If pointer.Peek(0) = ByteBKSlash AndAlso
                       pointer.Peek(1) = ByteDoubleQuot Then
                    pointer.Skip(2)
                ElseIf pointer.Peek(0) = ByteDoubleQuot Then
                    pointer.Skip(1)
                    Return New TomlToken(TokenTypeEnum.LiteralString, raw.GetRange(start, pointer.Index - 1))
                Else
                    pointer.Skip(1)
                End If
            Else
                pointer.Skip(cs.skip)
            End If
        Loop
        Throw New TomlSyntaxException($"文字列の終端が見つかりません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    Private Function GetStringFromSingleQuot(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        If pointer.Peek(0) = ByteSingleQuot AndAlso pointer.Peek(1) = ByteSingleQuot AndAlso pointer.Peek(2) = ByteSingleQuot Then
            pointer.Skip(3)

            Do While Not pointer.IsEnd
                Dim cs = pointer.GetCurrentByteAndSkip()
                If cs.skip = 1 Then
                    If pointer.Peek(0) = ByteSingleQuot AndAlso
                       pointer.Peek(1) = ByteSingleQuot AndAlso
                       pointer.Peek(2) = ByteSingleQuot AndAlso
                       pointer.Peek(3) <> ByteSingleQuot Then
                        Dim chkd = raw.GetRange(start + 3, pointer.Index - 1)
                        For i As Integer = 0 To chkd.Length - 3
                            If chkd(i + 2) <> ByteSingleQuot Then
                                i += 2
                            ElseIf chkd(i + 1) <> ByteSingleQuot Then
                                i += 1
                            ElseIf chkd(i + 0) <> ByteSingleQuot Then
                            Else
                                Throw New TomlSyntaxException($"3 つ以上の一重引用符の連続は許可されません。:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                            End If
                        Next
                        pointer.Skip(3)
                        Return New TomlToken(TokenTypeEnum.LiteralString, raw.GetRange(start, pointer.Index - 1))
                    End If
                    pointer.Skip(1)
                Else
                    pointer.Skip(cs.skip)
                End If
            Loop
            Throw New TomlSyntaxException($"文字列の終端が見つかりません。:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
        End If

        pointer.Skip(1)

        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteSingleQuot
                        pointer.Skip(1)
                        Return New TomlToken(TokenTypeEnum.LiteralString, raw.GetRange(start, pointer.Index - 1))
                End Select
                pointer.Skip(1)
            Else
                pointer.Skip(cs.skip)
            End If
        Loop
        Throw New TomlSyntaxException($"文字列の終端が見つかりません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    Private Function GetKeyChar(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteUpA To ByteUpZ, ByteLowA To ByteLowZ, ByteCh0 To ByteCh9, ByteUnderBar, ByteHyphen
                        pointer.Skip(1)
                    Case Else
                        Return New TomlToken(TokenTypeEnum.KeyString, raw.GetRange(start, pointer.Index - 1))
                End Select
            Else
                Return New TomlToken(TokenTypeEnum.KeyString, raw.GetRange(start, pointer.Index - 1))
            End If
        Loop
        Return New TomlToken(TokenTypeEnum.KeyString, raw.GetRange(start, pointer.Index - 1))
    End Function

    Public Sub SkipChars(raw As RawSource, pointer As RawSource.Pointer, ParamArray ch As Byte())
        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 AndAlso Array.IndexOf(ch, cs.curByte) >= 0 Then
                pointer.Skip(1)
            Else
                Return
            End If
        Loop
    End Sub

    Public Function LexicalValue(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteDoubleQuot
                        Return GetStringFromDoubleQuot(raw, pointer)

                    Case ByteSingleQuot
                        Return GetStringFromSingleQuot(raw, pointer)

                    Case ByteCh0 To ByteCh9
                        Return GetNumber(raw, pointer)

                    Case BytePlus
                        Return GetPlusNumber(raw, pointer)

                    Case ByteMinus
                        Return GetMinusNumber(raw, pointer)

                    Case ByteLowT
                        Return GetTrueLiteral(raw, pointer)

                    Case ByteLowF
                        Return GetFalseLiteral(raw, pointer)

                    Case ByteLowI
                        Return GetInfLiteral(raw, pointer, pointer.Index)

                    Case ByteLowN
                        Return GetNanLiteral(raw, pointer, pointer.Index)

                    Case ByteLBacket
                        Return GetArray(raw, pointer)

                    Case ByteLBrace
                        Return GetInlineTable(raw, pointer)

                    Case Else
                        Exit Do
                End Select
            Else
                pointer.Skip(cs.skip)
            End If
        Loop
        Throw New TomlSyntaxException($"値の解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    Private Function GetTrueLiteral(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        If pointer.Peek(0) = AscW("t"c) AndAlso
           pointer.Peek(1) = AscW("r"c) AndAlso
           pointer.Peek(2) = AscW("u"c) AndAlso
           pointer.Peek(3) = AscW("e"c) Then
            pointer.Skip(4)
            Return New TomlToken(TokenTypeEnum.TrueLiteral, raw.GetRange(start, pointer.Index - 1))
        End If
        Throw New TomlSyntaxException($"解析できないリテラルです:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    Private Function GetFalseLiteral(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index

        If pointer.Peek(0) = AscW("f"c) AndAlso
           pointer.Peek(1) = AscW("a"c) AndAlso
           pointer.Peek(2) = AscW("l"c) AndAlso
           pointer.Peek(3) = AscW("s"c) AndAlso
           pointer.Peek(4) = AscW("e"c) Then
            pointer.Skip(5)
            Return New TomlToken(TokenTypeEnum.FalseLiteral, raw.GetRange(start, pointer.Index - 1))
        End If
        Throw New TomlSyntaxException($"解析できないリテラルです:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    Private Function GetInfLiteral(raw As RawSource, pointer As RawSource.Pointer, start As Integer) As TomlToken
        If pointer.Peek(0) = AscW("i"c) AndAlso
           pointer.Peek(1) = AscW("n"c) AndAlso
           pointer.Peek(2) = AscW("f"c) Then
            pointer.Skip(3)
            Return New TomlToken(TokenTypeEnum.InfLiteral, raw.GetRange(start, pointer.Index - 1))
        End If
        Throw New TomlSyntaxException($"解析できないリテラルです:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    Private Function GetNanLiteral(raw As RawSource, pointer As RawSource.Pointer, start As Integer) As TomlToken
        If pointer.Peek(0) = AscW("n"c) AndAlso
           pointer.Peek(1) = AscW("a"c) AndAlso
           pointer.Peek(2) = AscW("n"c) Then
            pointer.Skip(3)
            Return New TomlToken(TokenTypeEnum.NanLiteral, raw.GetRange(start, pointer.Index - 1))
        End If
        Throw New TomlSyntaxException($"解析できないリテラルです:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    Private Function GetNumber(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Return GetNumber(raw, pointer, pointer.Index, True)
    End Function



    Private Function GetDateTime(raw As RawSource, pointer As RawSource.Pointer, start As Integer) As TomlToken
        Dim tmPtr = raw.GetPointer(start)
        If tmPtr.Peek(0) >= ByteCh0 AndAlso tmPtr.Peek(0) <= ByteCh9 AndAlso
           tmPtr.Peek(1) >= ByteCh0 AndAlso tmPtr.Peek(1) <= ByteCh9 AndAlso
           tmPtr.Peek(2) >= ByteCh0 AndAlso tmPtr.Peek(2) <= ByteCh9 AndAlso
           tmPtr.Peek(3) >= ByteCh0 AndAlso tmPtr.Peek(3) <= ByteCh9 AndAlso
           tmPtr.Peek(4) = ByteHyphen AndAlso
           tmPtr.Peek(5) >= ByteCh0 AndAlso tmPtr.Peek(5) <= ByteCh1 AndAlso
           tmPtr.Peek(6) >= ByteCh0 AndAlso tmPtr.Peek(6) <= ByteCh9 AndAlso
           tmPtr.Peek(7) = ByteHyphen AndAlso
           tmPtr.Peek(8) >= ByteCh0 AndAlso tmPtr.Peek(8) <= ByteCh3 AndAlso
           tmPtr.Peek(9) >= ByteCh0 AndAlso tmPtr.Peek(9) <= ByteCh9 Then
            pointer.Skip(6)
            If pointer.Peek(0) <> ByteUpT AndAlso (pointer.Peek(0) <> ByteSpace OrElse pointer.Peek(1) < ByteCh0 OrElse pointer.Peek(1) > ByteCh9) Then
                Return New TomlToken(TokenTypeEnum.DateLiteral, raw.GetRange(start, pointer.Index - 1))
            End If

            pointer.Skip(1)
            Dim timePtr = pointer.Index
            pointer.Skip(2)
            GetTime(raw, pointer, timePtr)
            If pointer.Peek(0) = ByteUpZ Then
                pointer.Skip(1)
                Return New TomlToken(TokenTypeEnum.DateLiteral, raw.GetRange(start, pointer.Index - 1))
            ElseIf pointer.Peek(0) <> BytePlus AndAlso pointer.Peek(0) <> ByteMinus Then
                Return New TomlToken(TokenTypeEnum.DateLiteral, raw.GetRange(start, pointer.Index - 1))
            End If

            pointer.Skip(1)
            If pointer.Peek(0) >= ByteCh0 AndAlso pointer.Peek(0) <= ByteCh9 AndAlso
               pointer.Peek(1) >= ByteCh0 AndAlso pointer.Peek(1) <= ByteCh9 AndAlso
               pointer.Peek(2) = ByteColon AndAlso
               pointer.Peek(3) >= ByteCh0 AndAlso pointer.Peek(3) <= ByteCh9 AndAlso
               pointer.Peek(4) >= ByteCh0 AndAlso pointer.Peek(4) <= ByteCh9 Then
                pointer.Skip(5)
                Return New TomlToken(TokenTypeEnum.DateLiteral, raw.GetRange(start, pointer.Index - 1))
            End If
        End If
        Throw New TomlSyntaxException($"日付の解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    Private Function GetTime(raw As RawSource, pointer As RawSource.Pointer, start As Integer) As TomlToken
        Dim tmPtr = raw.GetPointer(start)
        If tmPtr.Peek(0) >= ByteCh0 AndAlso tmPtr.Peek(0) <= ByteCh9 AndAlso
           tmPtr.Peek(1) >= ByteCh0 AndAlso tmPtr.Peek(1) <= ByteCh9 AndAlso
           tmPtr.Peek(2) = ByteColon AndAlso
           tmPtr.Peek(3) >= ByteCh0 AndAlso tmPtr.Peek(3) <= ByteCh9 AndAlso
           tmPtr.Peek(4) >= ByteCh0 AndAlso tmPtr.Peek(4) <= ByteCh9 AndAlso
           tmPtr.Peek(5) = ByteColon AndAlso
           tmPtr.Peek(6) >= ByteCh0 AndAlso tmPtr.Peek(6) <= ByteCh9 AndAlso
           tmPtr.Peek(7) >= ByteCh0 AndAlso tmPtr.Peek(7) <= ByteCh9 Then
            pointer.Skip(6)
            If pointer.Peek(0) <> ByteDot Then
                Return New TomlToken(TokenTypeEnum.TimeLiteral, raw.GetRange(start, pointer.Index - 1))
            End If
            pointer.Skip(1)
            Do While Not pointer.IsEnd
                Dim cs = pointer.GetCurrentByteAndSkip()
                If cs.skip = 1 Then
                    Select Case cs.curByte
                        Case ByteCh0 To ByteCh9
                            pointer.Skip(1)
                        Case Else
                            Return New TomlToken(TokenTypeEnum.TimeLiteral, raw.GetRange(start, pointer.Index - 1))
                    End Select
                Else
                    Throw New TomlSyntaxException($"時間の解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                End If
            Loop
            Return New TomlToken(TokenTypeEnum.TimeLiteral, raw.GetRange(start, pointer.Index - 1))
        Else
            Throw New TomlSyntaxException($"時間の解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
        End If
    End Function

#Region "数値"

    Private Function GetPlusNumber(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        pointer.Skip(1)
        If pointer.Peek(0) = ByteLowI Then
            Return GetInfLiteral(raw, pointer, start)
        ElseIf pointer.Peek(0) = ByteLowN Then
            Return GetNanLiteral(raw, pointer, start)
        Else
            Return GetNumber(raw, pointer, start, False)
        End If
    End Function

    Private Function GetMinusNumber(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        Dim start = pointer.Index
        pointer.Skip(1)
        If pointer.Peek(0) = ByteLowI Then
            Return GetInfLiteral(raw, pointer, start)
        ElseIf pointer.Peek(0) = ByteLowN Then
            Return GetNanLiteral(raw, pointer, start)
        Else
            Return GetNumber(raw, pointer, start, False)
        End If
    End Function

    Private Function GetNumber(raw As RawSource, pointer As RawSource.Pointer, start As Integer, usePrefix As Boolean) As TomlToken
        If Not pointer.IsEnd Then
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteCh0
                        Select Case pointer.Peek(1)
                            Case ByteLowX
                                If usePrefix Then
                                    Return GetHexadecimal(raw, pointer, start, 16, Function(v) (v >= ByteCh0 AndAlso v <= ByteCh9) OrElse (v >= ByteUpA AndAlso v <= ByteUpF) OrElse (v >= ByteLowA AndAlso v <= ByteLowF))
                                Else
                                    Throw New TomlSyntaxException($"符号付きの16進数は許可されません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                                End If
                            Case ByteLowO
                                If usePrefix Then
                                    Return GetHexadecimal(raw, pointer, start, 22, Function(v) v >= ByteCh0 AndAlso v <= ByteCh7)
                                Else
                                    Throw New TomlSyntaxException($"符号付きの8進数は許可されません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                                End If
                            Case ByteLowB
                                If usePrefix Then
                                    Return GetHexadecimal(raw, pointer, start, 64, Function(v) v >= ByteCh0 AndAlso v <= ByteCh1)
                                Else
                                    Throw New TomlSyntaxException($"符号付きの2進数は許可されません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                                End If
                            Case ByteCh0 To ByteCh9, ByteUnderBar
                                If pointer.Peek(2) <> ByteColon Then
                                    Throw New TomlSyntaxException($"先行ゼロは許可されません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                                End If
                        End Select
                End Select
            End If
        End If

        Dim tknType = TokenTypeEnum.NumberLiteral
        Dim prev As Byte = 0
        Dim dotCount As Integer = 0
        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteCh0 To ByteCh9
                        pointer.Skip(1)
                    Case ByteLowE, ByteUpE
                        pointer.Skip(1)
                        Return GetExponents(raw, pointer, start)
                    Case ByteDot
                        If (prev < ByteCh0 OrElse prev > ByteCh9) OrElse
                           (pointer.Peek(1) < ByteCh0 OrElse pointer.Peek(1) > ByteCh9) Then
                            Throw New TomlSyntaxException($"小数点の両側には1桁以上の数字が必要です:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If
                        If dotCount > 0 Then
                            Throw New TomlSyntaxException($"小数点は1つまでです:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If
                        tknType = TokenTypeEnum.RealLiteral
                        pointer.Skip(1)
                    Case ByteUnderBar
                        If (prev < ByteCh0 OrElse prev > ByteCh9) OrElse
                           (pointer.Peek(1) < ByteCh0 OrElse pointer.Peek(1) > ByteCh9) Then
                            Throw New TomlSyntaxException($"アンダーバーの両側には1桁以上の数字が必要です:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If
                        pointer.Skip(1)
                    Case ByteSpace, ByteTab, ByteCR, ByteLF, ByteComma, ByteRBacket, ByteRBrace
                        Return New TomlToken(tknType, raw.GetRange(start, pointer.Index - 1))
                    Case ByteHyphen
                        Return GetDateTime(raw, pointer, start)
                    Case ByteColon
                        Return GetTime(raw, pointer, start)
                    Case Else
                        Throw New TomlSyntaxException($"数値の解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                End Select
                prev = cs.curByte
            Else
                Throw New TomlSyntaxException($"数値に全角文字は使用できません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
            End If
        Loop
        Return New TomlToken(tknType, raw.GetRange(start, pointer.Index - 1))
    End Function

    ''' <summary>指数表現の数値を取得します。</summary>
    ''' <param name="raw">生値ソース。</param>
    ''' <param name="pointer">ポインター。</param>
    ''' <param name="start">開始位置。</param>
    ''' <returns>トークン。</returns>
    Private Function GetExponents(raw As RawSource, pointer As RawSource.Pointer, start As Integer) As TomlToken
        Dim middle = pointer.Index
        Dim isFirst As Boolean = True
        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case BytePlus, ByteMinus
                        ' 符号ならば次へ（ただし、先頭以外はエラー）
                        If Not isFirst Then
                            Throw New TomlSyntaxException($"指数表記に間違いがあります:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If
                        pointer.Skip(1)

                    Case ByteCh0 To ByteCh9
                        ' 数値ならば次へ
                        pointer.Skip(1)

                    Case ByteSpace, ByteTab, ByteCR, ByteLF, ByteComma, ByteRBacket, ByteRBrace
                        ' 区切りに使用できる文字ならばトークン作成
                        Dim subTkn = {
                            New TomlToken(TokenTypeEnum.Other, raw.GetRange(start, middle - 2)),
                            New TomlToken(TokenTypeEnum.Other, raw.GetRange(middle, pointer.Index - 1))
                        }
                        Return New TomlHasSubToken(TokenTypeEnum.RealExpLiteral, raw.GetRange(start, pointer.Index - 1), subTkn)

                    Case Else
                        ' 数値以外の文字が来た場合は例外
                        Throw New TomlSyntaxException($"数値の解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                End Select
            Else
                Throw New TomlSyntaxException($"数値に全角文字は使用できません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
            End If
            isFirst = False
        Loop

        ' 最後まで到達した場合はトークン作成
        Dim lsubTkn = {
            New TomlToken(TokenTypeEnum.Other, raw.GetRange(start, middle - 2)),
            New TomlToken(TokenTypeEnum.Other, raw.GetRange(middle, pointer.Index - 1))
        }
        Return New TomlHasSubToken(TokenTypeEnum.RealExpLiteral, raw.GetRange(start, pointer.Index - 1), lsubTkn)
    End Function

    ''' <summary>2, 8, 16進数リテラルを取得します。</summary>
    ''' <param name="raw">生値ソース。</param>
    ''' <param name="pointer">ポインター。</param>
    ''' <param name="start">開始位置。</param>
    ''' <param name="limit">桁数制限。</param>
    ''' <param name="checker">チェッカー。</param>
    ''' <returns>トークン。</returns>
    Private Function GetHexadecimal(raw As RawSource,
                                    pointer As RawSource.Pointer,
                                    start As Integer,
                                    limit As Integer,
                                    checker As Func(Of Byte, Boolean)) As TomlToken
        ' 0x、0o、0b をスキップ
        pointer.Skip(2)

        Dim prev As Byte = 0
        Dim numCount As Integer = 0
        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                If checker(cs.curByte) Then
                    ' 桁数をカウントし、制限を超えたら例外
                    numCount += 1
                    If numCount > limit Then
                        Throw New TomlSyntaxException($"桁数を超えています:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                    End If
                    pointer.Skip(1)

                ElseIf cs.curByte = ByteUnderBar Then
                    ' _ は両端が数値であることを確認して次へ
                    If Not checker(prev) OrElse Not checker(pointer.Peek(1)) Then
                        Throw New TomlSyntaxException($"アンダーバーの両側には1桁以上の数字が必要です:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                    End If
                    pointer.Skip(1)

                ElseIf cs.curByte = ByteSpace OrElse
                       cs.curByte = ByteTab OrElse
                       cs.curByte = ByteCR OrElse
                       cs.curByte = ByteLF OrElse
                       cs.curByte = ByteComma OrElse
                       cs.curByte = ByteRBacket OrElse
                       cs.curByte = ByteRBrace Then
                    ' 区切りに使用できる文字ならばトークン作成
                    Return New TomlToken(TokenTypeEnum.NumberHexLiteral, raw.GetRange(start, pointer.Index - 1))
                Else
                    Throw New TomlSyntaxException($"数値の解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                End If
                prev = cs.curByte
            Else
                Throw New TomlSyntaxException($"数値に全角文字は使用できません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
            End If
        Loop
        Return New TomlToken(TokenTypeEnum.NumberHexLiteral, raw.GetRange(start, pointer.Index - 1))
    End Function

#End Region

    ''' <summary>配列トークンを取得します。</summary>
    ''' <param name="raw">生値ソース。</param>
    ''' <param name="pointer">ポインター。</param>
    ''' <returns>トークン。</returns>
    Private Function GetArray(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        ' [ をスキップ
        Dim start = pointer.Index
        pointer.Skip(1)

        Dim items As New List(Of TomlToken)()
        Dim needSplit As Boolean = False
        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteSpace, ByteTab, ByteCR, ByteLF
                        ' スペース、タブ、改行はスキップ
                        pointer.Skip(1)

                    Case ByteComma
                        ' カンマは次の要素のフラグを設定
                        If needSplit Then
                            pointer.Skip(1)
                            needSplit = False
                        Else
                            Throw New TomlSyntaxException($"要素が正しく , で区切られていません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If

                    Case ByteSharp
                        ' コメントを読み込む
                        items.Add(LexicalComment(raw, pointer))

                    Case ByteRBacket
                        ' ] で項目読み込み後ならばトークン作成
                        pointer.Skip(1)
                        Return New TomlHasSubToken(TokenTypeEnum.Array, raw.GetRange(start, pointer.Index - 1), items.ToArray())

                    Case Else
                        ' それ以外はキーと値を読み込む（先頭かガンマ後）
                        If Not needSplit Then
                            items.Add(LexicalValue(raw, pointer))
                            needSplit = True
                        Else
                            Throw New TomlSyntaxException($"配列の要素がカンマで区切られていません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If
                End Select
            Else
                Throw New TomlSyntaxException($"配列の解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
            End If
        Loop
        Throw New TomlSyntaxException($"配列が閉じられていません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

    ''' <summary>インラインテーブルトークンを取得します。</summary>
    ''' <param name="raw">生値ソース。</param>
    ''' <param name="pointer">ポインター。</param>
    ''' <returns>トークン。</returns>
    Private Function GetInlineTable(raw As RawSource, pointer As RawSource.Pointer) As TomlToken
        ' { をスキップ
        Dim start = pointer.Index
        pointer.Skip(1)

        Dim items As New List(Of TomlToken)()
        Dim needSplit As Boolean = False
        Do While Not pointer.IsEnd
            Dim cs = pointer.GetCurrentByteAndSkip()
            If cs.skip = 1 Then
                Select Case cs.curByte
                    Case ByteSpace, ByteTab
                        ' スペース、タブはスキップ
                        pointer.Skip(1)

                    Case ByteComma
                        ' カンマは次の要素のフラグを設定
                        If needSplit Then
                            pointer.Skip(1)
                            needSplit = False
                        Else
                            Throw New TomlSyntaxException($"要素が正しく , で区切られていません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If

                    Case ByteRBrace
                        ' } で項目読み込み後ならばトークン作成
                        If needSplit Then
                            pointer.Skip(1)
                            Return New TomlHasSubToken(TokenTypeEnum.Inline, raw.GetRange(start, pointer.Index - 1), items.ToArray())
                        Else
                            Throw New TomlSyntaxException($"インラインテーブルの要素がありません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If

                    Case Else
                        ' それ以外はキーと値を読み込む（先頭かガンマ後）
                        If Not needSplit Then
                            items.Add(LexicalKeyAndValue(raw, pointer))
                            needSplit = True
                        Else
                            Throw New TomlSyntaxException($"インラインテーブルの要素がカンマで区切られていません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
                        End If
                End Select
            Else
                Throw New TomlSyntaxException($"インラインテーブルの解析に失敗しました:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
            End If
        Loop
        Throw New TomlSyntaxException($"インラインテーブルが閉じられていません:{raw.GetPointer(start).TakeChar(pointer.Index - start + 1)}")
    End Function

End Module
