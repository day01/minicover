﻿using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;
using Mono.Cecil.Tests;
using Shouldly;
using Xunit;

namespace MiniCover.UnitTests.Instrumentation.IlProcessorExtensionsTests
{
    public class EncapsulateMethodBodyWithTryFinallyBlockShould : BaseTestFixture
    {
                [Fact]
        public void EncapsulateWithTryFinallyShould_Not_EncapsulateCtor_with_a_field_initialized_out_side_the_constructor()
        {
            var expectedIl = @".locals ()
IL_0000: nop
.try
{
IL_0001: nop
IL_0002: ldarg.0 // this
IL_0003: ldc.i4.5
IL_0004: stfld System.Int32 Sample.TryFinally.AClassWithFieldInitializedOutsideConstructor::value
IL_0009: leave.s IL_000d
}
catch System.Exception
{
IL_000b: rethrow
}
IL_000d: ldarg.0 // this
IL_000e: call System.Void System.Object::.ctor()
IL_0013: nop
IL_0014: ret";

            TestModule("Sample.dll", module =>
            {
                var type = module.GetType("Sample.TryFinally.AClassWithFieldInitializedOutsideConstructor");
                var constructor = type.GetMethod(".ctor");
                Assert.NotNull(constructor);
                ApplyTryFinally(constructor, module);
                Normalize(Formatter.FormatMethodBody(constructor)).ShouldBe(Normalize(expectedIl));
            });
        }

        [Fact]
        public void EncapsulateWithTryFinallyShould_Not_EncapsulateCtor_with_existing_try_finally_from_dll()
        {
            var expectedIl = @".locals init ()

// [8 8 - 9 52]
IL_0000: ldarg.0 // this
IL_0001: call System.Void System.Object::.ctor()
.try
{
IL_0006: nop
IL_0007: nop

// [9 9 - 9 10]
IL_0008: nop
.try
{

// [11 11 - 13 14]
IL_0009: nop

// [12 12 - 17 27]
IL_000a: ldarg.0 // this
IL_000b: ldc.i4.5
IL_000c: stfld System.Int32 Sample.TryFinally.AClassWithATryFinallyInConstructor::value

// [13 13 - 13 14]
IL_0011: nop
IL_0012: leave.s IL_001e
}
finally
{

// [15 15 - 13 14]
IL_0014: nop

// [16 16 - 17 24]
IL_0015: ldarg.0 // this
IL_0016: call System.Void Sample.TryFinally.AClassWithATryFinallyInConstructor::Exit()
IL_001b: nop

// [17 17 - 13 14]
IL_001c: nop
IL_001d: endfinally
}
IL_001e: leave.s IL_0022
}
finally
{
IL_0020: nop
IL_0021: endfinally
}
IL_0022: ret";

            TestModule("Sample.dll", module =>
            {
                var type = module.GetType("Sample.TryFinally.AClassWithATryFinallyInConstructor");
                var constructor = type.GetMethod(".ctor");
                Assert.NotNull(constructor);
                ApplyTryFinally(constructor, module);
                Normalize(Formatter.FormatMethodBody(constructor)).ShouldBe(Normalize(expectedIl));
            }, typeof(PdbReaderProvider));
        }

        [Fact]
        public void EncapsulateWithTryFinallyShould_correctly_encapsulate_a_methode_with_return_value_dll()
        {
            var expectedIl = @".locals init (System.Int32 V_0, System.Int32 V_1)
IL_0000: nop
.try
{

// [6 6 - 9 10]
IL_0001: nop

// [7 7 - 13 30]
IL_0002: ldarg.1
IL_0003: ldc.i4.2
IL_0004: mul
IL_0005: stloc.0
IL_0006: br.s IL_0008

// [8 8 - 9 10]
IL_0008: ldloc.0
IL_0009: stloc.1
IL_000a: leave.s IL_000e
}
finally
{
IL_000c: nop
IL_000d: endfinally
}
IL_000e: ldloc.1
IL_000f: ret";

            TestModule("Sample.dll", module =>
            {
                var type = module.GetType("Sample.TryFinally.AnotherClassWithoutTryFinally");
                var constructor = type.GetMethod("MultiplyByTwo");
                Assert.NotNull(constructor);
                ApplyTryFinally(constructor, module);
                Normalize(Formatter.FormatMethodBody(constructor)).ShouldBe(Normalize(expectedIl));
            }, typeof(PdbReaderProvider));
        }

