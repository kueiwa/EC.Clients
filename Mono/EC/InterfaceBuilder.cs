using System;
using System.Reflection.Emit;
using System.Reflection;

namespace EC
{
    class InterfaceBuilder
    {
        private AssemblyBuilder mAssemblyBuilder;

        private TypeBuilder mTypeBuilder;

        private Type mServiceType;

        private string mClassName;

        private MethodInfo mChannelGetMethod;

        private Type mServiceImplType;

        private string mAssemblyName;

		private static System.Collections.Generic.Dictionary<Type,InterfaceBuilder> mBuilders = new System.Collections.Generic.Dictionary<Type, InterfaceBuilder>();

		public static T CreateInstance<T>()
		{
			return (T)CreateInstance (typeof(T));
		}

		public static Object CreateInstance(Type type)
		{
            lock (mBuilders)
            {
				InterfaceBuilder ib = null;
				if (!mBuilders.TryGetValue (type, out ib)) {
					ib = new InterfaceBuilder (type);
					ib.Builder ();
					mBuilders [type] = ib;
				}
				return ib.CreateInstance ();
			}
		}

        public InterfaceBuilder(Type type)
        {
            mServiceType = type;
            mAssemblyName =  mServiceType.Name+"_impl" ;
            mClassName = type.Name + "_impl";

        }

	



		public Object CreateInstance()
		{
			return Activator.CreateInstance (mServiceImplType);
		}

        private void Builder_CommunicationObject(TypeBuilder myTypeBuilder)
        {
            //builder IClient
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual |
               MethodAttributes.Final;
            FieldBuilder _mClient = myTypeBuilder.DefineField("_mClient", typeof(IClient), FieldAttributes.Private);
            MethodBuilder _mClientGet = myTypeBuilder.DefineMethod("EC.ICommunicationObject.get_Client", getSetAttr, typeof(IClient), Type.EmptyTypes);
            ILGenerator il = _mClientGet.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _mClient);
            il.Emit(OpCodes.Ret);

            myTypeBuilder.DefineMethodOverride(_mClientGet, typeof(ICommunicationObject).GetMethod("get_Client"));

            MethodBuilder _mClientSet = myTypeBuilder.DefineMethod("EC.ICommunicationObject.set_Client", getSetAttr, null, new Type[] { typeof(IClient) });
            il = _mClientSet.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, _mClient);
            il.Emit(OpCodes.Ret);
            myTypeBuilder.DefineMethodOverride(_mClientSet, typeof(ICommunicationObject).GetMethod("set_Client"));

            PropertyBuilder ClientProperty = myTypeBuilder.DefineProperty("EC.ICommunicationObject.Client", PropertyAttributes.HasDefault, typeof(IClient), Type.EmptyTypes);
            ClientProperty.SetGetMethod(_mClientGet);
            ClientProperty.SetSetMethod(_mClientSet);

