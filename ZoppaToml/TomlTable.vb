Option Strict On
Option Explicit On

Imports System.Dynamic

''' <summary>テーブルを表すクラスです。</summary>
Public NotInheritable Class TomlTable
    Inherits DynamicObject
    Implements ITomlElement

    ''' <summary>子要素を取得します。</summary>
    Public ReadOnly Property Children As New SortedList(Of String, ITomlElement)()

    ''' <summary>値の型を取得します。</summary>
    ''' <returns>値の型。</returns>
    Public ReadOnly Property GetValueType As Type Implements ITomlElement.GetValueType
        Get
            Return Me.GetType()
        End Get
    End Property

    ''' <summary>値の型を取得します。</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>値の型。</returns>
    Public ReadOnly Property GetValueType(index As Integer) As Type Implements ITomlElement.GetValueType
        Get
            Return Me.GetValueType()
        End Get
    End Property

    ''' <summary>値の数を取得します。</summary>
    ''' <returns>値の数。</returns>
    Public ReadOnly Property Length As Integer Implements ITomlElement.Length
        Get
            Return Me.Children.Count
        End Get
    End Property

    ''' <summary>要素を参照します。</summary>
    ''' <param name="keyName">要素名。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(keyName As String) As ITomlElement Implements ITomlElement.Items
        Get
            If Me.Children.ContainsKey(keyName) Then
                Dim res = TryCast(Me.Children(keyName), ITomlElement)
                If res IsNot Nothing Then
                    Return res
                End If
            End If
            Throw New KeyNotFoundException(GetMessage("E023"))
        End Get
    End Property

    ''' <summary>要素を参照します（配列）</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(index As Integer) As ITomlElement Implements ITomlElement.Items
        Get
            Throw New NotImplementedException(GetMessage("E024"))
        End Get
    End Property

    ''' <summary>値を取得します。</summary>
    ''' <returns>値。</returns>
    Public Function [Get]() As Object Implements ITomlElement.Get
        Return Me
    End Function

    ''' <summary>値を取得します。</summary>
    ''' <typeparam name="T">型。</typeparam>
    ''' <param name="index">インデックス。</param>
    ''' <returns>値。</returns>
    Public Function GetValue(Of T)() As T Implements ITomlElement.GetValue
        Return DirectCast(DirectCast(Me, Object), T)
    End Function

    ''' <summary>要素を参照します（配列）</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>要素。</returns>
    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlElement.GetValue
        Return Me.GetValue(Of T)()
    End Function

    ''' <summary>.で結合したキー名から要素を取得します。</summary>
    ''' <param name="keyNames">キー名。</param>
    ''' <returns>要素。</returns>
    Public Function GetByKeyNames(keyNames As String) As ITomlElement
        ' キーの名称を分割
        Dim keynm = LexicalKey(keyNames)

        ' キーの名称をたどってテーブルを取得
        '
        ' 1. キーを取得
        ' 2. キーのテーブルを取得
        Dim current As TomlTable = Me
        For i As Integer = 0 To keynm.Count - 2
            Dim key = keynm(i).GetKeyString()           ' 1

            If current.Children.ContainsKey(key) Then   ' 2
                Dim res = TryCast(current.Children(key), TomlTable)
                If res IsNot Nothing Then
                    current = res
                Else
                    Throw New KeyNotFoundException(GetMessage("E025", key))
                End If
            Else
                Throw New KeyNotFoundException(GetMessage("E025", key))
            End If
        Next

        ' 最後のキーから要素を取得
        Dim lastKey = keynm(keynm.Count - 1).GetKeyString()
        If current.Children.ContainsKey(lastKey) Then
            Dim res = TryCast(current.Children(lastKey), ITomlElement)
            If res IsNot Nothing Then
                Return res
            End If
        End If
        Throw New KeyNotFoundException(GetMessage("E026", lastKey))
    End Function

    ''' <summary>列挙子を取得します。</summary>
    ''' <returns>列挙子。</returns>
    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Dim res As New List(Of ITomlElement)(Me.Children.Values)
        Return New TomlCollectionEnumerator(Of ITomlElement)(res)
    End Function

    ''' <summary>動的メンバーを取得します。</summary>
    ''' <param name="binder">バインダー。</param>
    ''' <param name="result">取得値。</param>
    ''' <returns>取得出来たら真。</returns>
    Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
        If Me.Children.ContainsKey(binder.Name) Then
            result = Me.Children(binder.Name)
            Return True
        Else
            Return False
        End If
    End Function

End Class
