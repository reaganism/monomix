using Mono.Cecil.Cil;
using Reaganism.Recon.Matching;
using static Reaganism.Recon.Matching.PatternBuilder;

namespace Reaganism.MonoMix.Cil.Match;

// Instruction for demonstration purposes.
public static class PatternBuilderExtensions {
    public sealed class OpCodePatternBuilder(OpCode opCode) : PatternBuilder<Instruction> {
        private sealed class Pattern(OpCode opCode) : Pattern<Instruction> {
            public override int MinimumLength => 1;

            public override bool Match(MatchContext<Instruction> ctx) {
                var success = ctx.Cursor.Next?.OpCode == opCode;
                ctx.Advance();
                return success;
            }
        }

        public override Pattern<Instruction> Build() {
            return new Pattern(opCode);
        }
    }

    #region OpCode
    public static Pattern<Instruction> OpCode(OpCode opCode) {
        return new OpCodePatternBuilder(opCode).Build();
    }

    public static PatternBuilder<Instruction> OpCode(this PatternBuilder<Instruction> builder, OpCode opCode) {
        return new OpCodePatternBuilder(opCode);
    }
    #endregion

    #region Optional
    public static Pattern<Instruction> Optional(OpCode opCode) {
        return new PatternBuilder<Instruction>.Optional(new OpCodePatternBuilder(opCode).Build()).Build();
    }

    public static PatternBuilder<Instruction> Optional(this PatternBuilder<Instruction> builder, OpCode opCode) {
        builder.Optional(new OpCodePatternBuilder(opCode).Build());
        return builder;
    }
    #endregion

    private static void Test() {
        Sequence<Instruction>(
            x => {
                x.Optional(OpCodes.Nop);
                x.Either(
                    OpCode(OpCodes.Ldsfld),
                    Sequence<Instruction>(
                        y => {
                            y.OpCode(OpCodes.Ldarg_0);
                            y.OpCode(OpCodes.Ldfld);
                        }
                    )
                );

                x.Optional(
                    Sequence<Instruction>(
                        y => {
                            y.OpCode(OpCodes.Stloc_0);
                            y.OpCode(OpCodes.Br_S);
                            y.OpCode(OpCodes.Ldloc_0);
                        }
                    )
                );

                x.Optional(OpCodes.Br_S);
                x.OpCode(OpCodes.Ret);
            }
        );
    }
}
