package com.hwx.common.util;


import java.beans.Introspector;
import java.beans.PropertyDescriptor;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.*;

public class EntityUtils {
    /**
     * Map转实体
     */
    public static  <T> T toEntity(Map<String,Object> map,Class<T> clazz){
        try {
            T entity = clazz.newInstance();
            PropertyDescriptor[] properties = getProperties(clazz);
            for (PropertyDescriptor property : properties) {
                Object value = map.get(property.getName());
                if(value!=null){
                    property.getWriteMethod().invoke(entity,value);
                }
            }
            return entity;
        }
        catch (Exception ex){
            throw new RuntimeException(ex);
        }
    }

    /**
     *复制属性
     */
    public static void copyProperties(Object src,Object des){
        copyProperties(src,des,true);
    }

    /**
     * 复制属性
     * @param override 是否覆盖原属性
     */
    public static void copyProperties(Object src,Object des,boolean override){
        try {
            PropertyDescriptor[] srcProperties = getProperties(src.getClass());
            PropertyDescriptor[] desProperties = getProperties(des.getClass());
            for (PropertyDescriptor srcProperty : srcProperties) {
                for (PropertyDescriptor desProperty : desProperties) {
                    if (srcProperty.getName().equals(desProperty.getName())) {
                        Method readMethod =  srcProperty.getReadMethod();
                        Method writerMethod = desProperty.getWriteMethod();
                        if(readMethod!=null && writerMethod!=null) {
                            if(!override){
                                Method method = desProperty.getReadMethod();
                                if(method!=null&&method.invoke(des)!=null){
                                    continue;
                                }
                            }
                            Object value = readMethod.invoke(src);
                            writerMethod.invoke(des, value);
                        }
                    }
                }
            }
        }
        catch (Exception ex){
            throw new RuntimeException(ex);
        }
    }

    /**
     * 实体转Map
     */
    public static <T> Map<String,Object> toMap(T entity){
        Map<String,Object> map = new HashMap<>();
        try {
            Class clazz = entity.getClass();
            Field[] fields = getAllField(clazz);
            for (Field field : fields) {
                PropertyDescriptor property = new PropertyDescriptor(field.getName(), clazz);
                Object value = property.getReadMethod().invoke(entity);
                map.put(property.getName(),value);
            }
            return map;
        }catch (Exception ex){
            throw new RuntimeException(ex);
        }
    }




    /**
     * 反射得到所以字段
     */
    public static Field[] getAllField(Class clazz){
        List<Field> fieldList = new ArrayList<>();
        do{//反射
            List<Field> tempList = Arrays.asList(clazz.getDeclaredFields());//array转list
            fieldList.addAll(tempList);
            clazz = clazz.getSuperclass();
        }while (clazz!=null);
        //list转array
        Field[] fields = new Field[fieldList.size()];
        return fieldList.toArray(fields);
    }

    /**
     * 内省得到所有属性
     */
    public static PropertyDescriptor[] getProperties(Class clazz){
        try {
            //内省
            return Introspector.getBeanInfo(clazz).getPropertyDescriptors();
        }
        catch (Exception ex){
            throw new RuntimeException(ex);
        }
    }





}
