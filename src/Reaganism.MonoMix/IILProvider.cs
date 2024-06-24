﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Reaganism.MonoMix;

/// <summary>
///     Minimally-contractual object that provides enumeration over
///     Mono.Cecil-compatible IL instructions.
/// </summary>
public interface IILProvider : IEnumerable<Instruction>, IDisposable {
    private sealed class ILProvider(IEnumerable<Instruction> instructions, params IDisposable[] disposables) : IILProvider {
        public IEnumerator<Instruction> GetEnumerator() {
            // ReSharper disable once NotDisposedResourceIsReturned
            return instructions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Dispose() {
            foreach (var disposable in disposables)
                disposable.Dispose();
        }
    }

    public static IILProvider FromMethodDefinition(MethodDefinition methodDefinition) {
        return FromMethodBody(methodDefinition.Body);
    }

    public static IILProvider FromMethodBody(MethodBody methodBody) {
        var instructions = methodBody.Instructions;
        return new ILProvider(instructions);
    }

    public static IILProvider FromILContext(ILContext context) {
        return FromMethodBody(context.Body);
    }

    public static IILProvider FromILCursor(ILCursor cursor) {
        return FromMethodBody(cursor.Body);
    }

    public static IILProvider FromMethodBaseAsCecil(MethodBase methodBase) {
        var dynDef = new DynamicMethodDefinition(methodBase);
        return new ILProvider(dynDef.Definition.Body.Instructions, dynDef);
    }

    public static IILProvider FromMethodBaseAsSystem(MethodBase methodBase) {
        var instructions = MethodBodyDisassembler.FromMethodBase(methodBase);
        return new ILProvider(instructions);
    }
}
