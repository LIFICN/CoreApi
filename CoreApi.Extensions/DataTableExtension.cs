using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace CoreApi.Extensions
{
    public static class DataTableExtension
    {
        private static readonly MethodInfo getValueMethod = typeof(DataRow).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo setValueMethod = typeof(DataRow).GetMethod("set_Item", new Type[] { typeof(string), typeof(object) });
        private static readonly MethodInfo getRowMethod = typeof(DataTable).GetMethod("get_Rows");
        private static readonly MethodInfo isDBNullMethod = typeof(DataRow).GetMethod("IsNull", new Type[] { typeof(int) });
        private static readonly MethodInfo addRowMethod = typeof(DataRowCollection).GetMethod("Add", new Type[] { typeof(DataRow) });
        private static readonly MethodInfo newRowMethod = typeof(DataTable).GetMethod("NewRow");
        private static readonly ConcurrentDictionary<Type, Delegate> toListCacheDic = new ConcurrentDictionary<Type, Delegate>();
        private static readonly ConcurrentDictionary<Type, Delegate> toDataTableCacheDic = new ConcurrentDictionary<Type, Delegate>();
        private static readonly ConcurrentDictionary<Type, DataTable> dataTableCacheDic = new ConcurrentDictionary<Type, DataTable>();

        public static List<T> ToList<T>(this DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return null;

            Func<DataRow, T> func = null;
            Type targetTaype = typeof(T);

            if (!toListCacheDic.TryGetValue(targetTaype, out Delegate action))
            {
                DynamicMethod method = new DynamicMethod("DynamicCreateEntity_" + targetTaype.Name, targetTaype, new Type[] { typeof(DataRow) }, targetTaype, true);
                ILGenerator generator = method.GetILGenerator();
                LocalBuilder result = generator.DeclareLocal(targetTaype);
                generator.Emit(OpCodes.Newobj, targetTaype.GetConstructor(Type.EmptyTypes));
                generator.Emit(OpCodes.Stloc, result);

                for (int index = 0; index < dt.Columns.Count; index++)
                {
                    PropertyInfo propertyInfo = targetTaype.GetProperty(dt.Columns[index].ColumnName);
                    Label endIfLabel = generator.DefineLabel();
                    if (propertyInfo != null && propertyInfo.GetSetMethod() != null)
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldc_I4, index);
                        generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                        generator.Emit(OpCodes.Brtrue, endIfLabel);
                        generator.Emit(OpCodes.Ldloc, result);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldc_I4, index);
                        generator.Emit(OpCodes.Callvirt, getValueMethod);
                        generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                        generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                        generator.MarkLabel(endIfLabel);
                    }
                }
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ret);

                func = (Func<DataRow, T>)method.CreateDelegate(typeof(Func<DataRow, T>));

                if (func != null)
                    toListCacheDic.TryAdd(targetTaype, func);
            }
            else
            {
                func = (Func<DataRow, T>)action;
            }

            var rows = dt.Rows;
            List<T> list = new List<T>(rows.Count);

            for (int i = 0; i < rows.Count; i++)
                list.Add(func(rows[i]));

            return list;
        }

        public static DataTable ToDataTable<T>(this List<T> list)
        {
            if (list == null && list.Count == 0)
                return null;

            var type = typeof(T);
            var properties = type.GetProperties();
            Action<DataTable, T> ac = null;

            if (!toDataTableCacheDic.TryGetValue(type, out Delegate action))
            {
                DynamicMethod method = new DynamicMethod(type.Name + "ToDataTable", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, null, new Type[] { typeof(DataTable), type }, type.Module, true);
                ILGenerator generator = method.GetILGenerator();

                //创建行 实现DataRow row=dt.NewRow();
                LocalBuilder reslut = generator.DeclareLocal(typeof(DataRow));
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, newRowMethod);
                generator.Emit(OpCodes.Stloc, reslut);//结果存储

                Dictionary<string, Type> dic = new Dictionary<string, Type>();
                Dictionary<string, LocalBuilder> dicLocalBuilder = new Dictionary<string, LocalBuilder>();
                List<LocalBuilder> lstLocal = new List<LocalBuilder>();
                var table = new DataTable();

                //获取空类型属性
                for (int i = 0; i < properties.Length; i++)
                {
                    var item = properties[i];
                    var cur = Nullable.GetUnderlyingType(item.PropertyType);
                    var propType = item.PropertyType;
                    var fullName = propType.FullName;

                    if (cur != null)
                    {
                        //定义足够的bool
                        lstLocal.Add(generator.DeclareLocal(typeof(bool)));

                        //获取所有空类型属性的类型
                        dic[item.Name] = propType;

                        //定义包含的空类型
                        if (!dicLocalBuilder.ContainsKey(fullName))
                        {
                            dicLocalBuilder[fullName] = generator.DeclareLocal(propType);
                        }
                    }

                    //创建dataColumn模板
                    table.Columns.Add(new DataColumn(item.Name, cur ?? propType));
                }

                //添加dataTable到缓存
                dataTableCacheDic.TryAdd(type, table);

                //没有列名称映射
                int index = -1;//必须-1合适

                //遍历属性
                foreach (var p in properties)
                {
                    if (dic.TryGetValue(p.Name, out Type prop))
                    {
                        var endIfLabel = generator.DefineLabel();
                        //判断
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Call, p.GetGetMethod());//

                        generator.Emit(OpCodes.Stloc_S, dicLocalBuilder[p.PropertyType.FullName]);
                        generator.Emit(OpCodes.Ldloca_S, dicLocalBuilder[p.PropertyType.FullName]);
                        generator.Emit(OpCodes.Call, prop.GetMethod("get_HasValue"));

                        generator.Emit(OpCodes.Stloc, lstLocal[++index]);
                        generator.Emit(OpCodes.Ldloc, lstLocal[index]);
                        generator.Emit(OpCodes.Brfalse_S, endIfLabel);
                        //赋值
                        generator.Emit(OpCodes.Ldloc, reslut);//取出变量
                        generator.Emit(OpCodes.Ldstr, p.Name);//row["Name"]
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Call, p.GetGetMethod());//
                                                                       //
                        generator.Emit(OpCodes.Stloc_S, dicLocalBuilder[p.PropertyType.FullName]);
                        generator.Emit(OpCodes.Ldloca_S, dicLocalBuilder[p.PropertyType.FullName]);
                        generator.Emit(OpCodes.Call, prop.GetMethod("get_Value"));

                        generator.Emit(OpCodes.Box, Nullable.GetUnderlyingType(p.PropertyType));
                        generator.Emit(OpCodes.Call, setValueMethod);
                        generator.MarkLabel(endIfLabel);
                    }
                    else
                    {
                        generator.Emit(OpCodes.Ldloc, reslut);
                        generator.Emit(OpCodes.Ldstr, p.Name);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Call, p.GetGetMethod());//获取属性

                        if (p.PropertyType.IsValueType)
                            generator.Emit(OpCodes.Box, p.PropertyType);
                        else
                            generator.Emit(OpCodes.Castclass, p.PropertyType);

                        generator.Emit(OpCodes.Call, setValueMethod);
                    }
                }
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, getRowMethod);
                generator.Emit(OpCodes.Ldloc, reslut);
                generator.Emit(OpCodes.Call, addRowMethod);
                generator.Emit(OpCodes.Ret);

                ac = (Action<DataTable, T>)method.CreateDelegate(typeof(Action<DataTable, T>));

                if (ac != null)
                    toDataTableCacheDic.TryAdd(type, ac);
            }
            else
            {
                ac = (Action<DataTable, T>)action;
            }

            DataTable newTable = dataTableCacheDic[type].Clone();

            for (int i = 0; i < list.Count; i++)
                ac(newTable, list[i]);

            return newTable;
        }
    }
}