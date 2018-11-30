using BenchmarkDotNet.Attributes;
using Rdd.Domain;
using Rdd.Domain.Models;
using System;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Rdd.Benchmarks
{
    public class ExpressionInstanciator<TEntity> : IInstanciator<TEntity>
        where TEntity : class, new()
    {
        private readonly Func<TEntity> _new;

        public ExpressionInstanciator()
        {
            Expression<Func<TEntity>> NewExpression = () => new TEntity();
            _new = NewExpression.Compile();
        }
        public TEntity InstanciateNew(ICandidate<TEntity> candidate) => _new();
    }

    //https://blogs.msdn.microsoft.com/seteplia/2017/02/01/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/
    public class ILInstanciator<TEntity> : IInstanciator<TEntity>
        where TEntity : class, new()
    {
        private readonly Func<TEntity> _new;

        public ILInstanciator()
        {
            var type = typeof(TEntity);
            var constructor = Expression.New(type).Constructor;

            var method = new DynamicMethod("constructor", type, new Type[0], typeof(IEntityBase).Module, true);
            var ilGen = method.GetILGenerator();

            if (constructor == null) // value types
            {
                var temp = ilGen.DeclareLocal(type);
                ilGen.Emit(OpCodes.Ldloca, temp);
                ilGen.Emit(OpCodes.Initobj, type);
                ilGen.Emit(OpCodes.Ldloc, temp);
            }
            else
            {
                ilGen.Emit(OpCodes.Newobj, constructor);
            }

            ilGen.Emit(OpCodes.Ret);

            _new = (Func<TEntity>)method.CreateDelegate(typeof(Func<TEntity>));
        }

        public TEntity InstanciateNew(ICandidate<TEntity> candidate) => _new();
    }

    [MemoryDiagnoser]
    public class Instanciators
    {
        class User
        {
            public Guid Id { get; set; }
        }

        private readonly DefaultInstanciator<User> _default = new DefaultInstanciator<User>();
        private readonly ExpressionInstanciator<User> _expression = new ExpressionInstanciator<User>();
        private readonly ILInstanciator<User> _il = new ILInstanciator<User>();

        private static Func<User> Function => () => new User();

        [Benchmark]
        public void UserFactory()
        {
            Function();
        }

        [Benchmark(Baseline = true)]
        public void DefaultInstanciator()
        {
            _default.InstanciateNew(null);
        }

        [Benchmark]
        public void ExpressionInstanciator()
        {
            _expression.InstanciateNew(null);
        }

        [Benchmark]
        public void ILInstanciator()
        {
            _il.InstanciateNew(null);
        }
    }
}