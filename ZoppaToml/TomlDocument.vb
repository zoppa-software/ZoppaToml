Option Strict On
Option Explicit On

''' <summary>テーブルトークンを表すクラスです。</summary>
Public NotInheritable Class TomlDocument

    ' 生値ソース
    Private ReadOnly mRaw As RawSource

    ' ルートテーブル
    Private ReadOnly mRoot As TomlTable

#Region "properties"

    ''' <summary>ドキュメントの要素の数を取得します。</summary>
    ''' <returns>要素の数。</returns>
    Public ReadOnly Property Count As Integer
        Get
            Return Me.mRoot.Length
        End Get
    End Property

    ''' <summary>要素を参照します。。</summary>
    ''' <param name="keyName">キーの名前。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(index As String) As ITomlElement
        Get
            Return Me.mRoot.Items(index)
        End Get
    End Property

#End Region

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="raw">生値ソース。</param>
    Private Sub New(raw As RawSource)
        Me.mRaw = raw
        Me.mRoot = Analisys(raw.Lexical())
    End Sub

    ''' <summary>ファイルから読み込みます。</summary>
    ''' <param name="path">ファイルパス。</param>
    ''' <returns>TomlDocument。</returns>
    Public Shared Function LoadFromFile(path As String) As TomlDocument
        Dim fi As New IO.FileInfo(path)
        If fi.Exists Then
            ' BOMを取得、あれば除去
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

            ' インスタンスを生成
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
                    ' キーと値トークン
                    With DirectCast(tkn, TomlKeyValueToken)
                        current.TraverseTable(.Keys).Children.Add(.Keys.Last().GetKeyString(), CreateTomlValue(.Value))
                    End With

                Case TomlToken.TokenTypeEnum.TableHeader
                    ' テーブルヘッダトークン
                    With DirectCast(tkn, TomlHasSubToken)
                        current = root.TraverseTable(.SubTokens, False)
                    End With

                Case TomlToken.TokenTypeEnum.TableArrayHeader
                    ' テーブル配列ヘッダトークン
                    With DirectCast(tkn, TomlHasSubToken)
                        Dim parent = root.TraverseTable(.SubTokens)

                        Dim arrnm = .SubTokens.Last().GetKeyString()
                        If parent.Children.ContainsKey(arrnm) Then
                            current = TryCast(parent.Children(arrnm), TomlArray).GetNew()
                        Else
                            Dim arr = New TomlArray(tkn.Range)
                            parent.Children.Add(arrnm, arr)
                            current = arr.GetNew()
                        End If
                    End With
            End Select
        Next

        Return root
    End Function

    ''' <summary>.で結合したキー名から要素を取得します。</summary>
    ''' <param name="keyNames">キー名。</param>
    ''' <returns>要素。</returns>
    Public Function GetByKeyNames(keyNames As String) As ITomlElement
        Return Me.mRoot.GetByKeyNames(keyNames)
    End Function

End Class
