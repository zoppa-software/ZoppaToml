Option Strict On
Option Explicit On

''' <summary>Tomlコレクション列挙子を表すクラスです。</summary>
Friend NotInheritable Class TomlCollectionEnumerator(Of T)
    Implements IEnumerator

    ' 対象のリスト
    Private ReadOnly mItems As List(Of T)

    ' インデックス
    Private mIndex As Integer = -1

    ''' <summary>新しいインスタンスを生成します。</summary>
    ''' <param name="items">対象のリスト。</param>
    Public Sub New(items As List(Of T))
        Me.mItems = items
    End Sub

    ''' <summary>現在の要素を取得します。</summary>
    ''' <returns>要素。</returns>
    Public ReadOnly Property Current As Object Implements IEnumerator.Current
        Get
            Return Me.mItems(Me.mIndex)
        End Get
    End Property

    ''' <summary>次の要素が存在するかどうかを判定します。</summary>
    ''' <returns>次の要素が存在する場合はTrue。</returns>
    Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
        Me.mIndex += 1
        Return Me.mIndex < Me.mItems.Count
    End Function

    ''' <summary>リセットします。</summary>
    Public Sub Reset() Implements IEnumerator.Reset
        Me.mIndex = 0
    End Sub

    ''' <summary>解放します。</summary>
    Public Sub Dispose()

    End Sub

End Class