            //builder IServiceChannel
            FieldBuilder _mChannel = myTypeBuilder.DefineField("_mChannel", typeof(IServiceChannel), FieldAttributes.Private);
            MethodBuilder _mChannelGet = myTypeBuilder.DefineMethod("EC.ICommunicationObject.get_Channel", getSetAttr, typeof(IServiceChannel), Type.EmptyTypes);
            il = _mChannelGet.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _mChannel);
            il.Emit(OpCodes.Ret);
            myTypeBuilder.DefineMethodOverride(_mChannelGet, typeof(ICommunicationObject).GetMethod("get_Channel"));

            MethodBuilder _mChannelSet = myTypeBuilder.DefineMethod("EC.ICommunicationObject.set_Channel", getSetAttr, null, new Type[] { typeof(IServiceChannel) });
            il = _mChannelSet.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, _mChannel);
            il.Emit(OpCodes.Ret);
            myTypeBuilder.DefineMethodOverride(_mChannelSet, typeof(ICommunicationObject).GetMethod("set_Channel"));

            PropertyBuilder ChannelProperty = myTypeBuilder.DefineProperty("EC.ICommunicationObject.Channel", PropertyAttributes.None, typeof(IServiceChannel), Type.EmptyTypes);
            ChannelProperty.SetGetMethod(_mChannelGet);
            ChannelProperty.SetSetMethod(_mChannelSet);
            mChannelGetMethod = _mChannelGet;
        }

        private Type GetType(Type type)
        {
            return (type.HasElementType && !type.IsArray) ? type.GetElementType() : type;
        }

        public void Save()
        {
            mAssemblyBuilder.Save(mAssemblyName + ".dll");
        }

        public void Builder_Service(TypeBuilder myTypeBuilder)
        {
            //builder service
            byte pindex;
            MethodAttributes getSetAttr = MethodAttributes.Public |   MethodAttributes.HideBySig |    MethodAttributes.NewSlot | MethodAttributes.Virtual | 
                MethodAttributes.Final;
            Type ptype;
            foreach (MethodInfo method in mServiceType.GetMembers())
            {
                System.Collections.Generic.List<Type> ptypes = new System.Collections.Generic.List<Type>();
                ParameterInfo[] pis = method.GetParameters();
                foreach (ParameterInfo p in method.GetParameters())
                    ptypes.Add(p.ParameterType);
                MethodBuilder builder = myTypeBuilder.DefineMethod(mServiceType.FullName+"."+ method.Name, getSetAttr, method.ReturnType,
                    ptypes.ToArray());
                for (int i = 0; i < pis.Length; i++)
                {
                    ParameterAttributes pa = ParameterAttributes.None;
                    if (pis[i].IsIn)
                        pa |= ParameterAttributes.In;
                    if (pis[i].IsLcid)
                        pa |= ParameterAttributes.Lcid;
                    if (pis[i].IsOptional)
                        pa |= ParameterAttributes.Optional;
                    if (pis[i].IsOut)
                        pa |= ParameterAttributes.Out;
                    builder.DefineParameter(i + 1, pa, pis[i].Name);
                }
                ILGenerator il = builder.GetILGenerator();
                LocalBuilder result = il.DeclareLocal(typeof(Result));
                LocalBuilder datas = il.DeclareLocal(typeof(object[]));
                for (int i = 0; i < pis.Length; i++)
                {
                    if (pis[i].IsOut)
                    {
                        pindex = (byte)(i + 1);
                        il.Emit(OpCodes.Ldarg_S, pindex);
                        il.Emit(OpCodes.Initobj, GetType(pis[i].ParameterType));
                    }
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, mChannelGetMethod);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, mServiceType.Name);
                il.Emit(OpCodes.Call, typeof(System.Reflection.MethodBase).GetMethod("GetCurrentMethod"));
                il.Emit(OpCodes.Ldc_I4, pis.Length);
                il.Emit(OpCodes.Newarr, typeof(object));
                il.Emit(OpCodes.Stloc_1);
              

                for (int i = 0; i < pis.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg_S, (byte)(i + 1));
                    ptype = GetType(pis[i].ParameterType);
                    if (ptype.IsValueType)
                    {
                        il.Emit(OpCodes.Ldobj, ptype);
                        il.Emit(OpCodes.Box, ptype);
                    }
                    il.Emit(OpCodes.Stelem_Ref);
                }
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Callvirt, typeof(IServiceChannel).GetMethod("Execute"));
                il.Emit(OpCodes.Stloc_0);
                for (int i = 0; i < pis.Length; i++)
                {
                    if (pis[i].IsOut)
                    {
                        il.Emit(OpCodes.Ldarg_S, (byte)(i + 1));
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldstr, pis[i].Name);
                        il.Emit(OpCodes.Callvirt, typeof(Result).GetMethod("get_Item"));
                        ptype = GetType(pis[i].ParameterType);
                        if (ptype.IsValueType)
                        {
                            il.Emit(OpCodes.Unbox_Any, ptype);
                            il.Emit(OpCodes.Stobj, ptype);
                        }
                        else
                        {
                            il.Emit(OpCodes.Castclass, ptype);
                            il.Emit(OpCodes.Stind_Ref);
                        }
                    }
                }
                if (method.ReturnType != typeof(void))
                {
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Callvirt, typeof(Result).GetMethod("get_Data"));
                    if (method.ReturnType.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox_Any, method.ReturnType);
                       // il.Emit(OpCodes.Stobj, method.ReturnType);
                       
                    }
                    else
                    {
                        il.Emit(OpCodes.Castclass, method.ReturnType);
                      
                    }
                   
                }

                il.Emit(OpCodes.Ret);
                myTypeBuilder.DefineMethodOverride(builder, mServiceType.GetMethod(method.Name, ptypes.ToArray()));
            }
        }
        public void Builder()
        {
            AppDomain myCurrentDomain = AppDomain.CurrentDomain;
            AssemblyName myAssemblyName = new AssemblyName();
            myAssemblyName.Name = mAssemblyName;

            // Define a dynamic assembly in the current application domain.
            mAssemblyBuilder = myCurrentDomain.DefineDynamicAssembly
                (myAssemblyName, AssemblyBuilderAccess.RunAndSave);

            // Define a dynamic module in this assembly.
            ModuleBuilder myModuleBuilder = mAssemblyBuilder.
                DefineDynamicModule(mAssemblyName+".dll",mAssemblyName+".dll");

            // Define a runtime class with specified name and attributes.
            mTypeBuilder = myModuleBuilder.DefineType
                (mClassName, TypeAttributes.Public, typeof(object), new Type[]{mServiceType, typeof(ICommunicationObject)});
            Builder_CommunicationObject(mTypeBuilder);
            Builder_Service(mTypeBuilder);
            mServiceImplType=mTypeBuilder.CreateType();
            
            
        }
    }
}

