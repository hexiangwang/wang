package com.hwx.common.util;

import net.sf.cglib.proxy.Enhancer;
import net.sf.cglib.proxy.MethodInterceptor;
import net.sf.cglib.proxy.MethodProxy;

import java.lang.reflect.Method;

public class CglibProxy  implements MethodInterceptor {

    private Enhancer enhancer = new Enhancer();
    public Object getProxy(Class clazz){
        //生成指定类对象的子类,也就是重写类中的业务函数，在重写中加入intercept()函数而已。
        enhancer.setSuperclass(clazz);
        //这里是回调函数，enhancer中肯定有个MethodInterceptor属性。
        //回调函数是在setSuperclass中的那些重写的方法中调用---猜想
        enhancer.setCallback(this);
        //创建这个子类对象
        return enhancer.create();
    }

    public Object intercept(Object obj, Method method, Object[] args,
                            MethodProxy proxy) throws Throwable {
        System.out.println(method.getName()+"执行之前做一些准备工作");
        //一不小心写成下面被注释一行代码了。 StackOverflowError
        Object result = proxy.invokeSuper(obj,args);
        System.out.println(method.getName()+"执行之后做一些准备的工作");
        return result;
    }

}
