Option Strict On
Option Explicit On

Imports System.Runtime.CompilerServices
Imports System.Xml

Public Module TomlMethod

    <Extension>
    Public Function Load(raw As RawSource) As IList(Of ITomlValue)
        Dim res As New List(Of ITomlValue)()

        Dim pointer = raw.GetPointer()
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteSharp
                        res.Add(LoadComment(raw, pointer))

                    Case ByteLBacket
                        If pointer.Peek(1) = ByteLBacket Then
                            res.Add(LoadTableArrayHeader(raw, pointer))
                        Else
                            res.Add(LoadTableHeader(raw, pointer))
                        End If

                    Case ByteCR, ByteLF
                        res.Add(LoadLineFeed(raw, pointer))

                    Case ByteSpace, ByteTab
                        res.Add(LoadSpace(raw, pointer))

                    Case Else
                        pointer.Next()
                End Select
            Else
                pointer.Next()
            End If
        Loop
        Return res
    End Function

    Public Function LoadComment(raw As RawSource, pointer As RawSource.Pointer) As TomlComment
        Dim start = pointer.Index
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 AndAlso (c(0) = ByteCR OrElse c(0) = ByteLF) Then
                Return New TomlComment(raw.GetRange(start, pointer.Index))
            Else
                pointer.Next()
            End If
        Loop
        Return New TomlComment(raw.GetRange(start, pointer.Index))
    End Function

    Public Function LoadLineFeed(raw As RawSource, pointer As RawSource.Pointer) As TomlLineFeed
        Dim c = pointer.Current
        If pointer.Peek(0) = ByteCR AndAlso pointer.Peek(1) = ByteLF Then
            Dim res As New TomlLineFeed(raw.GetRange(pointer.Index, pointer.Index + 2))
            pointer.Skip(2)
            Return res
        Else
            Dim res As New TomlLineFeed(raw.GetRange(pointer.Index, pointer.Index + 1))
            pointer.Skip(1)
            Return res
        End If
    End Function

    Public Function LoadSpace(raw As RawSource, pointer As RawSource.Pointer) As TomlSpace
        Dim start = pointer.Index
        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 AndAlso (c(0) = ByteSpace OrElse c(0) = ByteTab) Then
                pointer.Next()
            Else
                Exit Do
            End If
        Loop
        Return New TomlSpace(raw.GetRange(start, pointer.Index))
    End Function

    Public Function LoadTableHeader(raw As RawSource, pointer As RawSource.Pointer) As TomlTableHeader
        Dim escChar As Byte = 0

        Dim start = pointer.Index
        pointer.Skip(1)

        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteDoubleQuot, ByteSingleQuot
                        If escChar = 0 Then
                            escChar = c(0)
                        ElseIf escChar = c(0) Then
                            escChar = 0
                        End If

                    Case ByteRBacket
                        If escChar = 0 Then
                            pointer.Skip(1)
                            Return New TomlTableHeader(raw.GetRange(start, pointer.Index + 1))
                        End If
                End Select
                pointer.Skip(1)
            Else
                pointer.Next()
            End If
        Loop
        Throw New TomlSyntaxException()
    End Function

    Public Function LoadTableArrayHeader(raw As RawSource, pointer As RawSource.Pointer) As TomlTableHeader
        Dim escChar As Byte = 0

        Dim start = pointer.Index
        pointer.Skip(2)

        Do While Not pointer.IsEnd
            Dim c = pointer.Current
            If c.Length = 1 Then
                Select Case c(0)
                    Case ByteDoubleQuot, ByteSingleQuot
                        If escChar = 0 Then
                            escChar = c(0)
                        ElseIf escChar = c(0) Then
                            escChar = 0
                        End If

                    Case ByteRBacket
                        If escChar = 0 Then
                            If pointer.Peek(1) = ByteRBacket Then
                                pointer.Skip(2)
                                Return New TomlTableHeader(raw.GetRange(start, pointer.Index + 1))
                            End If
                        End If
                End Select
                pointer.Skip(1)
            Else
                pointer.Next()
            End If
        Loop
        Throw New TomlSyntaxException()
    End Function

End Module
