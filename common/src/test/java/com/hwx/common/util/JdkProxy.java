package com.hwx.common.util;

import java.lang.reflect.InvocationHandler;
import java.lang.reflect.Method;
import java.lang.reflect.Proxy;

public class JdkProxy implements InvocationHandler {
    private Object obj;

    public Object getProxy(Class clazz) throws Exception{
//        this.obj = obj;
//        Class clazz = obj.getClass();
         this.obj=clazz.newInstance();
        return Proxy.newProxyInstance(clazz.getClassLoader(),clazz.getInterfaces(),this);
    }

    public Object invoke(Object proxy, Method method, Object[] args) throws Throwable {
        System.out.println(method.getName()+"执行之前做一些准备工作");
        Object result = method.invoke(obj,args);
        System.out.println(method.getName()+"执行之后做一些准备的工作");
        return result;
    }

}
