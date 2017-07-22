using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InterfaceProxy
{
    public class ProxyImplement
    {
        private const MethodAttributes ImplicitImplementation =
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
        public virtual void Intercept()
        {
        }
        public static T HookUp<T>()
        {
            AssemblyName assemblyName = new AssemblyName("DataBuilderAssembly");
            AssemblyBuilder assemBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemBuilder.DefineDynamicModule("DataBuilderModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType("NewClass", TypeAttributes.Class, typeof(ProxyImplement));
            typeBuilder.AddInterfaceImplementation(typeof(T));
            var myMethodImpl = typeBuilder.DefineMethod("Add", ImplicitImplementation,
                typeof(int), new[] { typeof(int), typeof(int) });
            ILGenerator il = myMethodImpl.GetILGenerator();

            //il.Emit(OpCodes.Ldc_I4_1);
            //il.Emit(OpCodes.Newarr, typeof(object));
            //il.Emit(OpCodes.Stloc, arr);
            //il.Emit(OpCodes.Ldloc, arr);
            //il.Emit(OpCodes.Ldc_I4_0);
            //il.Emit(OpCodes.Ldc_I4, 500);
            //il.Emit(OpCodes.Castclass, typeof(object));
            //il.Emit(OpCodes.Stelem_I4);

            //il.Emit(OpCodes.Ldloc, arr);
            var mi = typeof(ProxyImplement).GetMethod("Intercept", new Type[] { });
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, mi);
            //var mi = typeof(Console).GetMethod("WriteLine", new Type[] { });
            //il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ldc_I4, 3);
            il.Emit(OpCodes.Ret);
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
        }
    }
}
