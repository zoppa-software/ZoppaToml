Option Strict On
Option Explicit On

''' <summary>項目を表すインターフェイスです。</summary>
Public Interface ITomlElement

    ''' <summary>値の型を取得します。</summary>
    ''' <returns>値の型。</returns>
    ReadOnly Property GetValueType() As Type

    ''' <summary>値の型を取得します。</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>値の型。</returns>
    ReadOnly Property GetValueType(index As Integer) As Type

    ''' <summary>値の数を取得します。</summary>
    ''' <returns>値の数。</returns>
    ReadOnly Property Length() As Integer

    ''' <summary>値を取得します。</summary>
    ''' <returns>値。</returns>
    Function [Get]() As Object

    ''' <summary>値を取得します。</summary>
    ''' <typeparam name="T">型。</typeparam>
    ''' <returns>値。</returns>
    Function GetValue(Of T)() As T

    ''' <summary>値を取得します。</summary>
    ''' <typeparam name="T">型。</typeparam>
    ''' <param name="index">インデックス。</param>
    ''' <returns>値。</returns>
    Function GetValue(Of T)(index As Integer) As T

    ''' <summary>要素を参照します。</summary>
    ''' <param name="keyName">要素名。</param>
    ''' <returns>要素。</returns>
    Default ReadOnly Property Items(keyName As String) As ITomlElement

    ''' <summary>要素を参照します（配列）</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>要素。</returns>
    Default ReadOnly Property Items(index As Integer) As ITomlElement

End Interface
