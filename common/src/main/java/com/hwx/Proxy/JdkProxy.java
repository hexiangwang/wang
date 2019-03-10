package com.hwx.Proxy;

import java.lang.reflect.InvocationHandler;
import java.lang.reflect.Method;

public class JdkProxy implements InvocationHandler{
    private Object target;
    public JdkProxy(){

    }
    public JdkProxy(Object target){
        this.target = target;
    }
    public Object invoke(Object proxy, Method method, Object[] args) throws Throwable {
        Object result = method.invoke(target, args);
        return result;
    }

    public Object getProxy(Object target) throws Exception{
        this.target = target;
        return  newProxyInstance(target.getClass(),this);
    }


    public static Object newProxyInstance(Class clazz,InvocationHandler invocationHandler){
        return java.lang.reflect.Proxy.newProxyInstance(clazz.getClassLoader(),clazz.getInterfaces(),invocationHandler);
    }
}