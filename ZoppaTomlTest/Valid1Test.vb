Imports System
Imports System.Runtime.Intrinsics.X86
Imports Xunit
Imports ZoppaToml
Imports ZoppaToml.TomlLexical

Public Class Valid1Test

    <Fact>
    Sub FloatTest1()
        Dim doc = TomlDocument.LoadFromFile("valid\float\exponent.toml")

        Assert.Equal(300.0, doc("lower").GetValue(Of Double)())
        Assert.Equal(300.0, doc("upper").GetValue(Of Double)())
        Assert.Equal(0.03, doc("neg").GetValue(Of Double)())
        Assert.Equal(300.0, doc("pos").GetValue(Of Double)())
        Assert.Equal(3.0, doc("zero").GetValue(Of Double)())
        Assert.Equal(310.0, doc("pointlower").GetValue(Of Double)())
        Assert.Equal(310.0, doc("pointupper").GetValue(Of Double)())
        Assert.Equal(-0.1, doc("minustenth").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest2()
        Dim doc = TomlDocument.LoadFromFile("valid\float\float.toml")

        Assert.Equal(3.14, doc("pi").GetValue(Of Double)())
        Assert.Equal(+3.14, doc("pospi").GetValue(Of Double)())
        Assert.Equal(-3.14, doc("negpi").GetValue(Of Double)())
        Assert.Equal(0.123, doc("zero-intpart").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest3()
        Dim doc = TomlDocument.LoadFromFile("valid\float\inf-and-nan.toml")

        Assert.Equal(Double.NaN, doc("nan").GetValue(Of Double)())
        Assert.Equal(-Double.NaN, doc("nan_neg").GetValue(Of Double)())
        Assert.Equal(Double.NaN, doc("nan_plus").GetValue(Of Double)())
        Assert.Equal(Double.PositiveInfinity, doc("infinity").GetValue(Of Double)())
        Assert.Equal(Double.NegativeInfinity, doc("infinity_neg").GetValue(Of Double)())
        Assert.Equal(Double.PositiveInfinity, doc("infinity_plus").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest4()
        Dim doc = TomlDocument.LoadFromFile("valid\float\long.toml")

        Assert.Equal(3.1415926535897931, doc("longpi").GetValue(Of Double)())
        Assert.Equal(-3.1415926535897931, doc("neglongpi").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest5()
        Dim doc = TomlDocument.LoadFromFile("valid\float\max-int.toml")

        Assert.Equal(9.00719925474099E+15, doc("max_float").GetValue(Of Double)())
        Assert.Equal(-9.00719925474099E+15, doc("min_float").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest6()
        Dim doc = TomlDocument.LoadFromFile("valid\float\underscore.toml")

        Assert.Equal(3141.5927, doc("before").GetValue(Of Double)())
        Assert.Equal(3141.5927, doc("after").GetValue(Of Double)())
        Assert.Equal(300000000000000.0, doc("exponent").GetValue(Of Double)())
    End Sub

    <Fact>
    Sub FloatTest7()
        Dim doc = TomlDocument.LoadFromFile("valid\float\zero.toml")

        Assert.Equal(0.0, doc("zero").GetValue(Of Double)())
        Assert.Equal(0.0, doc("signed-pos").GetValue(Of Double)())
        Assert.Equal(-0.0, doc("signed-neg").GetValue(Of Double)())
        Assert.Equal(0E0, doc("exponent").GetValue(Of Double)())
        Assert.Equal(0E00, doc("exponent-two-0").GetValue(Of Double)())
        Assert.Equal(+0E0, doc("exponent-signed-pos").GetValue(Of Double)())
        Assert.Equal(-0E0, doc("exponent-signed-neg").GetValue(Of Double)())
    End Sub

End Class