        [Fact]
        public void EncapsulateWithTryFinallyShould_correctly_encapsulate_the_content_of_a_lambda()
        {
            var expectedIl = @".locals init (System.Int32 V_0, System.Int32 V_1)
IL_0000: nop
.try
{
IL_0001: nop
IL_0002: ldarg.1
IL_0003: ldsfld System.Func`2<System.Int32,System.Int32> Sample.TryFinally.ClassWithSimpleLambda/<>c::<>9__0_0
IL_0008: dup
IL_0009: brtrue.s IL_0022
IL_000b: pop
IL_000c: ldsfld Sample.TryFinally.ClassWithSimpleLambda/<>c Sample.TryFinally.ClassWithSimpleLambda/<>c::<>9
IL_0011: ldftn System.Int32 Sample.TryFinally.ClassWithSimpleLambda/<>c::<Add2ToEachValueAndSumThem>b__0_0(System.Int32)
IL_0017: newobj System.Void System.Func`2<System.Int32,System.Int32>::.ctor(System.Object,System.IntPtr)
IL_001c: dup
IL_001d: stsfld System.Func`2<System.Int32,System.Int32> Sample.TryFinally.ClassWithSimpleLambda/<>c::<>9__0_0
IL_0022: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Int32,System.Int32>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
IL_0027: call System.Int32 System.Linq.Enumerable::Sum(System.Collections.Generic.IEnumerable`1<System.Int32>)
IL_002c: stloc.0
IL_002d: br.s IL_002f
IL_002f: ldloc.0
IL_0030: stloc.1
IL_0031: leave.s IL_0035
}
finally
{
IL_0033: nop
IL_0034: endfinally
}
IL_0035: ldloc.1
IL_0036: ret";
            var expectedNestedTypeMethodIl = @".locals (System.Int32 V_0)
IL_0000: nop
.try
{
IL_0001: ldarg.1
IL_0002: ldc.i4.2
IL_0003: add
IL_0004: stloc.0
IL_0005: leave.s IL_0009
}
finally
{
IL_0007: nop
IL_0008: endfinally
}
IL_0009: ldloc.0
IL_000a: ret";

            TestModule("Sample.dll", module =>
            {
                var type = module.GetType("Sample.TryFinally.ClassWithSimpleLambda");
                type.HasNestedTypes.ShouldBeTrue();

                var method = type.GetMethod("Add2ToEachValueAndSumThem");
                method.ShouldNotBeNull();

                type.NestedTypes.Count.ShouldBe(1);
                var nestedType = type.NestedTypes.First();
                nestedType.Methods.Where(a => !a.IsConstructor).ShouldHaveSingleItem();
                var nestedTypeMethod = nestedType.Methods.First(a => !a.IsConstructor);
                ApplyTryFinally(method, module);
                ApplyTryFinally(nestedTypeMethod, module);
                Normalize(Formatter.FormatMethodBody(method)).ShouldBe(Normalize(expectedIl));
                Normalize(Formatter.FormatMethodBody(nestedTypeMethod)).ShouldBe(Normalize(expectedNestedTypeMethodIl));
            });
        }

        [Fact]
        public void EncapsulateWithTryFinallyShould_correctly_encapsulate_the_content_of_a_lambda_having_body()
        {
            var expectedIl = @".locals init (System.Int32 V_0)
IL_0000: nop
IL_0001: ldarg.1
IL_0002: ldsfld System.Func`2<System.Int32,System.Int32> Sample.TryFinally.ClassWithComplicatedLambda/<>c::<>9__0_0
IL_0007: dup
IL_0008: brtrue.s IL_0021
IL_000a: pop
IL_000b: ldsfld Sample.TryFinally.ClassWithComplicatedLambda/<>c Sample.TryFinally.ClassWithComplicatedLambda/<>c::<>9
IL_0010: ldftn System.Int32 Sample.TryFinally.ClassWithComplicatedLambda/<>c::<Add2ToEachValueAndSumThemWithConsoleWrite>b__0_0(System.Int32)
IL_0016: newobj System.Void System.Func`2<System.Int32,System.Int32>::.ctor(System.Object,System.IntPtr)
IL_001b: dup
IL_001c: stsfld System.Func`2<System.Int32,System.Int32> Sample.TryFinally.ClassWithComplicatedLambda/<>c::<>9__0_0
IL_0021: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Int32,System.Int32>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
IL_0026: call System.Int32 System.Linq.Enumerable::Sum(System.Collections.Generic.IEnumerable`1<System.Int32>)
IL_002b: stloc.0
IL_002c: br.s IL_002e
IL_002e: ldloc.0
IL_002f: ret";
            var expectedNestedTypeMethodIl = @".locals init (System.Int32 V_0, System.Int32 V_1)
IL_0000: nop
IL_0001: ldarg.1
IL_0002: ldc.i4.2
IL_0003: add
IL_0004: stloc.0
IL_0005: ldloc.0
IL_0006: call System.Void System.Console::WriteLine(System.Int32)
IL_000b: nop
IL_000c: ldloc.0
IL_000d: stloc.1
IL_000e: br.s IL_0010
IL_0010: ldloc.1
IL_0011: ret";

            TestModule("Sample.dll", module =>
            {
                var type = module.GetType("Sample.TryFinally.ClassWithComplicatedLambda");
                type.HasNestedTypes.ShouldBeTrue();

                var method = type.GetMethod("Add2ToEachValueAndSumThemWithConsoleWrite");
                method.ShouldNotBeNull();

                type.NestedTypes.Count.ShouldBe(1);
                var nestedType = type.NestedTypes.First();
                nestedType.Methods.Where(a => !a.IsConstructor).ShouldHaveSingleItem();
                var nestedTypeMethod = nestedType.Methods.First(a => !a.IsConstructor);
                //ApplyTryFinally(method, module);
                Normalize(Formatter.FormatMethodBody(method)).ShouldBe(Normalize(expectedIl));
                Normalize(Formatter.FormatMethodBody(nestedTypeMethod)).ShouldBe(Normalize(expectedNestedTypeMethodIl));
            });
        }


        private void ApplyTryFinally(MethodDefinition methodDefinition, ModuleDefinition moduleDefinition)
        {
            var processor = methodDefinition.Body.GetILProcessor();
            var firstInstruction = methodDefinition.Body.Instructions.First();
            processor.EncapsulateMethodBodyWithTryFinallyBlock(firstInstruction, (ilProcessor, instruction) => { });
            
            processor.Body.OptimizeMacros();
        }
    }
}