Option Strict On
Option Explicit On

''' <summary>テーブルトークンを表すクラスです。</summary>
Public NotInheritable Class TomlDocument
    Implements ITomlItem

    ' ソースバイト配列
    Private ReadOnly mRaw As RawSource

    Private ReadOnly mRoot As TomlTable

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="raw">ソースバイト配列。</param>
    Public Sub New(raw As RawSource)
        Me.mRaw = raw

        Me.mRoot = Analisys(raw.Lexical())
    End Sub

    Public ReadOnly Property GetValueType As Type Implements ITomlItem.GetValueType
        Get
            Return Me.mRoot.GetValueType
        End Get
    End Property

    Public ReadOnly Property GetValueType(index As Integer) As Type Implements ITomlItem.GetValueType
        Get
            Return Me.mRoot.GetValueType(index)
        End Get
    End Property

    Public ReadOnly Property Length As Integer Implements ITomlItem.Length
        Get
            Return Me.mRoot.Length
        End Get
    End Property

    Default Public ReadOnly Property Item(index As String) As ITomlItem Implements ITomlItem.Item
        Get
            Return Me.mRoot.Item(index)
        End Get
    End Property

    ''' <summary>ファイルから読み込みます。</summary>
    ''' <param name="path">ファイルパス。</param>
    ''' <returns>TomlDocument。</returns>
    Public Shared Function LoadFromFile(path As String) As TomlDocument
        Dim fi As New IO.FileInfo(path)
        If fi.Exists Then
            Dim bom As Byte() = {0, 0, 0}
            Dim bytes As Byte()
            Using fr As New IO.FileStream(fi.FullName, IO.FileMode.Open, IO.FileAccess.Read)
                Dim bomln = fr.Read(bom, 0, 3)
                If bomln < 3 OrElse (bom(0) <> &HEF OrElse bom(1) <> &HBB OrElse bom(2) <> &HBF) Then
                    fr.Seek(0, IO.SeekOrigin.Begin)
                    bytes = New Byte(CInt(fr.Length - 1)) {}
                Else
                    bytes = New Byte(CInt(fr.Length - 4)) {}
                End If
                fr.Read(bytes, 0, bytes.Length)
            End Using
            Return New TomlDocument(New RawSource(bytes))
        Else
            Throw New IO.FileNotFoundException($"対象ファイルが存在しません:{path}")
        End If
    End Function

    ''' <summary>文字列から読み込みます。</summary>
    ''' <param name="inputStr">入力文字列。</param>
    ''' <returns>TomlDocument。</returns>
    Public Shared Function Load(inputStr As String) As TomlDocument
        Return New TomlDocument(New RawSource(inputStr))
    End Function

    ''' <summary>解析を行います。</summary>
    ''' <param name="token">トークンリスト。</param>
    ''' <returns>TomlTable。</returns>
    Private Shared Function Analisys(token As List(Of TomlToken)) As TomlTable
        Dim root As New TomlTable("")
        Dim current = root

        For Each tkn In token
            Select Case tkn.TokenType
                Case TomlToken.TokenTypeEnum.KeyAndValue
                    With DirectCast(tkn, TomlKeyValueToken)
                        Dim t = TrackTableName(current, .Keys)
                        t.Children.Add(.Keys.Last().GetKeyString(), CreateTomlValue(.Value))
                    End With

                Case TomlToken.TokenTypeEnum.TableHeader
                    With DirectCast(tkn, TomlHasSubToken)
                        current = TrackTableName(root, .SubTokens, False)
                    End With

                Case TomlToken.TokenTypeEnum.TableArrayHeader
                    With DirectCast(tkn, TomlHasSubToken)
                        Dim t = TrackTableName(root, .SubTokens)
                        t.Children.Add(.SubTokens.Last().GetKeyString(), New TomlArray(tkn.Range))
                    End With
            End Select
        Next

        Return root
    End Function

    Private Shared Function TrackTableName(current As TomlTable, keys As TomlToken(), Optional isKeyAndValue As Boolean = True) As TomlTable
        For i As Integer = 0 To keys.Length + If(isKeyAndValue, -2, -1)
            Dim knm = keys(i).GetKeyString()
            If current.Children.ContainsKey(knm) Then
                current = DirectCast(current.Children(knm), TomlTable)
            Else
                Dim tbl As New TomlTable(knm)
                current.Children.Add(knm, tbl)
                current = tbl
            End If
        Next
        Return current
    End Function

    Public Function GetValue(Of T)() As T Implements ITomlItem.GetValue
        Return Me.mRoot.GetValue(Of T)()
    End Function

    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlItem.GetValue
        Return Me.mRoot.GetValue(Of T)(index)
    End Function

End Class
