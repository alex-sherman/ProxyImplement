using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace InterfaceProxy
{
    public abstract class ProxyImplement
    {
        private const MethodAttributes ImplicitImplementation =
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
        static MethodInfo interceptInfo = typeof(ProxyImplement).GetMethod("intercept", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new Type[] { typeof(string), typeof(List<object>) }, null);
        static MethodInfo interceptInfoVoid = typeof(ProxyImplement).GetMethod("interceptVoid", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new Type[] { typeof(string), typeof(List<object>) }, null);
        public abstract object Intercept(string methodName, object[] args);
        protected T intercept<T>(string methodName, List<object> args)
        {
            object output = Intercept(methodName, args.ToArray());
            if (output == null)
                return default(T);
            return (T)output;
        }
        protected void interceptVoid(string methodName, List<object> args)
        {
            Intercept(methodName, args.ToArray());
        }
        public static T HookUp<T, U>() where U : ProxyImplement
        {
            Type target = typeof(T);
            Type source = typeof(U);
            AssemblyName assemblyName = new AssemblyName("DataBuilderAssembly");
            AssemblyBuilder assemBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemBuilder.DefineDynamicModule("DataBuilderModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(target.Name + "_" + source.Name, TypeAttributes.Class, source);
            typeBuilder.AddInterfaceImplementation(target);
            foreach (var info in target.GetMethods())
            {
                var builder = typeBuilder.DefineMethod(info.Name, ImplicitImplementation,
                    info.ReturnType, info.GetParameters().Select(pinfo => pinfo.ParameterType).ToArray());
                Implement(builder, info);
            }
            
            Type type = typeBuilder.CreateType();
            return (T)type.GetConstructor(new Type[] { }).Invoke(new object[] { });
        }

        static void Implement(MethodBuilder builder, MethodInfo info)
        {
            var il = builder.GetILGenerator();
            LocalBuilder arr = il.DeclareLocal(typeof(List<object>), true);
            il.Emit(OpCodes.Newobj, typeof(List<object>).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);
            var paramInfos = info.GetParameters();
            for(int i = 0; i < paramInfos.Length; i++)
            {
                var paramInfo = paramInfos[i];
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldarg, i + 1);
                if (paramInfo.ParameterType.IsValueType)
                    il.Emit(OpCodes.Box, paramInfo.ParameterType);
                il.Emit(OpCodes.Callvirt, typeof(List<object>).GetMethod("Add"));
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, builder.Name);
            il.Emit(OpCodes.Ldloc_0);
            if (builder.ReturnType == typeof(void))
                il.Emit(OpCodes.Callvirt, interceptInfoVoid);
            else
                il.Emit(OpCodes.Callvirt, interceptInfo.MakeGenericMethod(info.ReturnType));
            il.Emit(OpCodes.Ret);
        }
    }
}
